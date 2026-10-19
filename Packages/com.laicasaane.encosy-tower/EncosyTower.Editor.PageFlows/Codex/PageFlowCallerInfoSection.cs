#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.PageFlows;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Builds the Caller Info foldout of the settings windows, which chooses the <see cref="PubSubCallerInfoOption"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The option is not a serialized setting. It is stored as scripting define symbols on every supported named build
    /// target, so it is not part of any <see cref="SerializedObject"/> and has no Undo. Changing it saves the assets
    /// and refreshes the <see cref="AssetDatabase"/>, which recompiles the scripts.
    /// </para>
    /// <para>
    /// The foldout reads the current option from <see cref="PageFlowPublishingContext.CallerInfoOption"/>, so it shows
    /// the option of the current compilation.
    /// </para>
    /// </remarks>
    internal static class PageFlowCallerInfoSection
    {
        /// <summary>
        /// The define symbol that includes caller info in all builds.
        /// </summary>
        /// <remarks>
        /// Must equal the symbol that the runtime tests for in <see cref="PageFlowPublishingContext"/>.
        /// </remarks>
        public const string SYMBOL_ALWAYS = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO";

        /// <summary>
        /// The define symbol that includes caller info only in the Editor and in development builds.
        /// </summary>
        /// <remarks>
        /// Must equal the symbol that the runtime tests for in <see cref="PageFlowPublishingContext"/>.
        /// </remarks>
        public const string SYMBOL_FOR_DEV = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV";

        /// <summary>
        /// Creates the Caller Info foldout.
        /// </summary>
        /// <returns>
        /// The foldout, with the option field and a help box that names both define symbols. Do not bind it to a
        /// <see cref="SerializedObject"/>.
        /// </returns>
        public static Foldout Create()
        {
            var texts = PageFlowsViewResources.Get().CallerInfo;
            var foldout = new Foldout { text = texts.Heading };
            var option = new EnumField(texts.Option, default(PubSubCallerInfoOption)) {
                value = default(PageFlowPublishingContext).CallerInfoOption,
            };

            var helpBox = new HelpBox {
                messageType = HelpBoxMessageType.Info,
                text = texts.Help(SYMBOL_FOR_DEV, SYMBOL_ALWAYS),
            };

            foldout.Add(option.WithAlignFieldClass());
            foldout.Add(helpBox.WithAlignFieldClass());
            option.RegisterValueChangedCallback(OnOptionChanged);
            return foldout;
        }

        /// <summary>
        /// Applies the chosen option.
        /// </summary>
        /// <remarks>
        /// Ignores a value that is not a <see cref="PubSubCallerInfoOption"/> and an unchanged option. Otherwise it
        /// removes both define symbols from every supported named build target, adds the symbol of the new option
        /// (none for <see cref="PubSubCallerInfoOption.Never"/>), then saves the assets and refreshes the
        /// <see cref="AssetDatabase"/>. The Editor recompiles the scripts.
        /// </remarks>
        private static void OnOptionChanged(ChangeEvent<Enum> evt)
        {
            if (evt.newValue is not PubSubCallerInfoOption newValue)
            {
                return;
            }

            var currentOption = default(PageFlowPublishingContext).CallerInfoOption;

            if (currentOption == newValue)
            {
                return;
            }

            var buildTargets = BuildAPI.GetSupportedNamedBuildTargets();

            foreach (var buildTarget in buildTargets)
            {
                BuildAPI.RemoveScriptingDefineSymbols(buildTarget, SYMBOL_FOR_DEV, SYMBOL_ALWAYS);
            }

            var symbol = newValue switch {
                PubSubCallerInfoOption.ForDevelopment => SYMBOL_FOR_DEV,
                PubSubCallerInfoOption.Always => SYMBOL_ALWAYS,
                _ => string.Empty,
            };

            if (symbol.IsNotEmpty())
            {
                foreach (var buildTarget in buildTargets)
                {
                    BuildAPI.AddScriptingDefineSymbols(buildTarget, symbol);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

#endif
