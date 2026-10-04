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
