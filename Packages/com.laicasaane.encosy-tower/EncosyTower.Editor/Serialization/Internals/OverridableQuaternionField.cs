#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal sealed class OverridableQuaternionField : BaseField<Quaternion>
    {
        private readonly EulerField _eulerField;

        public OverridableQuaternionField() : this(new EulerField())
        {
        }

        private OverridableQuaternionField(EulerField eulerField) : base(label: null, visualInput: eulerField)
        {
            _eulerField = eulerField;
            AddToClassList("encosy-overridable__quaternion");
            _eulerField.RegisterValueChangedCallback(OnEulerChanged);
        }

        public override void SetValueWithoutNotify(Quaternion newValue)
        {
            base.SetValueWithoutNotify(newValue);
            _eulerField.SetValueWithoutNotify(rawValue.eulerAngles);
        }

        protected override void UpdateMixedValueContent()
        {
            _eulerField.showMixedValue = showMixedValue;
        }

        private void OnEulerChanged(ChangeEvent<Vector3> evt)
        {
            if (evt.target != _eulerField)
            {
                return;
            }

            value = Quaternion.Euler(evt.newValue);
            evt.StopPropagation();
        }

        private sealed class EulerField : Vector3Field
        {
            protected override void HandleEventBubbleUp(EventBase evt)
            {
                if (string.Equals(evt.GetType().Name, "SerializedObjectBindEvent", StringComparison.Ordinal))
                {
                    evt.StopPropagation();
                    return;
                }

                base.HandleEventBubbleUp(evt);
            }
        }
    }
}

#endif
