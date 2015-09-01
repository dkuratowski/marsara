using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.SpriteGroups;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Interface of a map display control.
    /// </summary>
    public interface IMapDisplay
    {
        /// <summary>
        /// Attaches the given mouse handler to the map display.
        /// </summary>
        /// <param name="handler">The handler to be attached.</param>
        /// <exception cref="InvalidOperationException">If another mouse handler has already been attached to the map display.</exception>
        void AttachMouseHandler(IMouseHandler handler);
        
        /// <summary>
        /// Detaches the currently attached mouse handler from the map display.
        /// </summary>
        /// <exception cref="InvalidOperationException">If ther is no mouse handler attached to the map display.</exception>
        void DetachMouseHandler();

        /// <summary>
        /// Gets the size of the map display in pixels.
        /// </summary>
        RCIntVector PixelSize { get; }
    }

    /// <summary>
    /// Contains informations for displaying object placement.
    /// </summary>
    public class ObjectPlacementInfo
    {
        /// <summary>
        /// Constructs an ObjectPlacementInfo instance.
        /// </summary>
        /// <param name="view">The view to be used.</param>
        /// <param name="sprites">The sprite group to be used.</param>
        public ObjectPlacementInfo(IObjectPlacementView view, ISpriteGroup sprites)
        {
            if (view == null) { throw new ArgumentNullException("view"); }
            if (sprites == null) { throw new ArgumentNullException("sprites"); }

            this.view = view;
            this.sprites = sprites;
        }

        /// <summary>
        /// Gets the view to be used.
        /// </summary>
        public IObjectPlacementView View { get { return this.view; } }

        /// <summary>
        /// Gets the sprite group to be used.
        /// </summary>
        public ISpriteGroup Sprites { get { return this.sprites; } }

        /// <summary>
        /// The view to be used.
        /// </summary>
        private IObjectPlacementView view;

        /// <summary>
        /// The sprite group to be used.
        /// </summary>
        private ISpriteGroup sprites;
    }

    /// <summary>
    /// Interface of a mouse handler.
    /// </summary>
    public interface IMouseHandler
    {
        /// <summary>
        /// Gets the selection box to be displayed or RCIntRectangle.Undefined if no selection box shall be displayed.
        /// </summary>
        RCIntRectangle SelectionBox { get; }

        /// <summary>
        /// Gets whether crosshairs shall be displayed or not.
        /// </summary>
        bool DisplayCrosshairs { get; }

        /// <summary>
        /// Gets the reference to the currently active object placement display informations or null if no object placement box shall be displayed.
        /// </summary>
        ObjectPlacementInfo ObjectPlacementInfo { get; }
    }
}
