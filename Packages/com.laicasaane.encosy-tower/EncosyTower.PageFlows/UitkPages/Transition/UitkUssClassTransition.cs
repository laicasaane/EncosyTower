using System.Collections.Generic;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    public sealed class UitkUssClassTransition : IPageTransition
    {
        private const float PRESET_DURATION_SECONDS = 0.2f;
        private const float PRESET_HIDDEN_SCALE = 0.98f;

        private static readonly StylePropertyName s_opacity = new("opacity");
        private static readonly StylePropertyName s_scale = new("scale");

        private readonly VisualElement _slot;
        private readonly HashSet<StylePropertyName> _runningProperties = new();

        private UnityTaskCompletionSource _completion;
        private CancellationTokenRegistration _registration;
        private bool _show;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private bool _hasRun;
        private int _runId;

        public UitkUssClassTransition(VisualElement slot)
        {
            _slot = slot;
            _slot.RegisterCallback<TransitionRunEvent>(OnTransitionRun);
            _slot.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            _slot.RegisterCallback<TransitionCancelEvent>(OnTransitionCancel);
            _slot.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        public string EnterFromClass { get; set; }

        public string EnterToClass { get; set; }

        public string ExitFromClass { get; set; }

        public string ExitToClass { get; set; }

        public bool ForceRunShow { get; set; }

        public bool ForceRunHide { get; set; }

        public bool IsRunning => _completion != null;

        public VisualElement Slot => _slot;

        private static List<TimeValue> CreateDuration(float seconds)
            => new() { new TimeValue(seconds, TimeUnit.Second) };

        public UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        )
        {
            _zeroShowDuration = showOptions.Contains(PageTransitionOptions.ZeroDuration);
            _zeroHideDuration = hideOptions.Contains(PageTransitionOptions.ZeroDuration);
            return UnityTask.CompletedTask;
        }

        public void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            Finish(transition == PageTransition.Show);
        }

        public UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token)
            => Run(
                  show: true
                , zeroDuration: _zeroShowDuration || options.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        public UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token)
            => Run(
                  show: false
                , zeroDuration: _zeroHideDuration || options.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        private UnityTask Run(bool show, bool zeroDuration, CancellationToken token)
        {
            if (IsRunning)
            {
                Finish(_show);
            }

            if (_slot.panel == null || token.IsCancellationRequested)
            {
                ApplyEndState(show);
                return UnityTask.CompletedTask;
            }

            var runId = ++_runId;

            _show = show;
            _hasRun = false;
            _runningProperties.Clear();
            _completion = new UnityTaskCompletionSource();
            ApplyStartState(show);

            if (token.CanBeCanceled)
            {
                _registration = token.Register(OnCanceled);
            }

            _slot.schedule.Execute(WaitForStartStyle);
            return _completion.Task;

            void WaitForStartStyle()
            {
                _slot.schedule.Execute(ApplyTarget);
            }

            void ApplyTarget()
            {
                ApplyTargetState(runId, zeroDuration);
            }
        }

        private void ApplyStartState(bool show)
        {
            RemoveTransitionClasses();
            _slot.style.transitionDuration = CreateDuration(0f);

            var fromClass = show ? EnterFromClass : ExitFromClass;

            if (HasClasses(show))
            {
                AddClass(fromClass);
                return;
            }

            SetPresetValues(visible: show == false);
        }

        private void ApplyTargetState(int runId, bool zeroDuration)
        {
            if (runId != _runId || IsRunning == false)
            {
                return;
            }

            var style = _slot.style;

            if (HasClasses(_show))
            {
                RemoveClass(_show ? EnterFromClass : ExitFromClass);
                AddClass(_show ? EnterToClass : ExitToClass);

                style.transitionDuration = zeroDuration
                    ? CreateDuration(0f)
                    : new StyleList<TimeValue>(StyleKeyword.Null);
            }
            else
            {
                style.transitionProperty = new List<StylePropertyName> { s_opacity, s_scale };
                style.transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseOut) };
                style.transitionDuration = CreateDuration(zeroDuration ? 0f : PRESET_DURATION_SECONDS);

                SetPresetValues(visible: _show);
            }

            _slot.schedule.Execute(WaitForRunEvent);

            void WaitForRunEvent()
            {
                _slot.schedule.Execute(CheckRun);
            }

            void CheckRun()
            {
                CompleteIfNotRunning(runId);
            }
        }

        private void CompleteIfNotRunning(int runId)
        {
            if (runId == _runId && IsRunning && _hasRun == false)
            {
                Complete();
            }
        }

        private void OnTransitionRun(TransitionRunEvent evt)
        {
            if (IsRunning == false || evt.target != _slot)
            {
                return;
            }

            _hasRun = true;

            foreach (var property in evt.stylePropertyNames)
            {
                _runningProperties.Add(property);
            }
        }

        private void OnTransitionEnd(TransitionEndEvent evt)
        {
            OnTransitionStopped(evt.target, evt.stylePropertyNames);
        }

        private void OnTransitionCancel(TransitionCancelEvent evt)
        {
            OnTransitionStopped(evt.target, evt.stylePropertyNames);
        }

        private void OnTransitionStopped(IEventHandler target, StylePropertyNameCollection properties)
        {
            if (IsRunning == false || target != _slot)
            {
                return;
            }

            foreach (var property in properties)
            {
                _runningProperties.Remove(property);
            }

            if (_hasRun && _runningProperties.Count < 1)
            {
                Complete();
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (IsRunning)
            {
                Finish(_show);
            }
        }

        private void OnCanceled()
        {
            if (IsRunning)
            {
                Finish(_show);
            }
        }

        private void Finish(bool show)
        {
            ApplyEndState(show);

            if (IsRunning)
            {
                Complete();
            }
        }

        private void ApplyEndState(bool show)
        {
            RemoveTransitionClasses();

            var style = _slot.style;
            style.transitionDuration = CreateDuration(0f);

            if (HasClasses(show))
            {
                AddClass(show ? EnterToClass : ExitToClass);
                return;
            }

            SetPresetValues(visible: show);
        }

        private void Complete()
        {
            _runId++;
            _registration.Dispose();
            _registration = default;

            var completion = _completion;
            _completion = null;
            completion?.TrySetResult();
        }

        private void SetPresetValues(bool visible)
        {
            var scale = visible ? 1f : PRESET_HIDDEN_SCALE;
            _slot.style.opacity = visible ? 1f : 0f;
            _slot.style.scale = new Scale(new Vector2(scale, scale));
        }

        private bool HasClasses(bool show)
            => show
                ? string.IsNullOrEmpty(EnterFromClass) == false || string.IsNullOrEmpty(EnterToClass) == false
                : string.IsNullOrEmpty(ExitFromClass) == false || string.IsNullOrEmpty(ExitToClass) == false;

        private void RemoveTransitionClasses()
        {
            RemoveClass(EnterFromClass);
            RemoveClass(EnterToClass);
            RemoveClass(ExitFromClass);
            RemoveClass(ExitToClass);
        }

        private void AddClass(string className)
        {
            if (string.IsNullOrEmpty(className) == false)
            {
                _slot.AddToClassList(className);
            }
        }

        private void RemoveClass(string className)
        {
            if (string.IsNullOrEmpty(className) == false)
            {
                _slot.RemoveFromClassList(className);
            }
        }
    }
}
