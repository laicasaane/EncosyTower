namespace EncosyTower.Processing.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed partial class ProcessingRequestAnalyzer : DiagnosticAnalyzer
    {
        private const string PROCESSING_ATTRIBUTE = "global::EncosyTower.Processing.ProcessingAttribute";
        private const string REQUEST_T = "global::EncosyTower.Processing.IRequest<TResult>";
        private const string ASYNC_REQUEST = "global::EncosyTower.Processing.IAsyncRequest";
        private const string ASYNC_REQUEST_T = "global::EncosyTower.Processing.IAsyncRequest<TResult>";
        private const string HUB_T = "EncosyTower.Processing.Processor+Hub`1";
        private const string UNITY_HUB_T = "EncosyTower.Processing.Processor+UnityHub`1";
        private const string GLOBAL_SCOPE = "EncosyTower.Common.GlobalScope";
        private const string SKIP_ATTRIBUTE =
            "global::EncosyTower.Processing.SkipSourceGeneratorsForAssemblyAttribute";
        private const string UNITY_OBJECT = "global::UnityEngine.Object";

        private static readonly SymbolDisplayFormat s_displayFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol request
                || request.HasAttribute(PROCESSING_ATTRIBUTE, token) == false
                || context.Compilation.Assembly.HasAttribute(SKIP_ATTRIBUTE, token)
            )
            {
                return;
            }

            var globalScope = context.Compilation.GetTypeByMetadataName(GLOBAL_SCOPE);

            if (globalScope is null)
            {
                return;
            }

            var attributes = GetDirectProcessingAttributes(request, token);

            if (attributes.IsDefaultOrEmpty)
            {
                return;
            }

            var requestName = request.ToDisplayString(s_displayFormat);
            var requestLocation = GetRequestIdentifierLocation(request, token);

            if (IsUnsupportedDeclaration(request, token))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsupportedDeclaration, requestLocation, requestName));
                return;
            }

            var withAsync = AnalyzeScopes(context, requestName, attributes, globalScope);
            AnalyzeEmissionSpecific(context, request, requestName, requestLocation, withAsync);
        }

        private static ImmutableArray<AttributeData> GetDirectProcessingAttributes(
              INamedTypeSymbol request
            , CancellationToken token
        )
        {
            var builder = ImmutableArray.CreateBuilder<AttributeData>();
            var attributes = request.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (attribute.AttributeClass.HasFullName(PROCESSING_ATTRIBUTE, token))
                {
                    builder.Add(attribute);
                }
            }

            return builder.ToImmutable();
        }

        private static bool AnalyzeScopes(
              SymbolAnalysisContext context
            , string requestName
            , ImmutableArray<AttributeData> attributes
            , INamedTypeSymbol globalScope
        )
        {
            var scopes = new List<ProcessingScopeInfo>(attributes.Length);
            var withAsync = false;
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (TryDecodeMode(
                      attribute
                    , context.CancellationToken
                    , out var mode
                    , out var modeLocation
                    , out var rawMode
                    , out var hasModeValue
                ) == false)
                {
                    if (hasModeValue)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              InvalidApiMode
                            , modeLocation
                            , requestName
                            , rawMode
                        ));
                    }

                    continue;
                }

                if (TryDecodeState(
                      attribute
                    , context.CancellationToken
                    , out _
                    , out var stateLocation
                    , out var rawState
                    , out var hasStateValue
                ) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidStateMode
                        , stateLocation
                        , requestName
                        , rawState
                    ));
                }

                var scopeInfo = DecodeScope(attribute, globalScope, context.CancellationToken);

                if (IsValidScope(scopeInfo.Scope, context.Compilation) == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidScope
                        , scopeInfo.Location
                        , requestName
                        , scopeInfo.DisplayName
                    ));
                    continue;
                }

                scopes.Add(scopeInfo);
                withAsync |= mode is ApiModeValue.Async or ApiModeValue.Both;
            }

            var counts = new Dictionary<ITypeSymbol, int>(SymbolEqualityComparer.Default);
            var scopeCount = scopes.Count;

            for (var i = 0; i < scopeCount; i++)
            {
                var scopeInfo = scopes[i];
                counts.TryGetValue(scopeInfo.Scope, out var count);
                counts[scopeInfo.Scope] = count + 1;
            }

            for (var i = 0; i < scopeCount; i++)
            {
                var scopeInfo = scopes[i];

                if (counts[scopeInfo.Scope] > 1)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          RepeatedScope
                        , scopeInfo.Location
                        , scopeInfo.DisplayName
                        , requestName
                    ));
                }
            }

            return withAsync;
        }

        private static void AnalyzeEmissionSpecific(
              SymbolAnalysisContext context
            , INamedTypeSymbol request
            , string requestName
            , Location requestLocation
            , bool withAsync
        )
        {
            var token = context.CancellationToken;
            var results = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var redundant = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var interfaces = request.AllInterfaces;
            var interfaceCount = interfaces.Length;

            for (var i = 0; i < interfaceCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var @interface = interfaces[i];

                if (@interface.OriginalDefinition.HasFullName(REQUEST_T, token))
                {
                    results.Add(@interface.TypeArguments[0]);
                }

                if (withAsync
                    && (@interface.HasFullName(ASYNC_REQUEST, token)
                    || @interface.OriginalDefinition.HasFullName(ASYNC_REQUEST_T, token)
                    )
                )
                {
                    redundant.Add(@interface);
                }
            }

            if (results.Count > 1)
            {
                var resultNames = new string[results.Count];
                var index = 0;

                foreach (var result in results)
                {
                    resultNames[index++] = result.ToDisplayString(s_displayFormat);
                }

                Array.Sort(resultNames, StringComparer.Ordinal);
                context.ReportDiagnostic(Diagnostic.Create(
                      ResultTypeConflict
                    , requestLocation
                    , requestName
                    , string.Join(", ", resultNames)
                ));
            }

            var redundantNames = new List<string>(redundant.Count);

            foreach (var @interface in redundant)
            {
                redundantNames.Add(@interface.ToDisplayString(s_displayFormat));
            }

            redundantNames.Sort(StringComparer.Ordinal);
            var redundantNameCount = redundantNames.Count;

            for (var i = 0; i < redundantNameCount; i++)
            {
                var redundantName = redundantNames[i];
                context.ReportDiagnostic(Diagnostic.Create(
                      RedundantAsyncRequestInterface
                    , requestLocation
                    , requestName
                    , redundantName
                ));
            }
        }

        private static ProcessingScopeInfo DecodeScope(
              AttributeData attribute
            , INamedTypeSymbol globalScope
            , CancellationToken token
        )
        {
            var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(token) as AttributeSyntax;
            var location = attributeSyntax?.Name.GetLocation() ?? Location.None;
            var arguments = attribute.NamedArguments;
            var argumentCount = arguments.Length;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.Key != "Scope")
                {
                    continue;
                }

                var valueSyntax = FindNamedArgument(attributeSyntax, "Scope")?.Expression;
                location = valueSyntax?.GetLocation() ?? location;

                if (argument.Value.IsNull || argument.Value.Value is not ITypeSymbol scope)
                {
                    return new ProcessingScopeInfo(null, location, "null");
                }

                return new ProcessingScopeInfo(scope, location, scope.ToDisplayString(s_displayFormat));
            }

            return new ProcessingScopeInfo(globalScope, location, globalScope.ToDisplayString(s_displayFormat));
        }

        private static AttributeArgumentSyntax FindNamedArgument(AttributeSyntax syntax, string name)
        {
            if (syntax?.ArgumentList is null)
            {
                return null;
            }

            var arguments = syntax.ArgumentList.Arguments;
            var argumentCount = arguments.Count;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.NameEquals?.Name.Identifier.ValueText == name)
                {
                    return argument;
                }
            }

            return null;
        }

        private static bool TryDecodeState(
              AttributeData attribute
            , CancellationToken token
            , out StateModeValue state
            , out Location location
            , out int rawState
            , out bool hasStateValue
        )
        {
            var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(token) as AttributeSyntax;
            var arguments = attribute.NamedArguments;

            state = StateModeValue.Both;
            location = attributeSyntax?.Name.GetLocation() ?? Location.None;
            rawState = default;
            hasStateValue = false;

            var argumentCount = arguments.Length;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.Key != "State" || argument.Value.Value is not int value)
                {
                    continue;
                }

                hasStateValue = true;
                rawState = value;

                var valueSyntax = FindNamedArgument(attributeSyntax, "State")?.Expression;
                location = valueSyntax?.GetLocation() ?? location;

                switch ((StateModeValue)value)
                {
                    case StateModeValue.Stateless:
                    {
                        state = StateModeValue.Stateless;
                        return true;
                    }

                    case StateModeValue.Stateful:
                    {
                        state = StateModeValue.Stateful;
                        return true;
                    }

                    case StateModeValue.Both:
                    {
                        state = StateModeValue.Both;
                        return true;
                    }

                    default:
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool TryDecodeMode(
              AttributeData attribute
            , CancellationToken token
            , out ApiModeValue mode
            , out Location location
            , out int rawMode
            , out bool hasModeValue
        )
        {
            var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(token) as AttributeSyntax;
            var argumentSyntax = FindConstructorArgument(attributeSyntax);
            var arguments = attribute.ConstructorArguments;

            mode = default;
            location = argumentSyntax?.Expression.GetLocation()
                ?? attributeSyntax?.Name.GetLocation()
                ?? Location.None;
            rawMode = default;
            hasModeValue = arguments.Length == 1 && arguments[0].Value is int;

            if (hasModeValue == false)
            {
                return false;
            }

            rawMode = (int)arguments[0].Value;

            switch ((ApiModeValue)rawMode)
            {
                case ApiModeValue.Sync:
                {
                    mode = ApiModeValue.Sync;
                    return true;
                }

                case ApiModeValue.Async:
                {
                    mode = ApiModeValue.Async;
                    return true;
                }

                case ApiModeValue.Both:
                {
                    mode = ApiModeValue.Both;
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }

        private static AttributeArgumentSyntax FindConstructorArgument(AttributeSyntax syntax)
        {
            if (syntax?.ArgumentList is null)
            {
                return null;
            }

            var arguments = syntax.ArgumentList.Arguments;
            var argumentCount = arguments.Count;

            for (var i = 0; i < argumentCount; i++)
            {
                var argument = arguments[i];

                if (argument.NameEquals is null)
                {
                    return argument;
                }
            }

            return null;
        }

        private static bool IsValidScope(ITypeSymbol scope, Compilation compilation)
        {
            if (scope is null
                || scope.TypeKind is TypeKind.Error or TypeKind.TypeParameter or TypeKind.Pointer
                    or TypeKind.FunctionPointer
                || scope.SpecialType is SpecialType.System_Void or SpecialType.System_TypedReference
                    or SpecialType.System_ArgIterator or SpecialType.System_RuntimeArgumentHandle
                || scope.IsRefLikeType
                || ContainsTypeParameter(scope)
            )
            {
                return false;
            }

            if (scope is INamedTypeSymbol { IsStatic: true })
            {
                return false;
            }

            var hubName = scope.InheritsFromType(UNITY_OBJECT) ? UNITY_HUB_T : HUB_T;
            return compilation.GetTypeByMetadataName(hubName) is not null;
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

            if (type is IPointerTypeSymbol pointer)
            {
                return ContainsTypeParameter(pointer.PointedAtType);
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

        private static bool IsUnsupportedDeclaration(INamedTypeSymbol request, CancellationToken token)
        {
            if (request.TypeKind is not (TypeKind.Class or TypeKind.Struct)
                || request.IsStatic
                || request.IsRefLikeType
            )
            {
                return true;
            }

            var syntaxReferences = request.DeclaringSyntaxReferences;
            var syntaxReferenceCount = syntaxReferences.Length;

            for (var i = 0; i < syntaxReferenceCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var syntaxReference = syntaxReferences[i];

                if (syntaxReference.GetSyntax(token) is not TypeDeclarationSyntax syntax)
                {
                    continue;
                }

                var modifiers = syntax.Modifiers;
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

        private static Location GetRequestIdentifierLocation(INamedTypeSymbol request, CancellationToken token)
        {
            var syntaxReferences = request.DeclaringSyntaxReferences;
            var syntaxReferenceCount = syntaxReferences.Length;

            for (var i = 0; i < syntaxReferenceCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var syntaxReference = syntaxReferences[i];

                if (syntaxReference.GetSyntax(token) is TypeDeclarationSyntax syntax)
                {
                    return syntax.Identifier.GetLocation();
                }
            }

            return request.Locations.Length > 0 ? request.Locations[0] : Location.None;
        }

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

        private readonly struct ProcessingScopeInfo
        {
            public ProcessingScopeInfo(ITypeSymbol scope, Location location, string displayName)
            {
                Scope = scope;
                Location = location;
                DisplayName = displayName;
            }

            public ITypeSymbol Scope { get; }

            public Location Location { get; }

            public string DisplayName { get; }
        }
    }
}
