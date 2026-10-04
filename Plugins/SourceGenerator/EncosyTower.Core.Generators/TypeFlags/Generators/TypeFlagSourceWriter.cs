using EncosyTower.Core.TypeFlags;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.Core.Generators.TypeFlags
{
    internal static class TypeFlagSourceWriter
    {
        private const string GENERATED_CODE
            = $"[g__SCDC.GeneratedCode(\"{TypeFlagRules.GENERATOR_METADATA_NAME}\", \"{SourceGenVersion.VALUE}\")]";

        private const string EXCLUDE_FROM_CODE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string INLINE = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string LINK_EXTENSIONS = "g__ETTF.TypeFlagLinkExtensions";
        private const string FLAG_EXTENSIONS = "g__ETTF.TypeFlagExtensions";
        private const string OBJECT_TYPE_PARAMETER = "TObject";
        private const string VALUE_TYPE_PARAMETER = "TValue";
        private const string DISABLE_CS0414 = "#pragma warning disable CS0414";
        private const string RESTORE_CS0414 = "#pragma warning restore CS0414";

        internal static SourceText Write(in TypeFlagSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var printer = new Printer(0, 1024 * 8, token);
            var hasNamespace = string.IsNullOrEmpty(spec.NamespaceName) == false;

            WriteAliases(ref printer, spec.Options.UseExtensions);
            printer.PrintEndLine();

            if (hasNamespace)
            {
                printer.PrintLine($"namespace {spec.NamespaceName}");
                printer.OpenScope();
            }

            foreach (var header in spec.ContainingTypeHeaders)
            {
                printer.PrintLine(header);
                printer.OpenScope();
            }

            printer.PrintLine(spec.DeclarationHeader);
            printer.OpenScope();
            {
                if (spec.Options.UseExtensions)
                {
                    WriteExtensionFields(ref printer, spec);
                }
                else
                {
                    WriteGeneratedApi(ref printer, spec);
                }
            }
            printer.CloseScope();

            foreach (var _ in spec.ContainingTypeHeaders)
            {
                printer.CloseScope();
            }

            if (hasNamespace)
            {
                printer.CloseScope();
            }

            return SourceText.From(printer.Result, Encoding.UTF8);
        }

        private static void WriteAliases(ref Printer printer, bool useExtensions)
        {
            printer.PrintLineIf(useExtensions == false, "using g__ETT = global::EncosyTower.Types;");
            printer.PrintLine("using g__ETTF = global::EncosyTower.TypeFlags;");
            printer.PrintLineIf(useExtensions == false, "using g__ETTs = global::EncosyTower.Tasks;");
            printer.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");

            if (useExtensions == false)
            {
                printer.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
                printer.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
                printer.PrintLine("using g__ST = global::System.Threading;");
            }
        }

        private static void WriteExtensionFields(ref Printer printer, in TypeFlagSpec spec)
        {
            var writeAccess = spec.Options.WriteAccess;
            var flagType = $"g__ETTF.TypeFlag<{spec.FullTypeName}>";
            var publicType = writeAccess == TypeFlagAccess.Public ? flagType : $"{flagType}.ReadOnly";
            var publicModifiers = GetFieldModifiers("public", IsHidden(spec, TypeFlagMember.TypeFlagField));

            printer.PrintLine(GENERATED_CODE);
            printer.PrintLine($"{publicModifiers} {publicType} {TypeFlagRules.TYPE_FLAG_NAME} = default;");

            if (writeAccess == TypeFlagAccess.Public)
            {
                return;
            }

            var isPrivate = writeAccess == TypeFlagAccess.Private;
            var writerModifiers = GetFieldModifiers(
                  isPrivate ? "private" : "internal"
                , IsHidden(spec, TypeFlagMember.ReadWriteField)
            );

            printer.PrintEndLine();
            printer.PrintLineIf(isPrivate, DISABLE_CS0414);
            printer.PrintLine(GENERATED_CODE);
            printer.PrintLine($"{writerModifiers} {flagType} {TypeFlagRules.READ_WRITE_FIELD_NAME} = default;");
            printer.PrintLineIf(isPrivate, RESTORE_CS0414);
        }

        private static void WriteGeneratedApi(ref Printer printer, in TypeFlagSpec spec)
        {
            var writeAccess = spec.Options.WriteAccess;
            var isPrivate = writeAccess == TypeFlagAccess.Private;
            var publicModifiers = GetFieldModifiers("public", IsHidden(spec, TypeFlagMember.TypeFlagField));
            var apiTypeName = TypeFlagRules.API_TYPE_NAME;

            printer.PrintLine(GENERATED_CODE);
            printer.PrintLine($"{publicModifiers} {apiTypeName} {TypeFlagRules.TYPE_FLAG_NAME} = default;");

            if (isPrivate)
            {
                var writerModifiers = GetFieldModifiers("private", IsHidden(spec, TypeFlagMember.ReadWriteField));
                var writerType = TypeFlagRules.READ_WRITE_TYPE_NAME;

                printer.PrintEndLine();
                printer.PrintLine(DISABLE_CS0414);
                printer.PrintLine(GENERATED_CODE);
                printer.PrintLine($"{writerModifiers} {writerType} {TypeFlagRules.READ_WRITE_FIELD_NAME} = default;");
                printer.PrintLine(RESTORE_CS0414);
            }

            var names = new TypeParameterNames(
                  GetFreeName(OBJECT_TYPE_PARAMETER, spec.Declarations)
                , GetFreeName(VALUE_TYPE_PARAMETER, spec.Declarations)
            );

            var apiType = new StructDeclaration(
                  Accessibility: "public"
                , Name: TypeFlagRules.API_TYPE_NAME
                , IsHidden: IsHidden(spec, TypeFlagMember.ApiType)
                , HasWriteMembers: isPrivate == false
                , WriteAccess: writeAccess == TypeFlagAccess.Internal ? "internal" : "public"
            );

            WriteStruct(ref printer, spec, names, apiType);

            if (isPrivate)
            {
                var readWriteType = new StructDeclaration(
                      Accessibility: "private"
                    , Name: TypeFlagRules.READ_WRITE_TYPE_NAME
                    , IsHidden: IsHidden(spec, TypeFlagMember.ReadWriteType)
                    , HasWriteMembers: true
                    , WriteAccess: "public"
                );

                WriteStruct(ref printer, spec, names, readWriteType);
            }
        }

        private static void WriteStruct(
              ref Printer printer
            , in TypeFlagSpec spec
            , in TypeParameterNames names
            , in StructDeclaration declaration
        )
        {
            var modifiers = declaration.IsHidden
                ? $"{declaration.Accessibility} new readonly struct"
                : $"{declaration.Accessibility} readonly struct";

            printer.PrintEndLine();
            printer.PrintLine(GENERATED_CODE);
            printer.PrintLine(EXCLUDE_FROM_CODE_COVERAGE);
            printer.PrintLine($"{modifiers} {declaration.Name}");
            printer.OpenScope();
            {
                WriteReadMembers(ref printer, spec, names);

                if (declaration.HasWriteMembers)
                {
                    WriteWriteMembers(ref printer, spec, names, declaration.WriteAccess);
                }
            }
            printer.CloseScope();
        }

        private static void WriteReadMembers(ref Printer printer, in TypeFlagSpec spec, in TypeParameterNames names)
        {
            var owner = spec.FullTypeName;
            var flag = $"default(g__ETTF.TypeFlag<{owner}>)";
            var self = $"default(g__ETTF.TypeFlagLink<{owner}, {owner}>)";
            var api = spec.Options.Api;
            var hasSelf = (api & TypeFlagApi.Self) != 0;
            var hasAsync = hasSelf && (api & TypeFlagApi.Async) != 0;

            WriteProperty(
                  printer: ref printer
                , documentation: FlagDoc("TypeId")
                , signature: $"public g__ETT.TypeId<{owner}> TypeId"
                , body: $"{flag}.TypeId"
                , separate: false
            );

            WriteProperty(
                  printer: ref printer
                , documentation: FlagDoc("IsEnabled")
                , signature: "public bool IsEnabled"
                , body: $"{flag}.IsEnabled"
                , separate: true
            );

            WriteMethod(
                  ref printer
                , FlagDoc("WaitUntilEnabledAsync")
                , "public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)"
                , $"{flag}.WaitUntilEnabledAsync(token)"
            );

            if (hasSelf && spec.IsValueType == false)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("TryGetInstance")
                    , $"public bool TryGetInstance([g__SDCA.MaybeNullWhen(false)] out {owner} instance)"
                    , $"{LINK_EXTENSIONS}.TryGetObject({self}, out instance)"
                );

                WriteMethod(
                      ref printer
                    , SelfDoc("GetInstanceOrThrow")
                    , $"public {owner} GetInstanceOrThrow()"
                    , $"{LINK_EXTENSIONS}.GetObjectOrThrow({self})"
                );
            }

            if (hasAsync && spec.IsValueType == false)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("GetInstanceAsync")
                    , $"public g__ETTs.UnityTask<{owner}> GetInstanceAsync(g__ST.CancellationToken token = default)"
                    , $"{LINK_EXTENSIONS}.GetObjectAsync({self}, token)"
                );
            }

            if (hasSelf && spec.IsValueType)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("TryGetValue")
                    , $"public bool TryGetValue(out {owner} value)"
                    , $"{LINK_EXTENSIONS}.TryGetValue({self}, out value)"
                );

                WriteMethod(
                      ref printer
                    , SelfDoc("GetValueOrThrow")
                    , $"public {owner} GetValueOrThrow()"
                    , $"{LINK_EXTENSIONS}.GetValueOrThrow({self})"
                );
            }

            if (hasAsync && spec.IsValueType)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("GetValueAsync")
                    , $"public g__ETTs.UnityTask<{owner}> GetValueAsync(g__ST.CancellationToken token = default)"
                    , $"{LINK_EXTENSIONS}.GetValueAsync({self}, token)"
                );
            }

            if ((api & TypeFlagApi.Related) == 0)
            {
                return;
            }

            var obj = names.Object;
            var value = names.Value;
            var objectLink = $"default(g__ETTF.TypeFlagLink<{owner}, {obj}>)";
            var valueLink = $"default(g__ETTF.TypeFlagLink<{owner}, {value}>)";

            WriteMethod(
                  ref printer
                , ObjectDoc("TryGetObject")
                , $"public bool TryGetObject<{obj}>([g__SDCA.MaybeNullWhen(false)] out {obj} obj)"
                , $"where {obj} : class"
                , $"{LINK_EXTENSIONS}.TryGetObject({objectLink}, out obj)"
            );

            WriteMethod(
                  ref printer
                , ObjectDoc("GetObjectOrThrow")
                , $"public {obj} GetObjectOrThrow<{obj}>()"
                , $"where {obj} : class"
                , $"{LINK_EXTENSIONS}.GetObjectOrThrow({objectLink})"
            );

            WriteMethod(
                  ref printer
                , ValueDoc("TryGetValue")
                , $"public bool TryGetValue<{value}>(out {value} value)"
                , $"where {value} : struct"
                , $"{LINK_EXTENSIONS}.TryGetValue({valueLink}, out value)"
            );

            WriteMethod(
                  ref printer
                , ValueDoc("GetValueOrThrow")
                , $"public {value} GetValueOrThrow<{value}>()"
                , $"where {value} : struct"
                , $"{LINK_EXTENSIONS}.GetValueOrThrow({valueLink})"
            );
        }

        private static void WriteWriteMembers(
              ref Printer printer
            , in TypeFlagSpec spec
            , in TypeParameterNames names
            , string access
        )
        {
            var owner = spec.FullTypeName;
            var flag = $"default(g__ETTF.TypeFlag<{owner}>)";
            var self = $"default(g__ETTF.TypeFlagLink<{owner}, {owner}>)";
            var api = spec.Options.Api;
            var hasSelf = (api & TypeFlagApi.Self) != 0;

            WriteMethod(ref printer, FlagDoc("Enable"), $"{access} bool Enable()", $"{flag}.Enable()");
            WriteMethod(ref printer, FlagDoc("Disable"), $"{access} bool Disable()", $"{flag}.Disable()");

            if (hasSelf && spec.IsValueType == false)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("TryRegister")
                    , $"{access} bool TryRegister([g__SDCA.NotNull] {owner} instance)"
                    , $"{FLAG_EXTENSIONS}.TryRegister({flag}, instance)"
                );

                WriteMethod(
                      ref printer
                    , SelfDoc("TryUnregister")
                    , $"{access} bool TryUnregister({owner} instance)"
                    , $"{FLAG_EXTENSIONS}.TryUnregister({flag}, instance)"
                );
            }

            if (hasSelf && spec.IsValueType)
            {
                WriteMethod(
                      ref printer
                    , SelfDoc("SetValue")
                    , $"{access} void SetValue({owner} value)"
                    , $"{LINK_EXTENSIONS}.SetValue({self}, value)"
                );

                WriteMethod(
                      ref printer
                    , SelfDoc("TryRemoveValue")
                    , $"{access} bool TryRemoveValue(out {owner} value)"
                    , $"{LINK_EXTENSIONS}.TryRemoveValue({self}, out value)"
                );
            }

            if ((api & TypeFlagApi.Related) == 0)
            {
                return;
            }

            var obj = names.Object;
            var value = names.Value;
            var objectLink = $"default(g__ETTF.TypeFlagLink<{owner}, {obj}>)";
            var valueLink = $"default(g__ETTF.TypeFlagLink<{owner}, {value}>)";

            WriteMethod(
                  ref printer
                , ObjectDoc("TryAddObject")
                , $"{access} bool TryAddObject<{obj}>([g__SDCA.NotNull] {obj} obj)"
                , $"where {obj} : class"
                , $"{LINK_EXTENSIONS}.TryAddObject({objectLink}, obj)"
            );

            WriteMethod(
                  ref printer
                , ObjectDoc("TryRemoveObject")
                , $"{access} bool TryRemoveObject<{obj}>({obj} expected)"
                , $"where {obj} : class"
                , $"{LINK_EXTENSIONS}.TryRemoveObject({objectLink}, expected)"
            );

            WriteMethod(
                  ref printer
                , ValueDoc("SetValue")
                , $"{access} void SetValue<{value}>({value} value)"
                , $"where {value} : struct"
                , $"{LINK_EXTENSIONS}.SetValue({valueLink}, value)"
            );

            WriteMethod(
                  ref printer
                , ValueDoc("TryRemoveValue")
                , $"{access} bool TryRemoveValue<{value}>(out {value} value)"
                , $"where {value} : struct"
                , $"{LINK_EXTENSIONS}.TryRemoveValue({valueLink}, out value)"
            );
        }

        private static void WriteProperty(
              ref Printer printer
            , string documentation
            , string signature
            , string body
            , bool separate
        )
        {
            if (separate)
            {
                printer.PrintEndLine();
            }

            printer.PrintLine($"/// <inheritdoc cref=\"{documentation}\"/>");
            printer.PrintLine(signature);
            printer.OpenScope();
            {
                printer.PrintLine(INLINE);
                printer.PrintLine($"get => {body};");
            }
            printer.CloseScope();
        }

        private static void WriteMethod(ref Printer printer, string documentation, string signature, string body)
        {
            printer.PrintEndLine();
            printer.PrintLine($"/// <inheritdoc cref=\"{documentation}\"/>");
            printer.PrintLine(INLINE);
            printer.PrintLine(signature);
            printer.WithIncreasedIndent().PrintLine($"=> {body};");
        }

        private static void WriteMethod(
              ref Printer printer
            , string documentation
            , string signature
            , string constraint
            , string body
        )
        {
            printer.PrintEndLine();
            printer.PrintLine($"/// <inheritdoc cref=\"{documentation}\"/>");
            printer.PrintLine(INLINE);
            printer.PrintLine(signature);

            var bodyPrinter = printer.WithIncreasedIndent();
            bodyPrinter.PrintLine(constraint);
            bodyPrinter.PrintLine($"=> {body};");
        }

        private static string FlagDoc(string member)
            => $"g__ETTF.TypeFlag{{T}}.{member}";

        private static string SelfDoc(string member)
            => $"{FLAG_EXTENSIONS}.{member}{{T}}";

        private static string ObjectDoc(string member)
            => $"{LINK_EXTENSIONS}.{member}{{TOwner, TObject}}";

        private static string ValueDoc(string member)
            => $"{LINK_EXTENSIONS}.{member}{{TOwner, TValue}}";

        private static string GetFieldModifiers(string accessibility, bool hide)
            => hide ? $"{accessibility} static new readonly" : $"{accessibility} static readonly";

        private static bool IsHidden(in TypeFlagSpec spec, TypeFlagMember member)
            => (spec.HiddenMembers & member) != 0;

        private static string GetFreeName(string name, EquatableArray<TypeFlagSpec.Declaration> declarations)
        {
            while (IsDeclared(name, declarations))
            {
                name += "_";
            }

            return name;
        }

        private static bool IsDeclared(string name, EquatableArray<TypeFlagSpec.Declaration> declarations)
        {
            foreach (var declaration in declarations)
            {
                var parameters = declaration.TypeParameterNames.Split(',');

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (string.Equals(parameters[i], name, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private readonly record struct TypeParameterNames(string Object, string Value);

        private readonly record struct StructDeclaration(
              string Accessibility
            , string Name
            , bool IsHidden
            , bool HasWriteMembers
            , string WriteAccess
        );
    }
}
