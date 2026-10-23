using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Views
{
    /// <summary>The colors of a button in one state: its fill, its border and its ink.</summary>
    /// <param name="Fill">The color of the fill image.</param>
    /// <param name="Border">The color of the border image.</param>
    /// <param name="Ink">The color of the label and of the icon.</param>
    internal readonly record struct ButtonLook(Color Fill, Color Border, Color Ink);
}
