using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface of views of the command buttons.
    /// </summary>
    public interface ICommandButtonView
    {
        /// <summary>
        /// Gets the sprite of the button to be displayed currently.
        /// </summary>
        SpriteInst ButtonSprite { get; }

        /// <summary>
        /// Gets the current state of the button.
        /// </summary>
        CmdButtonStateEnum State { get; }
    }
}
