using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.Mvvm.ViewBinding
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
    public struct BindingCommand
    {
        /// <summary>
        /// The <see cref="EncosyTower.Mvvm.Input.ICommand"/> whose container class
        /// is an <see cref="EncosyTower.Mvvm.ComponentModel.IObservableObject"/>.
        /// </summary>
        [field: SerializeField]
        public string TargetCommandName { get; set; }
    }
}
