using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// This class controls the mouse pointer on the screen. If you want to display the mouse pointer, the only thing
    /// to do is to create an instance of this class and pass a UISensitiveObject (called as target object in the future)
    /// to the constructor. The mouse pointer then will be bound to the target object. The mouse pointer is visible on
    /// the screen only if the target object has a parent and it is directly or indirectly attached to the current
    /// render manager.
    /// </summary>
    public sealed class UIMouseManager : UIObject, IDisposable
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="targetObj">The UISensitiveObject that this UIMouseManager will be bound to.</param>
        public UIMouseManager(UISensitiveObject targetObj)
            : base(targetObj.PixelScaling, targetObj.Position, targetObj.Range)
        {
            if (targetObj == null) { throw new ArgumentNullException("targetObj"); }
            if (targetObj.SensitiveParent != null) { throw new InvalidOperationException("targetObj must be the root of the sensitive-tree!"); }
            if (targetObj.Parent == null) { throw new InvalidOperationException("targetObj must have a parent!"); }

            this.objectDisposed = false;
            this.raisingMouseEvents = false;
            this.targetObject = targetObj;
            this.defaultMousePointer = null;

            /// Attach this UIMouseManager to the parent of targetObject.
            this.targetObject.Parent.Attach(this);
            this.SendInFrontOfTargetObject();

            /// Subscribe for the events of the target object.
            this.targetObject.Attached += this.OnInvalidTreeOperation;
            this.targetObject.Detached += this.OnInvalidTreeOperation;
            this.targetObject.BroughtForward += this.OnInvalidTreeOperation;
            this.targetObject.BroughtToTop += this.OnInvalidTreeOperation;
            this.targetObject.SentBackward += this.OnInvalidTreeOperation;
            this.targetObject.SentToBottom += this.OnInvalidTreeOperation;
            this.targetObject.ClipRectChanged += this.OnInvalidTreeOperation;
            this.targetObject.CloakRectChanged += this.OnInvalidTreeOperation;
            this.targetObject.RangeRectChanged += this.OnInvalidTreeOperation;
            this.targetObject.PositionChanged += this.OnInvalidTreeOperation;
            this.targetObject.ObjectAttached += this.ObjectAttached;
            this.targetObject.ObjectDetaching += this.ObjectDetaching;

            /// Subscribe for the events of this object
            this.Attached += this.OnInvalidTreeOperation;
            this.Detached += this.OnInvalidTreeOperation;
            this.BroughtForward += this.OnInvalidTreeOperation;
            this.BroughtToTop += this.OnInvalidTreeOperation;
            this.SentBackward += this.OnInvalidTreeOperation;
            this.SentToBottom += this.OnInvalidTreeOperation;
            this.ClipRectChanged += this.OnInvalidTreeOperation;
            this.CloakRectChanged += this.OnInvalidTreeOperation;
            this.RangeRectChanged += this.OnInvalidTreeOperation;
            this.PositionChanged += this.OnInvalidTreeOperation;
            this.ChildAttached += this.OnInvalidTreeOperation;
            this.ChildDetached += this.OnInvalidTreeOperation;
            this.AbsolutePixelScalingChanged += this.OnInvalidTreeOperation;

            /// Subscribe for system mouse events.
            UIRoot.Instance.MouseAccess.StateChanged += this.OnMouseEvent;

            /// Set the initial position of the mouse pointer in the scaled range
            this.scale = 1.0f;
            this.scaledRange = this.Range * this.scale;
            this.scaledPosition = this.scaledRange.Location + (this.scaledRange.Size / 2);
            this.pointerPosition = this.scaledPosition / this.scale;

            /// Set the initial state of the mouse wheel and buttons
            this.pressedButtons = new HashSet<UIMouseButton>();
            this.wheelPosition = 0;
            
            /// Create the UIMouseSensors
            this.allSensors = new HashSet<UIMouseSensor>();
            this.touchedSensors = new List<UIMouseSensor>();
            this.activeSensor = null;
            this.tmpSensorOperations = new List<bool>();
            this.tmpSensors = new List<UIMouseSensor>();

            List<UISensitiveObject> sensitiveTree = new List<UISensitiveObject>();
            this.targetObject.WalkSensitiveTreeDFS(ref sensitiveTree);
            foreach (UISensitiveObject obj in sensitiveTree)
            {
                UIMouseSensor sensor = obj.MouseSensor as UIMouseSensor;
                if (sensor == null) { throw new UIException("Incompatible mouse sensor!"); }
                this.allSensors.Add(sensor);
            }
        }

        #region IDisposable Members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.objectDisposed) { throw new ObjectDisposedException("UIMouseManager"); }

            /// Remove the default mouse pointer.
            this.defaultMousePointer = null;

            /// Destroy every registered UIMouseSensor objects.
            this.allSensors.Clear();
            this.touchedSensors.Clear();
            this.activeSensor = null;
            this.tmpSensorOperations.Clear();
            this.tmpSensors.Clear();

            /// Reset the state of the mouse wheel and buttons.
            this.pressedButtons.Clear();
            this.wheelPosition = 0;

            /// Unsubscribe from the events of the target object.
            this.targetObject.Attached -= this.OnInvalidTreeOperation;
            this.targetObject.Detached -= this.OnInvalidTreeOperation;
            this.targetObject.BroughtForward -= this.OnInvalidTreeOperation;
            this.targetObject.BroughtToTop -= this.OnInvalidTreeOperation;
            this.targetObject.SentBackward -= this.OnInvalidTreeOperation;
            this.targetObject.SentToBottom -= this.OnInvalidTreeOperation;
            this.targetObject.ClipRectChanged -= this.OnInvalidTreeOperation;
            this.targetObject.CloakRectChanged -= this.OnInvalidTreeOperation;
            this.targetObject.RangeRectChanged -= this.OnInvalidTreeOperation;
            this.targetObject.PositionChanged -= this.OnInvalidTreeOperation;
            this.targetObject.ObjectAttached -= this.ObjectAttached;
            this.targetObject.ObjectDetaching -= this.ObjectDetaching;

            /// Unsubscribe from the events of this object
            this.Attached -= this.OnInvalidTreeOperation;
            this.Detached -= this.OnInvalidTreeOperation;
            this.BroughtForward -= this.OnInvalidTreeOperation;
            this.BroughtToTop -= this.OnInvalidTreeOperation;
            this.SentBackward -= this.OnInvalidTreeOperation;
            this.SentToBottom -= this.OnInvalidTreeOperation;
            this.ClipRectChanged += this.OnInvalidTreeOperation;
            this.CloakRectChanged += this.OnInvalidTreeOperation;
            this.RangeRectChanged -= this.OnInvalidTreeOperation;
            this.PositionChanged -= this.OnInvalidTreeOperation;
            this.ChildAttached -= this.OnInvalidTreeOperation;
            this.ChildDetached -= this.OnInvalidTreeOperation;
            this.AbsolutePixelScalingChanged -= this.OnInvalidTreeOperation;

            /// Unsubscribe from system mouse events.
            UIRoot.Instance.MouseAccess.StateChanged -= this.OnMouseEvent;

            /// Detach from the parent.
            this.Parent.Detach(this);
            this.targetObject = null;
            this.raisingMouseEvents = false;
            this.objectDisposed = true;
        }

        #endregion IDisposable Members

        #region Public properties

        /// <summary>
        /// Gets or sets the sensitivity of the mouse pointer.
        /// </summary>
        public float Sensitivity
        {
            get { return this.scale; }
            set
            {
                if (value <= 0.0f) { throw new ArgumentOutOfRangeException("Sensitivity"); }

                this.scaledRange = this.Range * value;
                this.scaledPosition = this.scaledPosition * (value / this.scale);
                this.scale = value;
                this.pointerPosition = this.scaledPosition / this.scale;
            }
        }

        /// <summary>
        /// Sets the default mouse pointer.
        /// </summary>
        /// <param name="defaultPointer">The default mouse pointer or null if no default pointer is defined.</param>
        public void SetDefaultMousePointer(UIPointer defaultPointer) { this.defaultMousePointer = defaultPointer; }

        #endregion Public properties

        #region UIObject overrides

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            UIMouseSensor sensor = this.activeSensor != null ? this.activeSensor : this.touchedSensors[this.touchedSensors.Count - 1];
            UIPointer pointer = sensor.GetMousePointer(this.pointerPosition);
            if (pointer == null) { pointer = this.defaultMousePointer; }
            if (pointer != null)
            {
                renderContext.RenderSprite(pointer.Icon,
                                           this.pointerPosition - pointer.Offset);
            }
        }

        #endregion UIObject overrides

        #region Internal methods

        /// <summary>
        /// Sends this UIMouseManager just in front of the target object.
        /// </summary>
        /// <remarks>
        /// If the target object doesn't have parent then this function has no effect.
        /// </remarks>
        private void SendInFrontOfTargetObject()
        {
            if (this.targetObject.Parent != null && this.targetObject.Parent == this.Parent)
            {
                this.BringToTop();
                while (this.PreviousSibling != this.targetObject) { this.SendBackward(); }
            }
        }

        /// <summary>
        /// Called when a system mouse event arrived from the event queue.
        /// </summary>
        /// <param name="evt">The arguments of the mouse event.</param>
        private void OnMouseEvent()
        {
            RCIntVector mouseDelta = UIRoot.Instance.MouseAccess.Delta;
            HashSet<UIMouseButton> pressedButtons = UIRoot.Instance.MouseAccess.PressedButtons;
            int scrollWheelPos = UIRoot.Instance.MouseAccess.ScrollWheelPos;

            this.scaledPosition += mouseDelta;
            if (!this.scaledRange.Contains(this.scaledPosition))
            {
                this.scaledPosition = new RCIntVector(Math.Min(this.scaledRange.Right - 1, Math.Max(this.scaledRange.Left, this.scaledPosition.X)),
                                                   Math.Min(this.scaledRange.Bottom - 1, Math.Max(this.scaledRange.Top, this.scaledPosition.Y)));
            }

            RCIntVector prevPointerPos = this.pointerPosition;
            RCIntVector newPointerPos = this.scaledPosition / this.scale;

            if (prevPointerPos != newPointerPos ||
                !this.pressedButtons.SetEquals(pressedButtons) ||
                this.wheelPosition != scrollWheelPos)
            {
                this.raisingMouseEvents = true;

                /// Raise the appropriate mouse movement events on the sensors.
                List<UIMouseSensor> visibleSensors = this.GetVisibleSensors(newPointerPos);
                this.RaiseMovementEvents(visibleSensors, newPointerPos, newPointerPos != prevPointerPos);

                /// Raise the appropriate mouse wheel events on the appropriate sensor
                this.RaiseMouseWheelEvents(
                    scrollWheelPos,
                    newPointerPos,
                    this.activeSensor != null ? this.activeSensor : visibleSensors[visibleSensors.Count - 1]);

                /// Raise the appropriate mouse button events on the appropriate sensor
                this.RaiseMouseButtonEvents(
                    pressedButtons,
                    newPointerPos,
                    this.activeSensor != null ? this.activeSensor : visibleSensors[visibleSensors.Count - 1]);

                /// Refresh the active sensor reference
                if (this.activeSensor != null && this.activeSensor.ActivatorButton == UIMouseButton.Undefined)
                {
                    this.activeSensor = null;
                }
                else if (this.activeSensor == null &&
                         visibleSensors[visibleSensors.Count - 1].ActivatorButton != UIMouseButton.Undefined)
                {
                    this.activeSensor = visibleSensors[visibleSensors.Count - 1];
                }

                /// Refresh the list of the touched sensors
                this.touchedSensors = visibleSensors;

                this.raisingMouseEvents = false;
                this.ExecuteSensorOperations();
            }

            /// Refresh the status data
            this.pressedButtons = pressedButtons;
            this.wheelPosition = scrollWheelPos;
            this.pointerPosition = newPointerPos;
        }

        /// <summary>
        /// Called after a UISensitiveObject has been attached to the sensitive-tree.
        /// </summary>
        /// <param name="obj">The new object.</param>
        private void ObjectAttached(UISensitiveObject obj)
        {
            if (this.raisingMouseEvents)
            {
                /// Postpone the needed sensor operations
                List<UISensitiveObject> newObjects = new List<UISensitiveObject>();
                obj.WalkSensitiveTreeDFS(ref newObjects);
                foreach (UISensitiveObject newObj in newObjects)
                {
                    /// Save the created sensor and indicate that it will have to be registered.
                    UIMouseSensor newSensor = newObj.MouseSensor as UIMouseSensor;
                    if (newSensor == null) { throw new UIException("Incompatible mouse sensor!"); }
                    this.tmpSensors.Add(newSensor);
                    this.tmpSensorOperations.Add(true);
                }
            }
            else
            {
                /// Execute the needed sensor operations
                List<UISensitiveObject> newObjects = new List<UISensitiveObject>();
                obj.WalkSensitiveTreeDFS(ref newObjects);
                foreach (UISensitiveObject newObj in newObjects)
                {
                    UIMouseSensor newSensor = newObj.MouseSensor as UIMouseSensor;
                    if (newSensor == null) { throw new UIException("Incompatible mouse sensor!"); }
                    this.allSensors.Add(newSensor);
                }
            }
        }

        /// <summary>
        /// Called before a UISensitiveObject is detached from the sensitive-tree.
        /// </summary>
        /// <param name="obj">The detaching object.</param>
        private void ObjectDetaching(UISensitiveObject obj)
        {
            if (this.raisingMouseEvents)
            {
                /// Postpone the needed sensor operations
                List<UISensitiveObject> detachingObjects = new List<UISensitiveObject>();
                obj.WalkSensitiveTreeDFS(ref detachingObjects);
                foreach (UISensitiveObject detachingObj in detachingObjects)
                {
                    /// Save the removed sensor and indicate that it will have to be unregistered.
                    UIMouseSensor removedSensor = detachingObj.MouseSensor as UIMouseSensor;
                    if (removedSensor == null) { throw new UIException("Incompatible mouse sensor!"); }
                    this.tmpSensors.Add(removedSensor);
                    this.tmpSensorOperations.Add(false);
                }
            }
            else
            {
                /// Execute the needed sensor operations
                List<UISensitiveObject> detachingObjects = new List<UISensitiveObject>();
                obj.WalkSensitiveTreeDFS(ref detachingObjects);
                foreach (UISensitiveObject detachingObj in detachingObjects)
                {
                    UIMouseSensor deletedSensor = detachingObj.MouseSensor as UIMouseSensor;
                    if (deletedSensor != null && this.allSensors.Contains(deletedSensor))
                    {
                        deletedSensor.Reset();
                        this.allSensors.Remove(deletedSensor);
                        int idxOfDeleted = this.touchedSensors.IndexOf(deletedSensor);
                        if (idxOfDeleted != -1)
                        {
                            this.touchedSensors.RemoveRange(idxOfDeleted, this.touchedSensors.Count - idxOfDeleted);
                        }
                        if (this.activeSensor == deletedSensor) { this.activeSensor = null; }
                    }
                    else
                    {
                        throw new UIException("UIMouseSensor not registered!");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the sensors that are visible at the current position of the mouse pointer.
        /// </summary>
        private List<UIMouseSensor> GetVisibleSensors(RCIntVector pointerPosition)
        {
            List<UISensitiveObject> touchedObjects = this.targetObject.GetObjectsVisibleAt(pointerPosition);
            List<UIMouseSensor> retList = new List<UIMouseSensor>();
            foreach (UISensitiveObject obj in touchedObjects)
            {
                UIMouseSensor sensor = obj.MouseSensor as UIMouseSensor;
                if (sensor == null) { throw new UIException("Mouse sensor not found!"); }
                retList.Add(sensor);
            }
            return retList;
        }

        /// <summary>
        /// Raises the mouse movement events on the appropriate sensors.
        /// </summary>
        private void RaiseMovementEvents(List<UIMouseSensor> currVisibleSensors,
                                         RCIntVector newPointerPos,
                                         bool raiseOnMove)
        {
            if (this.activeSensor != null)
            {
                if (this.activeSensor.ActiveOver && !this.activeSensor.TargetObject.AbsoluteSensitiveRange.Contains(newPointerPos))
                {
                    this.activeSensor.OnLeave();
                }
                else if (!this.activeSensor.ActiveOver && this.activeSensor.TargetObject.AbsoluteSensitiveRange.Contains(newPointerPos))
                {
                    this.activeSensor.OnEnter();
                }
                if (raiseOnMove)
                {
                    this.activeSensor.OnMove(newPointerPos);
                }
            }

            int i = 0;
            /// Call the OnMove trigger while the two lists are equal.
            for (;
                 i < this.touchedSensors.Count &&
                 i < currVisibleSensors.Count &&
                 this.touchedSensors[i] == currVisibleSensors[i];
                 ++i)
            {
                if (raiseOnMove && currVisibleSensors[i] != this.activeSensor)
                {
                    currVisibleSensors[i].OnMove(newPointerPos);
                }
            }

            /// Call the OnLeave trigger for the remaining elements of the old list.
            int j = i;
            for (; i < this.touchedSensors.Count; ++i)
            {
                if (this.touchedSensors[i] != this.activeSensor)
                {
                    this.touchedSensors[i].OnLeave();
                }
            }

            /// Call the OnEnter trigger for the remaining elements of the new list.
            for (; j < currVisibleSensors.Count; ++j)
            {
                if (currVisibleSensors[j] != this.activeSensor)
                {
                    currVisibleSensors[j].OnEnter();
                }
            }
        }

        /// <summary>
        /// Raises the mouse button events on the appropriate sensor.
        /// </summary>
        private void RaiseMouseButtonEvents(HashSet<UIMouseButton> buttons,
                                            RCIntVector newPointerPos,
                                            UIMouseSensor targetSensor)
        {
            foreach (UIMouseButton btn in this.pressedButtons)
            {
                if (!buttons.Contains(btn))
                {
                    targetSensor.OnButtonUp(newPointerPos, btn);
                }
            }

            foreach (UIMouseButton btn in buttons)
            {
                if (!this.pressedButtons.Contains(btn))
                {
                    targetSensor.OnButtonDown(newPointerPos, btn);
                }
            }
        }

        /// <summary>
        /// Raises the mouse wheel events on the appropriate sensor.
        /// </summary>
        private void RaiseMouseWheelEvents(int wheelPos,
                                           RCIntVector newPointerPos,
                                           UIMouseSensor targetSensor)
        {
            if (this.wheelPosition != wheelPos)
            {
                targetSensor.OnWheel(newPointerPos, wheelPos - this.wheelPosition);
            }
        }

        /// <summary>
        /// Executes the saved sensor operations.
        /// </summary>
        private void ExecuteSensorOperations()
        {
            for (int i = 0; i < this.tmpSensorOperations.Count; i++)
            {
                if (this.tmpSensorOperations[i])
                {
                    /// Register sensor
                    UIMouseSensor newSensor = this.tmpSensors[i];
                    this.allSensors.Add(newSensor);
                }
                else
                {
                    /// Unregister sensor
                    UIMouseSensor deletedSensor = this.tmpSensors[i];
                    if (this.allSensors.Contains(deletedSensor))
                    {
                        deletedSensor.Reset();
                        this.allSensors.Remove(deletedSensor);
                        int idxOfDeleted = this.touchedSensors.IndexOf(deletedSensor);
                        if (idxOfDeleted != -1)
                        {
                            this.touchedSensors.RemoveRange(idxOfDeleted, this.touchedSensors.Count - idxOfDeleted);
                        }

                        if (this.activeSensor == deletedSensor) { this.activeSensor = null; }
                    }
                    else
                    {
                        throw new UIException("UIMouseSensor not registered!");
                    }
                }
            }
            this.tmpSensorOperations.Clear();
            this.tmpSensors.Clear();
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject parentObj, UIObject childObj)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender, RCIntVector prev, RCIntVector curr)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender, RCIntRectangle prev, RCIntRectangle curr)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        #endregion Internal methods

        #region Private fields

        /// <summary>
        /// Reference to the default mouse pointer or null if no default pointer defined.
        /// </summary>
        private UIPointer defaultMousePointer;

        /// <summary>
        /// Reference to the target object of this UIMouseManager.
        /// </summary>
        private UISensitiveObject targetObject;

        /// <summary>
        /// The current position of the pointer.
        /// </summary>
        private RCIntVector pointerPosition;

        /// <summary>
        /// The scaled range rectangle.
        /// </summary>
        private RCIntRectangle scaledRange;

        /// <summary>
        /// The scaling factor of the mouse range.
        /// </summary>
        private float scale;

        /// <summary>
        /// The position of the mouse pointer in the scaled range.
        /// </summary>
        private RCIntVector scaledPosition;

        /// <summary>
        /// This flag indicates whether this object has been disposed or not.
        /// </summary>
        private bool objectDisposed;

        /// <summary>
        /// List of all registered UIMouseSensor objects.
        /// </summary>
        private HashSet<UIMouseSensor> allSensors;

        /// <summary>
        /// List of the UIMouseSensors that are touched by the mouse pointer.
        /// </summary>
        private List<UIMouseSensor> touchedSensors;

        /// <summary>
        /// Reference to the currently active sensor.
        /// </summary>
        private UIMouseSensor activeSensor;

        /// <summary>
        /// List of the pressed mouse buttons.
        /// </summary>
        private HashSet<UIMouseButton> pressedButtons;

        /// <summary>
        /// The position of the scroll wheel on the mouse.
        /// </summary>
        private int wheelPosition;

        /// <summary>
        /// This flag indicates whether the mouse manager is currently raising the mouse events or not.
        /// </summary>
        private bool raisingMouseEvents;

        /// <summary>
        /// Temporary list of operations with sensors.
        /// </summary>
        private List<bool> tmpSensorOperations;

        /// <summary>
        /// Temporary list of sensors.
        /// </summary>
        private List<UIMouseSensor> tmpSensors;

        #endregion Private fields
    }
}
