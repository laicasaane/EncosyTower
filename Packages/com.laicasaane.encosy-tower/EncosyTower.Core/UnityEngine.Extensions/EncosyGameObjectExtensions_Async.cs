using System.Diagnostics.CodeAnalysis;
using System.Threading;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyGameObjectExtensions
    {
        /// <summary>
        /// SetActive(false) => 1 frame => SetActive(true);
        /// </summary>
        public static async UnityTask ActivateAsync([NotNull] this GameObject self, CancellationToken token = default)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            self.SetActive(false);

            await UnityTask.NextFrameAsync(token);

            self.SetActive(true);
        }

        /// <summary>
        /// SetActive(false) => N frame => SetActive(true);
        /// </summary>
        public static async UnityTask ActivateAsync(
              [NotNull] this GameObject self
            , int delayFrames
            , CancellationToken token = default
        )
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            self.SetActive(false);

            for (var i = 0; i < delayFrames; i++)
            {
                await UnityTask.NextFrameAsync(token);
            }

            self.SetActive(true);
        }
    }
}
