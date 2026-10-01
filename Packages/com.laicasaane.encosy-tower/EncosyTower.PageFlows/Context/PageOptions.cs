using System;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.PageFlows
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
    public struct PageOptions
    {
        public ShowOperationOptions showOptions;
        public HideOperationOptions hideOptions;

        public static Options SelectHideOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.hideThisPage
                : pageToShowOptions.showOptions.hideOtherPage;
        }

        public static Options SelectShowOptions(
              PageTransition transition
            , in PageOptions pageToHideOptions
            , in PageOptions pageToShowOptions
        )
        {
            return transition == PageTransition.Hide
                ? pageToHideOptions.hideOptions.showOtherPage
                : pageToShowOptions.showOptions.showThisPage;
        }

        [Serializable]
        [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
        public struct ShowOperationOptions
        {
            public Options showThisPage;
            public Options hideOtherPage;
        }

        [Serializable]
        [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
        public struct HideOperationOptions
        {
            public Options hideThisPage;
            public Options showOtherPage;
        }

        [Serializable]
        [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
        public struct Options
        {
            public bool forceUse;
            public PageTransitionOptions transitionOptions;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Option<PageTransitionOptions> GetTransitionOptions()
                => Option.SomeIf(forceUse, transitionOptions);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetTransitionOptions(Option<PageTransitionOptions> value)
                => transitionOptions = value.GetValueOrDefault(transitionOptions);
        }
    }
}
