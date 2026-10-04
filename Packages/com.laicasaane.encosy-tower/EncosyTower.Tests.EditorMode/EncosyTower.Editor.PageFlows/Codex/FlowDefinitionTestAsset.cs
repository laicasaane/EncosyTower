using System;
using UnityEngine;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class FlowDefinitionTestAsset : ScriptableObject
    {
        public Row[] flows = Array.Empty<Row>();

        [Serializable]
        public struct Row
        {
            public string identifier;
            public int kind;
        }
    }
}
