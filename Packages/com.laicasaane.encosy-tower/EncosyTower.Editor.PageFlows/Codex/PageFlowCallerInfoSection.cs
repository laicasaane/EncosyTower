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
    internal static class PageFlowCallerInfoSection
    {
        public const string SYMBOL_ALWAYS = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO";
        public const string SYMBOL_FOR_DEV = "ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV";

        public static Foldout Create()
        {
            var foldout = new Foldout { text = "Caller Info Option For Publishing Page Flow Messages" };
            var option = new EnumField("Option", default(PubSubCallerInfoOption)) {
                value = default(PageFlowPublishingContext).CallerInfoOption,
            };

            var helpBox = new HelpBox {
                messageType = HelpBoxMessageType.Info,
                text =
                    "Choose whether to include caller info when publishing page flow messages.\n" +
                    "Including caller info can be useful for debugging purposes, but may increase build size.\n" +
                    "- <b>Never:</b> Do not include caller info.\n" +
                    "- <b>For Development:</b> Include only for Editor and Development builds.\n" +
                    $"\tSymbol '{SYMBOL_FOR_DEV}' will be added to Player Settings.\n" +
                    "- <b>Always:</b> Include for all environments.\n" +
                    $"\tSymbol '{SYMBOL_ALWAYS}' will be added to Player Settings.",
            };

            foldout.Add(option.WithAlignFieldClass());
            foldout.Add(helpBox.WithAlignFieldClass());
            option.RegisterValueChangedCallback(OnOptionChanged);
            return foldout;
        }

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
