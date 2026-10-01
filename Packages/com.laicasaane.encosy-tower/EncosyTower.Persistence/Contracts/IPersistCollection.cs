using System.Collections.Generic;
using EncosyTower.Collections;

namespace EncosyTower.Persistences
{
    public interface IPersistCollection : IReadOnlyList<IPersist>, ICopyToSpan<IPersist>, ITryCopyToSpan<IPersist>
    {
    }
}
