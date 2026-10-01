namespace EncosyTower.Mvvm.Generators.RelayCommands
{
    public partial struct RelayCommandSpec : IEquatable<RelayCommandSpec>
    {
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";

        public string className;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public EquatableArray<MemberSpec> memberRefs;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(hintName) == false
            && memberRefs.Count > 0;

        public readonly bool Equals(RelayCommandSpec other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && memberRefs.Equals(other.memberRefs);

        public readonly override bool Equals(object obj)
            => obj is RelayCommandSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(className, hintName, openingSource, closingSource, memberRefs);

        public static RelayCommandSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol classSymbol
            )
            {
                return default;
            }

            var classNameBuilder = new StringBuilder(classSyntax.Identifier.Text);

            if (classSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                token.ThrowIfCancellationRequested();

                classNameBuilder.Append("<");

                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    token.ThrowIfCancellationRequested();

                    classNameBuilder.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        classNameBuilder.Append(", ");
                    }
                }

                classNameBuilder.Append(">");
            }

            var className = classNameBuilder.ToString();
            var semanticModel = context.SemanticModel;
            var hintName = classSymbol.ToMetadataName();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            token.ThrowIfCancellationRequested();

            using var memberRefsBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();

            var members = classSymbol.GetMembers();
            var methodMap = new Dictionary<string, IMethodSymbol>();
            var methodCandidates = new List<(IMethodSymbol method, string canExecuteName)>();

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method || method.Parameters.Length > 1)
                {
                    continue;
                }

                if (method.Parameters.Length == 1)
                {
                    var parameter = method.Parameters[0];

                    if (parameter.RefKind is not (RefKind.None or RefKind.In))
                    {
                        continue;
                    }
                }

                token.ThrowIfCancellationRequested();

                var relayCommandAttrib = method.GetAttribute(RELAY_COMMAND_ATTRIBUTE, token);

                if (relayCommandAttrib != null)
                {
                    if (relayCommandAttrib.NamedArguments.Length > 0)
                    {
                        foreach (var kv in relayCommandAttrib.NamedArguments)
                        {
                            token.ThrowIfCancellationRequested();

                            if (kv.Key == "CanExecute"
                                && kv.Value.Value is string canExecuteMethodName
                                && canExecuteMethodName != method.Name
                            )
                            {
                                methodCandidates.Add((method, canExecuteMethodName));
                            }
                        }
                    }
                    else
                    {
                        memberRefsBuilder.Add(new MemberSpec {
                            methodName = method.Name,
                            paramTypeName = method.Parameters.Length > 0
                                ? method.Parameters[0].Type.ToFullName()
                                : null,
                        });
                    }
                }

                methodMap[method.Name] = method;
            }

            token.ThrowIfCancellationRequested();

            var filtered = new HashSet<string>();

            foreach (var (method, canExecuteName) in methodCandidates)
            {
                token.ThrowIfCancellationRequested();

                filtered.Clear();

                if (methodMap.TryGetValue(canExecuteName, out var canExecuteMethod) == false
                    || canExecuteMethod.ReturnType.SpecialType != SpecialType.System_Boolean
                )
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                var isValid = true;

                if (canExecuteMethod.Parameters.Length != 0)
                {
                    if (canExecuteMethod.Parameters.Length != method.Parameters.Length)
                    {
                        continue;
                    }

                    foreach (var param in method.Parameters)
                    {
                        token.ThrowIfCancellationRequested();

                        filtered.Add(param.Type.ToFullName());
                    }

                    foreach (var param in canExecuteMethod.Parameters)
                    {
                        token.ThrowIfCancellationRequested();

                        if (filtered.Contains(param.Type.ToFullName()) == false)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    memberRefsBuilder.Add(new MemberSpec {
                        methodName = method.Name,
                        paramTypeName = method.Parameters.Length > 0 ? method.Parameters[0].Type.ToFullName() : null,
                        canExecuteMethodName = canExecuteMethod.Name,
                        canExecuteHasParam = canExecuteMethod.Parameters.Length > 0,
                    });
                }
            }

            token.ThrowIfCancellationRequested();

            if (memberRefsBuilder.Count == 0)
            {
                return default;
            }

            var pendingRefs = memberRefsBuilder.ToImmutable();
            using var finalBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();

            foreach (var memberRef in pendingRefs)
            {
                token.ThrowIfCancellationRequested();

                if (methodMap.TryGetValue(memberRef.methodName, out var method) == false)
                {
                    continue;
                }

                method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , out var fieldAttributes
                    , out var propertyAttributes
                );

                finalBuilder.Add(new MemberSpec {
                    methodName = memberRef.methodName,
                    paramTypeName = memberRef.paramTypeName,
                    canExecuteMethodName = memberRef.canExecuteMethodName,
                    canExecuteHasParam = memberRef.canExecuteHasParam,
                    forwardedFieldAttributes = fieldAttributes.AsEquatableArray(),
                    forwardedPropertyAttributes = propertyAttributes.AsEquatableArray(),
                });
            }

            if (finalBuilder.Count == 0)
            {
                return default;
            }

            return new RelayCommandSpec {
                className = className,
                hintName = hintName,
                openingSource = openingSource,
                closingSource = closingSource,
                memberRefs = finalBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SCM = global::System.ComponentModel;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__ETMI = global::EncosyTower.Mvvm.Input;");
            p.PrintLine("using g__ETMISG = global::EncosyTower.Mvvm.Input.SourceGen;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public struct MemberSpec : IEquatable<MemberSpec>
        {
            public string methodName;
            public string paramTypeName;
            public string canExecuteMethodName;
            public bool canExecuteHasParam;
            public EquatableArray<AttributeInfo> forwardedFieldAttributes;
            public EquatableArray<AttributeInfo> forwardedPropertyAttributes;

            public readonly bool Equals(MemberSpec other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramTypeName, other.paramTypeName, StringComparison.Ordinal)
                && string.Equals(canExecuteMethodName, other.canExecuteMethodName, StringComparison.Ordinal)
                && canExecuteHasParam == other.canExecuteHasParam
                && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes)
                && forwardedPropertyAttributes.Equals(other.forwardedPropertyAttributes);

            public readonly override bool Equals(object obj)
                => obj is MemberSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(methodName);
                hash.Add(paramTypeName);
                hash.Add(canExecuteMethodName);
                hash.Add(canExecuteHasParam);
                hash.Add(forwardedFieldAttributes);
                hash.Add(forwardedPropertyAttributes);
                return hash.ToHashCode();
            }
        }
    }
}
