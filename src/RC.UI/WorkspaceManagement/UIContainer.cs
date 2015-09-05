using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a container with controls.
    /// </summary>
    public class UIContainer : UISensitiveObject
    {
        /// <summary>
        /// Constructs a UIContainer at the given position with the given range.
        /// </summary>
        public UIContainer(RCIntVector position, RCIntRectangle range) :
            base(position, range)
        {
            this.controls = new List<UIControl>();
            this.areAttached = false;
            this.areAttachedSensitive = false;
        }

        /// <summary>
        /// Adds a control to this container.
        /// </summary>
        /// <param name="control">The control to add.</param>
        public void AddControl(UIControl control)
        {
            if (this.controls.Contains(control)) { throw new UIException("The control is already in the container!"); }

            if (this.areAttached)
            {
                this.Attach(control);
                if (this.areAttachedSensitive)
                {
                    this.AttachSensitive(control);
                }
            }

            this.controls.Add(control);
        }

        /// <summary>
        /// Removes a control from this container.
        /// </summary>
        /// <param name="control">The control to remove.</param>
        public void RemoveControl(UIControl control)
        {
            int ctrlIdx = this.controls.IndexOf(control);
            if (ctrlIdx == -1) { throw new UIException("The control was not in the container!"); }

            if (this.areAttachedSensitive)
            {
                this.DetachSensitive(control);
                if (this.areAttached)
                {
                    this.Detach(control);
                }
            }

            this.controls.Remove(control);
        }

        /// <summary>
        /// Attaches all the controls to this UIContainer into the UI-tree.
        /// </summary>
        public void AttachControls()
        {
            if (this.areAttached) { throw new UIException("The controls are already attached to the container!"); }

            foreach (UIControl ctrl in this.controls)
            {
                this.Attach(ctrl);
            }
            this.areAttached = true;
        }

        /// <summary>
        /// Detaches all the controls from this UIContainer out of the UI-tree.
        /// </summary>
        public void DetachControls()
        {
            if (!this.areAttached) { throw new UIException("The controls are not currently attached to the container!"); }
            if (this.areAttachedSensitive) { throw new UIException("The controls must be detached from the sensitive tree first!"); }

            foreach (UIControl ctrl in this.controls)
            {
                this.Detach(ctrl);
            }
            this.areAttached = false;
        }

        /// <summary>
        /// Attaches all the controls to this UIContainer into the sensitive tree.
        /// </summary>
        public void AttachControlsSensitive()
        {
            if (this.areAttachedSensitive) { throw new UIException("The controls are already attached to the container!"); }
            if (!this.areAttached) { throw new UIException("The controls must be attached to the UI-tree first!"); }

            foreach (UIControl ctrl in this.controls)
            {
                this.AttachSensitive(ctrl);
            }
            this.areAttachedSensitive = true;
        }

        /// <summary>
        /// Detaches all the controls from this UIContainer out of the sensitive tree.
        /// </summary>
        public void DetachControlsSensitive()
        {
            if (!this.areAttachedSensitive) { throw new UIException("The controls are not currently attached to the container!"); }

            foreach (UIControl ctrl in this.controls)
            {
                this.DetachSensitive(ctrl);
            }
            this.areAttachedSensitive = false;
        }

        /// <summary>
        /// List of the controls in this UIContainer.
        /// </summary>
        private readonly List<UIControl> controls;

        /// <summary>
        /// Flag indicates whether the added controls are attached to the UI-tree.
        /// </summary>
        private bool areAttached;

        /// <summary>
        /// Flag indicates whether the added controls are attached to the sensitive tree.
        /// </summary>
        private bool areAttachedSensitive;
    }
}
