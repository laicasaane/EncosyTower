namespace EncosyTower.Core.Generators.TypeWraps
{
    [Generator]
    public sealed class TypeWrapGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.TypeWraps";
        private const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        private const string WRAP_TYPE_ATTRIBUTE_METADATA = $"{NAMESPACE}.WrapTypeAttribute";
        private const string WRAP_RECORD_ATTRIBUTE_METADATA = $"{NAMESPACE}.WrapRecordAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var wrapTypeProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  WRAP_TYPE_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax or ClassDeclarationSyntax
                , ExtractSpecForWrapType
            ).WithTrackingName("TypeWrapGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("TypeWrapGenerator.ValidSpecs");

            var wrapRecordProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  WRAP_RECORD_ATTRIBUTE_METADATA
                , static (node, _) => node is RecordDeclarationSyntax recordSyntax
                    && recordSyntax.ParameterList != null
                    && recordSyntax.ParameterList.Parameters.Count > 0
                , ExtractSpecForWrapRecord
            ).WithTrackingName("TypeWrapGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("TypeWrapGenerator.ValidSpecs");

            var combinedWrapType = wrapTypeProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("TypeWrapGenerator.Outputs");

            var combinedWrapRecord = wrapRecordProvider
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("TypeWrapGenerator.Outputs");

            context.RegisterSourceOutput(combinedWrapType, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });

            context.RegisterSourceOutput(combinedWrapRecord, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        public static TypeWrapSpec ExtractSpecForWrapType(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attribute = context.Attributes[0];
            var semanticModel = context.SemanticModel;
            var enableNullable = semanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            switch (context.TargetNode)
            {
                case StructDeclarationSyntax structSyntax:
                {
                    if (TryGetWrapTypeInfo(attribute, token, out var candidate))
                    {
                        GetTypeName(structSyntax, token, ref candidate);
                        SetOtherFields(ref candidate, structSyntax, symbol, token);

                        if (candidate.IsValid == false)
                        {
                            return default;
                        }

                        candidate.isStruct = true;
                        candidate.isRefStruct = structSyntax.Modifiers.Any(SyntaxKind.RefKeyword);

                        var syntaxTree = structSyntax.SyntaxTree;
                        var fileTypeName = symbol.ToFileName();
                        var hintName = symbol.ToMetadataName();

                        TypeCreationHelpers.GenerateOpeningAndClosingSource(
                              structSyntax
                            , token
                            , out var openingSource
                            , out var closingSource
                            , printAdditionalUsings: PrintAdditionalUsings
                        );

                        return new TypeWrapSpec(
                              hintName
                            , openingSource
                            , closingSource
                            , candidate.symbol
                            , candidate.typeName
                            , candidate.typeNameWithTypeParams
                            , candidate.isStruct
                            , candidate.isRefStruct
                            , candidate.isRecord
                            , candidate.fieldTypeSymbol
                            , candidate.fieldName
                            , candidate.excludeConverter || candidate.isGeneric
                            , enableNullable
                            , token
                        ) {
                            containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(structSyntax, token),
                        };
                    }

                    break;
                }

                case ClassDeclarationSyntax classSyntax:
                {
                    if (TryGetWrapTypeInfo(attribute, token, out var candidate)
                        && InheritBaseClass(symbol, token) == false
                    )
                    {
                        GetTypeName(classSyntax, token, ref candidate);
                        SetOtherFields(ref candidate, classSyntax, symbol, token);

                        if (candidate.IsValid == false)
                        {
                            return default;
                        }

                        var syntaxTree = classSyntax.SyntaxTree;
                        var fileTypeName = symbol.ToFileName();
                        var hintName = symbol.ToMetadataName();

                        TypeCreationHelpers.GenerateOpeningAndClosingSource(
                              classSyntax
                            , token
                            , out var openingSource
                            , out var closingSource
                            , printAdditionalUsings: PrintAdditionalUsings
                        );

                        return new TypeWrapSpec(
                              hintName
                            , openingSource
                            , closingSource
                            , candidate.symbol
                            , candidate.typeName
                            , candidate.typeNameWithTypeParams
                            , candidate.isStruct
                            , candidate.isRefStruct
                            , candidate.isRecord
                            , candidate.fieldTypeSymbol
                            , candidate.fieldName
                            , candidate.excludeConverter || candidate.isGeneric
                            , enableNullable
                            , token
                        ) {
                            containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(classSyntax, token),
                        };
                    }

                    break;
                }
            }

            return default;
        }

        public static TypeWrapSpec ExtractSpecForWrapRecord(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not RecordDeclarationSyntax recordSyntax
                || context.TargetSymbol is not INamedTypeSymbol symbol
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            if (recordSyntax.ParameterList is not ParameterListSyntax { Parameters.Count: 1 })
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var enableNullable = semanticModel.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

            if (TryGetWrapRecordInfo(recordSyntax, context.Attributes[0], semanticModel, token, out var candidate)
                && (recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) == false
                    || InheritBaseClass(symbol, token) == false)
            )
            {
                GetTypeName(recordSyntax, token, ref candidate);
                SetOtherFields(ref candidate, recordSyntax, symbol, token);

                if (candidate.IsValid == false)
                {
                    return default;
                }

                candidate.isStruct = recordSyntax.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);
                candidate.isRecord = true;

                var syntaxTree = recordSyntax.SyntaxTree;
                var fileTypeName = symbol.ToFileName();
                var hintName = symbol.ToMetadataName();

                TypeCreationHelpers.GenerateOpeningAndClosingSource(
                      recordSyntax
                    , token
                    , out var openingSource
                    , out var closingSource
                    , printAdditionalUsings: PrintAdditionalUsings
                );

                return new TypeWrapSpec(
                      hintName
                    , openingSource
                    , closingSource
                    , candidate.symbol
                    , candidate.typeName
                    , candidate.typeNameWithTypeParams
                    , candidate.isStruct
                    , candidate.isRefStruct
                    , candidate.isRecord
                    , candidate.fieldTypeSymbol
                    , candidate.fieldName
                    , candidate.excludeConverter || candidate.isGeneric
                    , enableNullable
                    , token
                ) {
                    containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(recordSyntax, token),
                };
            }

            return default;
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SCM = System.ComponentModel;");
            p.PrintLine("using g__SC = global::System.Collections;");
            p.PrintLine("using g__SCG = global::System.Collections.Generic;");
            p.PrintLine("using g__SD = global::System.Diagnostics;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SG = global::System.Globalization;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETTW = global::EncosyTower.TypeWraps;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static bool InheritBaseClass(INamedTypeSymbol classSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var baseType = classSymbol.BaseType;

            while (baseType != null)
            {
                token.ThrowIfCancellationRequested();

                if (baseType.TypeKind == TypeKind.Class
                    && baseType.SpecialType != SpecialType.System_Object
                )
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private static bool TryGetWrapTypeInfo(
              AttributeData attribute
            , CancellationToken token
            , out Candidate result
        )
        {
            token.ThrowIfCancellationRequested();

            result = new Candidate {
                fieldName = string.Empty,
                excludeConverter = GetExcludeConverter(attribute, token),
            };

            var args = attribute.ConstructorArguments;

            if (args.Length < 1
                || args[0].Kind != TypedConstantKind.Type
                || args[0].Value is not ITypeSymbol fieldType
            )
            {
                return false;
            }

            result.fieldTypeSymbol = fieldType as INamedTypeSymbol;

            if (args.Length > 1 && args[1].Value is string memberName)
            {
                result.fieldName = memberName;
            }

            return true;
        }

        private static bool TryGetWrapRecordInfo(
              RecordDeclarationSyntax syntax
            , AttributeData attribute
            , SemanticModel semanticModel
            , CancellationToken token
            , out Candidate result
        )
        {
            token.ThrowIfCancellationRequested();

            result = new Candidate {
                excludeConverter = GetExcludeConverter(attribute, token),
            };

            if (syntax.ParameterList is not { Parameters.Count: > 0 } parameterList
                || semanticModel.GetDeclaredSymbol(parameterList.Parameters[0], token)
                    is not IParameterSymbol parameter
            )
            {
                return false;
            }

            result.fieldTypeSymbol = parameter.Type as INamedTypeSymbol;
            result.fieldName = parameter.Name;
            return true;
        }

        private static bool GetExcludeConverter(AttributeData attribute, CancellationToken token)
        {
            foreach (var namedArgument in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(namedArgument.Key, "ExcludeConverter", StringComparison.Ordinal)
                    && namedArgument.Value.Value is bool excludeConverter
                )
                {
                    return excludeConverter;
                }
            }

            return false;
        }

        private static void GetTypeName(TypeDeclarationSyntax syntax, CancellationToken token, ref Candidate candidate)
        {
            token.ThrowIfCancellationRequested();

            var typeNameWithTypeParamsBuilder = new StringBuilder(syntax.Identifier.ValueText);

            if (syntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                candidate.isGeneric = true;

                typeNameWithTypeParamsBuilder.Append("<");

                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    token.ThrowIfCancellationRequested();

                    typeNameWithTypeParamsBuilder.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        typeNameWithTypeParamsBuilder.Append(", ");
                    }
                }

                typeNameWithTypeParamsBuilder.Append(">");
            }

            candidate.typeName = syntax.Identifier.ValueText;
            candidate.typeNameWithTypeParams = typeNameWithTypeParamsBuilder.ToString();
        }

        private static void SetOtherFields(
              ref Candidate candidate
            , TypeDeclarationSyntax syntax
            , INamedTypeSymbol symbol
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            candidate.syntax = syntax;
            candidate.symbol = symbol;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , TypeWrapSpec declaration
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (declaration.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.TypeWraps.TypeWrapGenerator"
                , assemblyName
                , declaration.fullTypeName
                , "TypeWrap"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  declaration.openingSource
                , declaration.WriteCode(context.CancellationToken)
                , declaration.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        public partial struct Candidate : IEquatable<Candidate>
        {
            public TypeDeclarationSyntax syntax;
            public INamedTypeSymbol symbol;
            public string typeName;
            public string typeNameWithTypeParams;
            public bool isGeneric;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;
            public INamedTypeSymbol fieldTypeSymbol;
            public string fieldName;
            public bool excludeConverter;

            public readonly bool IsValid
                => syntax != null
                && symbol != null
                && fieldTypeSymbol != null
                && string.IsNullOrEmpty(typeName) == false
                && string.IsNullOrEmpty(typeNameWithTypeParams) == false
                && fieldTypeSymbol.TypeKind is not (TypeKind.Dynamic or TypeKind.Error)
                && fieldTypeSymbol.ContainsErrorType() == false;

            public readonly override bool Equals(object obj)
                => obj is Candidate other && Equals(other);

            public readonly bool Equals(Candidate other)
                => string.Equals(typeNameWithTypeParams, other.typeNameWithTypeParams, StringComparison.Ordinal)
                && string.Equals(fieldTypeSymbol?.ToFullName() ?? string.Empty, other.fieldTypeSymbol?.ToFullName() ?? string.Empty)
                && fieldTypeSymbol?.TypeKind == other.fieldTypeSymbol?.TypeKind
                ;

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      typeNameWithTypeParams
                    , fieldTypeSymbol?.ToFullName() ?? string.Empty
                    , fieldTypeSymbol?.TypeKind
                );
        }
    }
}
