using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>
    /// This interface contains every draw operations can be used in the RC.RenderSystem.
    /// </summary>
    public interface IDrawTarget
    {
        /// <summary>
        /// Draws the given ScaledBitmap with its original size so that its upper-left corner is at the
        /// point indicated by the given coordinates.
        /// </summary>
        /// <param name="src">The source ScaledBitmap that will be drawn.</param>
        /// <param name="x">The X coordinate of the upper-left corner.</param>
        /// <param name="y">The Y coordinate of the upper-left corner.</param>
        /// <remarks>
        /// If you want to make a color transparent in the source ScaledBitmap, then you have to use the function
        /// ScaledBitmap.MakeTransparent().
        /// </remarks>
        void DrawBitmap(ScaledBitmap src, int x, int y);

        /// <summary>
        /// Clears the entire drawing region of the draw target and fills it with the specified background color.
        /// </summary>
        /// <param name="clearWith">
        /// The background color with you want to fill the drawing region of the draw target.
        /// </param>
        void Clear(Color clearWith);

        /// <summary>
        /// Gets the clipping rectangle of the IDrawTarget (in logical pixels).
        /// </summary>
        Rectangle ClipBounds { get; }
    }
}
