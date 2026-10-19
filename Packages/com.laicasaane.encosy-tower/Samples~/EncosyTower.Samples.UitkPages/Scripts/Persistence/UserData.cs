using System;
using EncosyTower.Persistences;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Persistence
{
    /// <summary>
    /// The saved data of the player: the theme, and the progress with the runes, the discovered dimensions and the
    /// screen views.
    /// </summary>
    [Serializable]
    internal sealed class UserData : IPersist
    {
        /// <summary>The version of the saved format.</summary>
        public const int CURRENT_VERSION = 4;

        /// <summary>The identifier of the save.</summary>
        [field: SerializeField]
        public string Id { get; set; } = "player";

        /// <summary>The version of the saved format.</summary>
        [field: SerializeField]
        public int Version { get; set; } = CURRENT_VERSION;

        /// <summary>The rune that each outer dimension holds.</summary>
        [field: SerializeField]
        public Rune[] RuneForDimension { get; set; } = Array.Empty<Rune>();

        /// <summary>The dimensions in the order the player discovered them.</summary>
        [field: SerializeField]
        public int[] AcquisitionOrder { get; set; } = Array.Empty<int>();

        /// <summary>The discovered dimensions, one bit for each outer dimension.</summary>
        [field: SerializeField]
        public int DiscoveredMask { get; set; }

        /// <summary>Whether the player visited the core.</summary>
        [field: SerializeField]
        public bool CoreVisited { get; set; }

        /// <summary>Whether the player cast the incantation.</summary>
        [field: SerializeField]
        public bool IncantationCast { get; set; }

        /// <summary>Whether the notice that the core unlocked is still to be shown.</summary>
        [field: SerializeField]
        public bool CoreUnlockNoticePending { get; set; }

        /// <summary>The saved views of the dimension screens.</summary>
        [field: SerializeField]
        public ViewStateData[] ViewStates { get; set; } = Array.Empty<ViewStateData>();

        /// <summary>The theme the player picked.</summary>
        [field: SerializeField]
        public Theme Theme { get; set; } = Theme.Light;

        /// <summary>Gets whether the data holds the progress of a game, not only the theme.</summary>
        public bool HasProgress => RuneForDimension.Length > 0;

        /// <summary>Converts the progress to JSON.</summary>
        /// <param name="data">The progress.</param>
        /// <param name="text">The JSON text.</param>
        /// <returns><c>true</c> when the conversion worked; otherwise <c>false</c>.</returns>
        public static bool TrySerialize(UserData data, out string text)
        {
            if (data == null)
            {
                goto FAILED;
            }

            try
            {
                text = JsonUtility.ToJson(data);
                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ThrowHelper.LogException(ex);
            }

        FAILED:
            text = null;
            return false;
        }

        /// <summary>Reads progress from JSON and validates it.</summary>
        /// <param name="text">The JSON text.</param>
        /// <param name="data">The progress.</param>
        /// <returns><c>true</c> when the text holds valid progress; otherwise <c>false</c>.</returns>
        public static bool TryDeserialize(string text, out UserData data)
        {
            if (string.IsNullOrEmpty(text))
            {
                goto FAILED;
            }

            try
            {
                string reason = null;

                data = JsonUtility.FromJson<UserData>(text);

                if (data != null && data.TryValidate(out reason))
                {
                    return true;
                }

                ThrowHelper.LogError_InvalidProgress(reason ?? "the data is empty");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ThrowHelper.LogException(ex);
            }

        FAILED:
            data = null;
            return false;
        }

        /// <summary>Checks that the progress follows the rules of the Book.</summary>
        /// <param name="reason">Why the progress is invalid.</param>
        /// <returns><c>true</c> when the progress is valid; otherwise <c>false</c>.</returns>
        public bool TryValidate(out string reason)
        {
            if (Version != CURRENT_VERSION)
            {
                reason = "the version is not supported";
                return false;
            }

            if (Theme is not (Theme.Light or Theme.Dark))
            {
                reason = "the theme is not supported";
                return false;
            }

            if (HasProgress == false)
            {
                reason = null;
                return true;
            }

            if (HasValidPermutation(RuneForDimension) == false)
            {
                reason = "the rune assignment is not a permutation";
                return false;
            }

            if (HasValidAcquisition(out var mask) == false || mask != DiscoveredMask)
            {
                reason = "the acquisition order does not match the discovered mask";
                return false;
            }

            if (CoreVisited && DiscoveredMask != DimensionCatalog.FULL_MASK)
            {
                reason = "the Core is visited before every rune is discovered";
                return false;
            }

            if (IncantationCast && CoreVisited == false)
            {
                reason = "the incantation is cast before the Core is visited";
                return false;
            }

            if (HasValidViewStates() == false)
            {
                reason = "a saved view state is invalid";
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>Creates a deep copy of the progress.</summary>
        /// <returns>The copy.</returns>
        public UserData Clone()
            => new() {
                Id = Id,
                Version = Version,
                RuneForDimension = (Rune[])RuneForDimension.Clone(),
                AcquisitionOrder = (int[])AcquisitionOrder.Clone(),
                DiscoveredMask = DiscoveredMask,
                CoreVisited = CoreVisited,
                IncantationCast = IncantationCast,
                CoreUnlockNoticePending = CoreUnlockNoticePending,
                ViewStates = (ViewStateData[])ViewStates.Clone(),
                Theme = Theme,
            };

        /// <summary>Gets whether another progress holds the same content.</summary>
        /// <param name="other">The other progress.</param>
        /// <returns><c>true</c> when the content is the same; otherwise <c>false</c>.</returns>
        public bool ContentEquals(UserData other)
        {
            if (other == null
                || Version != other.Version
                || Theme != other.Theme
                || DiscoveredMask != other.DiscoveredMask
                || CoreVisited != other.CoreVisited
                || IncantationCast != other.IncantationCast
                || CoreUnlockNoticePending != other.CoreUnlockNoticePending
                || RunesEqual(RuneForDimension, other.RuneForDimension) == false
                || AcquisitionOrder.AsSpan().SequenceEqual(other.AcquisitionOrder) == false
                || ViewStates.Length != other.ViewStates.Length
            )
            {
                return false;
            }

            var count = ViewStates.Length;

            for (var i = 0; i < count; i++)
            {
                var a = ViewStates[i];
                var b = other.ViewStates[i];

                if (a.Dimension != b.Dimension
                    || a.Tab != b.Tab
                    || a.ConceptVisible != b.ConceptVisible
                    || a.InPracticeVisible != b.InPracticeVisible
                )
                {
                    return false;
                }
            }

            return true;
        }

        private static bool RunesEqual(Rune[] a, Rune[] b)
        {
            var left = a.AsSpan();
            var right = b.AsSpan();

            if (left.Length != right.Length)
            {
                return false;
            }

            var count = left.Length;

            for (var i = 0; i < count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasValidPermutation(Rune[] values)
        {
            if (values == null || values.Length != DimensionCatalog.OUTER_COUNT)
            {
                return false;
            }

            var seen = 0;
            var count = values.Length;

            for (var i = 0; i < count; i++)
            {
                var value = (int)values[i];

                if ((uint)value >= (uint)DimensionCatalog.OUTER_COUNT || (seen & (1 << value)) != 0)
                {
                    return false;
                }

                seen |= 1 << value;
            }

            return true;
        }

        private bool HasValidAcquisition(out int mask)
        {
            mask = 0;

            if (AcquisitionOrder == null || AcquisitionOrder.Length > DimensionCatalog.OUTER_COUNT)
            {
                return false;
            }

            var count = AcquisitionOrder.Length;

            for (var i = 0; i < count; i++)
            {
                var index = AcquisitionOrder[i];

                if ((uint)index >= (uint)DimensionCatalog.OUTER_COUNT || (mask & (1 << index)) != 0)
                {
                    return false;
                }

                mask |= 1 << index;
            }

            return true;
        }

        private bool HasValidViewStates()
        {
            if (ViewStates == null)
            {
                return false;
            }

            var count = ViewStates.Length;
            var seen = 0;

            for (var i = 0; i < count; i++)
            {
                var state = ViewStates[i];

                if (DimensionCatalog.TryGet(state.Dimension, out var data) == false
                    || data.Index >= DimensionCatalog.OUTER_COUNT
                    || (uint)state.Tab > 1u
                    || (seen & (1 << data.Index)) != 0
                )
                {
                    return false;
                }

                seen |= 1 << data.Index;
            }

            return true;
        }
    }
}
