#pragma warning disable 0219

using EncosyTower.Mvvm.ViewBinding.Components;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SCM = global::System.ComponentModel;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETA = global::EncosyTower.Annotations;
using g__ET = global::EncosyTower.Common;
using g__ETMCM = global::EncosyTower.Mvvm.ComponentModel;
using g__ETMI = global::EncosyTower.Mvvm.Input;
using g__ETMVB = global::EncosyTower.Mvvm.ViewBinding;
using g__ETMVBC = global::EncosyTower.Mvvm.ViewBinding.Components;
using g__ETMVBSG = global::EncosyTower.Mvvm.ViewBinding.SourceGen;
using g__ETVC = global::EncosyTower.Variants.Converters;
using g__UE = global::UnityEngine;
using g__UEE = global::UnityEngine.Events;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__S.Serializable]
    [g__ETMVB.Binder]
    [g__ETA.Label("Game Object")]
    partial class GameObjectBinder : g__ETMVBC.MonoBinder<global::UnityEngine.GameObject>
    {
        [g__S.Serializable]
        [g__ETMVB.Binder]
        [g__ETA.Label("Layer", "Game Object")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.MonoBinders.MonoBinderGenerator", "0.1.8-preview.1")]
        public sealed partial class BindingLayer : g__ETMVBC.MonoBindingProperty<global::UnityEngine.GameObject>
        {
            [g__ETMVB.BindingProperty]
            [field: g__UE.HideInInspector]
            private void SetLayer(int value)
            {
                var targets = Targets;
                var length = targets.Length;

                for (var i = 0; i < length; i++)
                {
                    targets[i].layer = value;
                }
            }
        }

        [g__S.Serializable]
        [g__ETMVB.Binder]
        [g__S.Obsolete("GameObject.active is obsolete. Use GameObject.SetActive(), GameObject.activeSelf or GameObject.activeInHierarchy.")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.MonoBinders.MonoBinderGenerator", "0.1.8-preview.1")]
        public sealed partial class BindingActive : g__ETMVBC.MonoBindingProperty<global::UnityEngine.GameObject>
        {
            [g__ETMVB.BindingProperty]
            [field: g__UE.HideInInspector]
            private void SetActive(bool value)
            {
                var targets = Targets;
                var length = targets.Length;

                for (var i = 0; i < length; i++)
                {
                    targets[i].active = value;
                }
            }
        }

        [g__S.Serializable]
        [g__ETMVB.Binder]
        [g__ETA.Label("Is Static", "Game Object")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.MonoBinders.MonoBinderGenerator", "0.1.8-preview.1")]
        public sealed partial class BindingIsStatic : g__ETMVBC.MonoBindingProperty<global::UnityEngine.GameObject>
        {
            [g__ETMVB.BindingProperty]
            [field: g__UE.HideInInspector]
            private void SetIsStatic(bool value)
            {
                var targets = Targets;
                var length = targets.Length;

                for (var i = 0; i < length; i++)
                {
                    targets[i].isStatic = value;
                }
            }
        }

        [g__S.Serializable]
        [g__ETMVB.Binder]
        [g__ETA.Label("Tag", "Game Object")]
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.MonoBinders.MonoBinderGenerator", "0.1.8-preview.1")]
        public sealed partial class BindingTag : g__ETMVBC.MonoBindingProperty<global::UnityEngine.GameObject>
        {
            [g__ETMVB.BindingProperty]
            [field: g__UE.HideInInspector]
            private void SetTag(string value)
            {
                var targets = Targets;
                var length = targets.Length;

                for (var i = 0; i < length; i++)
                {
                    targets[i].tag = value;
                }
            }
        }

    }

    partial class GameObjectBinder : g__ETMVB.IBinder
    {
        [g__ETMVBSG.BindingPropertyMethodInfo("SetLayer", typeof(int))]
        partial class BindingLayer : g__ETMVB.IBinder
        {
            /// <summary>The name of <see cref="SetLayer"/></summary>
            public const string BindingProperty_SetLayer = nameof(BindingLayer.SetLayer);


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedBindingProperty(BindingProperty_SetLayer, typeof(int))]
            private g__ETMVB.BindingProperty _bindingFieldForSetLayer = new g__ETMVB.BindingProperty();


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedConverter(BindingProperty_SetLayer, typeof(int))]
            private g__ETMVB.Converter _converterForSetLayer = new g__ETMVB.Converter();


            private readonly g__ETVC.CachedVariantConverter<int> _variantConverterInt = g__ETVC.CachedVariantConverter<int>.Default;


            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            private readonly g__ETMCM.PropertyChangeEventListener<BindingLayer> _listenerForSetLayer;


            private bool _isListening_BindingLayer;

            public BindingLayer() : base()
            {
                OnBeforeConstructor();

                this._listenerForSetLayer = new g__ETMCM.PropertyChangeEventListener<BindingLayer>(this)
                {
                    OnEventAction = static (instance, args) => instance.SetLayer__Variant(instance._converterForSetLayer.Convert(args.NewValue))
                };

                OnAfterConstructor();
            }

            partial void OnBeforeConstructor();

            partial void OnAfterConstructor();

            /// <inheritdoc/>
            public override void StartListening()
            {
                base.StartListening();

                if (this._isListening_BindingLayer) return;

                this._isListening_BindingLayer = true;

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.AttachPropertyChangedListener(this._bindingFieldForSetLayer.TargetPropertyName, this._listenerForSetLayer) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetLayer);
                    }

                }

            }

            partial void OnBindPropertyFailed(g__ETMVB.BindingProperty bindingProperty);

            /// <inheritdoc/>
            public override void StopListening()
            {
                base.StopListening();

                if (this._isListening_BindingLayer == false) return;

                this._isListening_BindingLayer = false;

                this._listenerForSetLayer.Detach();

            }

            /// <inheritdoc/>
            public override bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)
            {
                if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetLayer:
                    {
                        this._bindingFieldForSetLayer.TargetPropertyName = targetPropertyName;
                        return true;
                    }

                }

                return false;
            }

            /// <inheritdoc/>
            public override bool SetAdapter(string bindingPropertyName, g__ETMVB.IAdapter adapter)
            {
                if (base.SetAdapter(bindingPropertyName, adapter)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetLayer:
                    {
                        this._converterForSetLayer.Adapter = adapter;
                        return true;
                    }

                }

                return false;
            }

            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            [g__S.Obsolete("This method is not intended to be used directly by user code.")]
            private void SetLayer__Variant(in global::EncosyTower.Variants.Variant variant)
            {
                if (this._variantConverterInt.TryGetValue(variant, out int value))
                {
                    SetLayer(value);
                }
            }


            /// <inheritdoc/>
            public override void RefreshContext()
            {
                base.RefreshContext();

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.NotifyPropertyChanged(this._bindingFieldForSetLayer.TargetPropertyName, this._listenerForSetLayer) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetLayer);
                    }

                }
            }

        }

        [g__ETMVBSG.BindingPropertyMethodInfo("SetActive", typeof(bool))]
        partial class BindingActive : g__ETMVB.IBinder
        {
            /// <summary>The name of <see cref="SetActive"/></summary>
            public const string BindingProperty_SetActive = nameof(BindingActive.SetActive);


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedBindingProperty(BindingProperty_SetActive, typeof(bool))]
            private g__ETMVB.BindingProperty _bindingFieldForSetActive = new g__ETMVB.BindingProperty();


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedConverter(BindingProperty_SetActive, typeof(bool))]
            private g__ETMVB.Converter _converterForSetActive = new g__ETMVB.Converter();


            private readonly g__ETVC.CachedVariantConverter<bool> _variantConverterBool = g__ETVC.CachedVariantConverter<bool>.Default;


            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            private readonly g__ETMCM.PropertyChangeEventListener<BindingActive> _listenerForSetActive;


            private bool _isListening_BindingActive;

            public BindingActive() : base()
            {
                OnBeforeConstructor();

                this._listenerForSetActive = new g__ETMCM.PropertyChangeEventListener<BindingActive>(this)
                {
                    OnEventAction = static (instance, args) => instance.SetActive__Variant(instance._converterForSetActive.Convert(args.NewValue))
                };

                OnAfterConstructor();
            }

            partial void OnBeforeConstructor();

            partial void OnAfterConstructor();

            /// <inheritdoc/>
            public override void StartListening()
            {
                base.StartListening();

                if (this._isListening_BindingActive) return;

                this._isListening_BindingActive = true;

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.AttachPropertyChangedListener(this._bindingFieldForSetActive.TargetPropertyName, this._listenerForSetActive) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetActive);
                    }

                }

            }

            partial void OnBindPropertyFailed(g__ETMVB.BindingProperty bindingProperty);

            /// <inheritdoc/>
            public override void StopListening()
            {
                base.StopListening();

                if (this._isListening_BindingActive == false) return;

                this._isListening_BindingActive = false;

                this._listenerForSetActive.Detach();

            }

            /// <inheritdoc/>
            public override bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)
            {
                if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetActive:
                    {
                        this._bindingFieldForSetActive.TargetPropertyName = targetPropertyName;
                        return true;
                    }

                }

                return false;
            }

            /// <inheritdoc/>
            public override bool SetAdapter(string bindingPropertyName, g__ETMVB.IAdapter adapter)
            {
                if (base.SetAdapter(bindingPropertyName, adapter)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetActive:
                    {
                        this._converterForSetActive.Adapter = adapter;
                        return true;
                    }

                }

                return false;
            }

            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            [g__S.Obsolete("This method is not intended to be used directly by user code.")]
            private void SetActive__Variant(in global::EncosyTower.Variants.Variant variant)
            {
                if (this._variantConverterBool.TryGetValue(variant, out bool value))
                {
                    SetActive(value);
                }
            }


            /// <inheritdoc/>
            public override void RefreshContext()
            {
                base.RefreshContext();

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.NotifyPropertyChanged(this._bindingFieldForSetActive.TargetPropertyName, this._listenerForSetActive) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetActive);
                    }

                }
            }

        }

        [g__ETMVBSG.BindingPropertyMethodInfo("SetIsStatic", typeof(bool))]
        partial class BindingIsStatic : g__ETMVB.IBinder
        {
            /// <summary>The name of <see cref="SetIsStatic"/></summary>
            public const string BindingProperty_SetIsStatic = nameof(BindingIsStatic.SetIsStatic);


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedBindingProperty(BindingProperty_SetIsStatic, typeof(bool))]
            private g__ETMVB.BindingProperty _bindingFieldForSetIsStatic = new g__ETMVB.BindingProperty();


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedConverter(BindingProperty_SetIsStatic, typeof(bool))]
            private g__ETMVB.Converter _converterForSetIsStatic = new g__ETMVB.Converter();


            private readonly g__ETVC.CachedVariantConverter<bool> _variantConverterBool = g__ETVC.CachedVariantConverter<bool>.Default;


            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            private readonly g__ETMCM.PropertyChangeEventListener<BindingIsStatic> _listenerForSetIsStatic;


            private bool _isListening_BindingIsStatic;

            public BindingIsStatic() : base()
            {
                OnBeforeConstructor();

                this._listenerForSetIsStatic = new g__ETMCM.PropertyChangeEventListener<BindingIsStatic>(this)
                {
                    OnEventAction = static (instance, args) => instance.SetIsStatic__Variant(instance._converterForSetIsStatic.Convert(args.NewValue))
                };

                OnAfterConstructor();
            }

            partial void OnBeforeConstructor();

            partial void OnAfterConstructor();

            /// <inheritdoc/>
            public override void StartListening()
            {
                base.StartListening();

                if (this._isListening_BindingIsStatic) return;

                this._isListening_BindingIsStatic = true;

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.AttachPropertyChangedListener(this._bindingFieldForSetIsStatic.TargetPropertyName, this._listenerForSetIsStatic) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetIsStatic);
                    }

                }

            }

            partial void OnBindPropertyFailed(g__ETMVB.BindingProperty bindingProperty);

            /// <inheritdoc/>
            public override void StopListening()
            {
                base.StopListening();

                if (this._isListening_BindingIsStatic == false) return;

                this._isListening_BindingIsStatic = false;

                this._listenerForSetIsStatic.Detach();

            }

            /// <inheritdoc/>
            public override bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)
            {
                if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetIsStatic:
                    {
                        this._bindingFieldForSetIsStatic.TargetPropertyName = targetPropertyName;
                        return true;
                    }

                }

                return false;
            }

            /// <inheritdoc/>
            public override bool SetAdapter(string bindingPropertyName, g__ETMVB.IAdapter adapter)
            {
                if (base.SetAdapter(bindingPropertyName, adapter)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetIsStatic:
                    {
                        this._converterForSetIsStatic.Adapter = adapter;
                        return true;
                    }

                }

                return false;
            }

            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            [g__S.Obsolete("This method is not intended to be used directly by user code.")]
            private void SetIsStatic__Variant(in global::EncosyTower.Variants.Variant variant)
            {
                if (this._variantConverterBool.TryGetValue(variant, out bool value))
                {
                    SetIsStatic(value);
                }
            }


            /// <inheritdoc/>
            public override void RefreshContext()
            {
                base.RefreshContext();

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.NotifyPropertyChanged(this._bindingFieldForSetIsStatic.TargetPropertyName, this._listenerForSetIsStatic) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetIsStatic);
                    }

                }
            }

        }

        [g__ETMVBSG.BindingPropertyMethodInfo("SetTag", typeof(string))]
        partial class BindingTag : g__ETMVB.IBinder
        {
            /// <summary>The name of <see cref="SetTag"/></summary>
            public const string BindingProperty_SetTag = nameof(BindingTag.SetTag);


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedBindingProperty(BindingProperty_SetTag, typeof(string))]
            private g__ETMVB.BindingProperty _bindingFieldForSetTag = new g__ETMVB.BindingProperty();


            [g__UE.SerializeField]
            [g__ETMVBSG.GeneratedConverter(BindingProperty_SetTag, typeof(string))]
            private g__ETMVB.Converter _converterForSetTag = new g__ETMVB.Converter();


            private readonly g__ETVC.CachedVariantConverter<string> _variantConverterString = g__ETVC.CachedVariantConverter<string>.Default;


            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            private readonly g__ETMCM.PropertyChangeEventListener<BindingTag> _listenerForSetTag;


            private bool _isListening_BindingTag;

            public BindingTag() : base()
            {
                OnBeforeConstructor();

                this._listenerForSetTag = new g__ETMCM.PropertyChangeEventListener<BindingTag>(this)
                {
                    OnEventAction = static (instance, args) => instance.SetTag__Variant(instance._converterForSetTag.Convert(args.NewValue))
                };

                OnAfterConstructor();
            }

            partial void OnBeforeConstructor();

            partial void OnAfterConstructor();

            /// <inheritdoc/>
            public override void StartListening()
            {
                base.StartListening();

                if (this._isListening_BindingTag) return;

                this._isListening_BindingTag = true;

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.AttachPropertyChangedListener(this._bindingFieldForSetTag.TargetPropertyName, this._listenerForSetTag) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetTag);
                    }

                }

            }

            partial void OnBindPropertyFailed(g__ETMVB.BindingProperty bindingProperty);

            /// <inheritdoc/>
            public override void StopListening()
            {
                base.StopListening();

                if (this._isListening_BindingTag == false) return;

                this._isListening_BindingTag = false;

                this._listenerForSetTag.Detach();

            }

            /// <inheritdoc/>
            public override bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)
            {
                if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetTag:
                    {
                        this._bindingFieldForSetTag.TargetPropertyName = targetPropertyName;
                        return true;
                    }

                }

                return false;
            }

            /// <inheritdoc/>
            public override bool SetAdapter(string bindingPropertyName, g__ETMVB.IAdapter adapter)
            {
                if (base.SetAdapter(bindingPropertyName, adapter)) return true;

                switch (bindingPropertyName)
                {
                    case BindingProperty_SetTag:
                    {
                        this._converterForSetTag.Adapter = adapter;
                        return true;
                    }

                }

                return false;
            }

            [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
            [g__S.Obsolete("This method is not intended to be used directly by user code.")]
            private void SetTag__Variant(in global::EncosyTower.Variants.Variant variant)
            {
                if (this._variantConverterString.TryGetValue(variant, out string value))
                {
                    SetTag(value);
                }
            }


            /// <inheritdoc/>
            public override void RefreshContext()
            {
                base.RefreshContext();

                if (this.Context is g__ETMCM.INotifyPropertyChanged inpc)
                {
                    if (inpc.NotifyPropertyChanged(this._bindingFieldForSetTag.TargetPropertyName, this._listenerForSetTag) == false)
                    {
                        OnBindPropertyFailed(this._bindingFieldForSetTag);
                    }

                }
            }

        }

    }



}

