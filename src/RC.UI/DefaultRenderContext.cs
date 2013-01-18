using System;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Implements the render operations for the UIObjects.
    /// </summary>
    class DefaultRenderContext : IUIRenderContext
    {
        /// <summary>
        /// Constructs a DefaultRenderContext object.
        /// </summary>
        public DefaultRenderContext(UIObject targetObj)
        {
            this.enabled = false;
            this.targetObject = targetObj;
            this.clipRectCache = new CachedValue<RCIntRectangle>(this.ComputeClipRectangle);
            this.absClipRectCache = new CachedValue<RCIntRectangle>(this.ComputeAbsClipRectangle);
            this.screenContext = null;
        }

        #region IRenderContext methods

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position)
        {
            if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (sprite.PixelSize != this.targetObject.AbsolutePixelScaling) { throw new InvalidOperationException("Incompatible pixel size!"); }

            /// Render only if target object is not clipped
            if (this.absClipRectCache.Value != RCIntRectangle.Undefined)
            {
                this.screenContext.Clip = this.absClipRectCache.Value;
                RCIntVector absolutePos = this.targetObject.AbsolutePosition + position * this.targetObject.AbsolutePixelScaling;
                this.screenContext.RenderSprite(sprite, absolutePos);
            }
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderSprite(UISprite sprite, RCIntVector position, RCIntRectangle section)
        {
            if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
            if (sprite == null) { throw new ArgumentNullException("sprite"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (section == RCIntRectangle.Undefined) { throw new ArgumentNullException("section"); }
            if (sprite.PixelSize != this.targetObject.AbsolutePixelScaling) { throw new InvalidOperationException("Incompatible pixel size!"); }

            /// Render only if target object is not clipped
            if (this.absClipRectCache.Value != RCIntRectangle.Undefined)
            {
                this.screenContext.Clip = this.absClipRectCache.Value;
                RCIntVector absolutePos = this.targetObject.AbsolutePosition + position * this.targetObject.AbsolutePixelScaling;
                this.screenContext.RenderSprite(sprite, absolutePos, section);
            }
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderString(UIString str, RCIntVector position)
        {
            if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            /// Render only if target object is not clipped
            if (this.absClipRectCache.Value != RCIntRectangle.Undefined)
            {
                int cursorX = position.X;
                int cursorY = position.Y;
                foreach (UIStringFragment fragment in str.Fragments)
                {
                    if (fragment.Source != null)
                    {
                        this.RenderSprite(fragment.Source,
                                          new RCIntVector(cursorX, cursorY + fragment.Offset),
                                          fragment.Section);
                    }
                    cursorX += fragment.CursorStep;
                }
            }
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderString(UIString str, RCIntVector position, int width)
        {
            if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (width < 0) { throw new ArgumentOutOfRangeException("width"); }

            /// Render only if target object is not clipped
            if (this.absClipRectCache.Value != RCIntRectangle.Undefined)
            {
                int cursorX = position.X;
                int cursorY = position.Y;
                foreach (UIStringFragment fragment in str.Fragments)
                {
                    if (fragment.Source != null)
                    {
                        if (cursorX < position.X + width &&
                            cursorX + fragment.Section.Width >= position.X + width)
                        {
                            /// Render only a part of the fragment
                            this.RenderSprite(fragment.Source,
                                              new RCIntVector(cursorX, cursorY + fragment.Offset),
                                              new RCIntRectangle(fragment.Section.X,
                                                              fragment.Section.Y,
                                                              width - cursorX + position.X,
                                                              fragment.Section.Height));
                        }
                        else
                        {
                            /// Render the whole fragment
                            this.RenderSprite(fragment.Source,
                                              new RCIntVector(cursorX, cursorY + fragment.Offset),
                                              fragment.Section);
                        }
                    }
                    cursorX += fragment.CursorStep;

                    /// Stop rendering when we reached the end
                    if (cursorX >= position.X + width)
                    {
                        break;
                    }
                }
            }
        }

        /// <see cref="IUIRenderContext.RenderSprite"/>
        public void RenderString(UIString str, RCIntVector position, RCIntVector textboxSize, UIStringAlignment alignment)
        {
            if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
            if (str == null) { throw new ArgumentNullException("str"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            if (textboxSize == RCIntVector.Undefined) { throw new ArgumentNullException("textboxSize"); }
            if (textboxSize.X <= 0 || textboxSize.Y <= 0) { throw new ArgumentOutOfRangeException("textboxSize"); }

            /// TODO: implement this method.
            throw new NotImplementedException();
        }

        /// <see cref="IUIRenderContext.Clip"/>
        public RCIntRectangle Clip
        {
            get
            {
                if (!this.enabled) { throw new InvalidOperationException("Render context is not enabled!"); }
                return this.clipRectCache.Value;
            }

            set
            {
                /// TODO: implement this setter in the future if necessary.
                throw new NotImplementedException();
            }
        }

        #endregion IRenderContext methods

        /// <summary>
        /// Resets this render context before drawing.
        /// </summary>
        public void Reset(IUIRenderContext screenContext)
        {
            this.screenContext = screenContext;
            this.screenContext.Clip = RCIntRectangle.Undefined;
            this.absClipRectCache.Invalidate();
            this.clipRectCache.Invalidate();
        }

        /// <summary>
        /// Gets or sets whether this render context is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        /// <summary>
        /// Computes the clipping rectangle in this render context.
        /// </summary>
        /// <returns>The clipping rectangle in this render context.</returns>
        private RCIntRectangle ComputeClipRectangle()
        {
            /// Check for addition pixels
            bool additionalX = absClipRectCache.Value.Width % this.targetObject.AbsolutePixelScaling.X != 0 ||
                               absClipRectCache.Value.X % this.targetObject.AbsolutePixelScaling.X != 0;
            bool additionalY = absClipRectCache.Value.Height % this.targetObject.AbsolutePixelScaling.Y != 0 ||
                               absClipRectCache.Value.Y % this.targetObject.AbsolutePixelScaling.Y != 0;

            /// Transform from screen to local coordinate-system.
            RCIntRectangle retRect = (absClipRectCache.Value - this.targetObject.AbsolutePosition)
                                / this.targetObject.AbsolutePixelScaling;

            if (additionalX) { retRect.Width++; }
            if (additionalY) { retRect.Height++; }

            return retRect;
        }

        /// <summary>
        /// Computes the clipping rectangle of this render context in screen coordinates.
        /// </summary>
        /// <returns>The clipping rectangle of this render context in screen coordinates.</returns>
        private RCIntRectangle ComputeAbsClipRectangle()
        {
            RCIntRectangle absClipRect = this.targetObject.AbsoluteRange;
            UIObject current = this.targetObject.Parent;
            while (current != null && absClipRect != RCIntRectangle.Undefined)
            {
                absClipRect.Intersect(current.AbsoluteClip != RCIntRectangle.Undefined ?
                                      current.AbsoluteClip :
                                      current.AbsoluteRange);
                current = current.Parent;
            }
            return absClipRect;
        }

        /// <summary>
        /// This flag indicates whether this render context object is enabled or not.
        /// </summary>
        private bool enabled;

        /// <summary>
        /// The UIObject that will use this render context for rendering.
        /// </summary>
        private UIObject targetObject;

        /// <summary>
        /// The cache for the clipping rectangle of this render context. This cache can be reset with
        /// DefaultRenderContext.Reset.
        /// </summary>
        private CachedValue<RCIntRectangle> clipRectCache;

        /// <summary>
        /// The cache for the clipping rectangle of this render context in screen coordinates. This cache
        /// can be reset with DefaultRenderContext.Reset.
        /// </summary>
        private CachedValue<RCIntRectangle> absClipRectCache;

        /// <summary>
        /// Reference to the render context of the screen.
        /// </summary>
        private IUIRenderContext screenContext;
    }
}
