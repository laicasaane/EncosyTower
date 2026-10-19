#if UNITY_UGUI

using System.Diagnostics.CodeAnalysis;
using EncosyTower.AssetKeys;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>Identifies a page that a page flow pool created.</summary>
    /// <remarks>The pool adds it to the root GameObject of each page it rents and sets its values.</remarks>
    public class UguiPageIdentifier : MonoBehaviour
    {
        /// <summary>Gets the key of the page asset.</summary>
        public AssetKey AssetKey { get; internal set; }

        /// <summary>Gets the string id of <see cref="AssetKey"/>.</summary>
        public StringId KeyId { get; internal set; }

        /// <summary>Gets the id of the page GameObject.</summary>
        public UnityEntityId<GameObject> GameObjectId { get; internal set; }

        /// <summary>Gets the transform of the page.</summary>
        [NotNull]
        public Transform Transform { get; internal set; }

        /// <summary>Gets the GameObject of the page.</summary>
        [NotNull]
        public GameObject GameObject { get; internal set; }

        /// <summary>Gets the CanvasGroup of the page, or <c>null</c> when it has none.</summary>
        [AllowNull]
        public CanvasGroup CanvasGroup { get; internal set; }

        /// <summary>Gets the page component.</summary>
        public IUguiPage Page { get; internal set; }
    }
}

#endif
