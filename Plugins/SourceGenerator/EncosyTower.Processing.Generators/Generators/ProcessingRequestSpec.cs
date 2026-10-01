namespace EncosyTower.Processing.Generators
{
    internal readonly partial struct ProcessingRequestSpec : IEquatable<ProcessingRequestSpec>
    {
        public readonly ProcessingTypeDeclarationSpec Declaration;
        public readonly EquatableArray<ProcessingScopeSpec> Scopes;
        public readonly string ResultTypeName;
        public readonly string HintName;
        public readonly bool HasResult;
        public readonly bool GenerateRequestInterface;
        public readonly bool WithAsync;

        public ProcessingRequestSpec(
              ProcessingTypeDeclarationSpec declaration
            , EquatableArray<ProcessingScopeSpec> scopes
            , string resultTypeName
            , string hintName
            , bool hasResult
            , bool generateRequestInterface
            , bool withAsync
        )
        {
            Declaration = declaration;
            Scopes = scopes;
            ResultTypeName = resultTypeName;
            HintName = hintName;
            HasResult = hasResult;
            GenerateRequestInterface = generateRequestInterface;
            WithAsync = withAsync;
        }

        public bool IsValid => Declaration.IsValid && string.IsNullOrEmpty(HintName) == false;

        public static bool operator ==(ProcessingRequestSpec left, ProcessingRequestSpec right)
            => left.Equals(right);

        public static bool operator !=(ProcessingRequestSpec left, ProcessingRequestSpec right)
            => left.Equals(right) == false;

        public readonly ProcessingRequestSpec ToRequestOutput()
            => new(
                  Declaration
                , default
                , ResultTypeName
                , HintName
                , HasResult
                , GenerateRequestInterface
                , WithAsync
            );

        public readonly bool Equals(ProcessingRequestSpec other)
            => Declaration.Equals(other.Declaration)
            && Scopes.Equals(other.Scopes)
            && string.Equals(ResultTypeName, other.ResultTypeName, StringComparison.Ordinal)
            && string.Equals(HintName, other.HintName, StringComparison.Ordinal)
            && HasResult == other.HasResult
            && GenerateRequestInterface == other.GenerateRequestInterface
            && WithAsync == other.WithAsync
            ;

        public readonly override bool Equals(object obj)
            => obj is ProcessingRequestSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(Declaration);

            var scopeCount = Scopes.Count;

            for (var i = 0; i < scopeCount; i++)
            {
                hash = hash.Add(Scopes[i]);
            }

            hash = hash.Add(Scopes.Count);
            hash = hash.Add(ResultTypeName);
            hash = hash.Add(HintName);
            hash = hash.Add(HasResult);
            hash = hash.Add(GenerateRequestInterface);
            hash = hash.Add(WithAsync);
            return hash.ToHashCode();
        }
    }
}
