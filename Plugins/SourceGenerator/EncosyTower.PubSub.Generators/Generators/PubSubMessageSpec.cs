namespace EncosyTower.PubSub.Generators
{
    internal readonly partial struct PubSubMessageSpec : IEquatable<PubSubMessageSpec>
    {
        public readonly PubSubTypeDeclarationSpec Declaration;
        public readonly EquatableArray<PubSubScopeSpec> Scopes;
        public readonly string HintName;
        public readonly bool CanPublishParameterless;
        public readonly bool WithSync;
        public readonly bool WithAsync;

        public PubSubMessageSpec(
              PubSubTypeDeclarationSpec declaration
            , EquatableArray<PubSubScopeSpec> scopes
            , string hintName
            , bool canPublishParameterless
            , bool withSync
            , bool withAsync
        )
        {
            Declaration = declaration;
            Scopes = scopes;
            HintName = hintName;
            CanPublishParameterless = canPublishParameterless;
            WithSync = withSync;
            WithAsync = withAsync;
        }

        public bool IsValid => Declaration.IsValid && string.IsNullOrEmpty(HintName) == false;

        public static bool operator ==(PubSubMessageSpec left, PubSubMessageSpec right)
            => left.Equals(right);

        public static bool operator !=(PubSubMessageSpec left, PubSubMessageSpec right)
            => left.Equals(right) == false;

        public readonly PubSubMessageSpec ToMessageOutput()
            => new(
                  Declaration
                , default
                , HintName
                , CanPublishParameterless
                , WithSync
                , WithAsync
            );

        public readonly bool Equals(PubSubMessageSpec other)
            => Declaration.Equals(other.Declaration)
            && Scopes.Equals(other.Scopes)
            && CanPublishParameterless == other.CanPublishParameterless
            && WithSync == other.WithSync
            && WithAsync == other.WithAsync
            ;

        public readonly override bool Equals(object obj)
            => obj is PubSubMessageSpec other && Equals(other);

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
            hash = hash.Add(CanPublishParameterless);
            hash = hash.Add(WithSync);
            hash = hash.Add(WithAsync);
            return hash.ToHashCode();
        }
    }
}
