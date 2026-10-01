using EncosyTower.PolyEnumStructs;

namespace Samples.PolyEnumStructs.DependencyResults
{
    [PolyEnumStruct(Container = typeof(DependencyResult), WithEnumExtensions = true)]
    internal partial struct DependencyResult<TValue, TError>
    {
    }

    internal static partial class DependencyResult
    {
        internal readonly partial record struct None;

        internal readonly partial record struct Success<TValue>(TValue Value);

        internal readonly partial record struct Failure<TError>(TError Error);

        internal readonly partial record struct Pair<TValue, TError>(TValue Value, TError Error);
    }

    [PolyEnumFactoryFor(typeof(DependencyResult<,>))]
    internal readonly partial struct DependencyResultFactory<TValue, TError>
    {
    }

    internal static class DependencyResultSample
    {
        internal static bool Demonstrate()
        {
            DependencyResult<int, string> none = new DependencyResult.None();
            var converted = new DependencyResult.Success<int>(42)
                .ToDependencyResult<string>();
            var success = DependencyResultFactory<int, string>.Success(42);
            var failure = DependencyResultFactory<int, string>.Failure("invalid");
            var pair = DependencyResultFactory<int, string>.Pair(42, "invalid");

            return none.GetEnumCase() == DependencyResult.EnumCase.None
                && converted.GetEnumCase() == DependencyResult.EnumCase.Success
                && success.Is(DependencyResultFactory.Type.Success)
                && failure.Is(DependencyResultFactory.Type.Failure)
                && pair.Is(DependencyResultFactory.Type.Pair);
        }
    }
}
