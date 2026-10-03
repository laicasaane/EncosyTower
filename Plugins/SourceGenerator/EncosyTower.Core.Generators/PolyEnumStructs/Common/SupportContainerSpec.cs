using EncosyTower.SourceGen.Helpers.PolyEnumStructs;

namespace EncosyTower.Core.PolyEnumStructs
{
    internal readonly struct SupportContainerSpec : IEquatable<SupportContainerSpec>
    {
        public readonly string TypeName;
        public readonly string MetadataName;
        public readonly string NamespaceName;
        public readonly EquatableArray<SupportTypeSpec> Parents;
        public readonly SupportTypeSpec Declaration;

        public SupportContainerSpec(
              string typeName
            , string metadataName
            , string namespaceName
            , EquatableArray<SupportTypeSpec> parents
            , SupportTypeSpec declaration
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            TypeName = typeName;
            MetadataName = metadataName;
            NamespaceName = namespaceName;
            Parents = parents;
            Declaration = declaration;
        }

        public bool IsValid(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return string.IsNullOrEmpty(TypeName) == false && Declaration.IsValid(token);
        }

        public void WriteOpening(
              ref Printer p
            , PrinterAction printAdditionalUsings
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            printAdditionalUsings?.Invoke(ref p);

            if (string.IsNullOrEmpty(NamespaceName) == false)
            {
                p.PrintBeginLine("namespace ").Print(NamespaceName).PrintEndLine();
                p.OpenScope();
            }

            foreach (var parent in Parents.AsReadOnlySpan())
            {
                token.ThrowIfCancellationRequested();
                parent.WriteDeclaration(ref p, token);
                p.OpenScope();
            }
        }

        public void WriteClosing(ref Printer p, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            for (var i = Parents.Count - 1; i >= 0; i--)
            {
                token.ThrowIfCancellationRequested();
                p.CloseScope();
            }

            if (string.IsNullOrEmpty(NamespaceName) == false)
            {
                p.CloseScope();
            }
        }

        public bool Equals(SupportContainerSpec other)
        {
            return string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
                && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
                && string.Equals(NamespaceName, other.NamespaceName, StringComparison.Ordinal)
                && Parents.Equals(other.Parents)
                && Declaration.Equals(other.Declaration);
        }

        public override bool Equals(object obj)
            => obj is SupportContainerSpec other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(TypeName, MetadataName, NamespaceName, Parents, Declaration);
    }

    internal readonly struct SupportTypeSpec : IEquatable<SupportTypeSpec>
    {
        public readonly string Name;
        public readonly string Keyword;
        public readonly string Accessibility;
        public readonly string TypeParameters;
        public readonly string Constraints;
        public readonly bool IsStatic;
        public readonly bool IsReadOnly;
        public readonly bool IsRef;
        public readonly bool IsRecord;

        public SupportTypeSpec(
              string name
            , string keyword
            , string accessibility
            , string typeParameters
            , string constraints
            , bool isStatic
            , bool isReadOnly
            , bool isRef
            , bool isRecord
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            Name = name;
            Keyword = keyword;
            Accessibility = accessibility;
            TypeParameters = typeParameters;
            Constraints = constraints;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
            IsRef = isRef;
            IsRecord = isRecord;
        }

        public bool IsValid(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return string.IsNullOrEmpty(Name) == false && string.IsNullOrEmpty(Keyword) == false;
        }

        public void WriteDeclaration(ref Printer p, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            p.PrintBeginLine();

            if (string.IsNullOrEmpty(Accessibility) == false)
            {
                p.Print(Accessibility).Print(" ");
            }

            p.PrintIf(IsStatic, "static ")
                .PrintIf(IsReadOnly, "readonly ")
                .PrintIf(IsRef, "ref ")
                .Print("partial ")
                .PrintIf(IsRecord, "record ")
                .Print(Keyword).Print(" ").Print(Name).PrintEndLine(TypeParameters);

            if (string.IsNullOrEmpty(Constraints) == false)
            {
                foreach (var constraint in Constraints.Split('\n'))
                {
                    p.PrintLine(constraint);
                }
            }
        }

        public bool Equals(SupportTypeSpec other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal)
                && string.Equals(Keyword, other.Keyword, StringComparison.Ordinal)
                && string.Equals(Accessibility, other.Accessibility, StringComparison.Ordinal)
                && string.Equals(TypeParameters, other.TypeParameters, StringComparison.Ordinal)
                && string.Equals(Constraints, other.Constraints, StringComparison.Ordinal)
                && IsStatic == other.IsStatic
                && IsReadOnly == other.IsReadOnly
                && IsRef == other.IsRef
                && IsRecord == other.IsRecord;
        }

