#pragma warning disable 0219

using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Mvvm.Input;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SCM = global::System.ComponentModel;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__ETMI = global::EncosyTower.Mvvm.Input;
using g__ETMISG = global::EncosyTower.Mvvm.Input.SourceGen;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__ETMISG.RelayCommandInfo("OnSaveCommand", typeof(void))]
    [g__ETMISG.RelayCommandInfo("OnProcessCommand", typeof(int))]
    partial class Model : g__ETMI.ICommandListener
    {
        /// <summary>The name of <see cref="OnSaveCommand"/></summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        public const string CommandName_OnSaveCommand = nameof(Model.OnSaveCommand);

        /// <summary>The name of <see cref="OnProcessCommand"/></summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        public const string CommandName_OnProcessCommand = nameof(Model.OnProcessCommand);


        /// <summary>The backing field for <see cref="OnSaveCommand"/>.</summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
        private g__ETMI.RelayCommand _commandOnSave;

        /// <summary>The backing field for <see cref="OnProcessCommand"/>.</summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        [g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]
        private g__ETMI.RelayCommand<int> _commandOnProcess;


        /// <summary>Gets an <see cref="g__ETMI.IRelayCommand"/> instance wrapping <see cref="OnSave"/>.</summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__ETMISG.GeneratedRelayCommand(CommandName_OnSaveCommand)]
        public g__ETMI.IRelayCommand OnSaveCommand
        {
            get
            {
                if (this._commandOnSave == null)
                    this._commandOnSave = new g__ETMI.RelayCommand(OnSave);

                return this._commandOnSave;
            }
        }

        /// <summary>Gets an <see cref="g__ETMI.IRelayCommand{int}"/> instance wrapping <see cref="OnProcess"/>.</summary>
        [g__SCDC.GeneratedCode("EncosyTower.Mvvm.Generators.RelayCommands.RelayCommandGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__ETMISG.GeneratedRelayCommand(CommandName_OnProcessCommand)]
        public g__ETMI.IRelayCommand<int> OnProcessCommand
        {
            get
            {
                if (this._commandOnProcess == null)
                    this._commandOnProcess = new g__ETMI.RelayCommand<int>(OnProcess, CanProcess);

                return this._commandOnProcess;
            }
        }


        /// <inheritdoc/>
        public bool TryGetCommand<TCommand>(string commandName, out TCommand command)
            where TCommand : g__ETMI.ICommand
        {
            switch (commandName)
            {
                case CommandName_OnSaveCommand:
                {
                    if (this.OnSaveCommand is TCommand commandT)
                    {
                        command = commandT;
                        return true;
                    }

                    command = default;
                    return false;
                }

                case CommandName_OnProcessCommand:
                {
                    if (this.OnProcessCommand is TCommand commandT)
                    {
                        command = commandT;
                        return true;
                    }

                    command = default;
                    return false;
                }

                default:
                {
                    command = default;
                    return false;
                }
            }
        }

    }


}

