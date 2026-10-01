using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements.Internal
{
    internal static class BasePopupFieldInternalAccessor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCreateMenuCallback<TValueType, TValueChoice>(
              BasePopupField<TValueType, TValueChoice> field
            , Func<AbstractGenericMenu> createMenuCallback
        )
        {
            field.createMenuCallback = createMenuCallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGenericMenu<TValueType, TValueChoice>(
              BasePopupField<TValueType, TValueChoice> field
            , AbstractGenericMenu genericMenu
        )
        {
            field.m_GenericMenu = genericMenu;
        }
    }
}
