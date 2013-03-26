using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.Common.Diagnostics;
using RC.App.BizLogic;
using RC.Common.ComponentModel;
using RC.App.PresLogic.Controls;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The map editor panel on the map editor page.
    /// </summary>
    public class RCMapEditorPanel : RCAppPanel
    {
        /// <summary>
        /// Enumerates the possible modes of the map editor.
        /// </summary>
        public enum EditMode
        {
            DrawTerrain = 0,
            PlaceTerrainObject = 1,
            PlaceStartingPoint = 2
        }

        /// <summary>
        /// Prototype of methods that handle events on the map editor panel.
        /// </summary>
        public delegate void MapEditorPanelHdl();

        /// <summary>
        /// Raised when the selected edit mode has been changed.
        /// </summary>
        public event MapEditorPanelHdl EditModeChanged;

        /// <summary>
        /// Raised when the selected item has been changed.
        /// </summary>
        public event MapEditorPanelHdl SelectedItemChanged;

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
                               string backgroundSprite,
                               ITileSetView tilesetView)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            if (tilesetView == null) { throw new ArgumentNullException("tilesetView"); }
            this.tilesetView = tilesetView;

            /// Create the controls.
            this.editModeSelector = new RCDropdownSelector(new RCIntVector(4, 4), 85, new string[3] { "Draw terrain", "Place terrain object", "Place starting point" });
            this.paletteListbox = new RCListBox(new RCIntVector(4, 22), 85, 9, 100);
            this.saveButton = new RCMenuButton("Save", new RCIntRectangle(4, 144, 41, 15));
            this.exitButton = new RCMenuButton("Exit", new RCIntRectangle(48, 144, 41, 15));

            this.editModeSelector.SelectedIndexChanged += this.OnEditModeSelectionChanged;
            this.paletteListbox.SelectedIndexChanged += this.OnPaletteListboxSelectionChanged;

            this.AddControl(this.editModeSelector);
            this.AddControl(this.paletteListbox);
            this.AddControl(this.saveButton);
            this.AddControl(this.exitButton);

            this.ResetControls();
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
        /// Gets the currently selected edit mode.
        /// </summary>
        public EditMode SelectedMode { get { return (EditMode)this.editModeSelector.SelectedIndex; } }

        /// <summary>
        /// Gets the currently selected listbox item or null if none of the listbox items are selected.
        /// </summary>
        public string SelectedItem { get { return this.paletteListbox.SelectedIndex != -1 ? this.paletteListbox[this.paletteListbox.SelectedIndex] : null; } }

        /// <summary>
        /// Resets the controls of the panel.
        /// </summary>
        public void ResetControls()
        {
            switch ((EditMode)this.editModeSelector.SelectedIndex)
            {
                case EditMode.DrawTerrain:
                    string[] terrainTypes = this.tilesetView.GetTerrainTypeNames().ToArray();
                    this.paletteListbox.SetItems(terrainTypes);
                    this.saveButton.IsEnabled = true;
                    this.editModeSelector.IsEnabled = true;
                    this.paletteListbox.IsEnabled = true;
                    break;
                case EditMode.PlaceTerrainObject:
                    string[] terrainObjectTypes = this.tilesetView.GetTerrainObjectTypeNames().ToArray();
                    this.paletteListbox.SetItems(terrainObjectTypes);
                    this.saveButton.IsEnabled = true;
                    this.editModeSelector.IsEnabled = true;
                    this.paletteListbox.IsEnabled = true;
                    break;
                case EditMode.PlaceStartingPoint:
                    this.paletteListbox.SetItems(new string[0] { });
                    this.saveButton.IsEnabled = true;
                    this.editModeSelector.IsEnabled = true;
                    this.paletteListbox.IsEnabled = false;
                    break;
                default:
                    throw new InvalidOperationException("Invalid EditMode!");
            }
        }

        /// <summary>
        /// Called when the selection in the edit mode dropdown has been changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnEditModeSelectionChanged(UISensitiveObject sender)
        {
            this.ResetControls();
            if (this.EditModeChanged != null) { this.EditModeChanged(); }
        }

        /// <summary>
        /// Called when the selection in the palette listbox has been changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnPaletteListboxSelectionChanged(UISensitiveObject sender)
        {
            if (this.SelectedItemChanged != null) { this.SelectedItemChanged(); }
        }

        /// <summary>
        /// Reference to the tileset view.
        /// </summary>
        private ITileSetView tilesetView;

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
