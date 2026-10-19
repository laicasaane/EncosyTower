using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if UNITY_ADDRESSABLES
using EncosyTower.AddressableKeys;
#endif
using EncosyTower.AssetKeys;
using EncosyTower.Logging;
using EncosyTower.PolyEnumStructs;
using EncosyTower.ResourceKeys;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.Loading
{
    /// <summary>Describes why a page asset could not be loaded.</summary>
    /// <remarks>
    /// <para>The failure is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs</description>
    /// </listheader>
    /// <item>
    /// <term><c>Undefined</c></term>
    /// <description>
    /// The default value. The loader returns it only when a load ends with neither an asset nor an error.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>Resource</c></term>
    /// <description>Loading the asset with Resources failed. The case holds the <c>ResourceKeyError</c>.</description>
    /// </item>
    /// <item>
    /// <term><c>Addressable</c></term>
    /// <description>
    /// Loading the asset with Addressables failed. The case exists only when the Addressables package is installed,
    /// and holds the <c>AddressableKeyError</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>UnsupportedAsset</c></term>
    /// <description>
    /// The loaded asset is not a page source: a UI Toolkit page must be a <c>VisualTreeAsset</c> or a
    /// <c>GameObject</c> with a <c>UitkPageBehaviour</c>.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct PageAssetLoadError
    {
        internal readonly Error _error;

        private PageAssetLoadError(in Error error)
        {
            _error = error;
        }

        /// <summary>Gets a message that describes the failure.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToMessage()
            => _error.ToMessage();

        /// <inheritdoc cref="ToMessage"/>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string ToString()
            => ToMessage();

        /// <summary>Logs the failure as an error.</summary>
        /// <param name="logger">The logger to log with.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> is <c>null</c>.</exception>
        public void Log([NotNull] ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(logger);
            _error.Log(logger);
        }

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage();

                void Log(ILogger logger)
                    => logger.LogError(ToMessage());
            }

            /// <summary>
            /// The default value. The loader returns it only when a load ends with neither an asset nor an error.
            /// </summary>
            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "An unknown page asset loading error occurred.";
            }

            /// <summary>Loading the asset with Resources failed.</summary>
            /// <param name="KeyError">Why the Resources load failed.</param>
            public readonly partial record struct Resource(ResourceKeyError KeyError)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Resources: {KeyError}";

                public void Log(ILogger logger)
                    => KeyError.Log(logger);
            }

#if UNITY_ADDRESSABLES
            /// <summary>Loading the asset with Addressables failed.</summary>
            /// <param name="KeyError">Why the Addressables load failed.</param>
            public readonly partial record struct Addressable(AddressableKeyError KeyError)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Addressables: {KeyError}";

                public void Log(ILogger logger)
                    => KeyError.Log(logger);
            }
#endif

            /// <summary>
            /// The loaded asset is not a page source: a UI Toolkit page must be a <c>VisualTreeAsset</c> or a
            /// <c>GameObject</c> with a <c>UitkPageBehaviour</c>.
            /// </summary>
            /// <param name="AssetKey">The key of the asset.</param>
            /// <param name="AssetType">The type of the loaded asset, or <c>null</c> if the asset is destroyed.</param>
            public readonly partial record struct UnsupportedAsset(AssetKey AssetKey, System.Type AssetType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The asset loaded by the key '{AssetKey}' is a '{AssetType}'. " +
                       $"A UI Toolkit page must be a VisualTreeAsset or a GameObject with a UitkPageBehaviour.";
            }
        }
    }
}
