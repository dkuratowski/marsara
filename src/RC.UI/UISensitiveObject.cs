using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// This class represents a UIObject which has sensitive range. UISensitiveObjects are able to
    /// receive events from input devices such as mouse, keyboard, touchscreen, etc.
    /// </summary>
    public class UISensitiveObject : UIObject
    {
        #region Sensitive-tree event definitions

        /// <summary>
        /// Raised by the root of the sensitive-tree after a UISensitiveObject has been attached to the tree.
        /// </summary>
        public event ObjectAttachedDetachedHdl ObjectAttached;

        /// <summary>
        /// Raised by the root of the sensitive-tree before a UISensitiveObject is detached from the tree.
        /// </summary>
        public event ObjectAttachedDetachedHdl ObjectDetaching;

        /// <summary>
        /// Represents a method that can handle object attachment/detachment events.
        /// </summary>
        /// <param name="obj">The attached or detached UISensitiveObject.</param>
        public delegate void ObjectAttachedDetachedHdl(UISensitiveObject obj);

        #endregion Sensitive-tree event definitions

        /// <summary>
        /// Constructs a UISensitiveObject instance.
        /// </summary>
        /// <param name="position">The position of this UISensitiveObject relative to it's parent.</param>
        /// <param name="range">
        /// The sensitive range rectangle of this UISensitiveObject in it's local coordinate-system.
        /// </param>
        /// <remarks>
        /// The pixel scaling of this UISensitiveObject will be (1, 1) relative to it's parent automatically.
        /// </remarks>
        public UISensitiveObject(RCIntVector position, RCIntRectangle range)
            : base(new RCIntVector(1, 1), position, range)
        {
            this.mouseSensor = new UIMouseSensor(this);

            this.sensitiveChildren = new List<UISensitiveObject>();
            this.sensitiveChildrenSet = new HashSet<UISensitiveObject>();

            this.absSensitivePositionCache = new CachedValue<RCIntVector>(this.ComputeAbsSensitivePosition);
            this.absSensitiveClipCache = new CachedValue<RCIntRectangle>(this.ComputeAbsSensitiveClip);
            this.absSensitiveCloakCache = new CachedValue<RCIntRectangle>(this.ComputeAbsSensitiveCloak);
            this.absSensitiveRangeCache = new CachedValue<RCIntRectangle>(this.ComputeAbsSensitiveRange);
        }

        #region Sensitive tree manipulation

        /// <summary>
        /// Attaches the given UISensitiveObject to this UISensitiveObject as a sensitive child.
        /// </summary>
        /// <param name="otherObj">The UISensitiveObject you want to attach as a sensitive child.</param>
        public void AttachSensitive(UISensitiveObject otherObj)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (otherObj == null) { throw new ArgumentNullException("otherObj"); }
            if (otherObj == this) { throw new UIException("Unable to attach a UISensitiveObject to itself as a sensitive child!"); }
            if (otherObj.Parent != this) { throw new UIException("The object must be attached first by UIObject.Attach!"); }
            if (otherObj.sensitiveParent != null) { throw new UIException("The given UISensitiveObject must be detached from it's sensitive parent first!"); }

            /// Attach
            //this.Attach(otherObj);
            this.sensitiveChildrenSet.Add(otherObj);
            this.sensitiveChildren.Insert(0, otherObj);
            otherObj.sensitiveParent = this;

            /// Check the tree property
            if (this.CheckTreeProperty())
            {
                /// Subscribe for the Z-order events of otherObj
                otherObj.BroughtForward += this.BroughtForwardHdl;
                otherObj.BroughtToTop += this.BroughtToTopHdl;
                otherObj.SentBackward += this.SentBackwardHdl;
                otherObj.SentToBottom += this.SentToBottomHdl;

                /// Subscribe for the PositionChanged, ClipRectChanged, CloakRectChanged and RangeRectChanged
                /// events of otherObj
                otherObj.PositionChanged += this.ChildPositionChangedHdl;
                otherObj.ClipRectChanged += this.ChildClipRectChangedHdl;
                otherObj.CloakRectChanged += this.ChildCloakRectChangedHdl;
                otherObj.RangeRectChanged += this.ChildRangeRectChangedHdl;

                /// Invalidate the cached values in the attached UISensitiveObject
                otherObj.InvalidateSensitivePosition();
                otherObj.InvalidateSensitiveClipRect();
                otherObj.InvalidateSensitiveCloakRect();
                otherObj.InvalidateSensitiveRangeRect();

                /// Raise the UISensitiveObject.ObjectAttached event of the sensitive-root.
                if (this.SensitiveRoot.ObjectAttached != null)
                {
                    this.SensitiveRoot.ObjectAttached(otherObj);
                }
            }
            else
            {
                /// Tree property violation --> rollback and throw an exception.
                this.sensitiveChildren.RemoveAt(0);
                this.sensitiveChildrenSet.Remove(otherObj);
                otherObj.sensitiveParent = null;
                throw new UIException("Violating tree property!");
            }
        }

        /// <summary>
        /// Detaches the given sensitive child of this UISensitiveObject.
        /// </summary>
        /// <param name="whichChild">The UISensitiveObject you want to detach.</param>
        public void DetachSensitive(UISensitiveObject whichChild)
        {
            if (UIRoot.Instance.GraphicsPlatform.RenderLoop.IsRendering) { throw new InvalidOperationException("Rendering is in progress!"); }
            if (whichChild.Parent != this) { throw new UIException("The object is already detached by UIObject.Detach!"); }
            if (whichChild == null) { throw new ArgumentNullException("whichChild"); }

            if (!this.sensitiveChildrenSet.Contains(whichChild)) { return; }

            /// Raise the UISensitiveObject.ObjectDetaching event of the sensitive-root.
            if (this.SensitiveRoot.ObjectDetaching != null)
            {
                this.SensitiveRoot.ObjectDetaching(whichChild);
            }

            /// Detach
            //this.Detach(whichChild);
            this.sensitiveChildrenSet.Remove(whichChild);
            this.sensitiveChildren.Remove(whichChild);
            whichChild.sensitiveParent = null;

            /// Unbscribe from the PositionChanged, ClipRectChanged, CloakRectChanged and RangeRectChanged
            /// events of whichChild
            whichChild.PositionChanged -= this.ChildPositionChangedHdl;
            whichChild.ClipRectChanged -= this.ChildClipRectChangedHdl;
            whichChild.CloakRectChanged -= this.ChildCloakRectChangedHdl;
            whichChild.RangeRectChanged -= this.ChildRangeRectChangedHdl;

            /// Unsubscribe from the Z-order events of whichChild
            whichChild.BroughtForward -= this.BroughtForwardHdl;
            whichChild.BroughtToTop -= this.BroughtToTopHdl;
            whichChild.SentBackward -= this.SentBackwardHdl;
            whichChild.SentToBottom -= this.SentToBottomHdl;

            /// Invalidate the cached values in the detached UISensitiveObject
            whichChild.InvalidateSensitivePosition();
            whichChild.InvalidateSensitiveClipRect();
            whichChild.InvalidateSensitiveCloakRect();
            whichChild.InvalidateSensitiveRangeRect();
        }

        #endregion Sensitive tree manipulation

        #region Public properties and methods

        /// <summary>
        /// Gets the sensitive parent of this UISensitiveObject or a null reference if there is no sensitive parent.
        /// </summary>
        public UISensitiveObject SensitiveParent { get { return this.sensitiveParent; } }

        /// <summary>
        /// Gets the list of the sensitive children of this UISensitiveObject in the order of rendering.
        /// </summary>
        public UISensitiveObject[] SensitiveChildren { get { return this.sensitiveChildren.ToArray(); } }

        /// <summary>
        /// Gets the root object of the sensitive-tree.
        /// </summary>
        public UISensitiveObject SensitiveRoot
        {
            get { return this.sensitiveParent != null ? this.sensitiveParent.SensitiveRoot : this; }
        }

        /// <summary>
        /// Get the list of the UISensitiveObjects that are visible at the given position. The position has to be given
        /// in the local coordinate-system of the SensitiveRoot.
        /// </summary>
        /// <param name="sensitivePosition">The position to check.</param>
        /// <returns>
        /// The list of the visible UISensitiveObjects starting from SensitiveRoot down into the sensitive-tree.
        /// </returns>
        /// <remarks>You can call this method on any UISensitiveObject in the sensitive-tree.</remarks>
        public List<UISensitiveObject> GetObjectsVisibleAt(RCIntVector sensitivePosition)
        {
            if (this.sensitiveParent == null)
            {
                /// If this is the root of the sensitive-tree, start the algorythm.
                List<UISensitiveObject> visibleObjects = new List<UISensitiveObject>();
                if (this.AbsoluteSensitiveRange.Contains(sensitivePosition))
                {
                    this.GetObjectsVisibleAt(sensitivePosition, ref visibleObjects);
                }
                return visibleObjects;
            }
            else
            {
                /// Otherwise call the same method for the root object.
                return this.SensitiveRoot.GetObjectsVisibleAt(sensitivePosition);
            }
        }

        /// <summary>
        /// Transforms the given sensitive position to the local coordinate-system of this UISensitiveObject.
        /// </summary>
        /// <param name="sensitivePosition">The sensitive position to transform.</param>
        /// <returns>The position in the local coordinate-system of this UISensitiveObject.</returns>
        public RCIntVector TransformAbsToLocal(RCIntVector sensitivePosition)
        {
            return sensitivePosition - this.AbsoluteSensitivePosition;
        }

        /// <summary>
        /// Gets the position of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        public RCIntVector AbsoluteSensitivePosition { get { return this.absSensitivePositionCache.Value; } }

        /// <summary>
        /// Gets the clip rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        public RCIntRectangle AbsoluteSensitiveClip { get { return this.absSensitiveClipCache.Value; } }

        /// <summary>
        /// Gets the cloak rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        public RCIntRectangle AbsoluteSensitiveCloak { get { return this.absSensitiveCloakCache.Value; } }

        /// <summary>
        /// Gets the range rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        public RCIntRectangle AbsoluteSensitiveRange { get { return this.absSensitiveRangeCache.Value; } }

        /// <summary>
        /// Puts this UISensitiveObject and all of it's children to the end of objList recursively in a
        /// depth-first-search order.
        /// </summary>
        /// <param name="objList">The returned list.</param>
        public void WalkSensitiveTreeDFS(ref List<UISensitiveObject> objList)
        {
            if (objList == null) { throw new ArgumentNullException("objList"); }

            objList.Add(this);
            foreach (UISensitiveObject child in this.sensitiveChildren)
            {
                child.WalkSensitiveTreeDFS(ref objList);
            }
        }

        /// <summary>
        /// Gets the mouse pointer that shall be displayed at the given position if the mouse is over this sensitive object.
        /// </summary>
        /// <param name="localPosition">The position to test in the local coordinate system of this sensitive object.</param>
        /// <returns>
        /// The mouse pointer that shall be displayed at the given position if the mouse is over this sensitive object.
        /// If this method returns null then the default mouse pointer will be displayed.
        /// The default mouse pointer can be set using the UIWorkspace.SetDefaultMousePointer method.
        /// </returns>
        /// <remarks>Can be overriden in the derived classes. The default implementation doesn't display mouse pointer.</remarks>
        public virtual UIPointer GetMousePointer(RCIntVector localPosition) { return null; }

        /// <summary>
        /// This method is called automatically when this UISensitiveObject is being detached from the
        /// sensitive tree and it's internal state has to be reset. The default implementation doesn't
        /// do anything but the method can be overriden in the derived classes.
        /// </summary>
        public virtual void ResetState() { }

        #endregion Public properties and methods

        #region Sensors

        /// <summary>
        /// Gets the mouse sensor of this UISensitiveObject.
        /// </summary>
        public IUIMouseSensor MouseSensor
        {
            get { return this.mouseSensor; }
        }

        #endregion Sensors

        #region Internal methods

        /// <summary>
        /// Internal method for checking the existence of the tree property after an attach.
        /// </summary>
        /// <returns>True if the tree property still exists, false otherwise.</returns>
        private bool CheckTreeProperty()
        {
            UISensitiveObject current = this;
            while (current.sensitiveParent != null)
            {
                if (current.sensitiveParent == this)
                {
                    return false;
                }
                else
                {
                    current = current.sensitiveParent;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Called when a sensitive child has been brought forward in the Z-order.
        /// </summary>
        private void BroughtForwardHdl(UIObject whichChild)
        {
            UISensitiveObject sensitiveChild = whichChild as UISensitiveObject;
            if (sensitiveChild != null && (sensitiveChild.PreviousSibling as UISensitiveObject) != null)
            {
                int childIndex = this.sensitiveChildren.IndexOf(sensitiveChild);
                if (childIndex > 0)
                {
                    /// Swap the given UISensitiveObject and the UISensitiveObject in front of it.
                    this.sensitiveChildren[childIndex] = this.sensitiveChildren[childIndex - 1];
                    this.sensitiveChildren[childIndex - 1] = sensitiveChild;
                }
            }
        }

        /// <summary>
        /// Called when a sensitive child has been brought to top in the Z-order.
        /// </summary>
        private void BroughtToTopHdl(UIObject whichChild)
        {
            UISensitiveObject sensitiveChild = whichChild as UISensitiveObject;
            if (sensitiveChild != null)
            {
                int childIndex = this.sensitiveChildren.IndexOf(sensitiveChild);
                if (childIndex > 0)
                {
                    for (int i = childIndex; i > 0; i--)
                    {
                        this.sensitiveChildren[i] = this.sensitiveChildren[i - 1];
                    }
                    this.sensitiveChildren[0] = sensitiveChild;
                }
            }
        }

        /// <summary>
        /// Called when a sensitive child has been sent backward in the Z-order.
        /// </summary>
        private void SentBackwardHdl(UIObject whichChild)
        {
            UISensitiveObject sensitiveChild = whichChild as UISensitiveObject;
            if (sensitiveChild != null && (sensitiveChild.NextSibling as UISensitiveObject) != null)
            {
                int childIndex = this.sensitiveChildren.IndexOf(sensitiveChild);
                if (-1 != childIndex && childIndex < this.sensitiveChildren.Count - 1)
                {
                    /// Swap the given UISensitiveObject and the UISensitiveObject behind it.
                    this.sensitiveChildren[childIndex] = this.sensitiveChildren[childIndex + 1];
                    this.sensitiveChildren[childIndex + 1] = sensitiveChild;
                }
            }
        }

        /// <summary>
        /// Called when a sensitive child has been sent to bottom in the Z-order.
        /// </summary>
        private void SentToBottomHdl(UIObject whichChild)
        {
            UISensitiveObject sensitiveChild = whichChild as UISensitiveObject;
            if (sensitiveChild != null)
            {
                int childIndex = this.sensitiveChildren.IndexOf(sensitiveChild);
                if (-1 != childIndex && childIndex < this.sensitiveChildren.Count - 1)
                {
                    for (int i = childIndex; i < this.sensitiveChildren.Count - 1; i++)
                    {
                        this.sensitiveChildren[i] = this.sensitiveChildren[i + 1];
                    }
                    this.sensitiveChildren[this.sensitiveChildren.Count - 1] = sensitiveChild;
                }
            }
        }

        /// <summary>
        /// Computes the position of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private RCIntVector ComputeAbsSensitivePosition()
        {
            return this.sensitiveParent != null ?
                   this.Position + this.sensitiveParent.AbsoluteSensitivePosition :
                   new RCIntVector(0, 0);
        }

        /// <summary>
        /// Computes the clip rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private RCIntRectangle ComputeAbsSensitiveClip()
        {
            return this.Clip != RCIntRectangle.Undefined ?
                   this.Clip + this.AbsoluteSensitivePosition :
                   RCIntRectangle.Undefined;
        }

        /// <summary>
        /// Computes the cloak rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private RCIntRectangle ComputeAbsSensitiveCloak()
        {
            return this.Cloak != RCIntRectangle.Undefined ?
                   this.Cloak + this.AbsoluteSensitivePosition :
                   RCIntRectangle.Undefined;
        }

        /// <summary>
        /// Computes the range rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private RCIntRectangle ComputeAbsSensitiveRange()
        {
            return this.Range + this.AbsoluteSensitivePosition;
        }

        /// <summary>
        /// Invalidates the sensitive position of this UISensitiveObject and all of it's sensitive children.
        /// </summary>
        private void InvalidateSensitivePosition()
        {
            this.absSensitivePositionCache.Invalidate();
            foreach (UISensitiveObject child in this.sensitiveChildren)
            {
                child.InvalidateSensitivePosition();
            }
        }

        /// <summary>
        /// Invalidates the sensitive clipping rectangle of this UISensitiveObject and all of it's sensitive children.
        /// </summary>
        private void InvalidateSensitiveClipRect()
        {
            this.absSensitiveClipCache.Invalidate();
            foreach (UISensitiveObject child in this.sensitiveChildren)
            {
                child.InvalidateSensitiveClipRect();
            }
        }

        /// <summary>
        /// Invalidates the sensitive cloaking rectangle of this UISensitiveObject and all of it's sensitive children.
        /// </summary>
        private void InvalidateSensitiveCloakRect()
        {
            this.absSensitiveCloakCache.Invalidate();
            foreach (UISensitiveObject child in this.sensitiveChildren)
            {
                child.InvalidateSensitiveCloakRect();
            }
        }

        /// <summary>
        /// Invalidates the sensitive range rectangle of this UISensitiveObject and all of it's sensitive children.
        /// </summary>
        private void InvalidateSensitiveRangeRect()
        {
            this.absSensitiveRangeCache.Invalidate();
            foreach (UISensitiveObject child in this.sensitiveChildren)
            {
                child.InvalidateSensitiveRangeRect();
            }
        }

        /// <summary>
        /// Handler for child position changed events.
        /// </summary>
        private void ChildPositionChangedHdl(UIObject sender, RCIntVector prevPos, RCIntVector currPos)
        {
            UISensitiveObject sensitiveSender = sender as UISensitiveObject;
            if (sensitiveSender != null)
            {
                /// Invalidate the cached values of the sender.
                sensitiveSender.InvalidateSensitivePosition();
                sensitiveSender.InvalidateSensitiveClipRect();
                sensitiveSender.InvalidateSensitiveCloakRect();
                sensitiveSender.InvalidateSensitiveRangeRect();
            }
        }

        /// <summary>
        /// Handler for child clip rectangle changed events.
        /// </summary>
        private void ChildClipRectChangedHdl(UIObject sender, RCIntRectangle prevClip, RCIntRectangle currClip)
        {
            UISensitiveObject sensitiveSender = sender as UISensitiveObject;
            if (sensitiveSender != null)
            {
                /// Invalidate the cached clip rectangle.
                sensitiveSender.absSensitiveClipCache.Invalidate();
            }
        }

        /// <summary>
        /// Handler for child cloak rectangle changed events.
        /// </summary>
        private void ChildCloakRectChangedHdl(UIObject sender, RCIntRectangle prevCloak, RCIntRectangle currCloak)
        {
            UISensitiveObject sensitiveSender = sender as UISensitiveObject;
            if (sensitiveSender != null)
            {
                /// Invalidate the cached cloak rectangle.
                sensitiveSender.absSensitiveCloakCache.Invalidate();
            }
        }

        /// <summary>
        /// Handler for child range rectangle changed events.
        /// </summary>
        private void ChildRangeRectChangedHdl(UIObject sender, RCIntRectangle prevRange, RCIntRectangle currRange)
        {
            UISensitiveObject sensitiveSender = sender as UISensitiveObject;
            if (sensitiveSender != null)
            {
                /// Invalidate the cached range rectangle.
                sensitiveSender.absSensitiveRangeCache.Invalidate();
            }
        }

        /// <summary>
        /// This recursive method collects the UISensitiveObjects that are visible at a given point in
        /// the coordinate-system of the sensitive-root.
        /// </summary>
        /// <param name="sensitivePosition">The position to check the visibility from.</param>
        /// <param name="collectedObjects">The collected objects.</param>
        private void GetObjectsVisibleAt(RCIntVector sensitivePosition, ref List<UISensitiveObject> collectedObjects)
        {
            collectedObjects.Add(this);
            if (this.AbsoluteSensitiveClip != RCIntRectangle.Undefined ?
                this.AbsoluteSensitiveClip.Contains(sensitivePosition) :
                this.AbsoluteSensitiveRange.Contains(sensitivePosition))
            {
                /// Continue the recursion with the first visible sensitive child.
                foreach (UISensitiveObject child in this.sensitiveChildren)
                {
                    if (child.AbsoluteSensitiveRange.Contains(sensitivePosition))
                    {
                        child.GetObjectsVisibleAt(sensitivePosition, ref collectedObjects);
                        break;
                    }
                }
            }
        }

        #endregion Internal methods

        #region Private fields

        /// <summary>
        /// Reference to the sensitive parent of this UISensitiveObject or null if this is a top-level UISensitiveObject.
        /// </summary>
        private UISensitiveObject sensitiveParent;

        /// <summary>
        /// Ordered list of the sensitive children of this UISensitiveObject.
        /// </summary>
        private List<UISensitiveObject> sensitiveChildren;

        /// <summary>
        /// Unordered list of the sensitive children of this UISensitiveObject.
        /// </summary>
        /// <remarks>
        /// Just for easily check whether a UISensitiveObject is already a sensitive child of this UISensitiveObject or not.
        /// </remarks>
        private HashSet<UISensitiveObject> sensitiveChildrenSet;

        /// <summary>
        /// The cache of the position of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private CachedValue<RCIntVector> absSensitivePositionCache;

        /// <summary>
        /// The cache of the clip rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private CachedValue<RCIntRectangle> absSensitiveClipCache;

        /// <summary>
        /// The cache of the cloak rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private CachedValue<RCIntRectangle> absSensitiveCloakCache;

        /// <summary>
        /// The cache of the range rectangle of this UISensitiveObject in the coordinate-system of the sensitive-root.
        /// </summary>
        private CachedValue<RCIntRectangle> absSensitiveRangeCache;

        /// <summary>
        /// The mouse sensor attached to this UISensitiveObject.
        /// </summary>
        private UIMouseSensor mouseSensor;

        #endregion Private fields
    }
}
