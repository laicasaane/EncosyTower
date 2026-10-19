using System;
using UnityEngine;

namespace EncosyTower.PageFlows.UitkPages.Transitions
{
    /// <summary>The serialized settings of a <see cref="UitkUssClassTransition"/>.</summary>
    [Serializable]
    public struct UitkUssClassTransitionSettings
    {
        /// <summary>The USS class that holds the start state of the show.</summary>
        [Tooltip("The USS class that holds the start state of the show.")]
        public string enterFromClass;

        /// <summary>The USS class that holds the end state of the show.</summary>
        [Tooltip("The USS class that holds the end state of the show.")]
        public string enterToClass;

        /// <summary>The USS class that holds the start state of the hide.</summary>
        [Tooltip("The USS class that holds the start state of the hide.")]
        public string exitFromClass;

        /// <summary>The USS class that holds the end state of the hide.</summary>
        [Tooltip("The USS class that holds the end state of the hide.")]
        public string exitToClass;

        /// <summary>Whether the show step runs even with NoTransition.</summary>
        [Tooltip("Whether the show step runs even with NoTransition.")]
        public bool forceRunShow;

        /// <summary>Whether the hide step runs even with NoTransition.</summary>
        [Tooltip("Whether the hide step runs even with NoTransition.")]
        public bool forceRunHide;

        /// <summary>Copies the settings to a transition.</summary>
        /// <param name="transition">The transition to configure.</param>
        public readonly void ApplyTo(UitkUssClassTransition transition)
        {
            transition.EnterFromClass = enterFromClass;
            transition.EnterToClass = enterToClass;
            transition.ExitFromClass = exitFromClass;
            transition.ExitToClass = exitToClass;
            transition.ForceRunShow = forceRunShow;
            transition.ForceRunHide = forceRunHide;
        }
    }
}
