using System.Diagnostics.CodeAnalysis;
using System.Threading;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyComponentExtensions
    {
        /// <summary>
        /// .enabled = false => 1 frame => enabled = true;
        /// </summary>
        public static async UnityTask EnableAsync([NotNull] this Behaviour self, CancellationToken token = default)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            self.enabled = false;

            await UnityTask.NextFrameAsync(token);

            self.enabled = true;
        }

        /// <summary>
        /// .enabled = false => N frame => enabled = true;
        /// </summary>
        public static async UnityTask EnableAsync(
              [NotNull] this Behaviour self
            , int delayFrames
            , CancellationToken token = default
        )
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            self.enabled = false;

            for (var i = 0; i < delayFrames; i++)
            {
                await UnityTask.NextFrameAsync(token);
            }

            self.enabled = true;
        }
    }
}
