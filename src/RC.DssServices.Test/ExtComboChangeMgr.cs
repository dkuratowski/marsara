using System.Collections.Generic;
using System.Windows.Forms;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This class must be used if the selected index of a color selector ComboBoxes has been changed not by the user to avoid
    /// unexpected behaviour.
    /// </summary>
    class ExtComboChangeMgr
    {
        /// <summary>
        /// Creates an ExtComboChangeMgr object.
        /// </summary>
        public ExtComboChangeMgr()
        {
            this.changedFromOutside = new RCSet<ComboBox>();
        }

        /// <summary>
        /// Changes the selected index on the given ComboBox and register this change as an external change.
        /// </summary>
        public void ChangeSelectedIndex(ComboBox cb, int newSelectedIdx)
        {
            if (cb.SelectedIndex != newSelectedIdx)
            {
                if (!this.changedFromOutside.Contains(cb))
                {
                    this.changedFromOutside.Add(cb);
                }

                cb.SelectedIndex = newSelectedIdx;

                this.changedFromOutside.Remove(cb);
            }
        }

        /// <summary>
        /// Gets whether the given ComboBox has been changed by the user or not.
        /// </summary>
        /// <returns>True if it has been changed by the user, false otherwise.</returns>
        public bool IsComboBoxChangedByTheUser(ComboBox cb)
        {
            return !this.changedFromOutside.Contains(cb);
        }

        /// <summary>
        /// List of the color selector comboboxes that has been changed not by the user.
        /// </summary>
        private RCSet<ComboBox> changedFromOutside;
    }
}
