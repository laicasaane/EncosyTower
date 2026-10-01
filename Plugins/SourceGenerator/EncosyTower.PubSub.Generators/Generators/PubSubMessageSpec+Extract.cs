namespace EncosyTower.PubSub.Generators
{
    internal readonly partial struct PubSubMessageSpec
    {
        private static readonly SymbolDisplayFormat s_displayFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public static PubSubMessageSpec Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax currentDeclaration
                || context.TargetSymbol is not INamedTypeSymbol message
            )
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;

            if (compilation.IsValidCompilation(
                      token
                    , PubSubSourceGenContract.NAMESPACE
                    , PubSubSourceGenContract.SKIP_ATTRIBUTE
                ) == false
            )
            {
                return default;
            }

            var marker = compilation.GetTypeByMetadataName(PubSubSourceGenContract.PUBSUB_ATTRIBUTE);
            var globalScope = compilation.GetTypeByMetadataName(PubSubSourceGenContract.GLOBAL_SCOPE);

            if (marker is null || globalScope is null || IsUnsupportedDeclaration(message, token))
            {
                return default;
            }

            var applications = GetOrderedApplications(message, marker, compilation, token);

            if (applications.Count == 0
                || IsSameDeclaration(currentDeclaration, applications[0].Declaration) == false
            )
            {
                return default;
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  currentDeclaration
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PubSubAliasSet.WriteAliases
            );

            var declaration = new PubSubTypeDeclarationSpec(
                  openingSource
                , closingSource
                , GetDeclarationName(currentDeclaration)
                , message.ToPartialTypeKeyword()
                , message.ToDisplayString(s_displayFormat)
                , message.ToMetadataName()
                , message.IsReferenceType
            );
            var assemblyName = compilation.AssemblyName ?? string.Empty;
            var messageHint = SourceGenHelpers.BuildSemanticHintName(
                  PubSubSourceGenContract.GENERATOR_METADATA_NAME
                , assemblyName
                , declaration.MetadataName
                , PubSubSourceGenContract.MESSAGE_ROLE
                , string.Empty
            );
            var canPublishParameterless = DetermineCanPublishParameterless(message);
            var scopes = CreateScopes(
                  applications
                , message
                , declaration
                , canPublishParameterless
                , assemblyName
                , globalScope
                , compilation
                , token
            );
            var withSync = false;
            var withAsync = false;
            var scopeCount = scopes.Count;

            for (var i = 0; i < scopeCount; i++)
            {
                var scopeSpec = scopes[i];
                withSync |= scopeSpec.WithSync;
                withAsync |= scopeSpec.WithAsync;
            }

            return new PubSubMessageSpec(
                  declaration
                , scopes.ToImmutableArray().AsEquatableArray()
                , messageHint
                , canPublishParameterless
                , withSync
                , withAsync
            );
        }

        private static List<PubSubScopeSpec> CreateScopes(
              List<AttributeApplication> applications
            , INamedTypeSymbol message
            , PubSubTypeDeclarationSpec declaration
            , bool canPublishParameterless
            , string assemblyName
            , INamedTypeSymbol globalScope
            , Compilation compilation
            , CancellationToken token
        )
        {
            var scopes = new List<PubSubScopeSpec>(applications.Count);
            var publisher = compilation.GetTypeByMetadataName(PubSubSourceGenContract.PUBLISHER_OF_T);
            var unityPublisher = compilation.GetTypeByMetadataName(PubSubSourceGenContract.UNITY_PUBLISHER_OF_T);
            var unityObject = compilation.GetTypeByMetadataName(PubSubSourceGenContract.UNITY_OBJECT);
            var applicationCount = applications.Count;

            for (var i = 0; i < applicationCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var application = applications[i];

                if (TryDecodeMode(application.Attribute, out var withSync, out var withAsync) == false)
                {
                    continue;
                }

                DecodeScope(application.Attribute, globalScope, out var scope, out var hasExplicitScope);

                if (IsValidScope(scope, publisher, unityPublisher, unityObject) == false)
                {
                    continue;
                }

                DecodeState(application.Attribute, out var withStateless, out var withStateful);

                var isUnityScope = DerivesFrom(scope, unityObject);
                var isGlobalScope = SymbolEqualityComparer.Default.Equals(scope, globalScope);
                var scopeTypeName = isGlobalScope && hasExplicitScope == false
                    ? PubSubAliasSet.COMMON + ".GlobalScope"
                    : scope.ToDisplayString(s_displayFormat);
                var discriminator = CreateDiscriminator(application, message, token);
                var hintName = SourceGenHelpers.BuildSemanticHintName(
                      PubSubSourceGenContract.GENERATOR_METADATA_NAME
                    , assemblyName
                    , declaration.MetadataName
                    , PubSubSourceGenContract.SCOPE_ROLE
                    , discriminator
                );

                scopes.Add(new PubSubScopeSpec(
                      declaration
                    , scopeTypeName
                    , hintName
                    , canPublishParameterless
                    , withSync
                    , withAsync
                    , withStateless
                    , withStateful
                    , isGlobalScope
                    , isUnityScope
                ));
            }

            return scopes;
        }

        private static List<AttributeApplication> GetOrderedApplications(
              INamedTypeSymbol message
            , INamedTypeSymbol marker
            , Compilation compilation
            , CancellationToken token
        )
        {
            var treeOrdinals = new Dictionary<SyntaxTree, int>();
            var treeIndex = 0;

            foreach (var tree in compilation.SyntaxTrees)
            {
                treeOrdinals[tree] = treeIndex++;
            }

            var result = new List<AttributeApplication>();
            var attributes = message.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker) == false
                    || attribute.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax
                    || syntax.FirstAncestorOrSelf<TypeDeclarationSyntax>() is not { } declaration
                )
                {
                    continue;
                }

                treeOrdinals.TryGetValue(syntax.SyntaxTree, out var ordinal);
                result.Add(new AttributeApplication(attribute, syntax, declaration, ordinal));
            }

            result.Sort(static (left, right) => {
                var result = left.TreeOrdinal.CompareTo(right.TreeOrdinal);
                return result != 0 ? result : left.Syntax.SpanStart.CompareTo(right.Syntax.SpanStart);
            });
            return result;
        }

        private static void DecodeScope(
              AttributeData attribute
            , INamedTypeSymbol globalScope
            , out ITypeSymbol scope
            , out bool hasExplicitScope
        )
        {
            scope = globalScope;
            hasExplicitScope = false;

            var arguments = attribute.NamedArguments;
            var argumentCount = arguments.Length;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.Key == "Scope")
                {
                    hasExplicitScope = true;
                    scope = argument.Value.IsNull ? null : argument.Value.Value as ITypeSymbol;
                }
            }
        }

        private static bool TryDecodeMode(AttributeData attribute, out bool withSync, out bool withAsync)
        {
            withSync = false;
            withAsync = false;

            var arguments = attribute.ConstructorArguments;

            if (arguments.Length != 1 || arguments[0].Value is not int value)
            {
                return false;
            }

            switch ((ApiModeValue)value)
            {
                case ApiModeValue.Sync:
                {
                    withSync = true;
                    return true;
                }

                case ApiModeValue.Async:
                {
                    withAsync = true;
                    return true;
                }

                case ApiModeValue.Both:
                {
                    withSync = true;
                    withAsync = true;
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }

        private static void DecodeState(AttributeData attribute, out bool withStateless, out bool withStateful)
        {
            withStateless = true;
            withStateful = true;

            var arguments = attribute.NamedArguments;
            var argumentCount = arguments.Length;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.Key != "State" || argument.Value.Value is not int value)
                {
                    continue;
                }

                switch ((StateModeValue)value)
                {
                    case StateModeValue.Stateless:
                    {
                        withStateless = true;
                        withStateful = false;
                        break;
                    }

                    case StateModeValue.Stateful:
                    {
                        withStateless = false;
                        withStateful = true;
                        break;
                    }

                    default:
                    {
                        withStateless = true;
                        withStateful = true;
                        break;
                    }
                }
            }
        }

        private static bool IsValidScope(
              ITypeSymbol scope
            , INamedTypeSymbol publisher
            , INamedTypeSymbol unityPublisher
            , INamedTypeSymbol unityObject
        )
        {
            if (scope is null
                || scope.TypeKind is TypeKind.Error or TypeKind.TypeParameter or TypeKind.Pointer
                    or TypeKind.FunctionPointer
                || scope.SpecialType is SpecialType.System_Void or SpecialType.System_TypedReference
                    or SpecialType.System_ArgIterator or SpecialType.System_RuntimeArgumentHandle
                || scope.IsRefLikeType
                || ContainsTypeParameter(scope)
                || scope is INamedTypeSymbol { IsStatic: true }
            )
            {
                return false;
            }

            return DerivesFrom(scope, unityObject) ? unityPublisher is not null : publisher is not null;
        }

        private static bool ContainsTypeParameter(ITypeSymbol type)
        {
            if (type.TypeKind == TypeKind.TypeParameter)
            {
                return true;
            }

            if (type is IArrayTypeSymbol array)
            {
                return ContainsTypeParameter(array.ElementType);
            }

            if (type is INamedTypeSymbol named)
            {
                if (named.IsUnboundGenericType)
                {
                    return true;
                }

                var arguments = named.TypeArguments;
                var argumentCount = arguments.Length;

                for (var i = 0; i < argumentCount; i++)
                {
                    var argument = arguments[i];

                    if (ContainsTypeParameter(argument))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool DerivesFrom(ITypeSymbol type, INamedTypeSymbol baseType)
        {
            if (baseType is null || type is not INamedTypeSymbol named)
            {
                return false;
            }

            for (var current = named; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, baseType))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetDeclarationName(TypeDeclarationSyntax declaration)
            => declaration.Identifier.ValueText + declaration.TypeParameterList;

        private static bool IsSameDeclaration(TypeDeclarationSyntax left, TypeDeclarationSyntax right)
            => left.SyntaxTree == right.SyntaxTree && left.Span == right.Span;

        private static bool IsUnsupportedDeclaration(INamedTypeSymbol message, CancellationToken token)
        {
            if (message.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || message.IsStatic
                || message.IsRefLikeType
            )
            {
                return true;
            }

            var references = message.DeclaringSyntaxReferences;
            var referenceCount = references.Length;

            for (var i = 0; i < referenceCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var reference = references[i];

                if (reference.GetSyntax(token) is not TypeDeclarationSyntax declaration)
                {
                    continue;
                }

                var modifiers = declaration.Modifiers;
                var modifierCount = modifiers.Count;

                for (var j = 0; j < modifierCount; j++)
                {
                    var modifier = modifiers[j];

                    if (modifier.ValueText == "file")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool DetermineCanPublishParameterless(INamedTypeSymbol message)
        {
            if (message.TypeKind == TypeKind.Struct)
            {
                return true;
            }

            if (message.IsAbstract || message.IsStatic || message.TypeKind != TypeKind.Class)
            {
                return false;
            }

            var constructors = message.InstanceConstructors;
            var count = constructors.Length;

            for (var i = 0; i < count; i++)
            {
                var constructor = constructors[i];

                if (constructor.Parameters.Length == 0
                    && constructor.DeclaredAccessibility == Accessibility.Public
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static string CreateDiscriminator(
              AttributeApplication application
            , INamedTypeSymbol message
            , CancellationToken token
        )
        {
            var declarations = new List<TypeDeclarationSyntax>();
            var references = message.DeclaringSyntaxReferences;
            var referenceCount = references.Length;

            for (var i = 0; i < referenceCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var reference = references[i];

                if (reference.GetSyntax(token) is TypeDeclarationSyntax declaration)
                {
                    declarations.Add(declaration);
                }
            }

            declarations.Sort(static (left, right) => {
                var leftPath = NormalizePath(left.SyntaxTree.FilePath);
                var rightPath = NormalizePath(right.SyntaxTree.FilePath);
                var result = StringComparer.Ordinal.Compare(leftPath, rightPath);
                return result != 0 ? result : left.SpanStart.CompareTo(right.SpanStart);
            });
            var part = 0;
            var declarationCount = declarations.Count;

            for (var i = 0; i < declarationCount; i++)
            {
                if (IsSameDeclaration(declarations[i], application.Declaration))
                {
                    part = i;
                    break;
                }
            }

            var leafName = Path.GetFileName(application.Syntax.SyntaxTree.FilePath ?? string.Empty);
            return leafName + "|part:" + part + "|span:" + application.Syntax.SpanStart;
        }

        private static string NormalizePath(string path)
            => (path ?? string.Empty).Replace('\\', '/');

        private enum ApiModeValue
        {
            Sync = 0,
            Async = 1,
            Both = 2,
        }

        private enum StateModeValue
        {
            Stateless = 0,
            Stateful = 1,
            Both = 2,
        }

        private readonly struct AttributeApplication
        {
            public AttributeApplication(
                  AttributeData attribute
                , AttributeSyntax syntax
                , TypeDeclarationSyntax declaration
                , int treeOrdinal
            )
            {
                Attribute = attribute;
                Syntax = syntax;
                Declaration = declaration;
                TreeOrdinal = treeOrdinal;
            }

            public AttributeData Attribute { get; }

            public AttributeSyntax Syntax { get; }

            public TypeDeclarationSyntax Declaration { get; }

            public int TreeOrdinal { get; }
        }
    }
}
