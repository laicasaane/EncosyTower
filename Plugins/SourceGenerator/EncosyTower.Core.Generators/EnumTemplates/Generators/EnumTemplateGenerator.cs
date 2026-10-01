namespace EncosyTower.Core.Generators.EnumTemplates
{
    [Generator]
    internal sealed class EnumTemplateGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.EnumExtensions";
        private const string ENUM_TEMPLATE_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumTemplateAttribute";
        public const string ENUM_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumTemplateAttribute";
        public const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE = $"global::{NAMESPACE}.EnumMembersForTemplateAttribute";
        public const string TYPE_AS_MEMBER_ATTRIBUTE = $"global::{NAMESPACE}.TypeAsEnumMemberForTemplateAttribute";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE_METADATA = $"{NAMESPACE}.EnumMembersForTemplateAttribute";
        private const string TYPE_AS_MEMBER_ATTRIBUTE_METADATA = $"{NAMESPACE}.TypeAsEnumMemberForTemplateAttribute";
        public const string GENERATOR_NAME = nameof(EnumTemplateGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => EnumTemplateCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var templateProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_TEMPLATE_ATTRIBUTE_METADATA
                    , static (node, _) => node is StructDeclarationSyntax
                    , EnumTemplateSpec.Extract
                )
                .WithTrackingName("EnumTemplateGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("EnumTemplateGenerator.ValidSpecs");

            var enumMembersProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      ENUM_MEMBERS_FOR_TEMPLATE_ATTRIBUTE_METADATA
                    , static (node, _) => node is BaseTypeDeclarationSyntax t
                        && (t is not TypeDeclarationSyntax td || td.TypeParameterList is null)
                    , ExtractEnumMembersCandidate
                )
                .Where(static t => t.IsValid);

            var typeAsMemberProvider = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                      TYPE_AS_MEMBER_ATTRIBUTE_METADATA
                    , static (node, _) => node is BaseTypeDeclarationSyntax t
                        && (t is not TypeDeclarationSyntax td || td.TypeParameterList is null)
                    , ExtractTypeAsMemberCandidate
                )
                .Where(static t => t.IsValid);

            var memberProvider = enumMembersProvider.Collect()
                .Combine(typeAsMemberProvider.Collect())
                .Select(static (t, _) => t.Left.AddRange(t.Right));

            var combined = templateProvider
                .Combine(memberProvider)
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("EnumTemplateGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left.Left, source.Left.Right);
            });
        }

        private static TemplateMemberSpec ExtractEnumMembersCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: true);
        }

        private static TemplateMemberSpec ExtractTypeAsMemberCandidate(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            return ExtractMemberFromAttributeContext(context, token, isEnumMembers: false);
        }

        private static TemplateMemberSpec ExtractMemberFromAttributeContext(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
            , bool isEnumMembers
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
            {
                return default;
            }

            if (context.Attributes.Length < 1)
            {
                return default;
            }

            var attrib = context.Attributes[0];

            if (attrib.ConstructorArguments.Length < 2)
            {
                return default;
            }

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol templateSymbol
                || templateSymbol.IsUnmanagedType == false
                || templateSymbol.IsUnboundGenericType
                || templateSymbol.HasAttribute(ENUM_TEMPLATE_ATTRIBUTE, token) == false
            )
            {
                return default;
            }

            return TemplateMemberSpec.Extract(typeSymbol, templateSymbol.ToFullName(), attrib, isEnumMembers, token);
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , EnumTemplateCompilationSpec compilation
            , EnumTemplateSpec templateCandidate
            , ImmutableArray<TemplateMemberSpec> memberCandidates
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (templateCandidate.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var declaration = new EnumTemplateDeclaration(
                  templateCandidate
                , memberCandidates
                , compilation.UnityCollections
            );

            var assemblyName = compilation.Compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator"
                , assemblyName
                , templateCandidate.templateFullName
                , "EnumTemplate"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  templateCandidate.openingSource
                , declaration.WriteCode(context.CancellationToken)
                , templateCandidate.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }
    }
}
