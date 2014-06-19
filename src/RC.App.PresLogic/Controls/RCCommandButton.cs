using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a button on the command panel of the gameplay page.
    /// </summary>
    public class RCCommandButton : UIButton
    {
        /// <summary>
        /// Constructs a command button at the given rectangular area inside the command panel.
        /// </summary>
        /// <param name="buttonRect"></param>
        public RCCommandButton(RCIntRectangle buttonRect, string displayedStr) : base(buttonRect.Location, buttonRect.Size)
        {
            UIFont font = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.displayedText = new UIString(displayedStr, font, UIWorkspace.Instance.PixelScaling, new RCColor(220, 220, 220));

            this.tmpBrushA = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.tmpBrushB = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightRed, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.tmpBrushA.Upload();
            this.tmpBrushB.Upload();
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            UISprite brush = this.IsPushed ? tmpBrushA : tmpBrushB;
            renderContext.RenderRectangle(brush, new RCIntRectangle(0, 0, 20, 20));
            renderContext.RenderString(this.displayedText, new RCIntVector(2, 15));
        }

        /// <summary>
        /// Temporary brushes for testing the display of the command buttons.
        /// </summary>
        private UISprite tmpBrushA;
        private UISprite tmpBrushB;

        private UIString displayedText;
    }
}
