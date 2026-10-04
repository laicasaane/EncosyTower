#if UNITY_EDITOR && UNITY_UGUI

using System;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Types;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings.Views
{
    internal class SerializedContext
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly UguiPageFlowSettings Settings;
        public readonly SerializedObject Object;

        public SerializedContext(ScriptableObject settings, SerializedObject serializedObject)
        {
            Settings = settings as UguiPageFlowSettings;
            Object = serializedObject;
            Type = Settings.GetType();
            Name = Type.GetNameWithoutSuffix("Settings");
        }

        public SerializedProperty GetWarnNoSubscriber()
            => Object.FindProperty(nameof(UguiPageFlowSettings.warnNoSubscriber));

        public SerializedProperty GetLoaderStrategyProperty()
            => Object.FindProperty(nameof(UguiPageFlowSettings.loaderStrategy));

        public SerializedProperty GetPoolRentingStrategyProperty()
            => Object.FindProperty(nameof(UguiPageFlowSettings.poolRentingStrategy));

        public SerializedProperty GetPoolReturningStrategyProperty()
            => Object.FindProperty(nameof(UguiPageFlowSettings.poolReturningStrategy));

        public SerializedProperty GetMessageScopeProperty()
            => Object.FindProperty(nameof(UguiPageFlowSettings.messageScope));

        public SerializedProperty GetLogEnvironmentProperty()
            => Object.FindProperty(nameof(UguiPageFlowSettings.logEnvironment));
    }
}

#endif
