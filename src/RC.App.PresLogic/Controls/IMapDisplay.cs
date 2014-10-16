using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Enumerates the possible scrolling directions of a map display.
    /// </summary>
    public enum ScrollDirection
    {
        NoScroll = -1,
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7
    }

    /// <summary>
    /// Interface of a map display control.
    /// </summary>
    public interface IMapDisplay
    {
        /// <summary>
        /// Gets the area that is currently displayed on the control.
        /// </summary>
        RCIntRectangle DisplayedArea { get; }

        /// <summary>
        /// Attaches the given mouse handler to the control.
        /// </summary>
        /// <param name="handler">The handler to be attached.</param>
        /// <exception cref="InvalidOperationException">If another mouse handler has already been attached to the control.</exception>
        void AttachMouseHandler(IMouseHandler handler);
        
        /// <summary>
        /// Detaches the currently attached mouse handler from the control.
        /// </summary>
        /// <exception cref="InvalidOperationException">If ther is no mouse handler attached to the control.</exception>
        void DetachMouseHandler();
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
        public ObjectPlacementInfo(IObjectPlacementView view, SpriteGroup sprites)
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
        public SpriteGroup Sprites { get { return this.sprites; } }

        /// <summary>
        /// The view to be used.
        /// </summary>
        private IObjectPlacementView view;

        /// <summary>
        /// The sprite group to be used.
        /// </summary>
        private SpriteGroup sprites;
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

        /// <summary>
        /// Gets the current scrolling direction.
        /// </summary>
        ScrollDirection CurrentScrollDirection { get; }
    }
}
