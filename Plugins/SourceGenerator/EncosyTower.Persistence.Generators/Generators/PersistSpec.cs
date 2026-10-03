using EncosyTower.SourceGen.Data.Helpers;
using static EncosyTower.Persistence.Generators.Helpers;

namespace EncosyTower.Persistence.Generators
{
    internal partial struct PersistSpec : IEquatable<PersistSpec>
    {
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string typeName;
        public string typeFullName;
        public string typeKeyword;
        public bool generateInterface;
        public MemberDefinition memberId;
        public MemberDefinition memberVersion;
        public EquatableArray<ContainingTypeSpec> containingTypes;

        public readonly bool IsValid => string.IsNullOrEmpty(typeName) == false;

        public static PersistSpec Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
                || context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
                || symbol.TypeKind is not (TypeKind.Class or TypeKind.Struct)
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;

            GetMemberDefinitions(symbol, semanticModel, token, out var memberId, out var memberVersion);

            var generateInterface = symbol.InheritsFromInterface(IPERSIST, false, token) == false;

            if (generateInterface == false
                && memberId.ShouldGenerate == false
                && memberVersion.ShouldGenerate == false
            )
            {
                return default;
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var syntaxTree = syntax.SyntaxTree;
            var fileTypeName = symbol.ToFileName();
            var hintName = symbol.ToMetadataName();

            return new PersistSpec {
                typeName = symbol.Name,
                typeFullName = symbol.ToFullName(),
                typeKeyword = symbol.ToPartialTypeKeyword(),
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                generateInterface = generateInterface,
                memberId = memberId,
                memberVersion = memberVersion,
                containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(syntax, token),
            };
        }

