#if UNITY_EDITOR

using EncosyTower.Core;

namespace EncosyTower.CodeGen
{
    [ApiForEditor]
    public interface ICodeGenerator
    {
        GeneratedCode[] Generate();
    }
}

#endif
