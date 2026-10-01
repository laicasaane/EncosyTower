using System.Runtime.CompilerServices;
using UnityEngine.Jobs;

namespace EncosyTower.UnityExtensions
{
    public readonly struct ScheduleOnlyTransformArray
    {
        private readonly TransformAccessArray _array;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ScheduleOnlyTransformArray(TransformAccessArray array)
        {
            _array = array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ScheduleOnlyTransformArray(TransformAccessArray array)
            => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator TransformAccessArray(ScheduleOnlyTransformArray array)
            => array._array;
    }
}
