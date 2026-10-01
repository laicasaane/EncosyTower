using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

using ILogger = EncosyTower.Logging.ILogger;

namespace EncosyTower.Serialization.NewtonsoftJson
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogException(Exception ex, ILogger logger)
            => (logger ?? DevLogger.Default).LogException(ex);

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorSerializeToNull(ILogger logger)
            => (logger ?? DevLogger.Default).LogError(GetSerializedStringIsNullMessage());

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetSerializedStringIsNullMessage()
            => "Serialized string is `null`.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetSerializedStringIsEmptyMessage()
            => "Serialized string is empty.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetDeserializedObjectIsNullMessage()
            => "Deserialized object is null.";
    }
}
