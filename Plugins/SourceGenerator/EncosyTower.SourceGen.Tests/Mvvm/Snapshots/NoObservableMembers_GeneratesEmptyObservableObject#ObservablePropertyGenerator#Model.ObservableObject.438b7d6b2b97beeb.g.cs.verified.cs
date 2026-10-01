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

    partial class Model : g__ETMCM.IObservableObject
        , g__ETMCM.INotifyPropertyChanging
        , g__ETMCM.INotifyPropertyChanged

    {
        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanging.AttachPropertyChangingListener{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool AttachPropertyChangingListener<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.AttachPropertyChangedListener{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool AttachPropertyChangedListener<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged{TInstance}(string, g__ETMCM.PropertyChangeEventListener{TInstance})" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool NotifyPropertyChanged<TInstance>(string propertyName, g__ETMCM.PropertyChangeEventListener<TInstance> listener) where TInstance : class
        {
            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged(string)" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual bool NotifyPropertyChanged(string propertyName)
        {
            return false;
        }

        /// <inheritdoc cref="g__ETMCM.INotifyPropertyChanged.NotifyPropertyChanged()" />
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.ObservableProperties.ObservablePropertyGenerator", "0.1.8-preview.1")]
        public virtual void NotifyPropertyChanged()
        {
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

