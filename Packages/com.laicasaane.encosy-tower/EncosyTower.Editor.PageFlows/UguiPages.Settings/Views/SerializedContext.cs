#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.PageFlows;
using UnityEditor;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings.Views
{
    /// <summary>
    /// Gives typed access to the serialized properties of <see cref="UguiPageFlowSettings"/>.
    /// </summary>
    /// <remarks>
    /// Finds each property with <c>nameof</c> of the runtime field, so a rename of the field is followed.
    /// </remarks>
    internal class SerializedContext
    {
        public readonly SerializedObject Object;

        public SerializedContext(SerializedObject serializedObject)
        {
            Object = serializedObject;
        }

        public SerializedProperty GetWarnNoSubscriber()
            => Object.FindProperty(nameof(UguiPageFlowSettings.warnNoSubscriber));

        public SerializedProperty GetRejectDuplicatePages()
            => Object.FindProperty(nameof(UguiPageFlowSettings.rejectDuplicatePages));

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
