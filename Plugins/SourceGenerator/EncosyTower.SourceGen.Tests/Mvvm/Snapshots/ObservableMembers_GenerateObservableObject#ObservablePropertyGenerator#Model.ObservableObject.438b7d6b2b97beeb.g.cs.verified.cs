#pragma warning disable 0219

using EncosyTower.Mvvm.ComponentModel;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SCM = global::System.ComponentModel;
using g__SCG = global::System.Collections.Generic;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETMCM = global::EncosyTower.Mvvm.ComponentModel;
using g__ETMCMSG = global::EncosyTower.Mvvm.ComponentModel.SourceGen;
using g__ETV = global::EncosyTower.Variants;
using g__ETVC = global::EncosyTower.Variants.Converters;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__ETMCMSG.NotifyPropertyChangingInfo("Value", typeof(int))]
    [g__ETMCMSG.NotifyPropertyChangingInfo("Name", typeof(string))]
    [g__ETMCMSG.NotifyPropertyChangedInfo("Value", typeof(int))]
    [g__ETMCMSG.NotifyPropertyChangedInfo("Name", typeof(string))]
    [g__ETMCMSG.NotifyPropertyChangedInfo("Progress", typeof(float))]
    partial class Model : g__ETMCM.IObservableObject
        , g__ETMCM.INotifyPropertyChanging
        , g__ETMCM.INotifyPropertyChanged

    {
        /// <summary>The name of <see cref="Value"/></summary>
        [g__ETMCMSG.GeneratedPropertyNameConstant]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public const string PropertyName_Value = nameof(Model.Value);

        /// <summary>The name of <see cref="Name"/></summary>
        [g__ETMCMSG.GeneratedPropertyNameConstant]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public const string PropertyName_Name = nameof(Model.Name);

        /// <summary>The name of <see cref="Progress"/></summary>
        [g__ETMCMSG.GeneratedPropertyNameConstant][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public const string PropertyName_Progress = nameof(Model.Progress);


        [g__ETMCMSG.GeneratedPropertyChangingEventHandler][g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private event g__ETMCM.PropertyChangingEventHandler _onChangingValue;

        [g__ETMCMSG.GeneratedPropertyChangedEventHandler][g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private event g__ETMCM.PropertyChangedEventHandler _onChangedValue;

        [g__ETMCMSG.GeneratedPropertyChangingEventHandler][g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private event g__ETMCM.PropertyChangingEventHandler _onChangingName;

        [g__ETMCMSG.GeneratedPropertyChangedEventHandler][g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private event g__ETMCM.PropertyChangedEventHandler _onChangedName;

        [g__ETMCMSG.GeneratedPropertyChangedEventHandler][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private event g__ETMCM.PropertyChangedEventHandler _onChangedProgress;

        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private readonly g__ETVC.CachedVariantConverter<int> _variantConverterInt = g__ETVC.CachedVariantConverter<int>.Default;

        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private readonly g__ETVC.CachedVariantConverter<float> _variantConverterFloat = g__ETVC.CachedVariantConverter<float>.Default;

        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private readonly g__ETVC.CachedVariantConverter<string> _variantConverterString = g__ETVC.CachedVariantConverter<string>.Default;

        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private string _name;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private string Get_Name()
        {
            return this._name;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        private void Set_Name(string value)
        {
            if (g__SCG.EqualityComparer<string>.Default.Equals(this._name, value)) return;

            {
                var oldValue = this._name;

                OnNameChanging(oldValue, value);

                var variantConverterString = this._variantConverterString;
                var argsName = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Name, variantConverterString.ToVariant(oldValue), variantConverterString.ToVariant(value));
                this._onChangingName?.Invoke(argsName);

                this._name = value;

                OnNameChanged(oldValue, value);
                this._onChangedName?.Invoke(argsName);
            }

        }

        /// <inheritdoc cref="_value"/>
        [g__ETMCMSG.GeneratedObservableProperty][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public int Value
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => this._value;
            set
            {
                if (g__SCG.EqualityComparer<int>.Default.Equals(this._value, value)) return;

                {
                    var oldValue = this._value;

                    OnValueChanging(oldValue, value);

                    var variantConverterInt = this._variantConverterInt;
                    var argsValue = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Value, variantConverterInt.ToVariant(oldValue), variantConverterInt.ToVariant(value));
                    this._onChangingValue?.Invoke(argsValue);

                    this._value = value;

                    OnValueChanged(oldValue, value);
                    this._onChangedValue?.Invoke(argsValue);
                }

                {
                    var oldValueProperty = this.Progress;

                    OnProgressChanged(oldValueProperty, this.Progress);

                    var converterFloat = this._variantConverterFloat;
                    var argsProgress = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Progress, converterFloat.ToVariant(oldValueProperty), converterFloat.ToVariant(this.Progress));
                    this._onChangedProgress?.Invoke(argsProgress);
                }

            }
        }

        /// <summary>Executes the logic for when <see cref="Value"/> is changing.</summary>
        /// <param name="oldValue">The previous property value that is being replaced.</param>
        /// <param name="newValue">The new property value being set.</param>
        /// <remarks>This method is invoked right before the value of <see cref="Value"/> is changed.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        partial void OnValueChanging(int oldValue, int newValue);

        /// <summary>Executes the logic for when <see cref="Value"/> just changed.</summary>
        /// <param name="oldValue">The previous property value that was replaced.</param>
        /// <param name="newValue">The new property value that was set.</param>
        /// <remarks>This method is invoked right after the value of <see cref="Value"/> is changed.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        partial void OnValueChanged(int oldValue, int newValue);

        /// <summary>Executes the logic for when <see cref="Name"/> is changing.</summary>
        /// <param name="oldValue">The previous property value that is being replaced.</param>
        /// <param name="newValue">The new property value being set.</param>
        /// <remarks>This method is invoked right before the value of <see cref="Name"/> is changed.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        partial void OnNameChanging(string oldValue, string newValue);

        /// <summary>Executes the logic for when <see cref="Name"/> just changed.</summary>
        /// <param name="oldValue">The previous property value that was replaced.</param>
        /// <param name="newValue">The new property value that was set.</param>
        /// <remarks>This method is invoked right after the value of <see cref="Name"/> is changed.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        partial void OnNameChanged(string oldValue, string newValue);

        /// <summary>Executes the logic for when <see cref="Progress"/> just changed.</summary>
        /// <param name="value">The new property value that was set.</param>
        /// <remarks>This method is invoked right after the value of <see cref="Progress"/> is changed.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        partial void OnProgressChanged(float oldValue, float newValue);

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanging.AttachPropertyChangingListener{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool AttachPropertyChangingListener<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            if (listener == null) throw new g__S.ArgumentNullException(nameof(listener));

            switch (propertyName)
            {
                case PropertyName_Value:
                {
                    this._onChangingValue += listener.OnEvent;
                    listener.OnDetachAction = (listener) => this._onChangingValue -= listener.OnEvent;
                    return true;
                }

                case PropertyName_Name:
                {
                    this._onChangingName += listener.OnEvent;
                    listener.OnDetachAction = (listener) => this._onChangingName -= listener.OnEvent;
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.AttachPropertyChangedListener{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool AttachPropertyChangedListener<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            if (listener == null) throw new g__S.ArgumentNullException(nameof(listener));

            switch (propertyName)
            {
                case PropertyName_Value:
                {
                    this._onChangedValue += listener.OnEvent;
                    listener.OnDetachAction = (listener) => this._onChangedValue -= listener.OnEvent;
                    return true;
                }

                case PropertyName_Name:
                {
                    this._onChangedName += listener.OnEvent;
                    listener.OnDetachAction = (listener) => this._onChangedName -= listener.OnEvent;
                    return true;
                }

                case PropertyName_Progress:
                {
                    this._onChangedProgress += listener.OnEvent;
                    listener.OnDetachAction = (listener) => this._onChangedProgress -= listener.OnEvent;
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool NotifyPropertyChanged<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            if (listener == null) throw new g__S.ArgumentNullException(nameof(listener));

            switch (propertyName)
            {
                case PropertyName_Value:
                {
                    var variantConverterInt = this._variantConverterInt;
                    var variant = variantConverterInt.ToVariant(this._value);
                    var argsValue = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Value, variant, variant);
                    listener.OnEvent(argsValue);
                    return true;
                }

                case PropertyName_Name:
                {
                    var variantConverterString = this._variantConverterString;
                    var variant = variantConverterString.ToVariant(this._name);
                    var argsName = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Name, variant, variant);
                    listener.OnEvent(argsName);
                    return true;
                }

                case PropertyName_Progress:
                {
                    var converterFloat = this._variantConverterFloat;
                    var variant = converterFloat.ToVariant(this.Progress);
                    var argsProgress = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Progress, variant, variant);
                    listener.OnEvent(argsProgress);
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged(string)" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool NotifyPropertyChanged(string propertyName)
        {

            switch (propertyName)
            {
                case PropertyName_Value:
                {
                    var variantConverterInt = this._variantConverterInt;
                    var variant = variantConverterInt.ToVariant(this._value);
                    var argsValue = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Value, variant, variant);
                    this._onChangedValue?.Invoke(argsValue);
                    return true;
                }

                case PropertyName_Name:
                {
                    var variantConverterString = this._variantConverterString;
                    var variant = variantConverterString.ToVariant(this._name);
                    var argsName = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Name, variant, variant);
                    this._onChangedName?.Invoke(argsName);
                    return true;
                }

                case PropertyName_Progress:
                {
                    var converterFloat = this._variantConverterFloat;
                    var variant = converterFloat.ToVariant(this.Progress);
                    var argsProgress = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Progress, variant, variant);
                    this._onChangedProgress?.Invoke(argsProgress);
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged()" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual void NotifyPropertyChanged()
        {

            {
                var variantConverterInt = this._variantConverterInt;
                var variant = variantConverterInt.ToVariant(this._value);
                var argsValue = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Value, variant, variant);
                this._onChangedValue?.Invoke(argsValue);
            }

            {
                var variantConverterString = this._variantConverterString;
                var variant = variantConverterString.ToVariant(this._name);
                var argsName = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Name, variant, variant);
                this._onChangedName?.Invoke(argsName);
            }

            {
                var converterFloat = this._variantConverterFloat;
                var variant = converterFloat.ToVariant(this.Progress);
                var argsProgress = new g__ETMCM.PropertyChangeEventArgs(this, PropertyName_Progress, variant, variant);
                this._onChangedProgress?.Invoke(argsProgress);
            }

        }

        /// <inheritdoc cref="g__ETMCM.IObservableObject.TryGetMemberObservableObject(g__SCG.Queue{string}, out g__ETMCM.IObservableObject)"/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public bool TryGetMemberObservableObject(g__SCG.Queue<string> propertyNames, out g__ETMCM.IObservableObject result)
        {
            result = default;
            return false;
        }

    }


}

