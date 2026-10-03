namespace EncosyTower.PubSub.Generators
{
    internal readonly struct PubSubScopeSpec : IEquatable<PubSubScopeSpec>
    {
        public readonly PubSubTypeDeclarationSpec Declaration;
        public readonly string ScopeTypeName;
        public readonly string HintName;
        public readonly bool CanPublishParameterless;
        public readonly bool WithSync;
        public readonly bool WithAsync;
        public readonly bool WithStateless;
        public readonly bool WithStateful;
        public readonly bool IsGlobalScope;
        public readonly bool IsUnityScope;
        public readonly string Discriminator;

        public PubSubScopeSpec(
              PubSubTypeDeclarationSpec declaration
            , string scopeTypeName
            , string hintName
            , bool canPublishParameterless
            , bool withSync
            , bool withAsync
            , bool withStateless
            , bool withStateful
            , bool isGlobalScope
            , bool isUnityScope
            , string discriminator = null
        )
        {
            Declaration = declaration;
            ScopeTypeName = scopeTypeName;
            HintName = hintName;
            CanPublishParameterless = canPublishParameterless;
            WithSync = withSync;
            WithAsync = withAsync;
            WithStateless = withStateless;
            WithStateful = withStateful;
            IsGlobalScope = isGlobalScope;
            IsUnityScope = isUnityScope;
            Discriminator = discriminator;
        }

        public bool IsValid => Declaration.IsValid && string.IsNullOrEmpty(ScopeTypeName) == false;

        public static bool operator ==(PubSubScopeSpec left, PubSubScopeSpec right)
            => left.Equals(right);

        public static bool operator !=(PubSubScopeSpec left, PubSubScopeSpec right)
            => left.Equals(right) == false;

        public readonly bool Equals(PubSubScopeSpec other)
            => Declaration.Equals(other.Declaration)
            && string.Equals(ScopeTypeName, other.ScopeTypeName, StringComparison.Ordinal)
            && CanPublishParameterless == other.CanPublishParameterless
            && WithSync == other.WithSync
            && WithAsync == other.WithAsync
            && WithStateless == other.WithStateless
            && WithStateful == other.WithStateful
            && IsGlobalScope == other.IsGlobalScope
            && IsUnityScope == other.IsUnityScope
            && string.Equals(Discriminator, other.Discriminator, StringComparison.Ordinal)
            ;

        public readonly override bool Equals(object obj)
            => obj is PubSubScopeSpec other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash = hash.Add(Declaration);
            hash = hash.Add(ScopeTypeName);
            hash = hash.Add(CanPublishParameterless);
            hash = hash.Add(WithSync);
            hash = hash.Add(WithAsync);
            hash = hash.Add(WithStateless);
            hash = hash.Add(WithStateful);
            hash = hash.Add(IsGlobalScope);
            hash = hash.Add(IsUnityScope);
            hash = hash.Add(Discriminator);
            return hash.ToHashCode();
        }
    }
}
