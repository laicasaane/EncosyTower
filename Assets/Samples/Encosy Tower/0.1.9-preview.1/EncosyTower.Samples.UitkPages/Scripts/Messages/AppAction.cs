namespace EncosyTower.Samples.UitkPages.Messages
{
    /// <summary>What a page asks the Book to do.</summary>
    /// <remarks>A page publishes it in a <see cref="AppActionMessage"/>; the codex handles it.</remarks>
    public enum AppAction
    {
        /// <summary>Starts a new game from the main menu.</summary>
        NewGame,

        /// <summary>Continues the saved game from the main menu.</summary>
        Continue,

        /// <summary>Saves the progress and returns to the main menu.</summary>
        Exit,

        /// <summary>Travels to the location.</summary>
        Travel,

        /// <summary>Goes back in the screen history, or closes the top dialog first.</summary>
        Back,

        /// <summary>Goes forward in the screen history.</summary>
        Forward,

        /// <summary>Shows the tab at the index.</summary>
        SelectTab,

        /// <summary>Shows or hides the card at the index.</summary>
        ToggleCard,

        /// <summary>Hides the card at the index.</summary>
        CloseCard,

        /// <summary>Opens the More dialog.</summary>
        More,

        /// <summary>Opens the Echo dialog of the current dimension.</summary>
        ReadEcho,

        /// <summary>Opens the lost appendix page, which does not exist.</summary>
        ReadAppendix,

        /// <summary>Closes the top dialog and offers to reopen it.</summary>
        Done,

        /// <summary>Aborts the dialog that is opening, back to its start.</summary>
        Cancel,

        /// <summary>Aborts the dialog that is closing, so it stays open at once.</summary>
        KeepReading,

        /// <summary>Aborts the dialog that is closing, back to its open state.</summary>
        StopClosing,

        /// <summary>Reopens the dialog that closed last.</summary>
        Reopen,

        /// <summary>Opens the map popup.</summary>
        OpenMap,

        /// <summary>Closes the map popup.</summary>
        CloseMap,

        /// <summary>Travels to the location, from the map popup.</summary>
        MapTravel,

        /// <summary>Casts the incantation in the core.</summary>
        CastIncantation,

        /// <summary>Reads the name of the tower that the incantation made.</summary>
        ReadName,

        /// <summary>Closes the incantation dialog and returns to the entrance.</summary>
        WriteInTheBook,

        /// <summary>Opens the settings dialog.</summary>
        OpenSettings,
    }
}