        public override bool Equals(object obj)
            => obj is SupportTypeSpec other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(Name);
            hash = hash.Add(Keyword);
            hash = hash.Add(Accessibility);
            hash = hash.Add(TypeParameters);
            hash = hash.Add(Constraints);
            hash = hash.Add(IsStatic);
            hash = hash.Add(IsReadOnly);
            hash = hash.Add(IsRef);
            hash = hash.Add(IsRecord);
            return hash.ToHashCode();
        }
    }

    internal static class SupportContainerSpecFactory
    {
        public static SupportContainerSpec CreateCustom(INamedTypeSymbol container, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(container, token);
            using var parents = ImmutableArrayBuilder<SupportTypeSpec>.Rent();

            for (var i = 0; i < chain.Count - 1; i++)
            {
                token.ThrowIfCancellationRequested();
                parents.Add(CreateSourceType(chain[i], token));
            }

            return new SupportContainerSpec(
                  container.ToFullName()
                , container.ToMetadataName()
                , GetNamespaceName(container, token)
                , parents.ToImmutable()
                , CreateSourceType(container, token)
                , token
            );
        }

        public static SupportContainerSpec CreateDefault(INamedTypeSymbol target, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(target, token);
            using var parents = ImmutableArrayBuilder<SupportTypeSpec>.Rent();
            var mirrored = false;

            for (var i = 0; i < chain.Count - 1; i++)
            {
                token.ThrowIfCancellationRequested();
                mirrored |= chain[i].Arity > 0;
                parents.Add(mirrored
                    ? CreateMirror(chain[i], isTopLevel: i == 0, token)
                    : CreateSourceType(chain[i], token));
            }

            var names = new List<string>();

            foreach (var type in chain)
            {
                token.ThrowIfCancellationRequested();
                names.Add(type.Name);
            }

            var typePath = string.Join(".", names);
            var namespaceName = GetNamespaceName(target, token);
            var namespacePrefix = string.IsNullOrEmpty(namespaceName) ? "global::" : $"global::{namespaceName}.";
            var metadataPrefix = string.IsNullOrEmpty(namespaceName) ? string.Empty : $"{namespaceName}.";

            return new SupportContainerSpec(
                  $"{namespacePrefix}{typePath}"
                , $"{metadataPrefix}{string.Join("+", names)}"
                , namespaceName
                , parents.ToImmutable()
                , CreateMirror(target, isTopLevel: chain.Count == 1, token)
                , token
            );
        }

        private static SupportTypeSpec CreateSourceType(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var syntax = GetTypeSyntax(type, token);

            if (syntax is null)
            {
                return default;
            }

            var keyword = syntax is RecordDeclarationSyntax record
                ? record.ClassOrStructKeyword.Text
                : syntax.Keyword.Text;
            return new SupportTypeSpec(
                  syntax.Identifier.Text
                , keyword
                , GetAccessibility(type.DeclaredAccessibility, token)
                , syntax.TypeParameterList?.ToString() ?? string.Empty
                , ContainerResolver.FormatConstraintClauses(type, token)
                , type.IsStatic
                , type.IsReadOnly
                , type.IsRefLikeType
                , type.IsRecord
                , token
            );
        }

        private static SupportTypeSpec CreateMirror(
              INamedTypeSymbol type
            , bool isTopLevel
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var accessibility = isTopLevel
                ? IsEffectivelyPublic(type, token) ? "public" : "internal"
                : "public";
            return new SupportTypeSpec(
                  type.Name
                , "class"
                , accessibility
                , string.Empty
                , string.Empty
                , isStatic: true
                , isReadOnly: false
                , isRef: false
                , isRecord: false
                , token
            );
        }

        private static string GetNamespaceName(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return type.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : type.ContainingNamespace.ToDisplayString();
        }

        private static string GetAccessibility(Accessibility accessibility, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => string.Empty,
            };
        }

        private static bool IsEffectivelyPublic(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            for (var current = type; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();

                if (current.DeclaredAccessibility != Accessibility.Public)
                {
                    return false;
                }
            }

            return true;
        }

        private static TypeDeclarationSyntax GetTypeSyntax(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var syntaxReference in type.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (syntaxReference.GetSyntax(token) is TypeDeclarationSyntax syntax)
                {
                    return syntax;
                }
            }

            return null;
        }

        private static List<INamedTypeSymbol> GetTypeChain(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var result = new List<INamedTypeSymbol>();

            for (var current = type; current is not null; current = current.ContainingType)
            {
                token.ThrowIfCancellationRequested();
                result.Add(current);
            }

            result.Reverse();
            return result;
        }
    }
}
