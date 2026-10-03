using EncosyTower.Core.Generators.EnumExtensions;
using EncosyTower.SourceGen.Helpers.UnionIds;

namespace EncosyTower.Core.Generators.UnionIds
{
    [Generator]
    internal sealed class UnionIdGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.UnionIds";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string UNION_ID_ATTRIBUTE_METADATA = $"{NAMESPACE}.UnionIdAttribute";
        public const string KIND_FOR_UNION_ID_ATTRIBUTE_METADATA = $"{NAMESPACE}.KindForUnionIdAttribute";
        public const string GENERATOR_NAME = nameof(UnionIdGenerator);

        private const string UNION_ID_ATTRIBUTE_FULL = $"global::{UNION_ID_ATTRIBUTE_METADATA}";
        private const string UNION_ID_KIND_ATTRIBUTE_FULL = $"global::{NAMESPACE}.UnionIdKindAttribute";
        private const string ENUM_EXTENSIONS_ATTRIBUTE_FULL = "global::EncosyTower.EnumExtensions.EnumExtensionsAttribute";
        private const string FLAGS_ATTRIBUTE_FULL = "global::System.FlagsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => UnionIdCompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var idProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  UNION_ID_ATTRIBUTE_METADATA
                , static (node, _) => node is StructDeclarationSyntax
                , GetUnionIdInfo
            ).WithTrackingName("UnionIdGenerator.Candidates")
                .Where(static x => x.IsValid)
                .WithTrackingName("UnionIdGenerator.ValidSpecs");

            var kindProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  KIND_FOR_UNION_ID_ATTRIBUTE_METADATA
                , static (node, _) => node is BaseTypeDeclarationSyntax typeSyntax
                    && typeSyntax.Kind() is SyntaxKind.EnumDeclaration
                        or SyntaxKind.StructDeclaration
                        or SyntaxKind.RecordStructDeclaration
                , GetKindForUnionIdInfo
            ).Where(static x => string.IsNullOrEmpty(x.kindFullName) == false
                && string.IsNullOrEmpty(x.idFullName) == false
            );

            var combined = idProvider
                .Combine(kindProvider.Collect())
                .Combine(compilationProvider)
                .Where(static t => t.Right.Compilation.IsValid)
                .WithTrackingName("UnionIdGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left.Left, source.Left.Right);
            });
        }

        private static IdSpec GetUnionIdInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsUnmanagedType == false
                || symbol.IsUnboundGenericType
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            if (HasErrorTypedInlineKind(symbol, token))
            {
                return default;
            }

            var attrib = context.Attributes[0];
            var info = new IdSpec {
                separator = '-',
            };

            var args = attrib.NamedArguments;

            foreach (var arg in args)
            {
                token.ThrowIfCancellationRequested();

                switch (arg.Key)
                {
                    case "Size":
                    {
                        if (arg.Value.Value is byte val)
                            info.size = (UnionIdSize)val;
                        break;
                    }

                    case "DisplayNameForId":
                    {
                        if (arg.Value.Value is string val)
                            info.displayNameForId = val;
                        break;
                    }

                    case "DisplayNameForKind":
                    {
                        if (arg.Value.Value is string val)
                            info.displayNameForKind = val;
                        break;
                    }

                    case "Separator":
                    {
                        if (arg.Value.Value is char val)
                            info.separator = val;
                        break;
                    }

                    case "KindSettings":
                    {
                        if (arg.Value.Value is byte val)
                            info.kindSettings = (UnionIdKindSettings)val;
                        break;
                    }

                    case "ConverterSettings":
                    {
                        if (arg.Value.Value is byte val)
                            info.converterSettings = (ParsableStructConverterSettings)val;
                        break;
                    }

                    case "FixedStringBytes":
                    {
                        if (arg.Value.Value is ushort val && val > 0)
                        {
                            info.fixedStringBytes = val switch {
                                <= 32 => 32 - 3,
                                <= 64 => 64 - 3,
                                <= 128 => 128 - 3,
                                <= 512 => 512 - 3,
                                _ => 4096 - 3,
                            };
                        }
                        break;
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            var ns = symbol.ContainingNamespace;
            info.fullName = symbol.ToFullName();
            info.simpleName = symbol.Name;
            info.fileHintName = symbol.ToFileName();
            info.namespaceName = ns is { IsGlobalNamespace: false } ? ns.ToDisplayString() : string.Empty;
            info.accessibility = symbol.DeclaredAccessibility;
            info.parentIsNamespace = context.TargetNode.Parent is BaseNamespaceDeclarationSyntax
                or CompilationUnitSyntax;
            info.generateTryFormat = CheckTryParse(symbol, token) == false;

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  context.TargetNode
                , token
                , out info.openingSource
                , out info.closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            info.containingTypes = symbol.GetContainingTypes(token);
            info.inlineKinds = GetInlineKinds(symbol, info.fullName, token);

            return info;
        }

        private static KindSpec GetKindForUnionIdInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol kindSymbol
                || kindSymbol.IsUnmanagedType == false
                || context.Attributes.Length < 1
            )
            {
                return default;
            }

            var attrib = context.Attributes[0];

            if (attrib.ConstructorArguments.Length < 1)
                return default;

            var typeArg = attrib.ConstructorArguments[0];

            if (typeArg.Kind != TypedConstantKind.Type
                || typeArg.Value is not INamedTypeSymbol idSymbol
                || idSymbol.IsUnmanagedType == false
                || idSymbol.IsUnboundGenericType
                || idSymbol.HasAttribute(UNION_ID_ATTRIBUTE_FULL, token) == false
            )
            {
                return default;
            }

            var ctorArgs = attrib.ConstructorArguments;
            ulong order = 0;
            string name = null;
            string displayName = null;
            bool signed = false;
            var toStringMethods = ToStringMethods.Default;
            var tryParseMethodType = TryParseMethodType.None;

            for (var i = 1; i < ctorArgs.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var arg = ctorArgs[i];

                if (i == 1 && arg.Value is ulong ulongVal) order = ulongVal;
                else if (i == 2 && arg.Value is string stringVal1) name = stringVal1;
                else if (i == 3 && arg.Value is string stringVal2) displayName = stringVal2;
                else if (i == 4 && arg.Value is bool boolVal) signed = boolVal;
                else if (i == 5 && arg.Value is byte byteVal1) toStringMethods = (ToStringMethods)byteVal1;
                else if (i == 6 && arg.Value is byte byteVal2) tryParseMethodType = (TryParseMethodType)byteVal2;
            }

            return BuildKindInfo(
                  kindSymbol
                , idSymbol.ToFullName()
                , order
                , name
                , displayName
                , signed
                , toStringMethods
                , tryParseMethodType
                , token
            );
        }

        private static EquatableArray<KindSpec> GetInlineKinds(
              INamedTypeSymbol idSymbol
            , string idFullName
            , CancellationToken token
        )
        {
            var attributes = idSymbol.GetAttributes(UNION_ID_KIND_ATTRIBUTE_FULL, token);
            using var builder = ImmutableArrayBuilder<KindSpec>.Rent();

            foreach (var attrib in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attrib == null)
                    continue;

                var args = attrib.ConstructorArguments;

                if (args.Length < 1)
                    continue;

                var typeArg = args[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol kindSymbol
                    || kindSymbol.IsUnmanagedType == false
                )
                {
                    continue;
                }

                ulong order = 0;
                string name = null;
                string displayName = null;
                bool signed = false;
                var toStringMethods = ToStringMethods.Default;
                var tryParseMethodType = TryParseMethodType.None;

                for (var i = 1; i < args.Length; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var arg = args[i];

                    if (i == 1 && arg.Value is ulong ulongVal) order = ulongVal;
                    else if (i == 2 && arg.Value is string stringVal1) name = stringVal1;
                    else if (i == 3 && arg.Value is string stringVal2) displayName = stringVal2;
                    else if (i == 4 && arg.Value is bool boolVal) signed = boolVal;
                    else if (i == 5 && arg.Value is byte byteVal1) toStringMethods = (ToStringMethods)byteVal1;
                    else if (i == 6 && arg.Value is byte byteVal2) tryParseMethodType = (TryParseMethodType)byteVal2;
                }

                var kindInfo = BuildKindInfo(
                      kindSymbol
                    , idFullName
                    , order
                    , name
                    , displayName
                    , signed
                    , toStringMethods
                    , tryParseMethodType
                    , token
                );

                if (string.IsNullOrEmpty(kindInfo.kindFullName) == false)
                    builder.Add(kindInfo);
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static bool HasErrorTypedInlineKind(INamedTypeSymbol idSymbol, CancellationToken token)
        {
            foreach (var attrib in idSymbol.GetAttributes(UNION_ID_KIND_ATTRIBUTE_FULL, token))
            {
                token.ThrowIfCancellationRequested();

                if (attrib == null || attrib.ConstructorArguments.Length < 1)
                {
                    continue;
                }

                var typeArg = attrib.ConstructorArguments[0];

                if (typeArg.Kind == TypedConstantKind.Type
                    && typeArg.Value is ITypeSymbol kindType
                    && kindType.ContainsErrorType(token)
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static KindSpec BuildKindInfo(
              INamedTypeSymbol kindSymbol
            , string idFullName
            , ulong order
            , string name
            , string displayName
            , bool signed
            , ToStringMethods toStringMethods
            , TryParseMethodType tryParseMethodType
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var info = new KindSpec {
                kindFullName = kindSymbol.ToFullName(),
                kindSimpleName = kindSymbol.Name,
                idFullName = idFullName,
                order = order,
                name = name ?? string.Empty,
                displayName = displayName ?? string.Empty,
                signed = signed,
                toStringMethods = toStringMethods,
            };

            var size = 0;
            var alignment = 1;
            kindSymbol.GetUnmanagedSizeAndAlignment(ref size, ref alignment, token);
            info.kindUnmanagedSize = size;

            var isEnum = kindSymbol.TypeKind == TypeKind.Enum;
            info.isEnum = isEnum;

            var kindUnionIdAttr = kindSymbol.GetAttribute(UNION_ID_ATTRIBUTE_FULL, token);
            info.isKindAlsoUnionId = kindUnionIdAttr != null;

            if (isEnum)
            {
                info.kindEnumHasFlags = kindSymbol.HasAttribute(FLAGS_ATTRIBUTE_FULL, token);
                info.kindEnumUnderlyingTypeName = kindSymbol.EnumUnderlyingType?.ToDisplayString() ?? "int";
                info.hasExternalEnumExtensions = kindSymbol.TryGetAttribute(ENUM_EXTENSIONS_ATTRIBUTE_FULL, out _, token);

                if (info.hasExternalEnumExtensions)
                {
                    info.externalEnumExtensionsFullName = $"{kindSymbol.ToFullName()}Extensions";
                }

                var enumMembers = kindSymbol.GetMembers();
                using var memberBuilder = ImmutableArrayBuilder<EnumMemberSpec>.Rent();
                var maxBytes = 0;
                var hasDisplayAttr = false;

                foreach (var member in enumMembers)
                {
                    token.ThrowIfCancellationRequested();

                    if (member is not IFieldSymbol field || field.ConstantValue is null)
                    {
                        continue;
                    }

                    string memberDisplayName = null;

                    foreach (var attr in field.GetAttributes())
                    {
                        token.ThrowIfCancellationRequested();

                        var attrName = attr.AttributeClass?.Name ?? string.Empty;

                        switch (attrName)
                        {
                            case nameof(ObsoleteAttribute):
                                goto CONTINUE;

                            case "LabelAttribute":
                            case "DescriptionAttribute":
                            case "DisplayAttribute":
                            case "DisplayNameAttribute":
                            case "InspectorNameAttribute":
                            {
                                if (attr.ConstructorArguments.Length > 0)
                                {
                                    var a = attr.ConstructorArguments[0];

                                    if (a.Kind == TypedConstantKind.Primitive && a.Value?.ToString() is string dn)
                                    {
                                        memberDisplayName = dn;
                                        goto ADD;
                                    }
                                }
                                else if (attr.NamedArguments.Length > 0)
                                {
                                    foreach (var na in attr.NamedArguments)
                                    {
                                        token.ThrowIfCancellationRequested();

                                        if (na.Key is "Name" or "DisplayName"
                                            && na.Value.Kind == TypedConstantKind.Primitive
                                            && na.Value.Value?.ToString() is string dn
                                        )
                                        {
                                            memberDisplayName = dn;
                                            goto ADD;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }

                    ADD:
                    {
                        if (string.IsNullOrEmpty(memberDisplayName) == false)
                        {
                            hasDisplayAttr = true;
                        }

                        var nb = member.Name.GetByteCount();
                        var db = memberDisplayName.GetByteCount();
                        maxBytes = Math.Max(maxBytes, Math.Max(nb, db));
                        memberBuilder.Add(new EnumMemberSpec { name = member.Name, displayName = memberDisplayName });
                        continue;
                    }

                    CONTINUE:
                    {
                        continue;
                    }
                }

                info.kindEnumValues = memberBuilder.ToImmutable().AsEquatableArray();
                info.kindEnumFixedStringBytes = maxBytes;
                info.kindEnumIsDisplayAttributeUsed = hasDisplayAttr;

                info.toStringMethods |= ToStringMethods.ToDisplayString;
            }
            else if (info.isKindAlsoUnionId)
            {
                info.tryParseSpan = new MemberExistence(true, false, false, 4);
                info.equality = new Equality(EqualityStrategy.Equals, false, false);
                info.toStringMethods |= ToStringMethods.All;

                var kindUnionIdSize = UnionIdSize.UInt;

                foreach (var arg in kindUnionIdAttr.NamedArguments)
                {
                    if (arg.Key == "Size" && arg.Value.Value is byte szVal)
                    {
                        kindUnionIdSize = (UnionIdSize)szVal;
                        break;
                    }
                }

                var unionSize = kindUnionIdSize switch {
                    UnionIdSize.UShort => 2,
                    UnionIdSize.UInt => 4,
                    UnionIdSize.ULong => 8,
                    UnionIdSize.UInt3 => 12,
                    UnionIdSize.ULong2 => 16,
                    _ => 4,
                };

                info.kindUnmanagedSize = Math.Max(info.kindUnmanagedSize, unionSize);
            }
            else
            {
                info.tryParseSpan = tryParseMethodType switch {
                    TryParseMethodType.Instance => new MemberExistence(true, false, false, 4),
                    TryParseMethodType.Static => new MemberExistence(true, true, false, 4),
                    _ => kindSymbol.FindTryParseSpan().DefaultIfNullableIs(true),
                };

                info.equality = kindSymbol.DetermineEquality();

                if (info.equality.IsNullable)
                    info.kindFullNameFromNullable = kindSymbol.GetTypeFromNullable().ToFullName();

                if (CheckToDisplayFixedString(kindSymbol, token))
                {
                    info.hasToDisplayString = true;
                    info.toStringMethods |= ToStringMethods.ToDisplayString;
                }

                if (CheckToFixedString(kindSymbol, "ToFixedString", out var fixedBytes, token))
                {
                    info.hasToFixedString = true;
                    info.toFixedStringBytes = fixedBytes;
                }

                if (CheckToFixedString(kindSymbol, "ToDisplayFixedString", out var displayFixedBytes, token))
                {
                    info.hasToDisplayFixedString = true;
                    info.toDisplayFixedStringBytes = displayFixedBytes;
                }
            }

            return info;
        }

        private static bool CheckToFixedString(
              INamedTypeSymbol symbol
            , string name
            , out int byteCount
            , CancellationToken token
        )
        {
            var members = symbol.GetMembers(name);

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic
                    || method.Parameters.Length != 0
                    || method.ReturnsVoid
                )
                {
                    continue;
                }

                var returnTypeName = method.ReturnType.ToFullName();

                if (returnTypeName.StartsWith("global::Unity.Collections.FixedString", StringComparison.Ordinal) == false)
                    continue;

                var indexOfBytes = returnTypeName.IndexOf("Bytes", 36);

                if (indexOfBytes < 39)
                    continue;

                if (int.TryParse(returnTypeName.Substring(37, indexOfBytes - 37), out byteCount))
                    return true;
            }

            byteCount = 0;
            return false;
        }

        private static bool CheckToDisplayFixedString(INamedTypeSymbol symbol, CancellationToken token)
        {
            var members = symbol.GetMembers("ToDisplayString");

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic
                    || method.Parameters.Length != 0
                    || method.ReturnType.SpecialType != SpecialType.System_String
                )
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static bool CheckTryParse(INamedTypeSymbol symbol, CancellationToken token)
        {
            var members = symbol.GetMembers("TryParse");

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.IsStatic
                    || method.Parameters.Length != 2
                    || method.ReturnType.SpecialType != SpecialType.System_Boolean
                )
                {
                    continue;
                }

                var parameters = method.Parameters;
                var p1 = parameters[0];
                var p2 = parameters[1];

                return p2.Type.SpecialType == SpecialType.System_Int32
                    && p2.RefKind == RefKind.Out
                    && p1.Type.IsType("global::System.Span<char>", token)
                    ;
            }

            return false;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , UnionIdCompilationSpec compilation
            , IdSpec idInfo
            , ImmutableArray<KindSpec> kindInfos
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (idInfo.IsValid == false)
            {
                return;
            }

            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.Compilation.AssemblyName;
            var declaration = new UnionIdDeclaration(
                  idInfo
                , kindInfos
                , compilation.Odin
                , compilation.Unity
                , compilation.UnityCollections
                , compilation.EnableNullable
            );

            if (declaration.IsInvalid)
            {
                return;
            }

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.UnionIds.UnionIdGenerator"
                , assemblyName
                , idInfo.fullName
                , "UnionId"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  idInfo.openingSource
                , declaration.WriteCode(context.CancellationToken)
                , idInfo.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SC = global::System.Collections;");
            p.PrintLine("using g__SCG = global::System.Collections.Generic;");
            p.PrintLine("using g__SCM = global::System.ComponentModel;");
            p.PrintLine("using g__SD = global::System.Diagnostics;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETCol = global::EncosyTower.Collections;");
            p.PrintLine("using g__ETColE = global::EncosyTower.Collections.Extensions;");
            p.PrintLine("using g__ETCon = global::EncosyTower.Conversion;");
            p.PrintLine("using g__ETDBG = global::EncosyTower.Debugging;");
            p.PrintLine("using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;");
            p.PrintLine("using g__ETEE = global::EncosyTower.EnumExtensions;");
            p.PrintLine("using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;");
            p.PrintLine("using g__ETUI = global::EncosyTower.UnionIds;");
            p.PrintLine("using g__ETUIT = global::EncosyTower.UnionIds.Types;");
            p.PrintLine("using g__ETS = global::EncosyTower.Serialization;");
            p.PrintLine("using g__ETSE = global::EncosyTower.SystemExtensions;");
            p.PrintLine("using g__UE = global::UnityEngine;");
            p.PrintLine("using g__UC = global::Unity.Collections;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }
    }
}
