using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.BizLogic.BusinessComponents
{
    /// <summary>
    /// The interface of a map window.
    /// </summary>
    interface IMapWindow
    {
        /// <summary>
        /// Calculates the map coordinates of the given point on the window based on the current window position.
        /// </summary>
        /// <param name="windowCoords">The window coordinates to transform.</param>
        /// <returns>The point in map coordinates.</returns>
        RCNumVector WindowToMapCoords(RCIntVector windowCoords);

        /// <summary>
        /// Calculates the map coordinates of the given rectangular area on the window based on the current window position.
        /// </summary>
        /// <param name="windowRect">The rectangular area on the window to transform.</param>
        /// <returns>The rectangular area in map coordinates.</returns>
        RCNumRectangle WindowToMapRect(RCIntRectangle windowRect);

        /// <summary>
        /// Calculates the window coordinates of the given point on the map based on the current window position.
        /// </summary>
        /// <param name="mapCoords">The map coordinates to transform.</param>
        /// <returns>The point in window coordinates.</returns>
        RCIntVector MapToWindowCoords(RCNumVector mapCoords);

        /// <summary>
        /// Calculates the window coordinates of the given rectangular area on the map based on the current window position.
        /// </summary>
        /// <param name="mapRect">The rectangular area to transform.</param>
        /// <returns>The rectangular area in window coordinates.</returns>
        RCIntRectangle MapToWindowRect(RCNumRectangle mapRect);

        /// <summary>
        /// Calculates the window coordinates of the given rectangle of quadratic tiles on the map based on the current window position.
        /// </summary>
        /// <param name="quadRect">The rectangle of quadratic tiles to transform.</param>
        /// <returns>The rectangular area in window coordinates.</returns>
        RCIntRectangle QuadToWindowRect(RCIntRectangle quadRect);

        /// <summary>
        /// Calculates the window coordinates of the given rectangle of cells on the map based on the current window position.
        /// </summary>
        /// <param name="cellRect">The rectangle of cells to transform.</param>
        /// <returns>The rectangular area in window coordinates.</returns>
        RCIntRectangle CellToWindowRect(RCIntRectangle cellRect);

        /// <summary>
        /// Gets the current window in map coordinates.
        /// </summary>
        RCNumRectangle WindowMapCoords { get; }

        /// <summary>
        /// Gets the rectangle of cells on the map that are visible through the window.
        /// </summary>
        RCIntRectangle CellWindow { get; }

        /// <summary>
        /// Gets the difference between the top-left corner of the window and the top-left corner of the
        /// top-left visible cell in window coordinates.
        /// </summary>
        RCIntVector WindowOffset { get; }

        /// <summary>
        /// Gets the rectangle of quadratic tiles on the map that are visible through the window.
        /// </summary>
        RCIntRectangle QuadTileWindow { get; }

        /// <summary>
        /// Gets the rectangular area of the window on the pixel grid.
        /// </summary>
        RCIntRectangle PixelWindow { get; }
    }
}
