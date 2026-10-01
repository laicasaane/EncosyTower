using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using EncosyTower.Types;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Variants.Converters
{
    public static partial class VariantConverter
    {
        private static readonly object s_cacheResetLock = new();
        private static readonly List<Action> s_cacheResetCallbacks = new();

        private static ConcurrentDictionary<TypeId, IVariantConverter> s_converters;

        static VariantConverter()
        {
            Init();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
#endif
        private static void Init()
        {
            lock (s_cacheResetLock)
            {
                var count = s_cacheResetCallbacks.Count;

                for (var i = 0; i < count; i++)
                {
                    s_cacheResetCallbacks[i]();
                }

                s_converters = new();

                TryRegisterGeneratedConverters();
                TryRegister(VariantConverterString.Default);
                TryRegister(VariantConverterObject.Default);
            }
        }

        static partial void TryRegisterGeneratedConverters();

        internal static void RegisterCacheReset(Action reset)
        {
            DebuggingThrowHelper.ThrowIfNull(reset);

            lock (s_cacheResetLock)
            {
                s_cacheResetCallbacks.Add(reset);
            }
        }

        public static bool TryRegister<T>([NotNull] IVariantConverter<T> converter)
        {
            DebuggingThrowHelper.ThrowIfNull(converter);
            ThrowHelper.ThrowIfSizeInvalid<T>(IsSizeValid<T>());

            return s_converters.TryAdd((TypeId)Type<T>.Id, converter);
        }

        public static IVariantConverter<T> GetConverter<T>()
        {
            if (s_converters.TryGetValue((TypeId)Type<T>.Id, out var candidate))
            {
                if (candidate is IVariantConverter<T> converterT)
                {
                    return converterT;
                }
            }

            if (EncosyTypeExtensions.IsUnmanaged<T>() == false)
            {
                return VariantConverterObject<T>.Default;
            }

            return VariantConverterUndefined<T>.Default;
        }

        public static bool TryGetConverter(TypeId typeId, out IVariantConverter result)
        {
            if (s_converters.TryGetValue(typeId, out var candidate) && candidate is not null)
            {
                result = candidate;
                return true;
            }

            result = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant ToVariant<T>(T value)
            => GetConverter<T>().ToVariant(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Variant<T> ToVariantT<T>(T value)
            => GetConverter<T>().ToVariantT(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(in Variant variant)
            => GetConverter<T>().GetValue(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(in Variant variant, out T result)
            => GetConverter<T>().TryGetValue(variant, out result);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetValueTo<T>(in Variant variant, ref T dest)
            => GetConverter<T>().TrySetValueTo(variant, ref dest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString<T>(in Variant variant)
            => GetConverter<T>().ToString(variant);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(in Variant variant)
        {
            return TryGetConverter(variant.TypeId, out var converter)
                ? converter.ToString(variant)
                : variant.TypeId.ToType().ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanStore<T>() where T : struct
            => UnsafeUtility.SizeOf<T>() <= VariantData.BYTE_COUNT;

        private static bool IsSizeValid<T>()
        {
            if (EncosyTypeExtensions.IsUnmanaged<T>() == false)
            {
                return true;
            }

            var sizeOfT = UnsafeUtility.SizeOf(typeof(T));
            return sizeOfT <= VariantData.BYTE_COUNT;
        }

    }
}
