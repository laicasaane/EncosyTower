using System.Collections.Generic;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages.Transitions
{
    /// <summary>Animates a page element with USS transitions between USS classes.</summary>
    /// <remarks>
    /// <para>
    /// A step sets the <c>From</c> class, then the <c>To</c> class, so the USS transitions of the element play.
    /// The step ends when they end. Without classes for a step, the transition uses a preset: it fades the opacity
    /// of the element and scales it from 0.98 over 0.2 seconds.
    /// </para>
    /// <para>
    /// <see cref="PageTransitionOptions.ZeroDuration"/> applies the end state at once. An abort continues from the
    /// current state. The element detaching from its panel ends the step with the end state.
    /// </para>
    /// </remarks>
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
        private PageAbortHandle _abort;
        private bool _show;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private bool _hasRun;
        private int _runId;

        /// <summary>Creates a transition for an element.</summary>
        /// <param name="slot">The element to animate.</param>
        public UitkUssClassTransition(VisualElement slot)
        {
            _slot = slot;
            _slot.RegisterCallback<TransitionRunEvent>(OnTransitionRun);
            _slot.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            _slot.RegisterCallback<TransitionCancelEvent>(OnTransitionCancel);
            _slot.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>Gets or sets the USS class that holds the start state of the show.</summary>
        public string EnterFromClass { get; set; }

        /// <summary>Gets or sets the USS class that holds the end state of the show.</summary>
        public string EnterToClass { get; set; }

        /// <summary>Gets or sets the USS class that holds the start state of the hide.</summary>
        public string ExitFromClass { get; set; }

        /// <summary>Gets or sets the USS class that holds the end state of the hide.</summary>
        public string ExitToClass { get; set; }

        /// <inheritdoc/>
        public bool ForceRunShow { get; set; }

        /// <inheritdoc/>
        public bool ForceRunHide { get; set; }

        /// <summary>Gets whether a step is running.</summary>
        public bool IsRunning => _completion != null;

        /// <summary>Gets the element the transition animates.</summary>
        public VisualElement Slot => _slot;

        private static List<TimeValue> CreateDuration(float seconds)
            => new() { new TimeValue(seconds, TimeUnit.Second) };

        /// <inheritdoc/>
        public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _zeroShowDuration = context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration);
            _zeroHideDuration = context.HideOptions.Contains(PageTransitionOptions.ZeroDuration);
            return UnityTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void OnAfterTransition(PageTransitionContext context)
        {
            var show = context.Transition == PageTransition.Show;
            var options = show ? context.ShowOptions : context.HideOptions;
            var forceRun = show ? ForceRunShow : ForceRunHide;

            if (options.Contains(PageTransitionOptions.NoTransition) && forceRun == false)
            {
                return;
            }

            Finish(show);
        }

        /// <inheritdoc/>
        public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: true
                , zeroDuration: _zeroShowDuration || context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        /// <inheritdoc/>
        public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: false
                , zeroDuration: _zeroHideDuration || context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        private UnityTask Run(PageTransitionContext context, bool show, bool zeroDuration, CancellationToken token)
        {
            _abort = context.Abort;

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

            if (context.Abort.IsRequested)
            {
                RemoveTransitionClasses();

                if (token.CanBeCanceled)
                {
                    _registration = token.Register(OnCanceled);
                }

                ApplyTargetState(runId, zeroDuration);
                return _completion.Task;
            }

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
            if (IsRunning == false)
            {
                return;
            }

            if (_abort.IsRequested)
            {
                Complete();
                return;
            }

            Finish(_show);
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
