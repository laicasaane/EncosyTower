#pragma warning disable 0219

using EncosyTower.Mvvm.ViewBinding;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SCM = global::System.ComponentModel;
using g__SCG = global::System.Collections.Generic;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ET = global::EncosyTower.Common;
using g__ETMCM = global::EncosyTower.Mvvm.ComponentModel;
using g__ETMCMSG = global::EncosyTower.Mvvm.ComponentModel.SourceGen;
using g__ETMI = global::EncosyTower.Mvvm.Input;
using g__ETMVB = global::EncosyTower.Mvvm.ViewBinding;
using g__ETMVBSG = global::EncosyTower.Mvvm.ViewBinding.SourceGen;
using g__ETV = global::EncosyTower.Variants;
using g__ETVC = global::EncosyTower.Variants.Converters;
using g__UE = global::UnityEngine;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__ETMVBSG.BindingPropertyMethodInfo("SetValue", typeof(int))]
    [g__ETMVBSG.BindingCommandMethodInfo("OnClick", typeof(void))]
    partial class SampleBinder : g__ETMVB.IBinder
    {
        /// <summary>The name of <see cref="SetValue"/></summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public const string BindingProperty_SetValue = nameof(SampleBinder.SetValue);


        /// <summary>The name of <see cref="OnClick"/></summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public const string BindingCommand_OnClick = nameof(SampleBinder.OnClick);


        /// <summary>The binding property for <see cref="SetValue"/></summary>
        [g__UE.SerializeField]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__ETMVBSG.GeneratedBindingProperty(BindingProperty_SetValue, typeof(int))]
        private g__ETMVB.BindingProperty _bindingFieldForSetValue =  new g__ETMVB.BindingProperty();


        /// <summary>The converter for the parameter of <see cref="SetValue"/></summary>
        [g__UE.SerializeField]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__ETMVBSG.GeneratedConverter(BindingProperty_SetValue, typeof(int))]
        private g__ETMVB.Converter _converterForSetValue = new g__ETMVB.Converter();


        /// <summary>The binding command for <see cref="OnClick"/></summary>
        [g__UE.SerializeField]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__ETMVBSG.GeneratedBindingCommand(BindingCommand_OnClick, typeof(void))]
        private g__ETMVB.BindingCommand _bindingCommandForOnClick =  new g__ETMVB.BindingCommand();


        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        private readonly g__ETVC.CachedVariantConverter<int> _variantConverterInt = g__ETVC.CachedVariantConverter<int>.Default;


        /// <summary>
        /// The listener that binds <see cref="SetValue"/>
        /// to the property chosen by <see cref="_bindingFieldForSetValue"/>.
        /// </summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
        private readonly g__ETMCM.PropertyChangeEventListener<SampleBinder> _listenerForSetValue;


        /// <summary>
        /// The relay command that binds <see cref="OnClick"/>
        /// to the command chosen by <see cref="_bindingCommandForOnClick"/>.
        /// </summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
        private g__ETMI.IRelayCommand _relayCommandForOnClick;


        /// <summary>A flag indicates whether this binder is listening to events from <see cref="Context"/>.</summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        private bool _isListening_TestProject_SampleBinder;

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public SampleBinder()
        {
            OnBeforeConstructor();

            this._listenerForSetValue = new g__ETMCM.PropertyChangeEventListener<SampleBinder>(this)
            {
                OnEventAction = static (instance, args) => instance.SetValue__Variant(instance._converterForSetValue.Convert(args.NewValue))
            };

            OnAfterConstructor();
        }

        /// <summary>Executes the logic at the beginning of the default constructor.</summary>
        /// <remarks>This method is invoked at the beginning of the default constructor.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        partial void OnBeforeConstructor();

        /// <summary>Executes the logic at the end of the default constructor.</summary>
        /// <remarks>This method is invoked at the end of the default constructor.</remarks>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        partial void OnAfterConstructor();

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual g__ETMCM.IObservableObject Context { get; private set; }

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual void StartListening()
        {
            if (this._isListening_TestProject_SampleBinder) return;

            this._isListening_TestProject_SampleBinder = true;

            if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
            {
                if (inpc.AttachPropertyChangedListener(this._bindingFieldForSetValue.TargetPropertyName, this._listenerForSetValue) == false)
                {
                    OnBindPropertyFailed(this._bindingFieldForSetValue);
                }

            }

            if (this.Context is g__ETMI.ICommandListener cl)
            {
                if (cl.TryGetCommand(this._bindingCommandForOnClick.TargetCommandName, out this._relayCommandForOnClick) == false)
                {
                    OnBindCommandFailed(this._bindingCommandForOnClick);
                }

            }
        }

        partial void OnBindPropertyFailed(g__ETMVB.BindingProperty bindingProperty);

        partial void OnBindCommandFailed(g__ETMVB.BindingCommand bindingCommand);

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual void StopListening()
        {
            if (this._isListening_TestProject_SampleBinder == false) return;

            this._isListening_TestProject_SampleBinder = false;

            this._listenerForSetValue.Detach();

            this._relayCommandForOnClick = null;
        }

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)
        {
            switch (bindingPropertyName)
            {
                case BindingProperty_SetValue:
                {
                    this._bindingFieldForSetValue.TargetPropertyName = targetPropertyName;
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual bool SetAdapter(string bindingPropertyName, g__ETMVB.IAdapter adapter)
        {
            switch (bindingPropertyName)
            {
                case BindingProperty_SetValue:
                {
                    this._converterForSetValue.Adapter = adapter;
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// This overload will try to get the value of type <see cref="int"/>
        /// from <see cref="g__ETV.Variant"/>
        /// to pass into <see cref="SetValue"/>.
        /// </summary>
        /// <remarks>This method is not intended to be used directly by user code.</remarks>
        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
        [g__S.Obsolete("This method is not intended to be used directly by user code.")]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        private void SetValue__Variant(in g__ETV.Variant variant)
        {
            if (this._variantConverterInt.TryGetValue(variant, out int value))
            {
                SetValue(value);
            }
        }


        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        partial void OnClick()
        {
            this._relayCommandForOnClick?.Execute();
        }


        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual bool SetTargetCommandName(string bindingCommandName, string targetCommandName)
        {
            switch (bindingCommandName)
            {
                case BindingCommand_OnClick:
                {
                    this._bindingCommandForOnClick.TargetCommandName = targetCommandName;
                    return true;
                }

            }

            return false;
        }

        /// <inheritdoc/>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.Binders.BinderGenerator", "0.1.7-preview.3")]
        public virtual void RefreshContext()
        {
            if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
            {
                if (inpc.NotifyPropertyChanged(this._bindingFieldForSetValue.TargetPropertyName, this._listenerForSetValue) == false)
                {
                    OnBindPropertyFailed(this._bindingFieldForSetValue);
                }

            }

        }

    }


}

