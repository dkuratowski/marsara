using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Adds the following new functionalities to the map display control:
    ///     - displays the current Fog Of War state of the quadratic tiles
    /// </summary>
    class RCFogOfWarDisplay : RCMapDisplayExtension
    {
        /// <summary>
        /// Constructs an RCFogOfWarDisplay extension for the given map display control.
        /// </summary>
        /// <param name="extendedControl">The map display control to extend.</param>
        public RCFogOfWarDisplay(RCMapDisplay extendedControl)
            : base(extendedControl)
        {
            this.fogOfWarView = null;
            this.partialFowSprites = null;
            this.fullFowSprites = null;

            /// Load the Fog Of War sprite palette.
            string fowSpritePalettePath = System.IO.Path.Combine(FOW_SPRITEPALETTE_DIR, FOW_SPRITEPALETTE_FILE);
            XDocument fowSpritePaletteXml = XDocument.Load(fowSpritePalettePath);
            this.fowSpritePalette = XmlHelper.LoadSpritePalette(fowSpritePaletteXml.Root, FOWTypeEnum.Full, FOW_SPRITEPALETTE_DIR);
        }
        
        #region Overrides

        /// <see cref="RCMapDisplayExtension.MapView"/>
        protected override IMapView MapView { get { return this.fogOfWarView; } }

        /// <see cref="RCMapDisplayExtension.ConnectEx_i"/>
        protected override void ConnectEx_i()
        {
            /// Create a Fog Of War view.
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.fogOfWarView = viewService.CreateView<IFogOfWarView>();

            /// Create the Fog Of War sprite groups.
            this.partialFowSprites = new FOWSpriteGroup(this.fowSpritePalette, FOWTypeEnum.Partial);
            this.fullFowSprites = new FOWSpriteGroup(this.fowSpritePalette, FOWTypeEnum.Full);
        }

        /// <see cref="RCMapDisplayExtension.ConnectExBackgroundProc_i"/>
        protected override void ConnectExBackgroundProc_i()
        {
            this.partialFowSprites.Load();
            this.fullFowSprites.Load();
        }

        /// <see cref="RCMapDisplayExtension.DisconnectExBackgroundProc_i"/>
        protected override void DisconnectExBackgroundProc_i()
        {
            this.partialFowSprites.Unload();
            this.fullFowSprites.Unload();

            this.fogOfWarView = null;
            this.partialFowSprites = null;
            this.fullFowSprites = null;
        }

        /// <see cref="RCMapDisplayExtension.RenderEx_i"/>
        protected override void RenderEx_i(IUIRenderContext renderContext)
        {
            if (this.DisplayedArea != RCIntRectangle.Undefined)
            {
                /// Display the partial FOW-tiles.
                foreach (SpriteInst partialFowTileInfo in this.fogOfWarView.GetPartialFOWTiles(this.DisplayedArea))
                {
                    UISprite fowSprite = this.partialFowSprites[partialFowTileInfo.Index];
                    renderContext.RenderSprite(fowSprite, partialFowTileInfo.DisplayCoords, partialFowTileInfo.Section);
                }

                /// Display the full FOW-tiles.
                foreach (SpriteInst fullFowTileInfo in this.fogOfWarView.GetFullFOWTiles(this.DisplayedArea))
                {
                    UISprite fowSprite = this.fullFowSprites[fullFowTileInfo.Index];
                    renderContext.RenderSprite(fowSprite, fullFowTileInfo.DisplayCoords, fullFowTileInfo.Section);
                }
            }
        }

        #endregion Overrides

        /// <summary>
        /// Reference to the Fog Of War view.
        /// </summary>
        private IFogOfWarView fogOfWarView;

        /// <summary>
        /// Sprite group for rendering the partial Fog Of War of the quadratic tiles.
        /// </summary>
        private SpriteGroup partialFowSprites;

        /// <summary>
        /// Sprite group for rendering the full Fog Of War of the quadratic tiles.
        /// </summary>
        private SpriteGroup fullFowSprites;

        /// <summary>
        /// Reference to the Fog Of War sprite palette.
        /// </summary>
        private ISpritePalette<FOWTypeEnum> fowSpritePalette;

        /// <summary>
        /// The directory and the file in which the FOW sprite-palette definition is located.
        /// </summary>
        private static readonly string FOW_SPRITEPALETTE_DIR = ConstantsTable.Get<string>("RC.App.PresLogic.FowSpritePaletteDir");
        private static readonly string FOW_SPRITEPALETTE_FILE = ConstantsTable.Get<string>("RC.App.PresLogic.FowSpritePaletteFile");
    }
}
