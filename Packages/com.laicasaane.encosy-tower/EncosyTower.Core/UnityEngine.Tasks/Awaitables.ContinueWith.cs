using System;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.Tasks
{
    public static partial class Awaitables
    {
        public static async Awaitable ContinueWith<T>(this Awaitable<T> self, Action<T> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            continuationFunction(await self);
        }

        public static async Awaitable ContinueWith<T>(this Awaitable<T> self, Func<T, Awaitable> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            await continuationFunction(await self);
        }

        public static async Awaitable<TR> ContinueWith<T, TR>(this Awaitable<T> self, Func<T, TR> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            return continuationFunction(await self);
        }

        public static async Awaitable<TR> ContinueWith<T, TR>(
              this Awaitable<T> self
            , Func<T, Awaitable<TR>> continuationFunction
        )
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            return await continuationFunction(await self);
        }

        public static async Awaitable ContinueWith(this Awaitable self, Action continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            await self;
            continuationFunction();
        }

        public static async Awaitable ContinueWith(this Awaitable self, Func<Awaitable> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            await self;
            await continuationFunction();
        }

        public static async Awaitable<T> ContinueWith<T>(this Awaitable self, Func<T> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            await self;
            return continuationFunction();
        }

        public static async Awaitable<T> ContinueWith<T>(this Awaitable self, Func<Awaitable<T>> continuationFunction)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(continuationFunction);
            await self;
            return await continuationFunction();
        }
    }
}
