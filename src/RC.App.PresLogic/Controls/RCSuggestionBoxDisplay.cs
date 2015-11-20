using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds new functionality to the map display control for displaying a suggestion boxes when an object
    /// is being placed onto the map.
    /// </summary>
    public class RCSuggestionBoxDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCSuggestionBoxDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCSuggestionBoxDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.suggestionBoxBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.LightGreen, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.suggestionBoxBrush.Upload();
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            if (this.MouseHandler != null &&
                this.MouseHandler.ObjectPlacementInfo != null)
            {
                List<RCIntRectangle> suggestionBoxes = this.MouseHandler.ObjectPlacementInfo.View.GetSuggestionBoxes();
                foreach (RCIntRectangle suggestionBox in suggestionBoxes)
                {
                    renderContext.RenderRectangle(this.suggestionBoxBrush, suggestionBox);
                }
            }
        }

        /// <summary>
        /// Brush for rendering the suggestion boxes.
        /// </summary>
        private readonly UISprite suggestionBoxBrush;
    }
}
