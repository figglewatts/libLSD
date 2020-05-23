namespace libLSD.Types
{
    /// <summary>
    /// Interface for color data.
    /// </summary>
    public interface IColor
    {
        /// <summary>
        /// The red component of the color.
        /// </summary>
        float Red { get; }

        /// <summary>
        /// The green component of the color.
        /// </summary>
        float Green { get; }

        /// <summary>
        /// The blue component of the color.
        /// </summary>
        float Blue { get; }

        /// <summary>
        /// The alpha of the color.
        /// </summary>
        float Alpha { get; }

        /// <summary>
        /// Whether or not this pixel is transparent.
        /// </summary>
        bool TransparencyControl { get; }

        /// <summary>
        /// Whether or not this pixel is black.
        /// </summary>
        bool IsBlack { get; }
    }
}
