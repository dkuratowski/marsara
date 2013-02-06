using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.BizLogic;
using RC.Common.ComponentModel;

namespace RC.App.PresLogic
{
    /// <summary>
    /// This control displays the map.
    /// </summary>
    class RCMapDisplay : UIObject
    {
        /// <summary>
        /// Constructs a map display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        public RCMapDisplay(RCIntVector position, RCIntVector size)
            : base(new RCIntVector(1, 1), position, new RCIntRectangle(0, 0, size.X, size.Y))
        {
            this.mapDisplayInfoProvider = null;
            this.mapGeneralInfoProvider = null;
            this.noMapTextFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B");
            this.noMapString = null;
            this.noMapTextPos = RCIntVector.Undefined;
            this.isotileHighlight = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.IsotileHighlight");
            this.isotileHighlightCoords = RCIntVector.Undefined;
            this.displayingMap = false;
            this.tileset = new List<UISprite>();
        }

        /// <summary>
        /// Gets or sets whether this control shall display the map or not.
        /// </summary>
        public bool DisplayingMap
        {
            get { return this.displayingMap; }
            set
            {
                if (this.displayingMap != value)
                {
                    this.displayingMap = value;
                    if (this.displayingMap)
                    {
                        /// Connect to the approriate business component interfaces.
                        this.mapDisplayInfoProvider = ComponentManager.GetInterface<IMapDisplayInfo>();
                        this.mapGeneralInfoProvider = ComponentManager.GetInterface<IMapGeneralInfo>();
                        ITileSetStore tilesetStore = ComponentManager.GetInterface<ITileSetStore>();
                        if (this.mapDisplayInfoProvider == null) { throw new InvalidOperationException(string.Format("Component not found that implements the interface '{0}'!", typeof(IMapDisplayInfo).FullName)); }
                        if (this.mapGeneralInfoProvider == null) { throw new InvalidOperationException(string.Format("Component not found that implements the interface '{0}'!", typeof(IMapGeneralInfo).FullName)); }
                        if (tilesetStore == null) { throw new InvalidOperationException(string.Format("Component not found that implements the interface '{0}'!", typeof(ITileSetStore).FullName)); }
                        if (!this.mapGeneralInfoProvider.IsMapOpened) { throw new InvalidOperationException("There is no opened map that can be displayed!"); }

                        /// Setup the window.
                        this.mapDisplayInfoProvider.Window =
                            new RCIntRectangle(0, 0, this.Range.Width % PIXEL_PER_NAVCELL > 0 ?
                                                     this.Range.Width / PIXEL_PER_NAVCELL + 1 :
                                                     this.Range.Width / PIXEL_PER_NAVCELL,
                                                     this.Range.Height % PIXEL_PER_NAVCELL > 0 ?
                                                     this.Range.Height / PIXEL_PER_NAVCELL + 1 :
                                                     this.Range.Height / PIXEL_PER_NAVCELL);

                        /// Load the tileset.
                        foreach (TileTypeInfo tileType in tilesetStore.GetTileTypes(this.mapGeneralInfoProvider.TilesetName))
                        {
                            UISprite tile = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(tileType.ImageData, UIWorkspace.Instance.PixelScaling);
                            if (tileType.Properties.ContainsKey(TILEPROP_TRANSPARENTCOLOR))
                            {
                                tile.TransparentColor = UIResourceLoader.LoadColor(tileType.Properties[TILEPROP_TRANSPARENTCOLOR]);
                            }
                            else
                            {
                                tile.TransparentColor = DEFAULT_TILE_TRANSPARENT_COLOR;
                            }
                            this.tileset.Add(tile);
                        }
                    }
                    else
                    {
                        /// Unload the tileset.
                        foreach (UISprite tile in this.tileset)
                        {
                            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(tile);
                        }
                        this.tileset.Clear();

                        /// Close the window.
                        this.mapDisplayInfoProvider.Window = RCIntRectangle.Undefined;

                        /// Disconnect from the appropriate business component interfaces.
                        this.mapDisplayInfoProvider = null;
                        this.mapGeneralInfoProvider = null;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the text to be displayed at the center if there is no map opened.
        /// </summary>
        public string TextIfNoMap
        {
            set
            {
                if (this.noMapString != null)
                {
                    this.noMapString.Dispose();
                    this.noMapString = null;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    this.noMapString = new UIString(value,
                                                    this.noMapTextFont,
                                                    UIWorkspace.Instance.PixelScaling,
                                                    UIColor.White);

                    this.noMapTextPos = new RCIntVector((this.Range.Width - this.noMapString.Width) / 2,
                        (this.Range.Height - (this.noMapTextFont.CharBottomMaximum + this.noMapTextFont.CharTopMaximum + 1)) / 2 +
                        this.noMapTextFont.CharTopMaximum);
                }
            }
        }

        /// <summary>
        /// Gets the displayed area of the map in navigation cells.
        /// </summary>
        public RCIntRectangle DisplayedArea { get { return this.mapDisplayInfoProvider.Window; } }

        /// <summary>
        /// Scrolls this map display to the given position.
        /// </summary>
        /// <param name="where">The top-left corner of the display area.</param>
        /// <remarks>Scrolling the map display automatically turns off any highlighting.</remarks>
        public void ScrollTo(RCIntVector where)
        {
            if (!this.displayingMap) { throw new InvalidOperationException("The MapDisplay is not displaying the map!"); }

            RCIntVector location =
                new RCIntVector(Math.Min(this.mapGeneralInfoProvider.NavSize.X - this.mapDisplayInfoProvider.Window.Size.X, Math.Max(0, where.X)),
                                Math.Min(this.mapGeneralInfoProvider.NavSize.Y - this.mapDisplayInfoProvider.Window.Size.Y, Math.Max(0, where.Y)));

            this.mapDisplayInfoProvider.Window = new RCIntRectangle(location, this.mapDisplayInfoProvider.Window.Size);
            this.isotileHighlightCoords = RCIntVector.Undefined;
        }

        /// <summary>
        /// Highlights the isometric tile under the given cursor position.
        /// </summary>
        /// <param name="cursor">The coordinates of the cursor in pixels.</param>
        public void HighlightIsoTileAt(RCIntVector cursor)
        {
            if (!this.displayingMap) { throw new InvalidOperationException("The MapDisplay is not displaying the map!"); }

            RCIntVector navCellCoord = cursor / PIXEL_PER_NAVCELL;
            this.isotileHighlightCoords = this.mapDisplayInfoProvider.GetIsoTileDisplayCoords(navCellCoord);
        }

        /// <summary>
        /// Unhighlights the currently highlighted isometric tile.
        /// </summary>
        public void UnhighlightIsoTile()
        {
            if (this.isotileHighlightCoords != RCIntVector.Undefined) { this.isotileHighlightCoords = RCIntVector.Undefined; }
        }

        /// <summary>
        /// Transforms the given pixel coordinates to navigation coordinates.
        /// </summary>
        /// <param name="pixelCoords">The pixel coordinates to be transformed.</param>
        /// <returns>The navigation coordinates.</returns>
        public RCIntVector TransformPixelToNavCoords(RCIntVector pixelCoords)
        {
            return pixelCoords / PIXEL_PER_NAVCELL;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (!this.displayingMap)
            {
                /// Display the no-map-string if we don't have to display the map.
                if (this.noMapString != null)
                {
                    renderContext.RenderString(this.noMapString, this.noMapTextPos);
                }
            }
            else
            {
                /// Display the currently visible tiles.
                foreach (IsoTileDisplayInfo tileDisplayInfo in this.mapDisplayInfoProvider.IsoTileDisplayInfos)
	            {
                    UISprite tileToDisplay = this.tileset[tileDisplayInfo.TileTypeIndex];
                    renderContext.RenderSprite(tileToDisplay, tileDisplayInfo.DisplayCoords * PIXEL_PER_NAVCELL);
	            }

                /// Display the isometric tile highlight if necessary.
                if (this.isotileHighlightCoords != RCIntVector.Undefined)
                {
                    renderContext.RenderSprite(this.isotileHighlight, this.isotileHighlightCoords * PIXEL_PER_NAVCELL);
                }
            }
        }

        /// <summary>
        /// Reference to the business component that provides informations for displaying the map.
        /// </summary>
        private IMapDisplayInfo mapDisplayInfoProvider;

        /// <summary>
        /// Reference to the business component that provides general informations about the map.
        /// </summary>
        private IMapGeneralInfo mapGeneralInfoProvider;

        /// <summary>
        /// This flag indicates whether this control is currently displaying the map or not.
        /// </summary>
        private bool displayingMap;

        /// <summary>
        /// The text to be displayed at the center when there is no opened map.
        /// </summary>
        private UIString noMapString;

        /// <summary>
        /// The font of the text to be displayed at the center when there is no opened map.
        /// </summary>
        private UIFont noMapTextFont;

        /// <summary>
        /// The position of the text to be displayed at the center when there is no opened map.
        /// </summary>
        private RCIntVector noMapTextPos;

        /// <summary>
        /// List of the tiles in the tileset of the currently displayed map.
        /// </summary>
        private List<UISprite> tileset;

        /// <summary>
        /// The sprite that is used to highlighting an isometric tile.
        /// </summary>
        private UISprite isotileHighlight;

        /// <summary>
        /// The coordinates of the navigation cell where to draw the isometric tile highlight or RCIntVector.Undefined
        /// if no isometric tile is highlighted.
        /// </summary>
        private RCIntVector isotileHighlightCoords;

        /// <summary>
        /// Name of the tile variant property that stores the transparent color.
        /// </summary>
        public const string TILEPROP_TRANSPARENTCOLOR = "TransparentColor";

        /// <summary>
        /// The default color of the transparent parts of the tiles.
        /// </summary>
        public readonly UIColor DEFAULT_TILE_TRANSPARENT_COLOR = new UIColor(255, 0, 255);

        /// <summary>
        /// Number of pixels per navigation cells in both horizontal and vertical direction.
        /// </summary>
        public const int PIXEL_PER_NAVCELL = 4;
    }
}
