using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Enumerates the possible alignments when rendering a UIString.
    /// </summary>
    public enum UIStringAlignment
    {
        /// <summary>
        /// Align string to the left.
        /// </summary>
        Left = 0,

        /// <summary>
        /// Align string to center.
        /// </summary>
        Center = 1,

        /// <summary>
        /// Align string to the right.
        /// </summary>
        Right = 2,

        /// <summary>
        /// Align string to both left and right margins adding extra space between words as necessary.
        /// This creates a clean look along the left and right side of the target rectangle.
        /// </summary>
        Justify = 3
    }

    /// <summary>
    /// This interface can be used for rendering to a target.
    /// </summary>
    /// <remarks>TODO: other render methods will be added later.</remarks>
    public interface IUIRenderContext
    {
        /// <summary>
        /// Renders the given sprite to the given position.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="position">The position where to render in the coordinate-system of the render context.</param>
        void RenderSprite(UISprite sprite, RCIntVector position);

        /// <summary>
        /// Renders the given section of the given sprite to the given position.
        /// </summary>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="position">The position where to render in the coordinate-system of the render context.</param>
        /// <param name="section">The section of the sprite to render in the coordinate-system of the sprite.</param>
        void RenderSprite(UISprite sprite, RCIntVector position, RCIntRectangle section);

        /// <summary>
        /// Renders the given string to the given position horizontally.
        /// </summary>
        /// <param name="str">The string to render.</param>
        /// <param name="position">The position where to render in the coordinate-system of the render context.</param>
        void RenderString(UIString str, RCIntVector position);

        /// <summary>
        /// Renders the given string to the given position horizontally with a maximum width. The overhanging part of the string
        /// will be cut.
        /// </summary>
        /// <param name="str">The string to render.</param>
        /// <param name="position">The position where to render in the coordinate-system of the render context.</param>
        /// <param name="width">The maximum width of the string in the coordinate-system of the string.</param>
        void RenderString(UIString str, RCIntVector position, int width);

        /// <summary>
        /// Renders the given string into a textbox with the given alignment. For more informations on string alignments
        /// see the description of the UIStringAlignment enumeration.
        /// </summary>
        /// <param name="str">The string to render.</param>
        /// <param name="position">The position where to render in the coordinate-system of the render context.</param>
        /// <param name="textboxSize">The size of the textbox in the coordinate-system of the string.</param>
        /// <param name="alignment">The alignment of the rendered string.</param>
        void RenderString(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment);

        /// <summary>
        /// Renders an unfilled rectangle at the given position using the given sprite as a brush.
        /// </summary>
        /// <param name="brush">The brush to use for rendering the rectangle.</param>
        /// <param name="rect">The position of the rectangle in the coordinate-system of the render context.</param>
        void RenderRectangle(UISprite brush, RCIntRectangle rect);

        /// <summary>
        /// Gets the clip rectangle of the current render operations in the coordinate-system of the render context.
        /// Render operations effects only the area inside this rectangle.
        /// You can define additional constraint by using the setter or turn off that constraint by using the setter
        /// with RCIntRectangle.Undefined.
        /// </summary>
        RCIntRectangle Clip { get; set; }
    }
}
