using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.UI;

namespace RC.App.PresLogic
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
        public CmdButtonSpriteGroup(ICommandPanelView cmdPanelView, CmdButtonStateEnum btnState)
            : base()
        {
            if (cmdPanelView == null) { throw new ArgumentNullException("cmdPanelView"); }

            this.commandPanelView = cmdPanelView;
            this.buttonState = btnState;
        }

        #region Overriden from MaskedSpriteGroup

        /// <see cref="MaskedSpriteGroup.SpriteDefinitions"/>
        protected override IEnumerable<SpriteDef> SpriteDefinitions { get { return this.commandPanelView.GetCmdButtonSpriteDefs(); } }

        /// <see cref="MaskedSpriteGroup.TargetColor"/>
        protected override UIColor TargetColor { get { return BUTTONSTATE_COLOR_MAPPINGS[this.buttonState]; } }

        #endregion Overriden from MaskedSpriteGroup

        /// <summary>
        /// Reference to the command panel view.
        /// </summary>
        private ICommandPanelView commandPanelView;

        /// <summary>
        /// The button state for which this sprite group belongs to.
        /// </summary>
        private CmdButtonStateEnum buttonState;

        /// <summary>
        /// Defines the colors for the different button states.
        /// </summary>
        private static readonly Dictionary<CmdButtonStateEnum, UIColor> BUTTONSTATE_COLOR_MAPPINGS = new Dictionary<CmdButtonStateEnum, UIColor>()
        {
            { CmdButtonStateEnum.Enabled, UIColor.Yellow },
            { CmdButtonStateEnum.Disabled, UIColor.Gray },
            { CmdButtonStateEnum.Highlighted, UIColor.WhiteHigh },
        };
    }
}
