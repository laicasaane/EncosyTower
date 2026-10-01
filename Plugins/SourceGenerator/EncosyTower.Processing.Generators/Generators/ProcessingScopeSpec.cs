namespace EncosyTower.Processing.Generators
{
    internal readonly struct ProcessingScopeSpec : IEquatable<ProcessingScopeSpec>
    {
        public readonly ProcessingTypeDeclarationSpec Declaration;
        public readonly string ResultTypeName;
        public readonly string ScopeTypeName;
        public readonly string HintName;
        public readonly bool HasResult;
        public readonly bool WithSync;
        public readonly bool WithAsync;
        public readonly bool WithStateless;
        public readonly bool WithStateful;
        public readonly bool IsGlobalScope;
        public readonly bool IsUnityScope;

        public ProcessingScopeSpec(
              ProcessingTypeDeclarationSpec declaration
            , string resultTypeName
            , string scopeTypeName
            , string hintName
            , bool hasResult
            , bool withSync
            , bool withAsync
            , bool withStateless
            , bool withStateful
            , bool isGlobalScope
            , bool isUnityScope
        )
        {
            Declaration = declaration;
            ResultTypeName = resultTypeName;
            ScopeTypeName = scopeTypeName;
            HintName = hintName;
            HasResult = hasResult;
            WithSync = withSync;
            WithAsync = withAsync;
            WithStateless = withStateless;
            WithStateful = withStateful;
            IsGlobalScope = isGlobalScope;
            IsUnityScope = isUnityScope;
        }

        public bool IsValid => Declaration.IsValid && string.IsNullOrEmpty(ScopeTypeName) == false;

        public static bool operator ==(ProcessingScopeSpec left, ProcessingScopeSpec right)
            => left.Equals(right);

        public static bool operator !=(ProcessingScopeSpec left, ProcessingScopeSpec right)
            => left.Equals(right) == false;

        public readonly bool Equals(ProcessingScopeSpec other)
            => Declaration.Equals(other.Declaration)
            && string.Equals(ResultTypeName, other.ResultTypeName, StringComparison.Ordinal)
            && string.Equals(ScopeTypeName, other.ScopeTypeName, StringComparison.Ordinal)
            && string.Equals(HintName, other.HintName, StringComparison.Ordinal)
            && HasResult == other.HasResult
            && WithSync == other.WithSync
            && WithAsync == other.WithAsync
            && WithStateless == other.WithStateless
            && WithStateful == other.WithStateful
            && IsGlobalScope == other.IsGlobalScope
            && IsUnityScope == other.IsUnityScope
            ;

        public readonly override bool Equals(object obj)
            => obj is ProcessingScopeSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(Declaration);
            hash = hash.Add(ResultTypeName);
            hash = hash.Add(ScopeTypeName);
            hash = hash.Add(HintName);
            hash = hash.Add(HasResult);
            hash = hash.Add(WithSync);
            hash = hash.Add(WithAsync);
            hash = hash.Add(WithStateless);
            hash = hash.Add(WithStateful);
            hash = hash.Add(IsGlobalScope);
            hash = hash.Add(IsUnityScope);
            return hash.ToHashCode();
        }
    }
}
