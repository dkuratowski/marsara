using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Views of the commands.
    /// </summary>
    /// TODO: implement this view!
    class CommandView : ICommandView
    {
        /// <summary>
        /// Constructs a CommandView instance.
        /// </summary>
        public CommandView()
        {
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
        }

        #region ICommandView methods

        /// <see cref="ICommandView.GetCmdButtonSpriteDefs"/>
        public List<SpriteDef> GetCmdButtonSpriteDefs()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (ISpritePalette palette in this.commandManager.SpritePalettes)
            {
                byte[] imageData = new byte[palette.ImageData.Length];
                Array.Copy(palette.ImageData, imageData, palette.ImageData.Length);
                SpriteDef info = new SpriteDef
                {
                    ImageData = imageData,
                    TransparentColor = palette.TransparentColor,
                    MaskColor = palette.MaskColor,
                    IsMaskableSprite = true
                };
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ICommandView.GetCmdButtonSprite"/>
        public SpriteInst GetCmdButtonSprite(RCIntVector panelPosition)
        {
            return this.commandManager.GetCmdButtonSprite(panelPosition);
        }

        /// <see cref="ICommandView.GetCmdButtonState"/>
        public CommandButtonStateEnum GetCmdButtonState(RCIntVector panelPosition)
        {
            return this.commandManager.GetCmdButtonState(panelPosition);
        }

        /// <see cref="ICommandView.IsWaitingForTargetPosition"/>
        public bool IsWaitingForTargetPosition { get { return this.commandManager.IsWaitingForTargetPosition; } }

        /// <see cref="ICommandView.SelectedBuildingType"/>
        public string SelectedBuildingType { get { return this.commandManager.SelectedBuildingType; } }

        #endregion ICommandView methods

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private ICommandManagerBC commandManager;
    }
}
