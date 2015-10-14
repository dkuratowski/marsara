using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.SpriteGroups
{
    /// <summary>
    /// This sprite group loads the sprites defined for the command buttons and masks them to a target color corresponding
    /// to different states of the buttons.
    /// </summary>
    class CmdButtonSpriteGroup : MaskedSpriteGroup
    {
        /// <summary>
        /// Constructs a CmdButtonSpriteGroup instance.
        /// </summary>
        /// <param name="cmdPanelView">Reference to the command panel view.</param>
        /// <param name="btnState">The button state for which this sprite group belongs to.</param>
        public CmdButtonSpriteGroup(ICommandView cmdPanelView, CommandButtonStateEnum btnState)
            : base()
        {
            if (cmdPanelView == null) { throw new ArgumentNullException("cmdPanelView"); }

            this.commandPanelView = cmdPanelView;
            this.buttonState = btnState;
        }

        #region Overriden from MaskedSpriteGroup

        /// <see cref="MaskedSpriteGroup.SpriteDefinitions"/>
        protected override IEnumerable<SpriteData> SpriteDefinitions { get { return this.commandPanelView.GetCmdButtonSpriteDatas(); } }

        /// <see cref="MaskedSpriteGroup.TargetColor"/>
        protected override RCColor TargetColor { get { return BUTTONSTATE_COLOR_MAPPINGS[this.buttonState]; } }

        #endregion Overriden from MaskedSpriteGroup

        /// <summary>
        /// Reference to the command panel view.
        /// </summary>
        private ICommandView commandPanelView;

        /// <summary>
        /// The button state for which this sprite group belongs to.
        /// </summary>
        private CommandButtonStateEnum buttonState;

        /// <summary>
        /// Defines the colors for the different button states.
        /// </summary>
        private static readonly Dictionary<CommandButtonStateEnum, RCColor> BUTTONSTATE_COLOR_MAPPINGS = new Dictionary<CommandButtonStateEnum, RCColor>()
        {
            { CommandButtonStateEnum.Enabled, RCColor.Yellow },
            { CommandButtonStateEnum.Disabled, RCColor.Gray },
            { CommandButtonStateEnum.Highlighted, RCColor.WhiteHigh },
        };
    }
}
