#if UNITY_EDITOR

using EncosyTower.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    /// <summary>
    /// Shows every <see cref="OverridableShowcaseAsset"/> use case through the real Inspector, so drawer changes
    /// are visible as soon as scripts recompile.
    /// </summary>
    public sealed class OverridableShowcaseWindow : EditorWindow
    {
        private const string TITLE = "Overridables";
        private const string MIXED_TEXT = "Mixed Values (2 Objects)";
        private const string IMGUI_TEXT = "IMGUI";
        private const string RESET_TEXT = "Reset Values";
        private const string SETTINGS_TEXT = "Project Settings";

        [SerializeField] private bool _mixed;
        [SerializeField] private bool _imgui;

        private OverridableShowcaseAsset _first;
        private OverridableShowcaseAsset _second;
        private UnityEditor.Editor _editor;
        private ScrollView _scrollView;

        [MenuItem("Dev Tools/Showcases/Overridables", priority = 90_77_00_00)]
        public static void OpenWindow()
        {
            var window = GetWindow<OverridableShowcaseWindow>();
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        private void CreateGUI()
        {
            var toolbar = new Toolbar();
            rootVisualElement.Add(toolbar);

            var mixed = new ToolbarToggle { text = MIXED_TEXT, value = _mixed };
            mixed.RegisterValueChangedCallback(OnMixedChanged);
            toolbar.Add(mixed);

            var imgui = new ToolbarToggle { text = IMGUI_TEXT, value = _imgui };
            imgui.RegisterValueChangedCallback(OnImguiChanged);
            toolbar.Add(imgui);

            toolbar.Add(new ToolbarSpacer { flex = true });
            toolbar.Add(new ToolbarButton(OpenSettings) { text = SETTINGS_TEXT });
            toolbar.Add(new ToolbarButton(ResetValues) { text = RESET_TEXT });

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.style.flexGrow = 1f;
            rootVisualElement.Add(_scrollView);

            Rebuild();
        }

        private void OnDisable()
        {
            DestroyTargets();
        }

        private void OnMixedChanged(ChangeEvent<bool> evt)
        {
            _mixed = evt.newValue;
            Rebuild();
        }

        private void OnImguiChanged(ChangeEvent<bool> evt)
        {
            _imgui = evt.newValue;
            Rebuild();
        }

        private static void OpenSettings()
        {
            OverridableShowcaseSettings.Instance.OpenSettingsWindow();
        }

        private void ResetValues()
        {
            DestroyTargets();
            Rebuild();
        }

        private void Rebuild()
        {
            OverridableShowcaseAssetEditor.UseImgui = _imgui;

            if (_first == false)
            {
                _first = CreateTarget("Overridables");
            }

            if (_second == false)
            {
                _second = CreateTarget("Overridables (Second)");
                ApplySecondValues(_second);
            }

            if (_editor)
            {
                DestroyImmediate(_editor);
            }

            var targets = _mixed ? new Object[] { _first, _second } : new Object[] { _first };
            _editor = UnityEditor.Editor.CreateEditor(targets);

            _scrollView.Clear();
            _scrollView.Add(new InspectorElement(_editor));
        }

        private void DestroyTargets()
        {
            if (_editor)
            {
                DestroyImmediate(_editor);
            }

            if (_first)
            {
                DestroyImmediate(_first);
            }

            if (_second)
            {
                DestroyImmediate(_second);
            }

            _editor = null;
            _first = null;
            _second = null;
        }

        private static OverridableShowcaseAsset CreateTarget(string name)
        {
            var target = CreateInstance<OverridableShowcaseAsset>();
            target.name = name;
            target.hideFlags = HideFlags.DontSave;
            return target;
        }

        private static void ApplySecondValues(OverridableShowcaseAsset target)
        {
            target.enabled = new(false, true);
            target.retryCount = new(5, true);
            target.quality = new(ShowcaseQuality.Low, true);
            target.layers = new(ShowcaseLayers.Ground | ShowcaseLayers.Air, true);
            target.material = new(null, true);
            target.displayName = new("Enemy", true);
        }
    }
}

#endif
