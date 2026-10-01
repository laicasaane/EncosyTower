#if UNITY_ADDRESSABLES

using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Tasks;
using UnityEngine.SceneManagement;

namespace EncosyTower.Scenes
{
    public static partial class SceneBuildIndexAsyncExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UnityTask<Scene> LoadAsync(
              this SceneBuildIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            var result = await TryLoadAsyncInternal(index, mode, token);
            return result.GetValueOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityTask<Option<Scene>> TryLoadAsync(
              this SceneBuildIndex index
            , LoadSceneMode mode = LoadSceneMode.Single
            , CancellationToken token = default
        )
        {
            return TryLoadAsyncInternal(index, mode, token);
        }

        private static async UnityTask<Option<Scene>> TryLoadAsyncInternal(
              SceneBuildIndex index
            , LoadSceneMode mode
            , CancellationToken token
        )
        {
#if UNITY_EDITOR
            if (Editor.Scenes.SceneBuildIndexEditorAPI.Validate(index) == false)
            {
                ThrowHelper.LogErrorIfInvalidInEditor(index);
                return Option.None;
            }
#else
            if (index.IsValid == false) return Option.None;
#endif

            var handle =  SceneManager.LoadSceneAsync(index.Index, mode);

            if (handle == null)
            {
                return Option.None;
            }

            while (handle.isDone == false)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await UnityTask.NextFrameAsync(token);

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }

            return token.IsCancellationRequested ? Option.None : SceneManager.GetSceneByBuildIndex(index.Index);
        }

    }
}

#endif
