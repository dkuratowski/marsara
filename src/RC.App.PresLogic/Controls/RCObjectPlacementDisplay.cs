using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Views;
using RC.App.BizLogic.Services;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds new functionality to the map display control for displaying a bounding box and an object placement mask when an object
    /// is being placed onto the map.
    /// </summary>
    public class RCObjectPlacementDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCObjectPlacementDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCObjectPlacementDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.objectPlacementMaskGreen = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ObjectPlacementMaskGreen");
            this.objectPlacementMaskRed = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ObjectPlacementMaskRed");
            this.lastKnownMousePosition = RCIntVector.Undefined;
        }

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            this.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.DisconnectEx_i"/>
        protected override void DisconnectEx_i()
        {
            this.MouseSensor.Move -= this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            if (this.MouseHandler != null &&
                this.MouseHandler.ObjectPlacementInfo != null &&
                this.lastKnownMousePosition != RCIntVector.Undefined)
            {
                ObjectPlacementBox placementBox =
                    this.MouseHandler.ObjectPlacementInfo.View.GetObjectPlacementBox(this.lastKnownMousePosition);

                foreach (SpriteRenderInfo spriteToDisplay in placementBox.Sprites)
                {
                    UISprite uiSpriteToDisplay = this.MouseHandler.ObjectPlacementInfo.Sprites[spriteToDisplay.Index];
                    renderContext.RenderSprite(uiSpriteToDisplay, spriteToDisplay.DisplayCoords, spriteToDisplay.Section);
                }

                foreach (RCIntRectangle part in placementBox.IllegalParts)
                {
                    renderContext.RenderSprite(this.objectPlacementMaskRed, part.Location);
                }

                foreach (RCIntRectangle part in placementBox.LegalParts)
                {
                    renderContext.RenderSprite(this.objectPlacementMaskGreen, part.Location);
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has been moved over the display.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.lastKnownMousePosition != evtArgs.Position)
            {
                this.lastKnownMousePosition = evtArgs.Position;
            }
        }

        /// <summary>
        /// The sprite for displaying the green parts of the object placement mask.
        /// </summary>
        private UISprite objectPlacementMaskGreen;

        /// <summary>
        /// The sprite for displaying the red parts of the object placement mask.
        /// </summary>
        private UISprite objectPlacementMaskRed;

        /// <summary>
        /// The last known position of the mouse cursor in the coordinate system of the display.
        /// </summary>
        private RCIntVector lastKnownMousePosition;
    }
}