        private static void GetMemberDefinitions(
              INamedTypeSymbol symbol
            , SemanticModel semanticModel
            , CancellationToken token
            , out MemberDefinition memberId
            , out MemberDefinition memberVersion
        )
        {
            token.ThrowIfCancellationRequested();

            memberId = memberVersion = default;

            GetMembers(symbol, false, semanticModel, token, ref memberId, ref memberVersion);

            if (HasBoth(memberId, memberVersion))
            {
                return;
            }

            var baseType = symbol.BaseType;

            while (baseType is { TypeKind: TypeKind.Class })
            {
                token.ThrowIfCancellationRequested();

                if (IsPersistTarget(baseType, token))
                {
                    if (memberId.IsValid == false)
                    {
                        memberId = new MemberDefinition {
                            name = "Id",
                            isField = false,
                            type = MemberDefinitionType.DefinedInBaseType,
                        };
                    }

                    if (memberVersion.IsValid == false)
                    {
                        memberVersion = new MemberDefinition {
                            name = "Version",
                            isField = false,
                            type = MemberDefinitionType.DefinedInBaseType,
                        };
                    }

                    return;
                }

                GetMembers(baseType, true, semanticModel, token, ref memberId, ref memberVersion);

                if (HasBoth(memberId, memberVersion))
                {
                    return;
                }

                baseType = baseType.BaseType;
            }

            static bool HasBoth(MemberDefinition memberId, MemberDefinition memberVersion)
                => memberId.IsValid && memberVersion.IsValid;

            static bool IsPersistTarget(INamedTypeSymbol type, CancellationToken token)
                => type.DeclaringSyntaxReferences.IsEmpty == false
                    && type.IsAbstract == false
                    && type.Arity == 0
                    && type.HasAttribute(PERSIST_ATTRIBUTE, token);

            static void GetMembers(
                  ITypeSymbol type
                , bool isBaseTypeSearch
                , SemanticModel semanticModel
                , CancellationToken token
                , ref MemberDefinition memberId
                , ref MemberDefinition memberVersion
            )
            {
                var members = type.GetMembers();
                var fields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);

                foreach (var member in members)
                {
                    token.ThrowIfCancellationRequested();

                    if (HasBoth(memberId, memberVersion))
                    {
                        return;
                    }

                    if (member is IFieldSymbol field)
                    {
                        fields.Add(field);
                    }

                    if (member is not IPropertySymbol property)
                    {
                        continue;
                    }

                    if (isBaseTypeSearch)
                    {
                        if (property.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected))
                        {
                            continue;
                        }
                    }
                    else if (property.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    if (memberId.IsValid == false && property.Name == "Id")
                    {
                        memberId = new MemberDefinition {
                            name = property.Name,
                            isField = false,
                            type = isBaseTypeSearch
                                ? property.IsAbstract
                                    ? MemberDefinitionType.DefinedInBaseTypeAsAbstract
                                    : MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                    else if (memberVersion.IsValid == false && property.Name == "Version")
                    {
                        memberVersion = new MemberDefinition {
                            name = property.Name,
                            isField = false,
                            type = isBaseTypeSearch
                                ? property.IsAbstract
                                    ? MemberDefinitionType.DefinedInBaseTypeAsAbstract
                                    : MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                }

                foreach (var field in fields)
                {
                    token.ThrowIfCancellationRequested();

                    if (HasBoth(memberId, memberVersion))
                    {
                        return;
                    }

                    if (isBaseTypeSearch
                        && field.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected)
                    )
                    {
                        continue;
                    }

                    if (memberId.IsValid == false && field.Name is ("id" or "_id" or "m_id"))
                    {
                        field.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , out ImmutableArray<(string, AttributeInfo)> propertyAttributes
                        );

                        using var attrBuilder = ImmutableArrayBuilder<ForwardedAttributeData>.Rent();

                        foreach (var (fullTypeName, attributeInfo) in propertyAttributes)
                        {
                            token.ThrowIfCancellationRequested();

                            attrBuilder.Add(new ForwardedAttributeData {
                                fullTypeName = fullTypeName,
                                syntax = attributeInfo.GetSyntax().ToFullString(),
                            });
                        }

                        memberId = new MemberDefinition {
                            name = field.Name,
                            forwardedAttributes = attrBuilder.ToImmutable().AsEquatableArray(),
                            isField = true,
                            type = isBaseTypeSearch
                                ? MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                    else if (memberVersion.IsValid == false && field.Name is ("version" or "_version" or "m_version"))
                    {
                        field.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , out ImmutableArray<(string, AttributeInfo)> propertyAttributes
                        );

                        using var attrBuilder = ImmutableArrayBuilder<ForwardedAttributeData>.Rent();

                        foreach (var (fullTypeName, attributeInfo) in propertyAttributes)
                        {
                            token.ThrowIfCancellationRequested();

                            attrBuilder.Add(new ForwardedAttributeData {
                                fullTypeName = fullTypeName,
                                syntax = attributeInfo.GetSyntax().ToFullString(),
                            });
                        }

                        memberVersion = new MemberDefinition {
                            name = field.Name,
                            forwardedAttributes = attrBuilder.ToImmutable().AsEquatableArray(),
                            isField = true,
                            type = isBaseTypeSearch
                                ? MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                }
            }
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__ETUV = global::EncosyTower.Persistences;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly bool Equals(PersistSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeKeyword, other.typeKeyword, StringComparison.Ordinal)
            && generateInterface == other.generateInterface
            && memberId.Equals(other.memberId)
            && memberVersion.Equals(other.memberVersion)
            && containingTypes.Equals(other.containingTypes);

        public readonly override bool Equals(object obj)
            => obj is PersistSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeFullName, typeKeyword, generateInterface, memberId, memberVersion)
            .Add(containingTypes);
    }

    internal struct MemberDefinition : IEquatable<MemberDefinition>
    {
        public string name;
        public EquatableArray<ForwardedAttributeData> forwardedAttributes;
        public MemberDefinitionType type;
        public bool isField;

        public readonly bool IsValid => type != MemberDefinitionType.Undefined && string.IsNullOrEmpty(name) == false;

        public readonly bool ShouldGenerate
            => isField || type is MemberDefinitionType.Undefined or MemberDefinitionType.DefinedInBaseTypeAsAbstract;

        public readonly bool Equals(MemberDefinition other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && forwardedAttributes.Equals(other.forwardedAttributes)
            && isField == other.isField
            && type == other.type;

        public readonly override bool Equals(object obj)
            => obj is MemberDefinition other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, forwardedAttributes, type, isField);
    }

    internal enum MemberDefinitionType : byte
    {
        Undefined = 0,
        Defined,
        DefinedInBaseType,
        DefinedInBaseTypeAsAbstract,
    }
}
