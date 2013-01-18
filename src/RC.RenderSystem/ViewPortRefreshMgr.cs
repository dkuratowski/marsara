using System;
using System.Collections.Generic;
using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>This is a helper class that is responsible for refreshing a specific ViewPort.</summary>
    /// <remarks>For internal use only.</remarks>
    class ViewPortRefreshMgr : IDrawTarget
    {
        /// <summary>
        /// Constructs a ViewPortRefreshMgr object.
        /// </summary>
        /// <param name="targetVP">The target ViewPort that this refresh manager is responsible for.</param>
        public ViewPortRefreshMgr(ViewPort targetVP, BitmapAccess frameBufferAccess)
        {
            if (null == targetVP) { throw new ArgumentNullException("targetVP"); }
            if (null == frameBufferAccess) { throw new ArgumentNullException("frameBufferAccess"); }

            this.targetVP = targetVP;
            this.frameBufferAccess = frameBufferAccess;
            this.drawAccessGranted = false;
            this.dirtyRectsVPC = new List<Rectangle>();
            this.dirtyRectsDC = new List<Rectangle>();
            this.invalidate = true;
        }

        /// <summary>
        /// Asks the ViewPortRefreshMgr to collect the dirty rectangles of it's ViewPort.
        /// </summary>
        public void CollectDirtyRects()
        {
            this.dirtyRectsDC.Clear();
            this.dirtyRectsVPC.Clear();
            /// Call this even if we have to invalidate because this function might have necessary side-effects
            /// in the derived classes.
            this.dirtyRectsVPC = this.targetVP.GetDirtyRects();

            if (this.invalidate)
            {
                /// If we have to invalidate, then clear all dirty rectangles and add only one: the whole area
                /// of the ViewPort.
                this.dirtyRectsVPC.Clear();
                this.dirtyRectsVPC.Add(new Rectangle(0, 0, this.targetVP.Width, this.targetVP.Height));
                this.invalidate = false;
            }

            if (null != this.dirtyRectsVPC)
            {
                foreach (Rectangle rect in this.dirtyRectsVPC)
                {
                    this.dirtyRectsDC.Add(new Rectangle(this.targetVP.X + rect.X,
                                                        this.targetVP.Y + rect.Y,
                                                        rect.Width,
                                                        rect.Height));
                }
            }
        }

        /// <summary>
        /// Notifies this refresh manager that an area of the corresponding ViewPort is hidden by another ViewPort.
        /// </summary>
        /// <param name="otherVPR">
        /// The refresh manager of the other ViewPort that hides the given area from the ViewPort corresponding to this
        /// refresh manager.
        /// </param>
        /// <param name="area">The area that is hidden (in the Display coordinate system).</param>
        public void AddHiddenArea(ViewPortRefreshMgr otherVPR, Rectangle area)
        {
            for (int idx = 0; idx < this.dirtyRectsDC.Count; idx++)
            {
                if (area.Contains(this.dirtyRectsDC[idx]))
                {
                    /// If the given area hides the current dirty rectangle, then it has to be
                    /// removed from the refresh list because it is hidden.
                    this.dirtyRectsDC[idx] = Rectangle.Empty;
                    this.dirtyRectsVPC[idx] = Rectangle.Empty;
                }
                else
                {
                    /// If the given area intersects the current dirty rectangle, then the intersection
                    /// has to be added to the refresh list of the other refresh manager.
                    Rectangle intersection = Rectangle.Intersect(this.dirtyRectsDC[idx], area);
                    if (!intersection.IsEmpty)
                    {
                        otherVPR.dirtyRectsDC.Add(intersection);
                        otherVPR.dirtyRectsVPC.Add(new Rectangle(intersection.X - otherVPR.targetVP.X,
                                                                 intersection.Y - otherVPR.targetVP.Y,
                                                                 intersection.Width,
                                                                 intersection.Height));
                    }
                }
                /// TODO: Here we could implement an optimalization: remove the parts of the current dirty
                /// rectangle that are hidden. This is difficult to implement as there are a lot of cases to
                /// handle but later we may implement this optimalization if necessary.
            }
        }

        /// <summary>
        /// Grant access to the frame buffer and call the draw function of the corresponding ViewPort.
        /// </summary>
        public void CallDrawFunction()
        {
            this.drawAccessGranted = true;
            for (int i = 0; i < this.dirtyRectsVPC.Count; i++)
            {
                if (!this.dirtyRectsVPC[i].IsEmpty)
                {
                    this.frameBufferAccess.ClipBounds = this.dirtyRectsDC[i];
                    this.targetVP.Draw(this, this.dirtyRectsVPC[i]);
                }
            }
            this.drawAccessGranted = false;
        }

        /// <summary>
        /// Computes the bounding box of all areas have been refreshed in the last frame.
        /// </summary>
        /// <returns>The bounding box of all refreshed areas (in Display coordinate system).</returns>
        public Rectangle ComputeRefreshedArea()
        {
            Rectangle boundingBox = new Rectangle();
            for (int i = 0; i < this.dirtyRectsDC.Count; i++)
            {
                if (!this.dirtyRectsDC[i].IsEmpty)
                {
                    if (!boundingBox.IsEmpty)
                    {
                        boundingBox = Rectangle.Union(boundingBox, this.dirtyRectsDC[i]);
                    }
                    else
                    {
                        boundingBox = this.dirtyRectsDC[i];
                    }
                }
            }
            return boundingBox;
        }

        /// <summary>
        /// Applies any changes in the ViewPort position at the end of a render operation.
        /// </summary>
        /// <returns>True if the position of the ViewPort has been changed, false otherwise.</returns>
        /// <remarks>
        /// You have to call this function at the end of a render operation from the same thread that is
        /// actually rendering.
        /// </remarks>
        public bool ApplyMoveViewPort()
        {
            bool vpMoved = this.targetVP.ApplyMove();
            this.invalidate = vpMoved;
            return vpMoved;
        }

        /// <summary>
        /// Invalidates the draw surface of the corresponding ViewPort.
        /// </summary>
        /// <remarks>For internal use only.</remarks>
        public void Invalidate()
        {
            this.invalidate = true;
        }

        #region IDrawTarget members

        /// <see cref="IDrawTarget.DrawBitmap"/>
        public void DrawBitmap(ScaledBitmap src, int x, int y)
        {
            if (!this.drawAccessGranted) { throw new RenderSystemException("Access denied on IDrawTarget"); }

            this.frameBufferAccess.DrawBitmap(src, x + this.targetVP.X, y + this.targetVP.Y);
        }

        /// <see cref="IDrawTarget.Clear"/>
        public void Clear(Color clearWith)
        {
            if (!this.drawAccessGranted) { throw new RenderSystemException("Access denied on IDrawTarget"); }

            this.frameBufferAccess.Clear(clearWith);
        }

        /// <see cref="IDrawTarget.ClipBounds"/>
        public Rectangle ClipBounds
        {
            get
            {
                if (!this.drawAccessGranted) { throw new RenderSystemException("Access denied on IDrawTarget"); }
                Rectangle clipBounds = this.frameBufferAccess.ClipBounds;
                clipBounds.X -= this.targetVP.X;
                clipBounds.Y -= this.targetVP.Y;
                return clipBounds;
            }
        }

        #endregion

        /// <summary>
        /// The target ViewPort that this refresh manager is responsible for.
        /// </summary>
        private ViewPort targetVP;

        /// <summary>
        /// List of the dirty rectangles in the corresponding ViewPort (in the own coordinate system of
        /// the ViewPort).
        /// </summary>
        private List<Rectangle> dirtyRectsVPC;

        /// <summary>
        /// List of the dirty rectangles in the corresponding ViewPort (in the Display coordinate system).
        /// </summary>
        private List<Rectangle> dirtyRectsDC;

        /// <summary>
        /// The access interface of the common frame buffer of the Display. All incoming draw calls are in
        /// ViewPort coordinate systems. This refresh manager is responsible for the coordinate transformations.
        /// </summary>
        private BitmapAccess frameBufferAccess;

        /// <summary>
        /// The draw functions of this refresh manager is only accessable if this flag is true.
        /// </summary>
        private bool drawAccessGranted;

        /// <summary>
        /// This flag is true if the whole contents of the corresponding ViewPort have to be redrawn.
        /// </summary>
        private bool invalidate;
    }
}
