using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.UI;
using RC.Common;

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
        /// <param name="map">Reference to a map view.</param>
        public RCObjectPlacementDisplay(RCMapDisplay extendedControl, IMapView map)
            : base(extendedControl, map)
        {
            this.objects = null;
            this.objectPlacementView = null;
            this.objectPlacementMaskGreen = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ObjectPlacementMaskGreen");
            this.objectPlacementMaskRed = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ObjectPlacementMaskRed");
            this.lastKnownMousePosition = RCIntVector.Undefined;
        }

        /// <summary>
        /// Starts placing an object onto the screen.
        /// </summary>
        /// <param name="objPlacementView">The object placement view that will provide the object placement box to be displayed.</param>
        /// <param name="objectGroup">The sprite-group indexed by the object placement view.</param>
        public void StartPlacingObject(IObjectPlacementView objPlacementView, SpriteGroup objectGroup)
        {
            if (objPlacementView == null) { throw new ArgumentNullException("objPlacementView"); }
            if (objectGroup == null) { throw new ArgumentNullException("objectGroup"); }
            if (this.objectPlacementView != null) { throw new InvalidOperationException("Object placement has already been started!"); }

            this.objectPlacementView = objPlacementView;
            this.objects = objectGroup;
        }

        /// <summary>
        /// Stops placing an object onto the screen. If object placing has already been stopped or has not yet started then this function has no effect.
        /// </summary>
        public void StopPlacingObject()
        {
            this.objectPlacementView = null;
            this.objects = null;
        }

        /// <summary>
        /// Gets whether this map display extension is currently displaying object placement or not.
        /// </summary>
        public bool PlacingObject { get { return this.objectPlacementView != null; } }

        /// <see cref="RCMapDisplayExtension.StartExtension_i"/>
        protected override void StartExtension_i()
        {
            this.MouseSensor.Move += this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension.StopExtension_i"/>
        protected override void StopExtension_i()
        {
            this.MouseSensor.Move -= this.OnMouseMove;
        }

        /// <see cref="RCMapDisplayExtension"/>
        protected override void RenderExtension_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined && this.objectPlacementView != null && this.lastKnownMousePosition != RCIntVector.Undefined)
            {
                ObjectPlacementBox placementBox = this.objectPlacementView.GetObjectPlacementBox(this.DisplayedArea, this.lastKnownMousePosition);
                
                UISprite spriteToDisplay = this.objects[placementBox.Sprite.Index];
                renderContext.RenderSprite(spriteToDisplay, placementBox.Sprite.DisplayCoords);
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
        /// Reference to the object placement view or null if there is no object being placed.
        /// </summary>
        private IObjectPlacementView objectPlacementView;

        /// <summary>
        /// List of the objects.
        /// </summary>
        private SpriteGroup objects;

        /// <summary>
        /// The last known position of the mouse cursor in the coordinate system of the display.
        /// </summary>
        private RCIntVector lastKnownMousePosition;
    }
}
