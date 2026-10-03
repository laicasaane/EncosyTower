namespace EncosyTower.Persistence.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class PersistenceDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string NAMESPACE = "EncosyTower.Persistences";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string PERSISTENCE_ATTRIBUTE = $"global::{NAMESPACE}.PersistenceAttribute";
        public const string ACCESSOR_ATTRIBUTE = $"global::{NAMESPACE}.PersistAccessorAttribute";
        public const string PERSISTENCE_ATTRIBUTE_METADATA = $"{NAMESPACE}.PersistenceAttribute";
        public const string ACCESSOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.PersistAccessorAttribute";
        public const string IPERSIST = $"global::{NAMESPACE}.IPersist";
        public const string IPERSIST_ACCESSOR = $"global::{NAMESPACE}.IPersistAccessor";
        public const string PERSIST_STORE_BASE = $"global::{NAMESPACE}.PersistStoreBase<";
        public const string ENCRYPTION_BASE = "EncryptionBase";
        public const string STRING_VAULT = "StringVault";
        public const string ILOGGER = "ILogger";
        public const string TASK_ARRAY_POOL = "global::System.Buffers.ArrayPool<UnityTask>";
        public const string STORE_ARGS = "PersistStoreArgs";
        public const string COMPLETED_TASK = "UnityTask.CompletedTask";

        private const string PERSIST_ATTRIBUTE = $"global::{NAMESPACE}.PersistAttribute";
        private const string PERSIST_DISPLAY = "[Persist]";
        private const string ID_PROPERTY = "Id";
        private const string VERSION_PROPERTY = "Version";

        public const string WHEN_ALL_TASKS = "UnityTask.WhenAll(tasks)";
        public const string NOT_NULL = "[NotNull]";
        public const string STRING_ID = "StringIdᐸstringᐳ";
        public const string GENERATED_CODE = $"[GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.Persistence.Generators.PersistenceGenerator\"";
        public const string HIDE_IN_CALL_STACK = "[HideInCallstack, StackTraceHidden]";

        public static readonly DiagnosticDescriptor MustHaveOnlyOneConstructor = new(
              id: "SG_PERSISTENCE_0001"
            , title: "Type must have only 01 constructor whose parameters are either PersistStoreBase<T> or IPersistAccessor"
            , messageFormat: "Type \"{0}\" must have only 01 constructor whose parameters are either PersistStoreBase<T> or IPersistAccessor"
            , category: "Persistence"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type must have only 01 constructor whose parameters are either PersistStoreBase<T> or IPersistAccessor"
        );

        public static readonly DiagnosticDescriptor ConstructorContainsUnsupportedType = new(
              id: "SG_PERSISTENCE_0002"
            , title: "Constructor parameter must be either PersistStoreBase<T> or IPersistAccessor"
            , messageFormat: "Parameter \"{1}\" of constructor of type \"{0}\" must be either PersistStoreBase<T> or IPersistAccessor"
            , category: "Persistence"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Constructor parameter must be either PersistStoreBase<T> or IPersistAccessor"
        );

        public static readonly DiagnosticDescriptor MustNotBeAbstract = new(
              id: "SG_PERSISTENCE_0003"
            , title: "[PersistAccessor] type must not be abstract"
            , messageFormat: "Type \"{0}\" is marked [PersistAccessor] but is abstract and cannot be processed by the source generator"
            , category: "Persistence"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A type marked with [PersistAccessor] must not be abstract"
        );

        public static readonly DiagnosticDescriptor MustNotBeGenericType = new(
              id: "SG_PERSISTENCE_0004"
            , title: "[PersistAccessor] type must not be a generic type"
            , messageFormat: "Type \"{0}\" is marked [PersistAccessor] but is a generic type "
                + "and cannot be processed by the source generator"
            , category: "Persistence"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A type marked with [PersistAccessor] must not be a generic type"
        );

        public static readonly DiagnosticDescriptor ForwardedAttributeUsesGeneratedType = new(
              id: "SG_PERSISTENCE_0005"
            , title: "Persist forwarded attribute argument uses a generated type"
            , messageFormat: "\"{0}\" is generated by {3}, so the generator for {1} on \"{2}\" cannot see it. "
                + "Declare \"{0}\" in hand-written source or move it to a referenced assembly."
            , category: "Persistence"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Persist forwarded attribute argument uses a generated type."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustHaveOnlyOneConstructor
                , ConstructorContainsUnsupportedType
                , MustNotBeAbstract
                , MustNotBeGenericType
                , ForwardedAttributeUsesGeneratedType
            );

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

            if (context.Symbol is not INamedTypeSymbol symbol)
            {
                return;
            }

            if (symbol.HasAttribute(PERSIST_ATTRIBUTE, token))
            {
                AnalyzePersist(context, symbol);
            }

            if (symbol.GetAttribute(ACCESSOR_ATTRIBUTE, token) is null)
            {
                return;
            }

            if (symbol.TryGetSourceLocation(out var symbolLocation) == false)
            {
                return;
            }

            if (symbol.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustNotBeAbstract, symbolLocation, symbol.Name));
                return;
            }

            if (symbol.IsGenericType)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustNotBeGenericType, symbolLocation, symbol.Name));
                return;
            }

            token.ThrowIfCancellationRequested();

            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (constructors[i].Parameters.Length > max)
                {
                    max = constructors[i].Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustHaveOnlyOneConstructor, symbolLocation, symbol.Name));

                return;
            }

            token.ThrowIfCancellationRequested();

            var constructor = constructors[constructorIndex];

            foreach (var param in constructor.Parameters)
            {
                token.ThrowIfCancellationRequested();

                if (TryGetParam(param.Type, token) == false)
                {
                    if (param.TryGetSourceLocation(out var location) == false)
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(
                          ConstructorContainsUnsupportedType
                        , location
                        , symbol.Name
                        , param.Name
                    ));
                }
            }
        }

        private static void AnalyzePersist(SymbolAnalysisContext context, INamedTypeSymbol symbol)
        {
            var token = context.CancellationToken;
            var members = symbol.GetMembers();
            var count = members.Length;
            var hasIdProperty = false;
            var hasVersionProperty = false;
            IFieldSymbol idField = null;
            IFieldSymbol versionField = null;

            for (var i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                switch (members[i])
                {
                    case IPropertySymbol { DeclaredAccessibility: Accessibility.Public } property
                        when property.HasOnlyGeneratedDeclarations(token) == false:
                    {
                        hasIdProperty |= string.Equals(property.Name, ID_PROPERTY, StringComparison.Ordinal);
                        hasVersionProperty |= string.Equals(property.Name, VERSION_PROPERTY, StringComparison.Ordinal);
                        break;
                    }

                    case IFieldSymbol field:
                    {
                        if (idField == null && field.Name is "id" or "_id" or "m_id")
                        {
                            idField = field;
                        }
                        else if (versionField == null && field.Name is "version" or "_version" or "m_version")
                        {
                            versionField = field;
                        }

                        break;
                    }
                }
            }

            if (idField != null && hasIdProperty == false)
            {
                ReportForwardedGeneratedMemberReferences(context, idField, SyntaxKind.PropertyKeyword);
            }

            if (versionField != null && hasVersionProperty == false)
            {
                ReportForwardedGeneratedMemberReferences(context, versionField, SyntaxKind.PropertyKeyword);
            }
        }

        private static void ReportForwardedGeneratedMemberReferences(
              SymbolAnalysisContext context
            , ISymbol member
            , SyntaxKind targetKeyword
        )
        {
            var references = member.FindForwardedGeneratedMemberReferences(
                  targetKeyword: targetKeyword
                , includePartialImplementation: false
                , compilation: context.Compilation
                , token: context.CancellationToken
            );

            var count = references.Length;

            for (var i = 0; i < count; i++)
            {
                var reference = references[i];

                context.ReportDiagnostic(Diagnostic.Create(
                      ForwardedAttributeUsesGeneratedType
                    , reference.Expression.GetLocation()
                    , reference.GeneratedType.ToGeneratedTypeFullName()
                    , PERSIST_DISPLAY
                    , member.Name
                    , reference.Tool
                ));
            }
        }

        public static bool TryGetParam(ITypeSymbol type, CancellationToken token)
        {
            if (type.IsAbstract)
            {
                return false;
            }

            if (type is INamedTypeSymbol namedType
                && namedType.TryGetGenericType(PERSIST_STORE_BASE, 1, out var baseType, token)
                && baseType.TypeArguments.Length == 1
                && baseType.TypeArguments[0].IsAbstract == false
            )
            {
                return true;
            }

            return type.Interfaces.DoesMatchInterface(IPERSIST_ACCESSOR, token)
                || type.AllInterfaces.DoesMatchInterface(IPERSIST_ACCESSOR, token);
        }
    }
}
