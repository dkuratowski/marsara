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
    /// Interface of views of the command panel.
    /// </summary>
    /// TODO: implement this view!
    class CommandPanelView : ICommandPanelView
    {
        /// <summary>
        /// Constructs a CommandPanelView instance.
        /// </summary>
        public CommandPanelView()
        {
            this.commandManager = ComponentManager.GetInterface<ICommandManagerBC>();
        }

        #region ICommandPanelView methods

        /// <see cref="ICommandPanelView.GetCmdButtonSpriteDefs"/>
        public List<SpriteDef> GetCmdButtonSpriteDefs()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (ISpritePalette palette in this.commandManager.CommandWorkflowSpritePalettes)
            {
                byte[] imageData = new byte[palette.ImageData.Length];
                Array.Copy(palette.ImageData, imageData, palette.ImageData.Length);
                SpriteDef info = new SpriteDef();
                info.ImageData = imageData;
                info.TransparentColor = palette.TransparentColor;
                info.MaskColor = palette.MaskColor;
                info.IsMaskableSprite = true;
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ICommandPanelView.GetCmdButtonSprite"/>
        public SpriteInst GetCmdButtonSprite(int row, int col)
        {
            /// TODO: Implement!
            return new SpriteInst
            {
                Index = 0,
                DisplayCoords = new RCIntVector(0, 0),
                Section = new RCIntRectangle(43, 1, 20, 20)
            };
        }

        /// <see cref="ICommandPanelView.GetCmdButtonState"/>
        public CmdButtonStateEnum GetCmdButtonState(int row, int col)
        {
            /// TODO: Implement!
            return CmdButtonStateEnum.Highlighted;
        }

        /// <see cref="ICommandPanelView.InputMode"/>
        public MouseInputModeEnum InputMode
        {
            get
            {
                /// TODO: Implement!
                throw new NotImplementedException();
            }
        }

        /// <see cref="ICommandPanelView.SelectedBuildingType"/>
        public string SelectedBuildingType
        {
            get
            {
                /// TODO: Implement!
                throw new NotImplementedException();
            }
        }

        #endregion ICommandPanelView methods

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private ICommandManagerBC commandManager;
    }
}
