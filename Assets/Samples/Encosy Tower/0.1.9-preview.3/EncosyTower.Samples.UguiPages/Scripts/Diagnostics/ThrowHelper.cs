using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.PageFlows.History;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Diagnostics
{
    /// <summary>The log and throw helpers of the sample.</summary>
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ConfigMissing()
            => DevLogger.Default.LogError("Tower Book configuration is not assigned; the Book cannot start.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ConfigInvalid()
            => DevLogger.Default.LogError(
                "Tower Book configuration must hold six dimension definitions, one core definition and "
                + "six rune definitions; the Book cannot start."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ConfigIdMissing(ConfigId id)
            => DevLogger.Default.LogError(
                $"Tower Book configuration has no entry for the id '{id.Kind.ToStringFast()}.{id.GetIdStringFast()}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ConfigDuplicateId(ConfigId id)
            => DevLogger.Default.LogError(
                $"Tower Book configuration lists the id '{id.Kind.ToStringFast()}.{id.GetIdStringFast()}' "
                + "more than once; the first entry is kept."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_InvalidProgress(string reason)
            => DevLogger.Default.LogError($"The saved progress is invalid: {reason}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_FlowMissing(string flowId)
            => DevLogger.Default.LogError($"The page flow '{flowId}' is missing from the codex.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_NoReceipt(string operation)
            => DevLogger.Default.LogError($"No page flow answered the call '{operation}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_StorageFailed(string operation, Exception exception)
            => DevLogger.Default.LogError($"Storage failed while it was {operation}: {exception.Message}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogException(Exception exception)
            => DevLogger.Default.LogException(exception);

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ButtonLabelMissing(UnityEngine.Object button)
            => DevLogger.Default.LogError(button, "The button has no TMP label; its text is not applied.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_SaveUnavailable()
            => DevLogger.Default.LogError("Saving is unavailable because the stored progress cannot be read.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarning_ProgressWriteFailed()
            => DevLogger.Default.LogWarning("The progress write failed; it stays in memory for the next save.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarning_SettingsFailed(string operation, string reason)
            => DevLogger.Default.LogWarning($"The settings could not be {operation}: {reason}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_HistoryFailure(EncosyTower.Logging.ILogger logger, PageHistoryError error)
        {
            if (error.TryGetValue(out PageHistoryError.ReplayFailed failed))
            {
                failed.Error.Log(logger);
            }
            else if (error.TryGetValue(out PageHistoryError.ReplayAborted aborted))
            {
                aborted.Error.Log(logger);
            }
            else
            {
                logger.LogError(error.ToMessage());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowInvalidOperation_AttachmentSetsExhausted()
            => throw new InvalidOperationException("Attachment sets exhausted");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowInvalidOperation_AttachmentPagesFailed(BookLocation location)
            => throw new InvalidOperationException($"The attached pages of screen '{location}' could not be added.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogInfo_Demo(string demo, string outcome)
            => DevLogger.Default.LogInfo($"[Abort demo] {demo}: {outcome}");
    }
}
