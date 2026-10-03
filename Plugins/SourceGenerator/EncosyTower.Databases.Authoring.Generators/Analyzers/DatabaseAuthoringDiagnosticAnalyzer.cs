#pragma warning disable RS2008 // Enable analyzer release tracking
#pragma warning disable RS1032 // Define diagnostic message correctly
#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace EncosyTower.Databases.Authoring.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DatabaseAuthoringDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        private const string DATABASES_AUTHORING_NAMESPACE = DATABASES_NAMESPACE + ".Authoring";
        private const string DATA_NAMESPACE = "EncosyTower.Data";

        private const string AUTHOR_DATABASE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.AuthorDatabaseAttribute";
        private const string CONVERTER_FOR_TABLE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForTableAttribute";
        private const string CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForDataPropertyAttribute";
        private const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        private const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";
        private const string DATA_TABLE_ASSET_BASE =
            $"global::{DATABASES_NAMESPACE}.DataTableAssetBase";
        private const string DATA_AUTHORING_CONVERTER_ATTRIBUTE = $"global::{DATA_NAMESPACE}.Authoring.DataAuthoringConverterAttribute";
        private const string DATA_MANUAL_AUTHORING_ATTRIBUTE =
            $"global::{DATA_NAMESPACE}.Authoring.DataManualAuthoringAttribute";
        private const string DATA_PROPERTY_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataPropertyAttribute";
        private const string GENERATED_PROPERTY_FROM_FIELD =
            $"global::{DATA_NAMESPACE}.SourceGen.GeneratedPropertyFromFieldAttribute";
        private const string HORIZONTAL_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.HorizontalAttribute";
        private const string IDATA = $"global::{DATA_NAMESPACE}.IData";
        private const string DATA_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataAttribute";
        private const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";
        private const string AUTHOR_DATABASE_DISPLAY = "[AuthorDatabase]";
        private const string DATA_PROPERTY_FIELD_TYPE_PARAMETER = "fieldType";
        private const string DATA_MANUAL_AUTHORING_TYPE_PARAMETER = "authoringType";

        public static readonly DiagnosticDescriptor MissingDefaultConstructor = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0010"
            , title: "Missing default constructor"
            , messageFormat: "The type \"{0}\" must contain a default (parameterless) constructor"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor StaticConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0020"
            , title: "Static \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public static methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InstancedConvertMethodAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0021"
            , title: "Instanced \"Convert\" method ambiguity"
            , messageFormat: "The type \"{0}\" contains multiple public instanced methods named \"Convert\" thus it cannot be used as a converter"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0030"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor MissingConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0031"
            , title: "Missing \"Convert\" method"
            , messageFormat: "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that accepts a single parameter of any non-void type and returns a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0040"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethodReturnType = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0041"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of type \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidStaticConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0042"
            , title: "Invalid static \"Convert\" method"
            , messageFormat: "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidInstancedConvertMethod = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0043"
            , title: "Invalid instanced \"Convert\" method"
            , messageFormat: "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void type and must return a value of any non-void type"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpression = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0050"
            , title: "Not a typeof expression"
            , messageFormat: "The first argument must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor NotTypeOfExpressionAt = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0051"
            , title: "Not a typeof expression"
            , messageFormat: "The argument at position {0} must be a 'typeof' expression"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor AbstractTypeNotSupported = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0060"
            , title: "Abstract type is not supported"
            , messageFormat: "The type \"{0}\" must not be abstract"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor OpenGenericTypeNotSupported = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0061"
            , title: "Open generic type is not supported"
            , messageFormat: "The type \"{0}\" must not be open generic"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor ConverterAmbiguity = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0070"
            , title: "Converter ambiguity"
            , messageFormat: "The type \"{0}\" at position {3} will be ignored because a \"Convert\" method that returns a value of \"{2}\" has already been defined in \"{1}\""
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DuplicateDataPropertyConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0090"
            , title: "Conflicting data property converters"
            , messageFormat: "Multiple different converters are specified by \"ConverterForDataProperty\" for property \"{0}\" of data type \"{1}\" within {2}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor RedundantDataPropertyConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0091"
            , title: "Redundant data property converter"
            , messageFormat: "The converter \"{0}\" is specified more than once by \"ConverterForDataProperty\" for property \"{1}\" of data type \"{2}\" within {3}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DuplicateTableConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0092"
            , title: "Conflicting table converters"
            , messageFormat: "Multiple different converters are specified by \"ConverterForTable\" for source type \"{0}\" within {1}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor RedundantTableConverter = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0093"
            , title: "Redundant table converter"
            , messageFormat: "The converter \"{0}\" is specified more than once by \"ConverterForTable\" for source type \"{1}\" within {2}"
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidHorizontalCollectionSelection = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0100"
            , title: "Invalid horizontal collection selection"
            , messageFormat: "Horizontal selection for property \"{0}\" of type \"{1}\" is invalid: {2}."
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor InvalidGeneratedKeyEqualityCustomization = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0101"
            , title: "Invalid generated key equality customization"
            , messageFormat: "Generated key equality customization for \"{0}\" is invalid: {1}."
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
        );

        public static readonly DiagnosticDescriptor DataMemberTypeIsGenerated = new DiagnosticDescriptor(
              id: "SG_AUTHOR_DATABASE_0110"
            , title: "Database authoring data member type is generated by another source generator"
            , messageFormat: "\"{0}\" is generated by {3}, so the generator for {1} on \"{2}\" cannot see it. "
                + "Declare \"{0}\" in hand-written source or move it to a referenced assembly."
            , category: "DatabaseGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Database authoring data member type is generated by another source generator."
        );



        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MissingDefaultConstructor
                , StaticConvertMethodAmbiguity
                , InstancedConvertMethodAmbiguity
                , MissingConvertMethod
                , MissingConvertMethodReturnType
                , InvalidStaticConvertMethodReturnType
                , InvalidInstancedConvertMethodReturnType
                , InvalidStaticConvertMethod
                , InvalidInstancedConvertMethod
                , NotTypeOfExpression
                , NotTypeOfExpressionAt
                , AbstractTypeNotSupported
                , OpenGenericTypeNotSupported
                , ConverterAmbiguity
                , DuplicateDataPropertyConverter
                , RedundantDataPropertyConverter
                , DuplicateTableConverter
                , RedundantTableConverter
                , InvalidHorizontalCollectionSelection
                , InvalidGeneratedKeyEqualityCustomization
                , DataMemberTypeIsGenerated
            );

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeAuthoringType, SymbolKind.NamedType);
        }


        private static void AnalyzeAuthoringType(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol authoringSymbol)
            {
                return;
            }

            AnalyzeHorizontalPlacement(context, authoringSymbol, token);

            var authorAttrib = authoringSymbol.GetAttribute(AUTHOR_DATABASE_ATTRIBUTE, token);

            if (authorAttrib == null)
            {
                return;
            }

            // AuthorDatabaseAttribute is (Type databaseType, params Type[] converters), so real usages have
            // two constructor arguments. Only the first (the database type) is required here.
            if (authorAttrib.ConstructorArguments.Length < 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
            )
            {
                return;
            }

            AnalyzeGeneratedKeyEqualityCustomizations(context, authoringSymbol, databaseSymbol, token);

            var dbAttrib = databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE, token);

            if (dbAttrib != null)
            {
                var dbConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in dbAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, dbAttrib, dbConverterMap, 0);
                        break;
                    }
                }

                ReportGeneratedDataMemberTypes(context, authoringSymbol, databaseSymbol, token);
            }

            token.ThrowIfCancellationRequested();

            AnalyzeConverterAttributes(context, authoringSymbol, databaseSymbol, token);

            token.ThrowIfCancellationRequested();

            foreach (var member in databaseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE, token);

                if (tableAttrib == null)
                {
                    continue;
                }

                var tableConverterMap = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Kind == TypedConstantKind.Array)
                    {
                        ValidateConverterMapArguments(context, arg.Values, tableAttrib, tableConverterMap, 2);
                        break;
                    }
                }

                if (property.Type is INamedTypeSymbol tableType && tableType.BaseType != null)
                {
                    ValidateHorizontalSelections(context, property, tableType);
                    ValidateDataMembers(context, tableType);
                }
            }
        }

        private static void AnalyzeHorizontalPlacement(
              SymbolAnalysisContext context
            , INamedTypeSymbol containingType
            , CancellationToken token
        )
        {
            var isDatabase = containingType.HasAttribute(DATABASE_ATTRIBUTE, token);

            foreach (var member in containingType.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                var attributes = member.GetAttributes(HORIZONTAL_ATTRIBUTE, token);

                foreach (var attribute in attributes)
                {
                    token.ThrowIfCancellationRequested();

                    if (isDatabase && member is IPropertySymbol && member.HasAttribute(TABLE_ATTRIBUTE, token))
                    {
                        continue;
                    }

                    if (attribute.ConstructorArguments.Length < 2
                        || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol targetType
                        || attribute.ConstructorArguments[1].Value is not string propertyName
                    )
                    {
                        continue;
                    }

                    var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(token);

                    if (syntax != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              InvalidHorizontalCollectionSelection
                            , syntax.GetLocation()
                            , propertyName
                            , targetType.Name
                            , "the attribute is not on a Table property"
                        ));
                    }
                }
            }
        }

        private static void AnalyzeGeneratedKeyEqualityCustomizations(
              SymbolAnalysisContext context
            , INamedTypeSymbol authoringType
            , INamedTypeSymbol databaseType
            , CancellationToken token
        )
        {
            var generatedKeyNames = GetGeneratedKeyCustomizationNames(databaseType, token);

            foreach (var sheetType in authoringType.GetTypeMembers())
            {
                token.ThrowIfCancellationRequested();

                foreach (var keyType in sheetType.GetTypeMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (generatedKeyNames.Contains(keyType.Name) == false
                        || TryGetEqualityCustomizationError(keyType, token, out var reason) == false
                    )
                    {
                        continue;
                    }

                    var syntax = keyType.DeclaringSyntaxReferences.Length > 0
                        ? keyType.DeclaringSyntaxReferences[0].GetSyntax(token)
                        : null;
                    var location = syntax is TypeDeclarationSyntax declaration
                        ? declaration.Identifier.GetLocation()
                        : keyType.Locations.Length > 0 ? keyType.Locations[0] : null;

                    if (location != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              InvalidGeneratedKeyEqualityCustomization
                            , location
                            , keyType.Name
                            , reason
                        ));
                    }
                }
            }
        }

        private static HashSet<string> GetGeneratedKeyCustomizationNames(
              INamedTypeSymbol databaseType
            , CancellationToken token
        )
        {
            var result = new HashSet<string>(System.StringComparer.Ordinal);
            var keyTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var keyQueue = new Queue<INamedTypeSymbol>();

            foreach (var member in databaseType.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property
                    || member.HasAttribute(TABLE_ATTRIBUTE, token) == false
                    || property.Type is not INamedTypeSymbol tableType
                )
                {
                    continue;
                }

                if (TryGetCanonicalTableTypes(tableType, token, out var tableIdType, out var tableDataType))
                {
                    AddGeneratedKeyName(FindCorrectIdType(tableDataType, tableIdType, token));
                }

                foreach (var dataType in GetReachableDataTypes(tableType, token))
                {
                    token.ThrowIfCancellationRequested();

                    foreach (var dataMember in dataType.GetMembers())
                    {
                        if (dataMember is IPropertySymbol dataProperty)
                        {
                            AddDictionaryKeyNames(dataProperty.Type);
                        }
                        else if (dataMember is IFieldSymbol dataField)
                        {
                            AddDictionaryKeyNames(dataField.Type);
                        }
                    }
                }
            }

            while (keyQueue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var keyType = keyQueue.Dequeue();

                foreach (var member in keyType.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IPropertySymbol property)
                    {
                        AddGeneratedKeyName(property.Type);
                    }
                    else if (member is IFieldSymbol field)
                    {
                        AddGeneratedKeyName(field.Type);
                    }
                }
            }

            return result;

            void AddGeneratedKeyName(ITypeSymbol type)
            {
                if (type is INamedTypeSymbol namedType && namedType.ImplementsInterface(IDATA, false, token))
                {
                    result.Add($"__{namedType.Name}");

                    if (keyTypes.Add(namedType))
                    {
                        keyQueue.Enqueue(namedType);
                    }
                }
            }

            void AddDictionaryKeyNames(ITypeSymbol type)
            {
                if (type is IArrayTypeSymbol arrayType)
                {
                    AddDictionaryKeyNames(arrayType.ElementType);
                    return;
                }

                if (type is not INamedTypeSymbol namedType)
                {
                    return;
                }

                foreach (var iface in namedType.AllInterfaces)
                {
                    if (iface.OriginalDefinition.ToDisplayString()
                            == "System.Collections.Generic.IDictionary<TKey, TValue>"
                        && iface.TypeArguments.Length == 2
                    )
                    {
                        AddGeneratedKeyName(iface.TypeArguments[0]);
                        break;
                    }
                }

                foreach (var argument in namedType.TypeArguments)
                {
                    AddDictionaryKeyNames(argument);
                }
            }
        }

        private static ITypeSymbol FindCorrectIdType(
              ITypeSymbol dataType
            , ITypeSymbol candidateIdType
            , CancellationToken token
        )
        {
            ISymbol foundMember = null;

            foreach (var member in dataType.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IPropertySymbol && member.Name == "Id"
                    || member is IFieldSymbol && member.Name == "_id"
                )
                {
                    foundMember = member;
                    break;
                }
            }

            if (foundMember is { }
                && foundMember.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE, token) is { } manualAuthoring
                && manualAuthoring.ConstructorArguments.Length > 0
                && manualAuthoring.ConstructorArguments[0].Value is ITypeSymbol manualType
            )
            {
                return manualType;
            }

            if (foundMember is IFieldSymbol field)
            {
                return field.Type;
            }

            if (foundMember is not IPropertySymbol property)
            {
                return candidateIdType;
            }

            foreach (var attribute in property.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length > 1
                    && attribute.AttributeClass.HasFullName(GENERATED_PROPERTY_FROM_FIELD, token)
                    && attribute.ConstructorArguments[1].Value is ITypeSymbol generatedFieldType
                )
                {
                    return generatedFieldType;
                }

                if (attribute.ConstructorArguments.Length > 0
                    && attribute.AttributeClass.HasFullName(DATA_PROPERTY_ATTRIBUTE, token)
                    && attribute.ConstructorArguments[0].Value is ITypeSymbol propertyType
                )
                {
                    return propertyType;
                }
            }

            return candidateIdType;
        }

        private static bool TryGetEqualityCustomizationError(
              INamedTypeSymbol type
            , CancellationToken token
            , out string reason
        )
        {
            var partial = false;

            foreach (var syntaxReference in type.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (syntaxReference.GetSyntax(token) is TypeDeclarationSyntax declaration
                    && declaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                )
                {
                    partial = true;
                    break;
                }
            }

            var hasReservedMember = false;
            var hasTypedEquals = false;
            var hasObjectEquals = false;
            var hasGetHashCode = false;

            foreach (var member in type.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method || method.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (method.Name == "Equals")
                {
                    hasReservedMember = true;
                    hasTypedEquals |= IsCompatibleTypedEquals(method, type);
                    hasObjectEquals |= IsCompatibleObjectEquals(method);
                }
                else if (method.Name == "GetHashCode")
                {
                    hasReservedMember = true;
                    hasGetHashCode |= IsCompatibleGetHashCode(method);
                }
            }

            if (hasReservedMember == false)
            {
                reason = default;
                return false;
            }

            if (partial == false)
            {
                reason = "the customization type must be partial";
                return true;
            }

            if (hasTypedEquals && hasObjectEquals && hasGetHashCode)
            {
                reason = default;
                return false;
            }

            reason = $"provide public bool Equals({type.Name}), public override bool Equals(object), and public " +
                "override int GetHashCode() together";
            return true;
        }

        private static bool IsCompatibleTypedEquals(IMethodSymbol method, INamedTypeSymbol containingType)
            => method.DeclaredAccessibility == Accessibility.Public
            && method.IsStatic == false
            && method.ReturnType.SpecialType == SpecialType.System_Boolean
            && method.Parameters.Length == 1
            && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, containingType);

        private static bool IsCompatibleObjectEquals(IMethodSymbol method)
            => method.DeclaredAccessibility == Accessibility.Public
            && method.IsOverride
            && method.IsStatic == false
            && method.ReturnType.SpecialType == SpecialType.System_Boolean
            && method.Parameters.Length == 1
            && method.Parameters[0].Type.SpecialType == SpecialType.System_Object;

        private static bool IsCompatibleGetHashCode(IMethodSymbol method)
            => method.DeclaredAccessibility == Accessibility.Public
            && method.IsOverride
            && method.IsStatic == false
            && method.ReturnType.SpecialType == SpecialType.System_Int32
            && method.Parameters.Length == 0;

        private static void ValidateHorizontalSelections(
              SymbolAnalysisContext context
            , IPropertySymbol tableProperty
            , INamedTypeSymbol tableType
        )
        {
            var token = context.CancellationToken;
            var attributes = tableProperty.GetAttributes(HORIZONTAL_ATTRIBUTE, token).ToImmutableArray();

            if (attributes.Length < 1)
            {
                return;
            }

            var reachableTypes = GetReachableDataTypes(tableType, token);

            foreach (var attribute in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length < 2
                    || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol targetType
                    || attribute.ConstructorArguments[1].Value is not string propertyName
                )
                {
                    continue;
                }

                string reason = null;

                if (targetType.IsAbstract || targetType.ImplementsInterface(IDATA, false, token) == false)
                {
                    reason = "the target type must be a non-abstract IData type";
                }
                else if (reachableTypes.Contains(targetType) == false)
                {
                    reason = "the target type is not reachable from this table";
                }
                else if (TryFindProperty(targetType, propertyName, out var targetProperty) == false)
                {
                    reason = "the target property does not exist";
                }
                else if (IsCollection(GetEffectiveAuthoringType(targetProperty, token)) == false)
                {
                    reason = "the target property is not a collection";
                }

                if (reason == null)
                {
                    continue;
                }

                var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(token);

                if (syntax != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          InvalidHorizontalCollectionSelection
                        , syntax.GetLocation()
                        , propertyName
                        , targetType.Name
                        , reason
                    ));
                }
            }
        }

        private static HashSet<INamedTypeSymbol> GetReachableDataTypes(
              INamedTypeSymbol tableType
            , CancellationToken token
        )
        {
            var result = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<INamedTypeSymbol>();

            if (TryGetCanonicalTableTypes(tableType, token, out var tableIdType, out var tableDataType))
            {
                Enqueue(tableIdType);
                Enqueue(tableDataType);
            }

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var type = queue.Dequeue();

                foreach (var member in type.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IPropertySymbol property)
                    {
                        Enqueue(property.Type);
                    }
                    else if (member is IFieldSymbol field)
                    {
                        Enqueue(field.Type);
                    }
                }

                if (type.BaseType is { } parent && parent.ImplementsInterface(IDATA, false, token))
                {
                    Enqueue(parent);
                }
            }

            return result;

            void Enqueue(ITypeSymbol type)
            {
                if (type is IArrayTypeSymbol array)
                {
                    Enqueue(array.ElementType);
                    return;
                }

                if (type is not INamedTypeSymbol named)
                {
                    return;
                }

                foreach (var argument in named.TypeArguments)
                {
                    Enqueue(argument);
                }

                if (named.ImplementsInterface(IDATA, false, token) && result.Add(named))
                {
                    queue.Enqueue(named);
                }
            }
        }

        private static bool TryGetCanonicalTableTypes(
              INamedTypeSymbol tableType
            , CancellationToken token
            , out ITypeSymbol idType
            , out ITypeSymbol dataType
        )
        {
            var current = tableType;

            while (current != null)
            {
                token.ThrowIfCancellationRequested();

                if ((current.Arity == 2 || current.Arity == 3)
                    && current.OriginalDefinition.HasFullName(DATA_TABLE_ASSET_BASE, token)
                )
                {
                    idType = current.TypeArguments[0];
                    dataType = current.TypeArguments[1];
                    return true;
                }

                current = current.BaseType;
            }

            idType = default;
            dataType = default;
            return false;
        }

        private static bool TryFindProperty(
              INamedTypeSymbol type
            , string propertyName
            , out IPropertySymbol property
        )
        {
            var current = type;

            while (current != null)
            {
                foreach (var member in current.GetMembers(propertyName))
                {
                    if (member is IPropertySymbol found)
                    {
                        property = found;
                        return true;
                    }
                }

                current = current.BaseType;
            }

            property = default;
            return false;
        }

        private static bool IsCollection(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol)
            {
                return true;
            }

            if (type is not INamedTypeSymbol namedType || namedType.SpecialType == SpecialType.System_String)
            {
                return false;
            }

            var originalName = namedType.OriginalDefinition.ToDisplayString();

            if (originalName == "System.Memory<T>"
                || originalName == "System.ReadOnlyMemory<T>"
                || originalName == "System.Span<T>"
                || originalName == "System.ReadOnlySpan<T>"
            )
            {
                return true;
            }

            foreach (var iface in namedType.AllInterfaces)
            {
                var name = iface.OriginalDefinition.ToDisplayString();

                if (name == "System.Collections.Generic.ICollection<T>"
                    || name == "System.Collections.Generic.IReadOnlyCollection<T>"
                    || name == "System.Collections.Generic.IDictionary<TKey, TValue>"
                    || name == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>"
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static ITypeSymbol GetEffectiveAuthoringType(IPropertySymbol property, CancellationToken token)
        {
            var manualAuthoring = property.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE, token);

            if (manualAuthoring is { ConstructorArguments.Length: > 0 }
                && manualAuthoring.ConstructorArguments[0].Value is ITypeSymbol manualType
            )
            {
                return manualType;
            }

            var converterAttribute = property.GetAttribute(DATA_AUTHORING_CONVERTER_ATTRIBUTE, token);

            if (converterAttribute is not { ConstructorArguments.Length: 1 }
                || converterAttribute.ConstructorArguments[0].Value is not INamedTypeSymbol converterType
            )
            {
                return property.Type;
            }

            IMethodSymbol selected = null;

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsGenericMethod
                    || method.Parameters.Length != 1
                    || SymbolEqualityComparer.Default.Equals(method.ReturnType, property.Type) == false
                )
                {
                    continue;
                }

                if (selected != null)
                {
                    return property.Type;
                }

                selected = method;
            }

            return selected?.Parameters[0].Type ?? property.Type;
        }

        private static void ValidateDataMembers(SymbolAnalysisContext context, INamedTypeSymbol tableType)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            var visited = new HashSet<string>(System.StringComparer.Ordinal);
            var queue = new Queue<INamedTypeSymbol>();
            queue.Enqueue(tableType);

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var type = queue.Dequeue();
                var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                if (visited.Contains(fullName))
                {
                    continue;
                }

                visited.Add(fullName);

                foreach (var member in type.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    ITypeSymbol memberType;

                    if (member is IPropertySymbol prop)
                    {
                        memberType = prop.Type;
                    }
                    else if (member is IFieldSymbol field)
                    {
                        memberType = field.Type;
                    }
                    else
                    {
                        continue;
                    }

                    var converterAttrib = member.GetAttribute(DATA_AUTHORING_CONVERTER_ATTRIBUTE, token);

                    if (converterAttrib != null)
                    {
                        ValidateMemberConverter(context, converterAttrib, memberType);
                    }

                    if (memberType is INamedTypeSymbol namedMemberType
                        && namedMemberType.ImplementsInterface(IDATA, false, token)
                    )
                    {
                        queue.Enqueue(namedMemberType);
                    }
                }
            }
        }

        private static void ReportGeneratedDataMemberTypes(
              SymbolAnalysisContext context
            , INamedTypeSymbol authoringSymbol
            , INamedTypeSymbol databaseSymbol
            , CancellationToken token
        )
        {
            var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var reported = new HashSet<Location>();
            var queue = new Queue<INamedTypeSymbol>();

            foreach (var member in databaseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol { Type: INamedTypeSymbol tableType }
                    || member.HasAttribute(TABLE_ATTRIBUTE, token) == false
                    || TryGetCanonicalTableTypes(tableType, token, out var tableIdType, out var tableDataType) == false
                )
                {
                    continue;
                }

                EnqueueDataTypes(tableDataType, visited, queue, token);
                EnqueueDataTypes(FindCorrectIdType(tableDataType, tableIdType, token), visited, queue, token);
            }

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var dataType = queue.Dequeue();

                foreach (var member in dataType.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member.DeclaringSyntaxReferences.Length < 1
                        || member.HasOnlyGeneratedDeclarations(token)
                        || TryGetAuthoredMemberType(
                              member
                            , token
                            , out var memberType
                            , out var memberTypeLocation
                        ) == false
                    )
                    {
                        continue;
                    }

                    Report(memberType, memberTypeLocation);
                    EnqueueDataTypes(memberType, visited, queue, token);

                    if (member.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE, token) is { } manualAuthoring
                        && manualAuthoring.ConstructorArguments.Length > 0
                        && manualAuthoring.ConstructorArguments[0].Value is ITypeSymbol manualType
                    )
                    {
                        var manualTypeLocation = GetTypeOfArgumentLocation(
                              manualAuthoring
                            , DATA_MANUAL_AUTHORING_TYPE_PARAMETER
                            , token
                        );

                        Report(manualType, manualTypeLocation);
                        EnqueueDataTypes(manualType, visited, queue, token);
                    }
                }

                if (dataType.BaseType is { TypeKind: TypeKind.Class } baseType
                    && baseType.HasAttribute(DATA_ATTRIBUTE, token)
                    && visited.Add(baseType)
                )
                {
                    queue.Enqueue(baseType);
                }
            }

            void Report(ITypeSymbol type, Location location)
            {
                if (location == null
                    || type.TryFindEncosyTowerGeneratedType(token, out var generatedType, out var tool) == false
                    || reported.Add(location) == false
                )
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      DataMemberTypeIsGenerated
                    , location
                    , generatedType.ToGeneratedTypeFullName()
                    , AUTHOR_DATABASE_DISPLAY
                    , authoringSymbol.Name
                    , tool
                ));
            }
        }

        private static bool TryGetAuthoredMemberType(
              ISymbol member
            , CancellationToken token
            , out ITypeSymbol memberType
            , out Location location
        )
        {
            if (member is IPropertySymbol property
                && property.GetAttribute(DATA_PROPERTY_ATTRIBUTE, token) is { } dataProperty
            )
            {
                if (dataProperty.ConstructorArguments.Length > 0
                    && dataProperty.ConstructorArguments[0].Value is ITypeSymbol fieldType
                )
                {
                    memberType = fieldType;
                    location = GetTypeOfArgumentLocation(dataProperty, DATA_PROPERTY_FIELD_TYPE_PARAMETER, token);
                    return true;
                }

                memberType = property.Type;
                location = GetDeclaredTypeLocation(property, token);
                return true;
            }

            if (member is IFieldSymbol field && field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE, token))
            {
                memberType = field.Type;
                location = GetDeclaredTypeLocation(field, token);
                return true;
            }

            memberType = default;
            location = default;
            return false;
        }

        private static Location GetDeclaredTypeLocation(ISymbol member, CancellationToken token)
        {
            foreach (var reference in member.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                switch (reference.GetSyntax(token))
                {
                    case PropertyDeclarationSyntax property:
                        return property.Type.GetLocation();

                    case VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration }:
                        return declaration.Type.GetLocation();
                }
            }

            return null;
        }

        private static Location GetTypeOfArgumentLocation(
              AttributeData attribute
            , string parameterName
            , CancellationToken token
        )
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax)
            {
                return null;
            }

            if (syntax.ArgumentList is not { } argumentList)
            {
                return syntax.GetLocation();
            }

            var arguments = argumentList.Arguments;

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];

                if (argument.NameEquals != null)
                {
                    continue;
                }

                var isTarget = argument.NameColon == null
                    ? i == 0
                    : argument.NameColon.Name.Identifier.ValueText == parameterName;

                if (isTarget && argument.Expression is TypeOfExpressionSyntax typeOfExpression)
                {
                    return typeOfExpression.Type.GetLocation();
                }
            }

            return syntax.GetLocation();
        }

        private static void EnqueueDataTypes(
              ITypeSymbol type
            , HashSet<INamedTypeSymbol> visited
            , Queue<INamedTypeSymbol> queue
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (type is IArrayTypeSymbol arrayType)
            {
                EnqueueDataTypes(arrayType.ElementType, visited, queue, token);
                return;
            }

            if (type is not INamedTypeSymbol namedType)
            {
                return;
            }

            foreach (var argument in namedType.TypeArguments)
            {
                EnqueueDataTypes(argument, visited, queue, token);
            }

            if (namedType.HasAttribute(DATA_ATTRIBUTE, token) && visited.Add(namedType))
            {
                queue.Enqueue(namedType);
            }
        }

        private static void ValidateMemberConverter(
              SymbolAnalysisContext context
            , AttributeData converterAttrib
            , ITypeSymbol targetType
        )
        {
            if (converterAttrib.ConstructorArguments.Length != 1)
                return;

            var syntax = converterAttrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);

            if (converterAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                if (syntax != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(NotTypeOfExpression, syntax.GetLocation()));
                }
                return;
            }

            ValidateConverterType(context, converterType, syntax, targetType);
        }

        private static void ValidateConverterMapArguments(
              SymbolAnalysisContext context
            , ImmutableArray<TypedConstant> values
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> converterMap
            , int offset
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (values.IsDefaultOrEmpty)
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);

            for (var i = 0; i < values.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (values[i].Value is not INamedTypeSymbol converterType)
                {
                    if (syntax != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              NotTypeOfExpressionAt
                            , syntax.GetLocation()
                            , offset + i
                        ));
                    }

                    continue;
                }

                if (ValidateConverterType(context, converterType, syntax, returnType: null) == false)
                {
                    continue;
                }

                if (TryFindConvertReturnType(converterType, out var returnTypeName, token) == false)
                {
                    continue;
                }

                if (converterMap.TryGetValue(returnTypeName, out var existing))
                {
                    if (syntax != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              ConverterAmbiguity
                            , syntax.GetLocation()
                            , converterType.Name
                            , existing.Name
                            , returnTypeName
                            , offset + i
                        ));
                    }
                }
                else
                {
                    converterMap[returnTypeName] = converterType;
                }
            }
        }

        private static void AnalyzeConverterAttributes(
              SymbolAnalysisContext context
            , INamedTypeSymbol authoringSymbol
            , INamedTypeSymbol databaseSymbol
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var tableTypeByPropName = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);

            foreach (var member in databaseSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is IPropertySymbol property
                    && property.GetAttribute(TABLE_ATTRIBUTE, token) != null
                    && property.Type is INamedTypeSymbol tableType
                )
                {
                    tableTypeByPropName[property.Name] = tableType;
                }
            }

            var dataPropGroups = new Dictionary<string, List<ConverterEntry>>(System.StringComparer.Ordinal);
            var tableGroups = new Dictionary<string, List<ConverterEntry>>(System.StringComparer.Ordinal);

            foreach (var attrib in authoringSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attribClass = attrib.AttributeClass;

                if (attribClass == null)
                {
                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE, token))
                {
                    CollectDataPropertyConverter(context, attrib, tableTypeByPropName, dataPropGroups, token);
                }
                else if (attribClass.HasFullName(CONVERTER_FOR_TABLE_ATTRIBUTE, token))
                {
                    CollectTableConverter(context, attrib, tableTypeByPropName, tableGroups, token);
                }
            }

            ReportDuplicateGroups(context, dataPropGroups, DuplicateDataPropertyConverter, RedundantDataPropertyConverter, isDataProperty: true);
            ReportDuplicateGroups(context, tableGroups, DuplicateTableConverter, RedundantTableConverter, isDataProperty: false);
        }

        private static void CollectDataPropertyConverter(
              SymbolAnalysisContext context
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
            , Dictionary<string, List<ConverterEntry>> groups
            , CancellationToken token
        )
        {
            var args = attrib.ConstructorArguments;

            if (args.Length < 3
                || args[0].Value is not INamedTypeSymbol dataType
                || args[1].Value is not string propertyName
                || string.IsNullOrWhiteSpace(propertyName)
                || args[2].Value is not INamedTypeSymbol converterType
            )
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);

            if (syntax == null)
            {
                return;
            }

            ValidateConverterType(context, converterType, syntax, returnType: null);

            var location = syntax.GetLocation();
            var converterFullName = converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var dataTypeFullName = dataType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            ReadTableScope(args, 3, out var tableName, out var tableType);

            foreach (var scopeKey in ResolveScopeKeys(tableName, tableType, tableTypeByPropName))
            {
                token.ThrowIfCancellationRequested();

                var groupKey = $"{dataTypeFullName}|{propertyName}|{scopeKey}";

                AddEntry(groups, groupKey, new ConverterEntry {
                    location = location,
                    converterTypeFullName = converterFullName,
                    descPrimary = propertyName,
                    descSecondary = dataTypeFullName,
                    descScope = ScopeLabel(scopeKey),
                });
            }
        }

        private static void CollectTableConverter(
              SymbolAnalysisContext context
            , AttributeData attrib
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
            , Dictionary<string, List<ConverterEntry>> groups
            , CancellationToken token
        )
        {
            var args = attrib.ConstructorArguments;

            if (args.Length != 2 || args[1].Value is not INamedTypeSymbol converterType)
            {
                return;
            }

            var syntax = attrib.ApplicationSyntaxReference?.GetSyntax(token);

            if (syntax == null)
            {
                return;
            }

            ValidateConverterType(context, converterType, syntax, returnType: null);

            if (TryFindConvertSourceType(converterType, out var sourceTypeFullName, token) == false)
            {
                return;
            }

            ReadTableScope(args, 0, out var tableName, out var tableType);

            if (tableName == null && tableType == null)
            {
                return;
            }

            var location = syntax.GetLocation();
            var converterFullName = converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            foreach (var scopeKey in ResolveScopeKeys(tableName, tableType, tableTypeByPropName))
            {
                token.ThrowIfCancellationRequested();

                var groupKey = $"{scopeKey}|{sourceTypeFullName}";

                AddEntry(groups, groupKey, new ConverterEntry {
                    location = location,
                    converterTypeFullName = converterFullName,
                    descPrimary = sourceTypeFullName,
                    descSecondary = string.Empty,
                    descScope = ScopeLabel(scopeKey),
                });
            }
        }

        private static void ReportDuplicateGroups(
              SymbolAnalysisContext context
            , Dictionary<string, List<ConverterEntry>> groups
            , DiagnosticDescriptor conflictDescriptor
            , DiagnosticDescriptor redundantDescriptor
            , bool isDataProperty
        )
        {
            foreach (var pair in groups)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var entries = pair.Value;

                if (entries.Count < 2)
                {
                    continue;
                }

                var distinctConverters = new HashSet<string>(System.StringComparer.Ordinal);

                foreach (var entry in entries)
                {
                    distinctConverters.Add(entry.converterTypeFullName);
                }

                var conflict = distinctConverters.Count > 1;

                foreach (var entry in entries)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (conflict)
                    {
                        context.ReportDiagnostic(isDataProperty
                            ? Diagnostic.Create(conflictDescriptor, entry.location, entry.descPrimary, entry.descSecondary, entry.descScope)
                            : Diagnostic.Create(conflictDescriptor, entry.location, entry.descPrimary, entry.descScope)
                        );
                    }
                    else
                    {
                        context.ReportDiagnostic(isDataProperty
                            ? Diagnostic.Create(redundantDescriptor, entry.location, entry.converterTypeFullName, entry.descPrimary, entry.descSecondary, entry.descScope)
                            : Diagnostic.Create(redundantDescriptor, entry.location, entry.converterTypeFullName, entry.descPrimary, entry.descScope)
                        );
                    }
                }
            }
        }

        private static void ReadTableScope(
              ImmutableArray<TypedConstant> args
            , int index
            , out string tableName
            , out INamedTypeSymbol tableType
        )
        {
            tableName = null;
            tableType = null;

            if (args.Length <= index)
            {
                return;
            }

            if (args[index].Value is string nameValue)
            {
                tableName = nameValue;
            }
            else if (args[index].Value is INamedTypeSymbol typeValue)
            {
                tableType = typeValue;
            }
        }

        private static List<string> ResolveScopeKeys(
              string tableName
            , INamedTypeSymbol tableType
            , Dictionary<string, INamedTypeSymbol> tableTypeByPropName
        )
        {
            var keys = new List<string>(1);

            if (tableName != null)
            {
                keys.Add(tableName);
                return keys;
            }

            if (tableType != null)
            {
                foreach (var pair in tableTypeByPropName)
                {
                    if (SymbolEqualityComparer.Default.Equals(pair.Value, tableType))
                    {
                        keys.Add(pair.Key);
                    }
                }

                if (keys.Count == 0)
                {
                    keys.Add(tableType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                return keys;
            }

            keys.Add("*");
            return keys;
        }

        private static string ScopeLabel(string scopeKey)
            => string.Equals(scopeKey, "*", System.StringComparison.Ordinal) ? "all tables" : $"table \"{scopeKey}\"";

        private static void AddEntry(Dictionary<string, List<ConverterEntry>> groups, string key, ConverterEntry entry)
        {
            if (groups.TryGetValue(key, out var list) == false)
            {
                groups[key] = list = new List<ConverterEntry>(1);
            }

            list.Add(entry);
        }

        private static bool TryFindConvertSourceType(
              INamedTypeSymbol converterType
            , out string sourceTypeName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method
                    && method.DeclaredAccessibility == Accessibility.Public
                    && method.IsGenericMethod == false
                    && method.Parameters.Length == 1
                    && method.ReturnsVoid == false
                )
                {
                    sourceTypeName = method.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    return true;
                }
            }

            sourceTypeName = null;
            return false;
        }

        private struct ConverterEntry
        {
            public Location location;
            public string converterTypeFullName;
            public string descPrimary;
            public string descSecondary;
            public string descScope;
        }

        private static bool TryFindConvertReturnType(
              INamedTypeSymbol converterType
            , out string returnTypeName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method
                    && method.DeclaredAccessibility == Accessibility.Public
                    && method.IsGenericMethod == false
                    && method.Parameters.Length == 1
                    && method.ReturnsVoid == false
                )
                {
                    returnTypeName = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    return true;
                }
            }

            returnTypeName = null;
            return false;
        }

        private static bool ValidateConverterType(
              SymbolAnalysisContext context
            , INamedTypeSymbol converterType
            , SyntaxNode reportSyntax
            , ITypeSymbol returnType
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (reportSyntax == null)
            {
                return true;
            }

            var location = reportSyntax.GetLocation();

            if (converterType.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(AbstractTypeNotSupported, location, converterType.Name));
                return false;
            }

            if (converterType.IsUnboundGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(OpenGenericTypeNotSupported, location, converterType.Name));
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var hasPublicParameterlessCtor = false;

                foreach (var ctor in converterType.GetMembers(".ctor"))
                {
                    token.ThrowIfCancellationRequested();

                    if (ctor is IMethodSymbol m
                        && m.DeclaredAccessibility == Accessibility.Public
                        && m.Parameters.Length == 0
                    )
                    {
                        hasPublicParameterlessCtor = true;
                        break;
                    }
                }

                if (hasPublicParameterlessCtor == false)
                {
                    context.ReportDiagnostic(Diagnostic.Create(MissingDefaultConstructor, location, converterType.Name));
                    return false;
                }
            }

            token.ThrowIfCancellationRequested();

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public
                )
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    if (multipleStatic == false)
                    {
                        if (staticMethod != null)
                        {
                            staticMethod = null;
                            multipleStatic = true;
                        }
                        else
                        {
                            staticMethod = method;
                        }
                    }
                }
                else
                {
                    if (multipleInstance == false)
                    {
                        if (instanceMethod != null)
                        {
                            instanceMethod = null;
                            multipleInstance = true;
                        }
                        else
                        {
                            instanceMethod = method;
                        }
                    }
                }
            }

            if (multipleStatic || (multipleStatic == false && multipleInstance))
            {
                var desc = multipleStatic ? StaticConvertMethodAmbiguity : InstancedConvertMethodAmbiguity;
                context.ReportDiagnostic(Diagnostic.Create(desc, location, converterType.Name));
                return false;
            }

            var convertMethod = staticMethod ?? instanceMethod;

            if (convertMethod == null)
            {
                var desc = returnType != null ? MissingConvertMethod : MissingConvertMethodReturnType;
                context.ReportDiagnostic(Diagnostic.Create(
                    desc
                    , location
                    , converterType.Name
                    , returnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty
                ));

                return false;
            }

            if (convertMethod.Parameters.Length != 1
                || convertMethod.ReturnsVoid
                || (returnType != null && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false)
            )
            {
                DiagnosticDescriptor desc;

                if (convertMethod.IsStatic)
                {
                    desc = returnType != null ? InvalidStaticConvertMethodReturnType : InvalidStaticConvertMethod;
                }
                else
                {
                    desc = returnType != null ? InvalidInstancedConvertMethodReturnType : InvalidInstancedConvertMethod;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                      desc
                    , location
                    , converterType.Name
                    , returnType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty
                ));

                return false;
            }

            return true;
        }
    }
}
