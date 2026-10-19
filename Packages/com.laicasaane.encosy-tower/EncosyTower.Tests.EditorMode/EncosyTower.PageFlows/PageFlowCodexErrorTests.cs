using System;
using System.Collections.Generic;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Codex;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

using ILogger = EncosyTower.Logging.ILogger;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageFlowCodexErrorTests
{
    public static IEnumerable<TestCaseData> ErrorCases
    {
        get
        {
            yield return new TestCaseData(PageFlowCodexError.Undefined(), "An unknown page codex error occurred.")
                .SetName("ErrorContract_Undefined");

            yield return new TestCaseData(
                  PageFlowCodexError.InitializerMissing()
                , "Cannot initialize the page codex without an initializer. "
                    + "Add a component that implements IUguiPageCodexOnInitialize or IUitkPageCodexOnInitialize "
                    + "to the GameObject of the codex."
            ).SetName("ErrorContract_InitializerMissing");

            yield return new TestCaseData(
                  PageFlowCodexError.ScopeCollectionApplierMissing()
                , "The codex initializer returned a null IPageFlowScopeCollectionApplier."
            ).SetName("ErrorContract_ApplierMissing");

            yield return new TestCaseData(
                  PageFlowCodexError.ScopeCollectionTypeMissing()
                , "The IPageFlowScopeCollectionApplier returned a null flow scope collection type."
            ).SetName("ErrorContract_CollectionTypeMissing");

            yield return new TestCaseData(
                  PageFlowCodexError.NoScopeProperties(typeof(int))
                , "The type 'System.Int32' has no valid PageFlowScope property. "
                    + "Declare it as a partial struct with [PageFlowScopeCollection] "
                    + "and properties of type PageFlowScope with a private setter."
            ).SetName("ErrorContract_NoScopeProperties");

            var validation = new PageFlowCodexValidation(
                  FirstEmptyIndex: 1
                , DuplicateIdentifiers: new[] { "X" }
                , DefinitionsWithoutScope: new[] { "Y" }
                , ScopesWithoutDefinition: new[] { "Z" }
            );

            yield return new TestCaseData(
                  PageFlowCodexError.InvalidDefinitions(validation)
                , "The flow definitions do not match the properties of the scope collection. "
                    + "The identifier of the definition at index 1 is null or empty. "
                    + "Duplicate identifiers: X. Properties without a definition: Z. Definitions without a property: Y."
            ).SetName("ErrorContract_InvalidDefinitions");

            var index = 2;
            var identifier = "Popup";

            yield return new TestCaseData(
                  PageFlowCodexError.FlowCreationFailed(index, identifier)
                , "Cannot create the flow 'Popup' for the definition at index 2."
            ).SetName("ErrorContract_FlowCreationFailed");

            identifier = "A";

            yield return new TestCaseData(
                  PageFlowCodexError.ScopeNotFound(identifier)
                , "Cannot find a flow scope for the property 'A' of the scope collection."
            ).SetName("ErrorContract_ScopeNotFound");

            yield return new TestCaseData(
                  PageFlowCodexError.ScopeNotAssigned(identifier)
                , "The scope collection did not accept a flow scope for the property 'A'."
            ).SetName("ErrorContract_ScopeNotAssigned");

            yield return new TestCaseData(
                  PageFlowCodexError.InitializerFailed(new InvalidOperationException("boom"))
                , "The codex initializer threw an exception: boom"
            ).SetName("ErrorContract_InitializerFailed");
        }
    }

    [TestCaseSource(nameof(ErrorCases))]
    public void ErrorMessageMatchesContract(PageFlowCodexError error, string message)
    {
        Assert.AreEqual(expected: message, actual: error.ToMessage());
        Assert.AreEqual(expected: message, actual: error.ToString());
    }

    [Test]
    public void DefaultErrorReportsUnknownCodexError()
    {
        PageFlowCodexError error = default;

        Assert.AreEqual(expected: "An unknown page codex error occurred.", actual: error.ToMessage());
    }

    [Test]
    public void InitializerFailed_LogReportsOneWrappedException()
    {
        var original = new InvalidOperationException("boom");
        var error = PageFlowCodexError.InitializerFailed(original);
        var logger = new CapturingLogger();

        LogAssert.Expect(LogType.Exception, "InvalidOperationException: boom");
        error.Log(DevLogger.Default);
        LogAssert.NoUnexpectedReceived();

        error.Log(logger);

        Assert.AreEqual(expected: 1, actual: logger.ExceptionCount);
        Assert.AreEqual(expected: 0, actual: logger.OtherLogCount);
        Assert.IsInstanceOf<InvalidOperationException>(logger.LoggedException);

        Assert.AreEqual(
              expected: "The codex initializer threw an exception: boom"
            , actual: logger.LoggedException.Message
        );

        Assert.AreSame(expected: original, actual: logger.LoggedException.InnerException);
        LogAssert.NoUnexpectedReceived();
    }

    private sealed class CapturingLogger : ILogger
    {
        public int ExceptionCount { get; private set; }

        public int OtherLogCount { get; private set; }

        public Exception LoggedException { get; private set; }

        public void LogException(Exception value)
        {
            ExceptionCount++;
            LoggedException = value;
        }

        public void LogInfo(object message)
        {
            OtherLogCount++;
        }

        public void LogInfoFormat(string format, params object[] args)
        {
            OtherLogCount++;
        }

        public void LogWarning(object message)
        {
            OtherLogCount++;
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            OtherLogCount++;
        }

        public void LogError(object message)
        {
            OtherLogCount++;
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            OtherLogCount++;
        }
    }
}
