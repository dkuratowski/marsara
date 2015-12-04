using RC.Common;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Base class of custom content display on the details panel that are rendering text informations.
    /// </summary>
    public abstract class RCTextInformationDisplay : UIControl
    {
        /// <summary>
        /// Constructs an RCTextInformationDisplay control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        protected RCTextInformationDisplay(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            this.middle = size / 2;
        }

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            List<UIString> textsToDisplay = this.GetDisplayedTexts();
            List<int> textCoordsY = this.CalculateTextCoordsY(textsToDisplay);

            for (int i = 0; i < textsToDisplay.Count; i++)
            {
                UIString textToDisplay = textsToDisplay[i];
                renderContext.RenderString(textToDisplay, new RCIntVector(this.middle.X - textToDisplay.Width / 2, textCoordsY[i]));
            }
        }

        /// <summary>
        /// Gets the list of the texts to be displayed.
        /// </summary>
        protected abstract List<UIString> GetDisplayedTexts();

        /// <summary>
        /// Calculates the Y coordinates of the texts to be displayed.
        /// </summary>
        /// <param name="textsToDisplay">The list of texts to be displayed.</param>
        /// <returns>The calculated Y coordinates.</returns>
        private List<int> CalculateTextCoordsY(List<UIString> textsToDisplay)
        {
            if (textsToDisplay.Count == 0) { return new List<int>(); }

            int totalHeightOfTexts = textsToDisplay.Count - 1;
            foreach (UIString text in textsToDisplay) { totalHeightOfTexts += text.Font.MinimumLineHeight; }

            List<int> coordsY = new List<int> { this.middle.Y - totalHeightOfTexts / 2 + textsToDisplay[0].Font.CharTopMaximum };
            for (int i = 1; i < textsToDisplay.Count; i++)
            {
                int previousCoordY = coordsY[i - 1];
                UIString previousText = textsToDisplay[i - 1];
                UIString currentText = textsToDisplay[i];
                coordsY.Add(previousCoordY + previousText.Font.CharBottomMaximum + currentText.Font.CharTopMaximum + 1);
            }
            return coordsY;
        }

        /// <summary>
        /// The middle of this control.
        /// </summary>
        private readonly RCIntVector middle;
    }
}
