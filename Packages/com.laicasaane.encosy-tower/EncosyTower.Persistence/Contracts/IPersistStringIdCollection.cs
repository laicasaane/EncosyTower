using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.StringIds;

namespace EncosyTower.Persistences
{
    public interface IPersistStringIdCollection : IReadOnlyList<StringId<string>>
        , ICopyToSpan<StringId<string>>, ITryCopyToSpan<StringId<string>>
    {
    }
}
