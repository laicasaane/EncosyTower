using EncosyTower.Samples.UitkPages.Ids;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>A place the Book shows: the main menu, the entrance, or the screen of a dimension.</summary>
    /// <remarks>
    /// Create one with <see cref="MainMenu"/>, <see cref="Entrance"/> or <see cref="Of"/>; the default location is the
    /// main menu. <see cref="Screen"/> is the screen page that shows the location, so its key is
    /// <c>AssetKeyAPI.Key(location.Screen)</c>. Travel requests, the current screen of the Book, screen page user data
    /// and attachment sets use it.
    /// </remarks>
    internal readonly record struct BookLocation
    {
        private BookLocation(ScreenPage screen, Dimension dimension)
        {
            Screen = screen;
            Dimension = dimension;
        }

        /// <summary>Gets the location of the main menu.</summary>
        public static BookLocation MainMenu => new(ScreenPage.MainMenu, default);

        /// <summary>Gets the location of the entrance.</summary>
        public static BookLocation Entrance => new(ScreenPage.Entrance, default);

        /// <summary>Gets the screen page that shows the location.</summary>
        public ScreenPage Screen { get; }

        /// <summary>Gets the dimension that the screen shows, when <see cref="IsDimension"/> is true.</summary>
        public Dimension Dimension { get; }

        /// <summary>Gets whether the location is the screen of a dimension.</summary>
        public bool IsDimension => Screen is ScreenPage.Dimension or ScreenPage.CoreDimension;

        /// <summary>Gets the location of the screen of a dimension.</summary>
        /// <param name="dimension">The dimension.</param>
        /// <returns>The location; its screen is <see cref="ScreenPage.CoreDimension"/> for the core.</returns>
        public static BookLocation Of(Dimension dimension)
            => new(dimension == Dimension.Core ? ScreenPage.CoreDimension : ScreenPage.Dimension, dimension);

        /// <summary>Gets the dimension that the screen shows.</summary>
        /// <param name="dimension">The dimension, or the default value when the location is not a dimension.</param>
        /// <returns><c>true</c> when the location is the screen of a dimension; otherwise <c>false</c>.</returns>
        public bool TryGetDimension(out Dimension dimension)
        {
            dimension = IsDimension ? Dimension : default;
            return IsDimension;
        }

        /// <summary>Returns the name of the dimension, or of the screen when the location is not a dimension.</summary>
        public override string ToString()
            => TryGetDimension(out var dimension) ? dimension.ToStringFast() : Screen.ToStringFast();
    }
}
