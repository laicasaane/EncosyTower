#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    /// <summary>
    /// A field that edits a <see cref="Quaternion"/> as Euler angles in a vector field.
    /// </summary>
    /// <remarks>
    /// The angles shown are <see cref="Quaternion.eulerAngles"/> of the value, so a round trip through the field can
    /// normalize them. The inner vector field ignores <c>SerializedObjectBindEvent</c>, so the bind of the
    /// surrounding row does not take it over.
    /// </remarks>
    internal sealed class OverridableQuaternionField : BaseField<Quaternion>
    {
        private readonly EulerField _eulerField;

        /// <summary>
        /// Creates the field without a label.
        /// </summary>
        public OverridableQuaternionField() : this(new EulerField())
        {
        }

        private OverridableQuaternionField(EulerField eulerField) : base(label: null, visualInput: eulerField)
        {
            _eulerField = eulerField;
            AddToClassList("encosy-overridable__quaternion");
            _eulerField.RegisterValueChangedCallback(OnEulerChanged);
        }

        /// <inheritdoc/>
        public override void SetValueWithoutNotify(Quaternion newValue)
        {
            base.SetValueWithoutNotify(newValue);
            _eulerField.SetValueWithoutNotify(rawValue.eulerAngles);
        }

        /// <inheritdoc/>
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
