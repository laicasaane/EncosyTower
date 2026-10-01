using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.Internal;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace UnityEngine.UIElements
{
    public static class EncosyBasePopupFieldExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCreateMenuCallback<TValueType, TValueChoice>(
              [NotNull] this BasePopupField<TValueType, TValueChoice> self
            , Func<AbstractGenericMenu> createMenuCallback
        )
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            BasePopupFieldInternalAccessor.SetCreateMenuCallback(self, createMenuCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGenericMenu<TValueType, TValueChoice>(
              [NotNull] this BasePopupField<TValueType, TValueChoice> self
            , AbstractGenericMenu genericMenu
        )
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            BasePopupFieldInternalAccessor.SetGenericMenu(self, genericMenu);
        }
    }
}
