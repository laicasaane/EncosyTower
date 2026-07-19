using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements.Internal
{
    internal static class BasePopupFieldInternalAccessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCreateMenuCallback<TValueType, TValueChoice>(
              [NotNull] BasePopupField<TValueType, TValueChoice> field
            , Func<AbstractGenericMenu> createMenuCallback
        )
        {
            field.createMenuCallback = createMenuCallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGenericMenu<TValueType, TValueChoice>(
              [NotNull] BasePopupField<TValueType, TValueChoice> field
            , AbstractGenericMenu genericMenu
        )
        {
            field.m_GenericMenu = genericMenu;
        }
    }
}
