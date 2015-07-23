using System;
using System.Collections.Generic;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Base class of classes that represent an object on the UI.
    /// </summary>
    public class UIObject
    {
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when a new child has been successfully attached to this UIObject.
        /// </summary>
        public event AttachDetachHdl ChildAttached;

        /// <summary>
        /// Occurs when a child has been successfully detached from this UIObject.
        /// </summary>
        public event AttachDetachHdl ChildDetached;

        /// <summary>
        /// Occurs when this UIObject has been successfully attached to another UIObject as a child.
        /// </summary>
        public event AttachDetachHdl Attached;

        /// <summary>
        /// Occurs when this UIObject has been successfully detached from it's parent.
        /// </summary>
        public event AttachDetachHdl Detached;

        /// <summary>
        /// Occurs when the Position property of this UIObject has been changed.
        /// </summary>
        public event VectChangedHdl PositionChanged;

        /// <summary>
        /// Occurs when the Clip property of this UIObject has been changed.
        /// </summary>
        public event RectChangedHdl ClipRectChanged;

        /// <summary>
        /// Occurs when the Cloak property of this UIObject has been changed.
        /// </summary>
        public event RectChangedHdl CloakRectChanged;

        /// <summary>
        /// Occurs when the Range property of this UIObject has been changed.
        /// </summary>
        public event RectChangedHdl RangeRectChanged;

        /// <summary>
        /// Occurs when the AbsolutePixelScaling property of this UIObject has been changed.
        /// </summary>
        public event VectChangedHdl AbsolutePixelScalingChanged;

        /// <summary>
        /// Occurs when an area of this UIObject has been invalidated and must be re-rendered.
        /// </summary>
        public event InvalidatedHdl Invalidated;

        /// <summary>
        /// Occurs when this UIObject has been brought forward in the Z-order.
        /// </summary>
        public event ZOrderChangeHdl BroughtForward;

        /// <summary>
        /// Occurs when this UIObject has been brought to the top in the Z-order.
        /// </summary>
        public event ZOrderChangeHdl BroughtToTop;

        /// <summary>
        /// Occurs when this UIObject has been sent backward in the Z-order.
        /// </summary>
        public event ZOrderChangeHdl SentBackward;

        /// <summary>
        /// Occurs when this UIObject has been sent to the bottom in the Z-order.
        /// </summary>
        public event ZOrderChangeHdl SentToBottom;

        /// <summary>
        /// Represents a method that will handle UIObject attachment/detachment events.
        /// </summary>
        /// <param name="parentObj">The parent UIObject.</param>
        /// <param name="childObj">The UIObject that has been attached/detached to/from parentObj.</param>
        public delegate void AttachDetachHdl(UIObject parentObj, UIObject childObj);

        /// <summary>
        /// Represents a method that will handle UIObject position/absolute pixel scaling change events.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="prevVect">The previous value of the changed RCIntVector.</param>
        /// <param name="currentVect">The current value of the changed RCIntVector.</param>
        public delegate void VectChangedHdl(UIObject sender, RCIntVector prevVect, RCIntVector currentVect);

        /// <summary>
        /// Represents a method that will handle clip/cloak/range rectangle change events of UIObjects.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="prevRect">The previous value of the clip/cloak/range rectangle.</param>
        /// <param name="currentRect">The current value of the clip/cloak/range rectangle.</param>
        public delegate void RectChangedHdl(UIObject sender, RCIntRectangle prevRect, RCIntRectangle currentRect);

        /// <summary>
        /// Represents a method that will handle invalidate events of UIObjects.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="invalidRect">
        /// The invalidated area of the sender in the LCS of the sender or RCIntRectangle.Undefined to invalidate
        /// the whole Range of the sender.
        /// </param>
        public delegate void InvalidatedHdl(UIObject sender, RCIntRectangle invalidRect);

        /// <summary>
        /// Represents a method that will handle Z-order change events of UIObjects.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        public delegate void ZOrderChangeHdl(UIObject sender);

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UIObject instance.
        /// </summary>
        /// <param name="pixelScaling">The pixel scaling value of this UIObject relative to it's parent.</param>
        /// <param name="position">The position of this UIObject relative to it's parent.</param>
        /// <param name="range">The range rectangle of this UIObject in it's local coordinate-system.</param>
        public UIObject(RCIntVector pixelScaling, RCIntVector position, RCIntRectangle range)
        {
            if (pixelScaling == RCIntVector.Undefined) { throw new ArgumentNullException("pixelScaling"); }
            if (pixelScaling.X <= 0 || pixelScaling.Y <= 0) { throw new ArgumentOutOfRangeException("pixelScaling"); }
            if (range == RCIntRectangle.Undefined) { throw new ArgumentNullException("range"); }
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }

            this.parent = null;
            this.children = new List<UIObject>();
            this.childrenSet = new RCSet<UIObject>();

            this.position = position;
            this.absPositionCache = new CachedValue<RCIntVector>(this.ComputeAbsolutePosition);
            this.clip = RCIntRectangle.Undefined;
            this.absClipCache = new CachedValue<RCIntRectangle>(this.ComputeAbsoluteClip);
            this.cloak = RCIntRectangle.Undefined;
            this.absCloakCache = new CachedValue<RCIntRectangle>(this.ComputeAbsoluteCloak);
            this.range = range;
            this.absRangeCache = new CachedValue<RCIntRectangle>(this.ComputeAbsoluteRange);
            this.pixelScaling = pixelScaling;
            this.absPixelScalingCache = new CachedValue<RCIntVector>(this.ComputeAbsolutePixelScaling);
        }

        #region Public methods

        /// <summary>
        /// Attaches the given UIObject to this UIObject as a child.
        /// </summary>
        /// <param name="otherObj">The UIObject you want to attach as a child.</param>
        /// <exception cref="UIException">
        /// If otherObj is already a child of another UIObject.
        /// If otherObj equals with this UIObject.
        /// If attaching otherObj to this UIObject violates the tree property of the UI-graph.
        /// </exception>
        /// <remarks>
        /// If otherObj is already a child of this UIObject then this function has no effect.
        /// If the operation was successful, otherObj will be placed to the end of the child-list.
        /// </remarks>
        public void Attach(UIObject otherObj)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (otherObj == null) { throw new ArgumentNullException("otherObj"); }
            if (otherObj == this) { throw new UIException("Unable to attach a UIObject to itself as a child!"); }
            if (otherObj.parent != null) { throw new UIException("The given UIObject must be detached from it's parent first!"); }

            if (this.childrenSet.Contains(otherObj)) { return; }

            /// Attach
            this.childrenSet.Add(otherObj);
            this.children.Add(otherObj);
            otherObj.parent = this;

            /// Check the tree property
            if (this.CheckTreeProperty())
            {
                /// Invalidate the cached values in the attached UIObject
                otherObj.InvalidateAbsPosition();
                otherObj.InvalidateAbsClipRect();
                otherObj.InvalidateAbsCloakRect();
                otherObj.InvalidateAbsRangeRect();
                otherObj.InvalidateAbsPixelScaling();

                /// Raise the appropriate events
                if (this.ChildAttached != null) { this.ChildAttached(this, otherObj); }
                if (otherObj.Attached != null) { otherObj.Attached(this, otherObj); }
            }
            else
            {
                /// Tree property violation --> rollback and throw an exception.
                this.children.RemoveAt(this.children.Count - 1);
                this.childrenSet.Remove(otherObj);
                otherObj.parent = null;
                throw new UIException("Violating tree property!");
            }
        }

        /// <summary>
        /// Detaches the given child of this UIObject.
        /// </summary>
        /// <param name="whichChild">The UIObject you want to detach.</param>
        /// <remarks>
        /// If whichChild is not the child of this UIObject then this function has no effect.
        /// </remarks>
        public void Detach(UIObject whichChild)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (whichChild == null) { throw new ArgumentNullException("whichChild"); }

            if (!this.childrenSet.Contains(whichChild)) { return; }

            /// Detach
            this.childrenSet.Remove(whichChild);
            this.children.Remove(whichChild);
            whichChild.parent = null;

            /// Invalidate the cached values in the detached UIObject
            whichChild.InvalidateAbsPosition();
            whichChild.InvalidateAbsClipRect();
            whichChild.InvalidateAbsCloakRect();
            whichChild.InvalidateAbsRangeRect();
            whichChild.InvalidateAbsPixelScaling();

            /// Raise the appropriate events
            if (this.ChildDetached != null) { this.ChildDetached(this, whichChild); }
            if (whichChild.Detached != null) { whichChild.Detached(this, whichChild); }
        }

        /// <summary>
        /// Brings this UIObject forward in the Z-order. If it is already on the top of the Z-order then
        /// this function has no effect.
        /// </summary>
        public void BringForward()
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.parent != null)
            {
                int thisIndex = this.parent.children.IndexOf(this);
                if (-1 != thisIndex && thisIndex < this.parent.children.Count - 1)
                {
                    /// Swap the given UIObject and the UIObject in front of it.
                    this.parent.children[thisIndex] = this.parent.children[thisIndex + 1];
                    this.parent.children[thisIndex + 1] = this;
                    if (this.BroughtForward != null) { this.BroughtForward(this); }
                }
            }
        }

        /// <summary>
        /// Brings this UIObject to the top in the Z-order. If it is already on the top of the Z-order then
        /// this function has no effect.
        /// </summary>
        public void BringToTop()
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.parent != null)
            {
                int thisIndex = this.parent.children.IndexOf(this);
                if (-1 != thisIndex && thisIndex < this.parent.children.Count - 1)
                {
                    for (int i = thisIndex; i < this.parent.children.Count - 1; i++)
                    {
                        this.parent.children[i] = this.parent.children[i + 1];
                    }
                    this.parent.children[this.parent.children.Count - 1] = this;
                    if (this.BroughtToTop != null) { this.BroughtToTop(this); }
                }
            }
        }

        /// <summary>
        /// Sends this UIObject backward in the Z-order. If it is already on the bottom of the Z-order then
        /// this function has no effect.
        /// </summary>
        public void SendBackward()
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.parent != null)
            {
                int thisIndex = this.parent.children.IndexOf(this);
                if (thisIndex > 0)
                {
                    /// Swap the given UIObject and the UIObject behind it.
                    this.parent.children[thisIndex] = this.parent.children[thisIndex - 1];
                    this.parent.children[thisIndex - 1] = this;
                    if (this.SentBackward != null) { this.SentBackward(this); }
                }
            }
        }

        /// <summary>
        /// Sends this UIObject to the bottom in the Z-order. If it is already on the bottom of the Z-order then
        /// this function has no effect.
        /// </summary>
        public void SendToBottom()
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.parent != null)
            {
                int thisIndex = this.parent.children.IndexOf(this);
                if (thisIndex > 0)
                {
                    for (int i = thisIndex; i > 0; i--)
                    {
                        this.parent.children[i] = this.parent.children[i - 1];
                    }
                    this.parent.children[0] = this;
                    if (this.SentToBottom != null) { this.SentToBottom(this); }
                }
            }
        }

        /// <summary>
        /// Call this method to invalidate the given area of this UIObject.
        /// </summary>
        /// <param name="invalidRect">
        /// The area to invalidate of RCIntRectangle.Undefined for invalidating the whole Range.
        /// </param>
        public void OnInvalidate(RCIntRectangle invalidRect)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (this.Invalidated != null) { this.Invalidated(this, invalidRect); }
        }

        /// <summary>
        /// This function is automatically called by the framework when this UIObject has to be rendered.
        /// </summary>
        /// <param name="renderContext">The context of the rendering operations.</param>
        public void Render(IUIRenderContext renderContext)
        {
            /// TODO: permit recursive calls on this method.
            this.Render_i(renderContext);
        }

        /// <summary>
        /// Puts this object and all of it's children to the end of objList recursively in a
        /// depth-first-search order.
        /// </summary>
        /// <param name="objList">The returned list.</param>
        public void WalkTreeDFS(ref List<UIObject> objList)
        {
            if (objList == null) { throw new ArgumentNullException("objList"); }

            objList.Add(this);
            foreach (UIObject child in this.children)
            {
                child.WalkTreeDFS(ref objList);
            }
        }

        #endregion Public methods

        #region Public properties

        #region Location properties

        /// <summary>
        /// Gets or sets the position of this UIObject in the parent coordinate system (PCS). This position
        /// will be the origin of the local coordinate system (LCS) of this UIObject. Setting this property
        /// is permitted while rendering is in progress.
        /// </summary>
        public RCIntVector Position
        {
            get { return this.position; }
            set
            {
                if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
                if (value == RCIntVector.Undefined) { throw new ArgumentNullException("Position"); }

                RCIntVector prevPos = this.position;
                this.position = value;

                /// Invalidate the cached values except absolute pixel scaling
                this.InvalidateAbsPosition();
                this.InvalidateAbsClipRect();
                this.InvalidateAbsCloakRect();
                this.InvalidateAbsRangeRect();

                if (this.PositionChanged != null) { this.PositionChanged(this, prevPos, this.position); }
            }
        }

        /// <summary>
        /// Gets the position of this UIObject in screen coordinates.
        /// </summary>
        public RCIntVector AbsolutePosition { get { return this.absPositionCache.Value; } }

        /// <summary>
        /// Gets or sets the clipping rectangle of this UIObject in the LCS. Rendering operations performed by
        /// the children will be clipped by this rectangle. If this rectangle is RCIntRectangle.Undefined, then
        /// this.Range will be used as a clip rectangle. Clip rectangle shall be entirely contained by this.Range
        /// or shall be RCIntRectangle.Undefined. Setting this property is permitted while rendering is in progress.
        /// </summary>
        public RCIntRectangle Clip
        {
            get { return this.clip; }
            set
            {
                if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
                if (!this.CheckRectangles(ref this.range, ref value))
                {
                    throw new ArgumentException("Violating containment relationship constraints!", "Clip");
                }

                RCIntRectangle prevClip = this.clip;
                this.clip = value;

                /// Invalidate the cached value only in this UIObject
                this.absClipCache.Invalidate();

                if (this.ClipRectChanged != null) { this.ClipRectChanged(this, prevClip, this.clip); }
            }
        }

        /// <summary>
        /// Gets the clipping rectangle of this UIObject in screen coordinates.
        /// </summary>
        public RCIntRectangle AbsoluteClip { get { return this.absClipCache.Value; } }

        /// <summary>
        /// Gets or sets the cloaking rectangle of this UIObject in the LCS. UIObjects behind this cloaking rectangle
        /// will be hidden entirely. Set this rectangle to RCIntRectangle.Undefined if you don't want to define a
        /// cloaking rectangle for this UIObject. Cloak rectangle shall be entirely contained by this.Range or shall
        /// be RCIntRectangle.Undefined. Setting this property is permitted while rendering is in progress.
        /// </summary>
        public RCIntRectangle Cloak
        {
            get { return this.cloak; }
            set
            {
                if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
                if (!this.CheckRectangles(ref this.range, ref value))
                {
                    throw new ArgumentException("Violating containment relationship constraints!", "Cloak");
                }

                RCIntRectangle prevCloak = this.cloak;
                this.cloak = value;

                /// Invalidate the cached value only in this UIObject
                this.absCloakCache.Invalidate();

                if (this.CloakRectChanged != null) { this.CloakRectChanged(this, prevCloak, this.cloak); }
            }
        }

        /// <summary>
        /// Gets the cloaking rectangle of this UIObject in screen coordinates.
        /// </summary>
        public RCIntRectangle AbsoluteCloak { get { return this.absCloakCache.Value; } }

        /// <summary>
        /// Gets or sets the boundary of the rendering area of this UIObject in the LCS. Rendering operations performed
        /// by this UIObject will be clipped by this rectangle. This rectangle shall not be RCIntRectangle.Undefined.
        /// Setting this property is permitted while rendering is in progress.
        /// </summary>
        public RCIntRectangle Range
        {
            get { return this.range; }
            set
            {
                if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
                if (!this.CheckRectangles(ref value, ref this.clip) ||
                    !this.CheckRectangles(ref value, ref this.cloak))
                {
                    throw new ArgumentException("Violating containment relationship constraints!", "Range");
                }

                RCIntRectangle prevRange = this.range;
                this.range = value;

                /// Invalidate the cached value only in this UIObject
                this.absRangeCache.Invalidate();

                if (this.RangeRectChanged != null) { this.RangeRectChanged(this, prevRange, this.range); }
            }
        }


        /// <summary>
        /// Gets the range rectangle of this UIObject in screen coordinates.
        /// </summary>
        public RCIntRectangle AbsoluteRange { get { return this.absRangeCache.Value; } }

        /// <summary>
        /// Gets the pixel scaling of this UIObject relative to it's parent.
        /// </summary>
        public RCIntVector PixelScaling { get { return this.pixelScaling; } }

        /// <summary>
        /// Gets the pixel scaling of this UIObject in screen coordinates.
        /// </summary>
        public RCIntVector AbsolutePixelScaling { get { return this.absPixelScalingCache.Value; } }

        #endregion Location properties

        #region Tree properties

        /// <summary>
        /// Gets the parent of this UIObject or a null reference if there is no parent.
        /// </summary>
        public UIObject Parent { get { return this.parent; } }

        /// <summary>
        /// Gets the list of the children of this UIObject in the order of rendering.
        /// </summary>
        public UIObject[] Children { get { return this.children.ToArray(); } }

        /// <summary>
        /// Gets the previous sibling of this UIObject or a null reference if there is no previous sibling.
        /// </summary>
        /// <remarks>
        /// The previous sibling is the UIObject which is directly before this UIObject in Parent.Children.
        /// </remarks>
        public UIObject PreviousSibling
        {
            get
            {
                if (this.parent != null)
                {
                    int thisIdx = this.parent.children.IndexOf(this);
                    return thisIdx > 0 ? this.parent.children[thisIdx - 1] : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the next sibling of this UIObject or a null reference if there is no next sibling.
        /// </summary>
        /// <remarks>
        /// The next sibling is the UIObject which is directly after this UIObject in Parent.Children.
        /// </remarks>
        public UIObject NextSibling
        {
            get
            {
                if (this.parent != null)
                {
                    int thisIdx = this.parent.children.IndexOf(this);
                    return thisIdx < this.parent.children.Count - 1 ? this.parent.children[thisIdx + 1] : null;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the root of the subtree that is next to this UIObject's subtree or a null reference
        /// if there is no next subtree.
        /// </summary>
        public UIObject NextSubTree
        {
            get
            {
                if (this.NextSibling != null)
                {
                    return this.NextSibling;
                }
                else
                {
                    return this.parent != null ? this.parent.NextSubTree : null;
                }
            }
        }

        /// <summary>
        /// Gets the root of the subtree that is before this UIObject's subtree or a null reference
        /// if there is no previous subtree.
        /// </summary>
        public UIObject PreviousSubTree
        {
            get
            {
                if (this.PreviousSibling != null)
                {
                    return this.PreviousSibling;
                }
                else
                {
                    return this.parent != null ? this.parent.PreviousSubTree : null;
                }
            }
        }

        #endregion Tree properties

        #endregion Public properties

        #region Overridables

        /// <summary>
        /// Internal function to actually perform the rendering operations. Can be overriden in derived classes.
        /// </summary>
        /// <param name="renderContext">The context of the rendering operations.</param>
        protected virtual void Render_i(IUIRenderContext renderContext) { }

        #endregion Overridables

        #region Internal methods

        /// <summary>
        /// Internal method for checking the existence of the tree property after an attach.
        /// </summary>
        /// <returns>True if the tree property still exists, false otherwise.</returns>
        private bool CheckTreeProperty()
        {
            UIObject current = this;
            while (current.parent != null)
            {
                if (current.parent == this)
                {
                    /// Circle found
                    return false;
                }
                else
                {
                    /// Continue towards the root
                    current = current.parent;
                }
            }
            return true;
        }

        /// <summary>
        /// Internal method for checking the containment relationships between range, clip and cloak.
        /// </summary>
        /// <param name="container">Reference to the rectangle that must be the container.</param>
        /// <param name="content">Reference to the rectangle that must be the content.</param>
        /// <returns>True if the containment relationships are OK.</returns>
        private bool CheckRectangles(ref RCIntRectangle container, ref RCIntRectangle content)
        {
            return (container != RCIntRectangle.Undefined) &&
                   ((content == RCIntRectangle.Undefined) || (container.Contains(content)));
        }

        /// <summary>
        /// Computes the position of this UIObject in screen coordinates.
        /// </summary>
        /// <returns>The position of this UIObject in screen coordinates.</returns>
        private RCIntVector ComputeAbsolutePosition()
        {
            return this.parent != null ?
                   this.parent.AbsolutePixelScaling * this.position + this.parent.AbsolutePosition :
                   this.position;
        }

        /// <summary>
        /// Computes the clipping rectangle of this UIObject in screen coordinates.
        /// </summary>
        /// <returns>The clipping rectangle of this UIObject in screen coordinates.</returns>
        private RCIntRectangle ComputeAbsoluteClip()
        {
            return this.clip != RCIntRectangle.Undefined ?
                   this.clip * this.AbsolutePixelScaling + this.AbsolutePosition :
                   RCIntRectangle.Undefined;
        }

        /// <summary>
        /// Computes the cloaking rectangle of this UIObject in screen coordinates.
        /// </summary>
        /// <returns>The cloaking rectangle of this UIObject in screen coordinates.</returns>
        private RCIntRectangle ComputeAbsoluteCloak()
        {
            return this.cloak != RCIntRectangle.Undefined ?
                   this.cloak * this.AbsolutePixelScaling + this.AbsolutePosition :
                   RCIntRectangle.Undefined;
        }

        /// <summary>
        /// Computes the range rectangle of this UIObject in screen coordinates.
        /// </summary>
        /// <returns>The range rectangle of this UIObject in screen coordinates.</returns>
        private RCIntRectangle ComputeAbsoluteRange()
        {
            return this.range * this.AbsolutePixelScaling + this.AbsolutePosition;
        }

        /// <summary>
        /// Computes the absolute pixel scaling of this UIObject in screen coordinates.
        /// </summary>
        /// <returns>The absolute pixel scaling of this UIObject in screen coordinates.</returns>
        private RCIntVector ComputeAbsolutePixelScaling()
        {
            return this.parent != null ?
                   this.parent.AbsolutePixelScaling * this.pixelScaling :
                   this.pixelScaling;
        }

        /// <summary>
        /// Invalidates the absolute position of this UIObject and all of it's children.
        /// </summary>
        private void InvalidateAbsPosition()
        {
            this.absPositionCache.Invalidate();
            foreach (UIObject child in this.children)
            {
                child.InvalidateAbsPosition();
            }
        }

        /// <summary>
        /// Invalidates the absolute pixel scaling of this UIObject and all of it's children.
        /// </summary>
        private void InvalidateAbsPixelScaling()
        {
            /// Invalidate the cache and raise the appropriate event if necessary.
            RCIntVector prevAbsPixelScaling = this.absPixelScalingCache.Value;
            this.absPixelScalingCache.Invalidate();
            if (this.AbsolutePixelScalingChanged != null && prevAbsPixelScaling != this.absPixelScalingCache.Value)
            {
                this.AbsolutePixelScalingChanged(this, prevAbsPixelScaling, this.absPixelScalingCache.Value);
            }

            /// Call this method recursively on all children.
            foreach (UIObject child in this.children)
            {
                child.InvalidateAbsPixelScaling();
            }
        }

        /// <summary>
        /// Invalidates the absolute clipping rectangle of this UIObject and all of it's children.
        /// </summary>
        private void InvalidateAbsClipRect()
        {
            this.absClipCache.Invalidate();
            foreach (UIObject child in this.children)
            {
                child.InvalidateAbsClipRect();
            }
        }

        /// <summary>
        /// Invalidates the absolute cloaking rectangle of this UIObject and all of it's children.
        /// </summary>
        private void InvalidateAbsCloakRect()
        {
            this.absCloakCache.Invalidate();
            foreach (UIObject child in this.children)
            {
                child.InvalidateAbsCloakRect();
            }
        }

        /// <summary>
        /// Invalidates the absolute range rectangle of this UIObject and all of it's children.
        /// </summary>
        private void InvalidateAbsRangeRect()
        {
            this.absRangeCache.Invalidate();
            foreach (UIObject child in this.children)
            {
                child.InvalidateAbsRangeRect();
            }
        }

        #endregion Internal methods

        #region Private fields

        /// <summary>
        /// Reference to the parent of this UIObject or null if this is a top-level UIObject.
        /// </summary>
        private UIObject parent;

        /// <summary>
        /// Ordered list of the children of this UIObject.
        /// </summary>
        /// <remarks>The children of this UIObject will be rendered in this order.</remarks>
        private List<UIObject> children;

        /// <summary>
        /// Unordered list of the children of this UIObject.
        /// </summary>
        /// <remarks>
        /// Just for easily check whether a UIObject is already a child of this UIObject or not.
        /// </remarks>
        private RCSet<UIObject> childrenSet;

        /// <summary>
        /// The position of this UIObject. See the corresponding property for more informations.
        /// </summary>
        private RCIntVector position;

        /// <summary>
        /// The cache of the position of this UIObject in screen coordinates.
        /// </summary>
        private CachedValue<RCIntVector> absPositionCache;

        /// <summary>
        /// The clipping rectangle of this UIObject. See the corresponding property for more informations.
        /// </summary>
        private RCIntRectangle clip;

        /// <summary>
        /// The cache of the clipping rectangle of this UIObject in screen coordinates.
        /// </summary>
        private CachedValue<RCIntRectangle> absClipCache;

        /// <summary>
        /// The cloaking rectangle of this UIObject. See the corresponding property for more informations.
        /// </summary>
        private RCIntRectangle cloak;

        /// <summary>
        /// The cache of the cloaking rectangle of this UIObject in screen coordinates.
        /// </summary>
        private CachedValue<RCIntRectangle> absCloakCache;

        /// <summary>
        /// The boundary of the drawing area of this UIObject. See the corresponding property for more
        /// informations.
        /// </summary>
        private RCIntRectangle range;

        /// <summary>
        /// The cache of the range rectangle of this UIObject in screen coordinates.
        /// </summary>
        private CachedValue<RCIntRectangle> absRangeCache;

        /// <summary>
        /// The pixel scaling of this UIObject relative to it's parent.
        /// </summary>
        private RCIntVector pixelScaling;

        /// <summary>
        /// The cache of the pixel scaling of this UIObject in screen coordinates.
        /// </summary>
        private CachedValue<RCIntVector> absPixelScalingCache;

        #endregion Private fields
    }
}
