using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
    [AsyncMethodBuilder(typeof(UnityTaskAsyncMethodBuilder))]
    public readonly partial struct UnityTask
    {
    }

    [AsyncMethodBuilder(typeof(UnityTaskAsyncMethodBuilder<>))]
    public readonly partial struct UnityTask<T>
    {
        public async UnityTask AsUnityTask()
            => _ = await this;
    }
}
