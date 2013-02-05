using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.Common.Diagnostics;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The map editor panel on the map editor page.
    /// </summary>
    public class RCMapEditorPanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCMapEditorPanel instance.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">
        /// Name of the sprite resource that will be the background of this panel or null if there is no background.
        /// </param>
        public RCMapEditorPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                               ShowMode showMode, HideMode hideMode,
                               int appearDuration, int disappearDuration,
                               string backgroundSprite)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            this.editModeSelector = new RCDropdownSelector(new RCIntVector(4, 4), 85, new string[3] { "Draw terrain", "Place terrain object", "Place starting point" });
            this.paletteListbox = new RCListBox(new RCIntVector(4, 22), 85, 9, 100);
            this.paletteListbox.SetItems(
                new string[25]
                {
                    "Item 0",
                    "Item 1",
                    "Item 2",
                    "Item 3",
                    "Item 4",
                    "Item 5",
                    "Item 6",
                    "Item 7",
                    "Item 8",
                    "Item 9",
                    "Item 10",
                    "Item 11",
                    "Item 12",
                    "Item 13",
                    "Item 14",
                    "Item 15",
                    "Item 16",
                    "Item 17",
                    "Item 18",
                    "Item 19",
                    "Item 20",
                    "Item 21",
                    "Item 22",
                    "Item 23",
                    "Item 24"
                });

            this.saveButton = new RCMenuButton("Save", new RCIntRectangle(4, 144, 41, 15));
            this.exitButton = new RCMenuButton("Exit", new RCIntRectangle(48, 144, 41, 15));

            this.AddControl(this.editModeSelector);
            this.AddControl(this.paletteListbox);
            this.AddControl(this.saveButton);
            this.AddControl(this.exitButton);
        }

        /// <summary>
        /// Gets the "Save" button.
        /// </summary>
        public RCMenuButton SaveButton { get { return this.saveButton; } }

        /// <summary>
        /// Gets the "Exit" button.
        /// </summary>
        public RCMenuButton ExitButton { get { return this.exitButton; } }

        /// <summary>
        /// The edit-mode selector control.
        /// </summary>
        private RCDropdownSelector editModeSelector;

        /// <summary>
        /// The palette listbox.
        /// </summary>
        private RCListBox paletteListbox;

        /// <summary>
        /// The "Save" button.
        /// </summary>
        private RCMenuButton saveButton;

        /// <summary>
        /// The "Exit" button.
        /// </summary>
        private RCMenuButton exitButton;
    }
}
