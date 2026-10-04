using System;

namespace EncosyTower.PageFlows.UitkPages
{
    [Serializable]
    public struct UitkUssClassTransitionSettings
    {
        public string enterFromClass;
        public string enterToClass;
        public string exitFromClass;
        public string exitToClass;
        public bool forceRunShow;
        public bool forceRunHide;

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
