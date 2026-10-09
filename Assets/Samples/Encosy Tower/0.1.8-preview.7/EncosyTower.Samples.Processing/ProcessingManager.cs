using System;
using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.Processing
{
    internal sealed partial class ProcessingManager : MonoBehaviour
    {
        [SerializeField] private Button _buttonClearLog;
        [SerializeField] private Toggle _toggleRegister;
        [SerializeField] private Button _buttonProcess;
        [SerializeField] private Button _buttonProcessAsync;
        [SerializeField] private Toggle _toggleWaitForHandler;
        [SerializeField] private TMP_InputField _inputText;
        [SerializeField] private TMP_InputField _output;
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private GameObject _asyncIndicatorPrefab;

        private GameObject _asyncIndicator;
        private readonly StringBuilderLogger _logger = new();
        private readonly List<ProcessRegistry> _registries = new();

        private bool _waitForHandler;
        private string _text;

        private void Awake()
        {
            _buttonClearLog.onClick.AddListener(Button_ClearLog);
            _toggleRegister.onValueChanged.AddListener(Toggle_Register);
            _buttonProcess.onClick.AddListener(Button_Process);
            _buttonProcessAsync.onClick.AddListener(Button_ProcessAsync);
            _toggleWaitForHandler.onValueChanged.AddListener(Toggle_WaitForHandler);
            _inputText.onValueChanged.AddListener(TextField_SetText);

            _text = _inputText.text;
            _asyncIndicator = Instantiate(_asyncIndicatorPrefab, _popupRoot.transform, false);
            _asyncIndicator.SetActive(false);

            _logger.OnLogEntryWritten += UpdateOutput;
            _logger.LogInfo("Processing Manager initialized.");
        }

        private void OnDestroy()
        {
            _registries.Unregister();
            _logger.OnLogEntryWritten -= UpdateOutput;
        }

        private void Register()
        {
            _registries.Unregister();

            var hub = GlobalProcessor.Instance.UnityScope(this);
            var registrationHub = hub.WithState(this).WithRegistries(_registries);

            FormatTextRequest.Register(in registrationHub, ProcessText);
            FormatTextRequest.Async.Register(in registrationHub, ProcessTextAsync);
        }

        private void Process()
        {
            var hub = GlobalProcessor.Instance.UnityScope(this);
            var context = ProcessingContext.DropIfNoHandler(logger: _logger, token: destroyCancellationToken);
            var result = FormatTextRequest.TryProcess(in hub, new(_text), context);

            if (result.TryGetValue(out var text))
            {
                Log($"Sync result: {text}");
            }
        }

        private async UnityTask ProcessAsync()
        {
            _asyncIndicator.SetActive(true);

            try
            {
                var hub = GlobalProcessor.Instance.UnityScope(this);
                var context = _waitForHandler
                    ? ProcessingContext.WaitForHandler(logger: _logger, token: destroyCancellationToken)
                    : ProcessingContext.DropIfNoHandler(logger: _logger, token: destroyCancellationToken);

                FormatTextRequest.Async request = new FormatTextRequest(_text);

                var result = await FormatTextRequest.Async.TryProcess(in hub, request, context);

                if (result.TryGetValue(out var text))
                {
                    Log($"Async result: {text}");
                }
            }
            finally
            {
                if (_asyncIndicator.IsValid())
                {
                    _asyncIndicator.SetActive(false);
                }
            }
        }

        private void Log(string text)
        {
            _logger.LogLine($"[{DateTime.Now.TimeOfDay:hh\\:mm\\:ss}] {text}");
        }

        private void UpdateOutput()
        {
            _output.text = _logger.ToString();
        }

        private static string ProcessText(ProcessingManager state, FormatTextRequest request, ProcessingContext context)
        {
            return request.Text.ToUpperInvariant();
        }

        private static async UnityTask<string> ProcessTextAsync(
              ProcessingManager state
            , FormatTextRequest.Async request
            , ProcessingContext context
        )
        {
            await UnityTask.NextFrameAsync(context.Token);

            FormatTextRequest formatRequest = request;
            return formatRequest.Text.ToUpperInvariant();
        }

        #region    UI EVENTS
        #endregion =========

        private void Button_ClearLog()
        {
            _logger.Clear();
            _output.text = string.Empty;
        }

        private void Toggle_Register(bool enabled)
        {
            _registries.Unregister();

            if (enabled)
            {
                Register();
            }
        }

        private void Button_Process()
        {
            Process();
        }

        private void Button_ProcessAsync()
        {
            UnityTaskExtensions.Forget(ProcessAsync());
        }

        private void Toggle_WaitForHandler(bool enabled)
        {
            _waitForHandler = enabled;
        }

        private void TextField_SetText(string value)
        {
            _text = value;
        }

        [Processing(ApiMode.Both, State = StateMode.Stateful, Scope = typeof(ProcessingManager))]
        internal readonly partial record struct FormatTextRequest(string Text) : IRequest<string>;
    }
}
