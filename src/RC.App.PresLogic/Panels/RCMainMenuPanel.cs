using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.PresLogic.Controls;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// Represents the panel on the main menu page.
    /// </summary>
    class RCMainMenuPanel : RCAppPanel
    {
        /// <summary>
        /// Constructs an RCMainMenuPanel object.
        /// </summary>
        /// <param name="location">The location of the panel in workspace coordinates.</param>
        /// <param name="menuPoints">List of the menupoints.</param>
        public RCMainMenuPanel(RCIntRectangle location, string[] menuPoints)
            : base(location, new RCIntRectangle(0, 0, location.Width, location.Height),
                   ShowMode.Appear, HideMode.Disappear, 0, 0, null)
        {
            if (menuPoints == null || menuPoints.Length == 0) { throw new ArgumentNullException("menuPoints"); }

            int menupointHeight = location.Height / menuPoints.Length;
            this.menuButtons = new RCMenuButton[menuPoints.Length];
            this.menuButtonLookup = new Dictionary<string, RCMenuButton>();
            for (int i = 0; i < this.menuButtons.Length; i++)
            {
                this.menuButtons[i] = new RCMenuButton(menuPoints[i],
                                                       new RCIntRectangle(0, i*menupointHeight, location.Width, menupointHeight));
                this.menuButtonLookup.Add(menuPoints[i], this.menuButtons[i]);
                this.AddControl(this.menuButtons[i]);
            }
        }

        /// <summary>
        /// Gets the RCMenuButton with the given text.
        /// </summary>
        public RCMenuButton this[string text]
        {
            get { return this.menuButtonLookup[text]; }
        }

        /// <summary>
        /// List of the menu buttons.
        /// </summary>
        private RCMenuButton[] menuButtons;

        /// <summary>
        /// List of the menu buttons mapped by their texts.
        /// </summary>
        private Dictionary<string, RCMenuButton> menuButtonLookup;
    }
}
