using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.PageFlows.History;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UitkPages.Diagnostics
{
    /// <summary>The log and throw helpers of the sample.</summary>
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ResourcesMissing()
            => DevLogger.Default.LogError("Tower Book resources are missing or incomplete; the Book does not start.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_LabelMissing(string name)
            => DevLogger.Default.LogError($"Cannot find Label by name `{name}`");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_StyleUnresolved()
            => DevLogger.Default.LogError("Tower Book styles did not resolve; the Book does not start.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_ConfigInvalid(string reason)
            => DevLogger.Default.LogError($"The Book configuration is invalid: {reason}; the Book does not start.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_PropertyMissing(string name)
            => DevLogger.Default.LogError($"Cannot find style property `{name}`; its value stays unset.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogError_IdUnknown(ConfigId id)
            => DevLogger.Default.LogError($"The configuration id `{id}` is not known; its value stays unset.");

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
        internal static void LogError_SaveUnavailable()
            => DevLogger.Default.LogError("Saving is unavailable because the stored progress cannot be read.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarning_ProgressWriteFailed()
            => DevLogger.Default.LogWarning("The progress write failed; it stays in memory for the next save.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogWarning_SettingsUnreadable()
            => DevLogger.Default.LogWarning("The saved settings cannot be read; the Light theme is used.");

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
        [HideInCallstack, StackTraceHidden]
        internal static void LogInfo_Demo(string demo, string outcome)
            => DevLogger.Default.LogInfo($"[Abort demo] {demo}: {outcome}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowAttachmentSetsExhausted()
            => throw new InvalidOperationException("Attachment sets exhausted");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowAttachmentSetPopulateFailed(BookLocation location)
            => throw new InvalidOperationException($"The attachment pages for '{location}' cannot be added.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowFlowCreationFailed(string flowId)
            => throw new InvalidOperationException($"The page flow '{flowId}' cannot be created.");
    }
}
