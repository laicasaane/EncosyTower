#if UNITY_EDITOR

using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Tests.Editor
{
    public sealed class SerializableGuidHost : ScriptableObject
    {
        [SerializeField] internal SerializableGuid _guid;
        [SerializeField] internal int _notAGuid;
    }
}

#endif
