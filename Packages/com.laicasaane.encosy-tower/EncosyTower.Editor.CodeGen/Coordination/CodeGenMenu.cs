#if UNITY_EDITOR

using UnityEditor;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Exposes manual Unity and .NET code generation commands in the Editor menu.
    /// </summary>
    internal static class CodeGenMenu
    {
        /// <summary>
        /// Defines the menu path for the in-process Unity backend.
        /// </summary>
        internal const string UNITY_MENU_PATH = "Encosy Tower/CodeGen/Generate in Unity";

        /// <summary>
        /// Defines the menu path for the isolated .NET backend.
        /// </summary>
        internal const string DOTNET_MENU_PATH = "Encosy Tower/CodeGen/Generate in .NET";

        /// <summary>
        /// Requests a manual run through the Unity backend.
        /// </summary>
        [MenuItem(UNITY_MENU_PATH, priority = 67_71_00_00)]
        private static void GenerateInUnity()
        {
            CodeGenAutoTrigger.RequestUnity();
        }

        /// <summary>
        /// Requests a manual run through the .NET backend.
        /// </summary>
        [MenuItem(DOTNET_MENU_PATH, priority = 67_71_00_01)]
        private static void GenerateInDotnet()
        {
            CodeGenAutoTrigger.RequestDotnet();
        }

        /// <summary>
        /// Validates whether the Unity menu command can run.
        /// </summary>
        [MenuItem(UNITY_MENU_PATH, true)]
        private static bool ValidateGenerateInUnity()
            => CanExecute(
                  EditorApplication.isCompiling || EditorApplication.isUpdating
                , CodeGenAutoTrigger.IsRunning
            );

        /// <summary>
        /// Validates whether the .NET menu command can run.
        /// </summary>
        [MenuItem(DOTNET_MENU_PATH, true)]
        private static bool ValidateGenerateInDotnet()
            => CanExecute(
                  EditorApplication.isCompiling || EditorApplication.isUpdating
                , CodeGenAutoTrigger.IsRunning
            );

        /// <summary>
        /// Determines whether a generation command can start in the current Editor state.
        /// </summary>
        internal static bool CanExecute(bool isEditorBusy, bool isRunning)
            => isEditorBusy == false && isRunning == false;
    }
}

#endif
