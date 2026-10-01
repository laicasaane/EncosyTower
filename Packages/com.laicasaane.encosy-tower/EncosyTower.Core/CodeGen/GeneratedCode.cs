#if UNITY_EDITOR

using System;
using EncosyTower.Core;

namespace EncosyTower.CodeGen
{
    [ApiForEditor]
    [Serializable]
    public struct GeneratedCode
    {
        public string filePath;
        public string content;

        public GeneratedCode(string content, string filePath)
        {
            this.content = content;
            this.filePath = filePath;
        }
    }
}

#endif
