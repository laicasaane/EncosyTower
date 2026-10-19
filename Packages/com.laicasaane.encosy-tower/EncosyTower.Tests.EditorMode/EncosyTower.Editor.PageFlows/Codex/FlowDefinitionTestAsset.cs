using System;
using UnityEngine;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class FlowDefinitionTestAsset : ScriptableObject
    {
        public Definition[] flows = Array.Empty<Definition>();

        [Serializable]
        public struct Definition
        {
            public string identifier;
            public int kind;
        }
    }
}
