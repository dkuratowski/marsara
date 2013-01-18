using System;
using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>
    /// Provides a drawing interface for a ScaledBitmap.
    /// </summary>
    public class BitmapAccess : IDrawTarget, IDisposable
    {
        /// <summary>
        /// Creates a BitmapAccess object for the given ScaledBitmap.
        /// </summary>
        /// <param name="bmp">The ScaledBitmap object for which you want to create a draw interface.</param>
        /// <returns>The created BitmapAccess object.</returns>
        public static BitmapAccess FromBitmap(ScaledBitmap bmp)
        {
            if (null == bmp) { throw new ArgumentNullException("bmp"); }

            return FromBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Creates a BitmapAccess object for the given ScaledBitmap with the given clipping rectangle.
        /// </summary>
        /// <param name="bmp">The ScaledBitmap object for which you want to create a draw interface</param>
        /// <param name="clipRect">
        /// The clipping rectangle of the draw interface (in logical pixels).
        /// </param>
        /// <returns>The created BitmapAccess object.</returns>
        public static BitmapAccess FromBitmap(ScaledBitmap bmp, Rectangle clipRect)
        {
            if (null == bmp) { throw new ArgumentNullException("bmp"); }
            if (null == clipRect) { throw new ArgumentNullException("clipRect"); }
            return new BitmapAccess(bmp, clipRect);
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.graphicContext.Dispose();
        }

        #endregion

        #region IDrawTarget Members

        /// <see cref="IDrawTarget.DrawBitmap"/>
        public void DrawBitmap(ScaledBitmap src, int x, int y)
        {
            if (null == src) { throw new ArgumentNullException("src"); }

            this.graphicContext.DrawImageUnscaled(src.RawBitmap, x * Display.HorizontalScale, y * Display.VerticalScale);
        }

        /// <see cref="IDrawTarget.ReplaceTransparentPixels"/>
        //public void ReplaceTransparentPixels(Color replaceTo)
        //{
        //    ScaledBitmap newBitmap = new ScaledBitmap(this.clipBounds.Width, this.clipBounds.Height);
        //    BitmapAccess newBmpAccess = BitmapAccess.FromBitmap(newBitmap);
        //    newBmpAccess.Clear(replaceTo);
        //    newBmpAccess.DrawBitmap(this.accessedBitmap, 0, 0);
        //    newBmpAccess.Dispose();
        //    DrawBitmap(newBitmap, this.clipBounds.X, this.clipBounds.Y);
        //    newBitmap.Dispose();
        //}

        /// <see cref="IDrawTarget.ReplaceTransparentPixels"/>
        //public void ReplaceTransparentPixels(ScaledBitmap replaceTo)
        //{
        //    if (null == replaceTo) { throw new ArgumentNullException("replaceTo"); }

        //    ScaledBitmap newBitmap = new ScaledBitmap(this.clipBounds.Width, this.clipBounds.Height);
        //    BitmapAccess newBmpAccess = BitmapAccess.FromBitmap(newBitmap);

        //    for (int currX = 0; currX + replaceTo.Width < newBitmap.Width; currX = currX + replaceTo.Width)
        //    {
        //        for (int currY = 0; currY + replaceTo.Height < newBitmap.Height; currY = currY + replaceTo.Height)
        //        {
        //            newBmpAccess.DrawBitmap(replaceTo, currX, currY);
        //        }
        //    }

        //    newBmpAccess.Dispose();
        //    DrawBitmap(newBitmap, this.clipBounds.X, this.clipBounds.Y);
        //    newBitmap.Dispose();
        //}

        /// <see cref="IDrawTarget.Clear"/>
        public void Clear(Color clearWith)
        {
            this.graphicContext.Clear(clearWith);
        }

        /// <see cref="IDrawTarget.ClipBounds"/>
        /// <remarks>The ClipBounds_set is not the part of the IDrawTarget interface.</remarks>
        public Rectangle ClipBounds
        {
            get { return this.clipBounds; }
            set
            {
                this.clipBounds = value;
                this.graphicContext.Clip = new Region(new Rectangle(this.clipBounds.X * Display.HorizontalScale,
                                                                    this.clipBounds.Y * Display.VerticalScale,
                                                                    this.clipBounds.Width * Display.HorizontalScale,
                                                                    this.clipBounds.Height * Display.VerticalScale));
            }
        }

        #endregion

        /// <summary>
        /// Constructs a BitmapAccess object for the given ScaledBitmap with the given clipping rectangle.
        /// </summary>
        /// <param name="bmp">The ScaledBitmap object for which you want to create a draw interface</param>
        /// <param name="clipRect">
        /// The clipping rectangle of the draw interface (in logical pixels).
        /// </param>
        private BitmapAccess(ScaledBitmap bmp, Rectangle clipRect)
        {
            this.graphicContext = Graphics.FromImage(bmp.RawBitmap);
            this.clipBounds = new Rectangle(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height);
            this.graphicContext.Clip = new Region(new Rectangle(clipRect.X * Display.HorizontalScale,
                                                                clipRect.Y * Display.VerticalScale,
                                                                clipRect.Width * Display.HorizontalScale,
                                                                clipRect.Height * Display.VerticalScale));
            this.accessedBitmap = bmp;
        }

        /// <summary>
        /// The underlying Graphics object of this BitmapAccess.
        /// </summary>
        private Graphics graphicContext;

        /// <summary>
        /// The ScaledBitmap object that is manipulated by this BitmapAccess.
        /// </summary>
        private ScaledBitmap accessedBitmap;

        /// <summary>
        /// The clipping rectangle of this BitmapAccess (in logical pixels).
        /// </summary>
        private Rectangle clipBounds;
    }
}
