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
    class RCMapDisplay : UIControl
    {
        /// <summary>
        /// Constructs a map display control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the map display control.</param>
        /// <param name="size">The size of the map display control.</param>
        public RCMapDisplay(RCIntVector position, RCIntVector size) : base(position, size)
        {
            this.mapDisplayInfoProvider = ComponentManager.GetInterface<IMapWindow>();
            this.tilesetInfoProvider = ComponentManager.GetInterface<ITileSetStore>();
            this.noOpenedMapFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B");
            this.noOpenedMapStr = new UIString("No opened map...",
                                               this.noOpenedMapFont,
                                               UIWorkspace.Instance.PixelScaling,
                                               UIColor.White);
            this.noOpenedMapTextPos = new RCIntVector((this.Range.Width - this.noOpenedMapStr.Width) / 2,
                (this.Range.Height - (this.noOpenedMapFont.CharBottomMaximum + this.noOpenedMapFont.CharTopMaximum + 1)) / 2 +
                this.noOpenedMapFont.CharTopMaximum);
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderString(this.noOpenedMapStr,
                                       this.noOpenedMapTextPos);
        }

        /// <summary>
        /// Reference to the business component that provides informations for displaying the map.
        /// </summary>
        private IMapWindow mapDisplayInfoProvider;

        /// <summary>
        /// Reference to the business component that provides informations about the tilesets.
        /// </summary>
        private ITileSetStore tilesetInfoProvider;

        /// <summary>
        /// The "No opened map..." text will be displayed at the center when there is no opened map.
        /// </summary>
        private UIString noOpenedMapStr;

        /// <summary>
        /// The font of the "No opened map..." text.
        /// </summary>
        private UIFont noOpenedMapFont;

        /// <summary>
        /// The position of the "No opened map..." text.
        /// </summary>
        private RCIntVector noOpenedMapTextPos;
    }
}
