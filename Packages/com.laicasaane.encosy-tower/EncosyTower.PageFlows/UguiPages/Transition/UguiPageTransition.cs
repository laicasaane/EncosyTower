#if UNITY_UGUI

using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    public abstract class UguiPageTransition : MonoBehaviour, IPageTransition
    {
        public string identifier;

        public abstract bool ForceRunHide { get; }

        public abstract bool ForceRunShow { get; }

        public abstract UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        );

        public abstract void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        );

        public abstract UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token);

        public abstract UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token);
    }
}

#endif
