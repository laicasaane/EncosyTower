namespace EncosyTower.Core.PolyEnumFactories
{
    internal readonly struct FactoryOutputScopeSpec : IEquatable<FactoryOutputScopeSpec>
    {
        public readonly string TypeName;
        public readonly string MetadataName;
        public readonly string NamespaceName;
        public readonly EquatableArray<FactoryOutputTypeSpec> Parents;
        public readonly FactoryOutputTypeSpec Declaration;

        public FactoryOutputScopeSpec(
              string typeName
            , string metadataName
            , string namespaceName
            , EquatableArray<FactoryOutputTypeSpec> parents
            , FactoryOutputTypeSpec declaration
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

        public void WriteOpening(
              ref Printer p
            , PrinterAction printAdditionalUsings
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            printAdditionalUsings?.Invoke(ref p);
            token.ThrowIfCancellationRequested();

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

        public bool Equals(FactoryOutputScopeSpec other)
            => string.Equals(TypeName, other.TypeName, StringComparison.Ordinal)
            && string.Equals(MetadataName, other.MetadataName, StringComparison.Ordinal)
            && string.Equals(NamespaceName, other.NamespaceName, StringComparison.Ordinal)
            && Parents.Equals(other.Parents)
            && Declaration.Equals(other.Declaration);

        public override bool Equals(object obj)
            => obj is FactoryOutputScopeSpec other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(TypeName, MetadataName, NamespaceName, Parents, Declaration);
    }

    internal readonly struct FactoryOutputTypeSpec : IEquatable<FactoryOutputTypeSpec>
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

        public FactoryOutputTypeSpec(
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
                p.PrintLine(Constraints);
            }
        }

        public bool Equals(FactoryOutputTypeSpec other)
            => string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(Keyword, other.Keyword, StringComparison.Ordinal)
            && string.Equals(Accessibility, other.Accessibility, StringComparison.Ordinal)
            && string.Equals(TypeParameters, other.TypeParameters, StringComparison.Ordinal)
            && string.Equals(Constraints, other.Constraints, StringComparison.Ordinal)
            && IsStatic == other.IsStatic
            && IsReadOnly == other.IsReadOnly
            && IsRef == other.IsRef
            && IsRecord == other.IsRecord;

        public override bool Equals(object obj)
            => obj is FactoryOutputTypeSpec other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(Name);
            hash.Add(Keyword);
            hash.Add(Accessibility);
            hash.Add(TypeParameters);
            hash.Add(Constraints);
            hash.Add(IsStatic);
            hash.Add(IsReadOnly);
            hash.Add(IsRef);
            hash.Add(IsRecord);
            return hash.ToHashCode();
        }
    }

    internal static class FactoryOutputScopeSpecFactory
    {
        public static FactoryOutputScopeSpec CreateSource(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(type, token);
            using var parents = ImmutableArrayBuilder<FactoryOutputTypeSpec>.Rent();

            for (var i = 0; i < chain.Count - 1; i++)
            {
                token.ThrowIfCancellationRequested();
                parents.Add(CreateSourceType(chain[i], token));
            }

            return Create(type, chain, parents.ToImmutable(), CreateSourceType(type, token), token);
        }

        public static FactoryOutputScopeSpec CreateCompanion(INamedTypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var chain = GetTypeChain(type, token);
            using var parents = ImmutableArrayBuilder<FactoryOutputTypeSpec>.Rent();
            var mirrored = false;

            for (var i = 0; i < chain.Count - 1; i++)
            {
                token.ThrowIfCancellationRequested();
                mirrored |= chain[i].Arity > 0;
                parents.Add(mirrored
                    ? CreateMirror(chain[i], isTopLevel: i == 0, token)
                    : CreateSourceType(chain[i], token));
            }

            return Create(
                  type
                , chain
                , parents.ToImmutable()
                , CreateMirror(type, isTopLevel: chain.Count == 1, token)
                , token
            );
        }

        private static FactoryOutputScopeSpec Create(
              INamedTypeSymbol type
            , IReadOnlyList<INamedTypeSymbol> chain
            , EquatableArray<FactoryOutputTypeSpec> parents
            , FactoryOutputTypeSpec declaration
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var names = new List<string>();

            foreach (var current in chain)
            {
                token.ThrowIfCancellationRequested();
                names.Add(current.Name);
            }

            var namespaceName = type.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : type.ContainingNamespace.ToDisplayString();
            var typePrefix = string.IsNullOrEmpty(namespaceName) ? "global::" : $"global::{namespaceName}.";
            var metadataPrefix = string.IsNullOrEmpty(namespaceName) ? string.Empty : $"{namespaceName}.";
            return new FactoryOutputScopeSpec(
                  $"{typePrefix}{string.Join(".", names)}"
                , $"{metadataPrefix}{string.Join("+", names)}"
                , namespaceName
                , parents
                , declaration
                , token
            );
        }

        private static FactoryOutputTypeSpec CreateSourceType(
              INamedTypeSymbol type
            , CancellationToken token
        )
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
            return new FactoryOutputTypeSpec(
                  syntax.Identifier.Text
                , keyword
                , GetAccessibility(type.DeclaredAccessibility, token)
                , syntax.TypeParameterList?.ToString() ?? string.Empty
                , syntax.ConstraintClauses.ToString()
                , type.IsStatic
                , type.IsReadOnly
                , type.IsRefLikeType
                , type.IsRecord
                , token
            );
        }

        private static FactoryOutputTypeSpec CreateMirror(
              INamedTypeSymbol type
            , bool isTopLevel
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            return new FactoryOutputTypeSpec(
                  type.Name
                , "class"
                , isTopLevel ? IsPublic(type, token) ? "public" : "internal" : "public"
                , string.Empty
                , string.Empty
                , true
                , false
                , false
                , false
                , token
            );
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

        private static bool IsPublic(INamedTypeSymbol type, CancellationToken token)
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

            foreach (var reference in type.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (reference.GetSyntax(token) is TypeDeclarationSyntax syntax)
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
