using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Mvvm.Generators.InternalStringAdapters
{
    partial class InternalStringAdapterGenerator
    {
        private const string AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Mvvm.Generators.InternalStringAdapters.InternalStringAdapterGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string IADAPTER = "g__ETMVB.IAdapter";
        private const string ADAPTER_ATTRIBUTE = "[g__ETMVB.Adapter(sourceType: typeof({0}), destType: typeof(string), order: 1)]";
        private const string LABEL_ATTRIBUTE = "[g__ETA.Label(\"{0}\", \"{1}\")]";
        private const string VARIANT = "g__ETV.Variant";
        private const string CACHED_VARIANT_CONVERTER = "g__ETVC.CachedVariantConverter";

        public static string WriteAdapter(
              EquatableArray<StringAdapterSpec> candidates
            , EquatableArray<string> existingAdapterTypeNames
            , string assemblyName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var typeFiltered = new Dictionary<string, StringAdapterSpec>(StringComparer.Ordinal);
            var typesToIgnore = new HashSet<string>(existingAdapterTypeNames, StringComparer.Ordinal);

            foreach (var candidate in candidates)
            {
                token.ThrowIfCancellationRequested();

                if (candidate.IsValid == false)
                {
                    continue;
                }

                var typeName = candidate.fullTypeName;

                if (typeName.ToUnionType().IsNativeUnionType()
                    || typesToIgnore.Contains(typeName)
                )
                {
                    continue;
                }

                if (typeFiltered.ContainsKey(typeName) == false)
                {
                    typeFiltered[typeName] = candidate;
                }
            }

            var p = new Printer(0, 1024 * 16, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLine($"namespace EncosyTower.Mvvm.ViewBinding.__InternalStringAdapters.{MvvmIdentifiers.FromName(assemblyName)}");
            p.OpenScope();
            {
                foreach (var type in typeFiltered.Values)
                {
                    token.ThrowIfCancellationRequested();

                    var adapterTypeName = AdapterTypeName(type);
                    var typeName = type.fullTypeName;
                    var label = $"{type.labelName} ⇒ String";

                    p.PrintLine(string.Format(ADAPTER_ATTRIBUTE, typeName));
                    p.PrintLine(string.Format(LABEL_ATTRIBUTE, label, $"Generated/{type.namespaceName}"));
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public sealed class {adapterTypeName} : {IADAPTER}");
                    p.OpenScope();
                    {
                        p.PrintLine($"private readonly {CACHED_VARIANT_CONVERTER}<{typeName}> _converter = {CACHED_VARIANT_CONVERTER}<{typeName}>.Default;");
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine($"public {VARIANT} Convert(in {VARIANT} variant)");
                        p.OpenScope();
                        {
                            p.PrintLine("return this._converter.ToString(variant);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            return p.Result;
        }

        public static string PrintAdditionalUsings(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var p = new Printer(0, 1024, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__ETA = global::EncosyTower.Annotations;");
            p.PrintLine("using g__ETMVB = global::EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using g__ETV = global::EncosyTower.Variants;");
            p.PrintLine("using g__ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            return p.Result;
        }

        private static string AdapterTypeName(StringAdapterSpec typeRef)
            => $"{typeRef.identifierName}ToStringAdapter";
    }
}
