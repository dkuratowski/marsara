using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a modal dialog on the UI.
    /// </summary>
    public class UIDialog : UIContainer
    {
        /// <summary>
        /// Constructs a UIDialog instance.
        /// </summary>
        /// <param name="size">The size of the dialog.</param>
        public UIDialog(RCIntVector size)
            : base((UIWorkspace.Instance.WorkspaceSize - size) / 2, new RCIntRectangle(0, 0, size.X, size.Y))
        {
            if (size.X > UIWorkspace.Instance.WorkspaceSize.X || size.Y > UIWorkspace.Instance.WorkspaceSize.Y)
            {
                throw new ArgumentOutOfRangeException("size", "Size of a UIDialog cannot be greater than the size of the UIWorkspace.");
            }
        }
    }
}
