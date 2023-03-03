using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// Wrapper class representing the main window of the RC application.
    /// </summary>
    class MonoGameWindow
    {
        /// <summary>
        /// Constructs a MonoGameWindow instance over the given Game instance.
        /// </summary>
        public MonoGameWindow(Game game)
        {
            if (game == null) { throw new ArgumentNullException("game"); }

            this.game = game;
        }

        /// <summary>
        /// Gets or sets whether the MonoGameWindow has a visible border or not.
        /// Only supported on WindowsDX and DesktopGL platforms.
        /// </summary>
        public bool HasBorder
        {
            get { return !this.game.Window.IsBorderless; }
            set { this.game.Window.IsBorderless = !value; }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the top of this MonoGameWindow in the global coordinate space
        /// which stretches across all screens.
        /// </summary>
        public int Top
        {
            get { return this.game.Window.Position.Y; }
            set { this.game.Window.Position = new Point(this.game.Window.Position.X, value); }
        }

        /// <summary>
        /// Gets or sets the X coordinate of the left side of this MonoGameWindow in the global coordinate space
        /// which stretches across all screens.
        /// </summary>
        public int Left
        {
            get { return this.game.Window.Position.X; }
            set { this.game.Window.Position = new Point(value, this.game.Window.Position.Y); }
        }

        /// <summary>
        /// Indicates if the main window of the RC application is in focus.
        /// </summary>
        public bool IsActive
        {
            get { return this.game.IsActive; }
        }

        /// <summary>
        /// Raised when the main window of the RC application gains focus.
        /// </summary>
        public event EventHandler<EventArgs> Activated
        {
            add { this.game.Activated += value; }
            remove { this.game.Activated -= value; }
        }

        /// <summary>
        /// Raised when the main window of the RC application loses focus.
        /// </summary>
        public event EventHandler<EventArgs> Deactivated
        {
            add { this.game.Deactivated += value; }
            remove { this.game.Deactivated -= value; }
        }

        /// <summary>
        /// Reference to the underlying Game instance.
        /// </summary>
        private Game game;
    }
}