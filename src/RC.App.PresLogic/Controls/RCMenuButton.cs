using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a button in the menu system of the RC application.
    /// </summary>
    public class RCMenuButton : UIButton
    {
        /// <summary>
        /// Constructs an RCMenuButton with the given text.
        /// </summary>
        public RCMenuButton(string text, RCIntRectangle buttonRect) : base(buttonRect.Location, buttonRect.Size)
        {
            this.menuButtonFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B");
            this.normalText = new UIString(text, this.menuButtonFont, UIWorkspace.Instance.PixelScaling, new UIColor(220, 220, 220));
            this.highlightedText = new UIString(text, this.menuButtonFont, UIWorkspace.Instance.PixelScaling, UIColor.White);
            this.textPosition = new RCIntVector((this.Range.Width - this.normalText.Width) / 2,
                (this.Range.Height - (this.menuButtonFont.CharBottomMaximum + this.menuButtonFont.CharTopMaximum + 1)) / 2 +
                this.menuButtonFont.CharTopMaximum);
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderString(this.IsHighlighted ? this.highlightedText : this.normalText,
                                       this.textPosition);
        }

        /// <summary>
        /// The normal text of this RCMenuButton.
        /// </summary>
        private UIString normalText;

        /// <summary>
        /// The highlighted text of this RCMenuButton.
        /// </summary>
        private UIString highlightedText;

        /// <summary>
        /// The computed position of the text.
        /// </summary>
        private RCIntVector textPosition;

        /// <summary>
        /// The font of the menu buttons.
        /// </summary>
        private UIFont menuButtonFont;
    }
}
