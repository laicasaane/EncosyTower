using System;

namespace EncosyTower.SourceGen
{
    public readonly struct UnmanagedSizeUpperBoundResult : IEquatable<UnmanagedSizeUpperBoundResult>
    {
        public readonly bool isKnown;
        public readonly int sizeUpperBound;
        public readonly UnmanagedSizeUpperBoundFailure failure;

        public UnmanagedSizeUpperBoundResult(
              bool isKnown
            , int sizeUpperBound
            , UnmanagedSizeUpperBoundFailure failure
        )
        {
            this.isKnown = isKnown;
            this.sizeUpperBound = sizeUpperBound;
            this.failure = failure;
        }

        public bool Equals(UnmanagedSizeUpperBoundResult other)
            => isKnown == other.isKnown
            && sizeUpperBound == other.sizeUpperBound
            && failure == other.failure;

        public override bool Equals(object obj)
            => obj is UnmanagedSizeUpperBoundResult other && Equals(other);

        public override int GetHashCode()
            => HashValue.Combine(isKnown).Add(sizeUpperBound).Add(failure);
    }
}
