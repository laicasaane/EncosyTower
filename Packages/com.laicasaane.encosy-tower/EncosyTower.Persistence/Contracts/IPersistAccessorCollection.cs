using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Initialization;

namespace EncosyTower.Persistences
{
    public interface IPersistAccessorCollection : IPersistAccessorReadOnlyCollection, IInitializable, IDeinitializable
    {
    }

    public interface IPersistAccessorReadOnlyCollection
        : IReadOnlyList<IPersistAccessor>, ICopyToSpan<IPersistAccessor>, ITryCopyToSpan<IPersistAccessor>
    {
    }
}
