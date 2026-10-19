#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Internals
{
    /// <summary>
    /// A menu entry for one choice, independent of the UI Toolkit or IMGUI menu that shows it.
    /// </summary>
    /// <param name="Content">The label of the entry.</param>
    /// <param name="IsChecked">Whether the entry is shown as selected.</param>
    /// <param name="Callback">The action that applies the choice.</param>
    internal readonly record struct ValueChoiceMenuItem(
          GUIContent Content
        , bool IsChecked
        , GenericMenu.MenuFunction Callback
    );
}

#endif
