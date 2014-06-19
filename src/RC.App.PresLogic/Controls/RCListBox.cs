using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a listbox control in the RC application.
    /// </summary>
    public class RCListBox : UIListBox
    {
        /// <summary>
        /// Constructs an RCListBox instance.
        /// </summary>
        /// <param name="position">The position of the RCListBox.</param>
        /// <param name="width">The width of the RCListBox.</param>
        /// <param name="visibleItemCount">
        /// The maximum number of items that can be visible in the listbox at a given time. If the number of
        /// items in the listbox is greater a vertical scrollbar is displayed along the right side of the control.
        /// </param>
        /// <param name="timeBetweenScrolls">
        /// The minimum time should be elapsed between scrolling the listbox in milliseconds.
        /// </param>
        public RCListBox(RCIntVector position, int width, int visibleItemCount, int timeBetweenScrolls)
            : base(position, new RCIntVector(width, HEIGHT), visibleItemCount, timeBetweenScrolls)
        {
            if (width <= TEXT_PADDING_LEFT + TEXT_PADDING_RIGHT) { throw new ArgumentOutOfRangeException("width", string.Format("Width must be at least {0}!", TEXT_PADDING_LEFT + TEXT_PADDING_RIGHT + 1)); }

            this.items = new UIString[0];
            this.highlightedItems = new UIString[0];
            this.disabledItems = new UIString[0];
            this.itemStrings = new string[0];
            this.textPartWidth = width - TEXT_PADDING_LEFT - TEXT_PADDING_RIGHT;

            this.controlSprite = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.ListBox");
        }

        /// <summary>
        /// Sets a new list of items to be displayed in this listbox.
        /// </summary>
        /// <param name="itemStrings">The list of the items to be displayed.</param>
        public void SetItems(string[] itemStrings)
        {
            if (itemStrings == null) { throw new ArgumentNullException("itemStrings"); }

            for (int i = 0; i < this.items.Length; i++)
            {
                this.items[i].Dispose();
                this.highlightedItems[i].Dispose();
                this.disabledItems[i].Dispose();
            }

            this.items = new UIString[itemStrings.Length];
            this.highlightedItems = new UIString[itemStrings.Length];
            this.disabledItems = new UIString[itemStrings.Length];
            this.itemStrings = new string[itemStrings.Length];

            for (int i = 0; i < itemStrings.Length; i++)
            {
                this.items[i] = new UIString(itemStrings[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, RCColor.Green);
                this.highlightedItems[i] = new UIString(itemStrings[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, RCColor.LightGreen);
                this.disabledItems[i] = new UIString(itemStrings[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), UIWorkspace.Instance.PixelScaling, RCColor.White);
                this.itemStrings[i] = itemStrings[i];
            }

            this.ItemCount = itemStrings.Length;
        }

        /// <summary>
        /// Gets the item of this listbox at the given index.
        /// </summary>
        /// <param name="index">The index of the item to get.</param>
        /// <returns>The item at the given index.</returns>
        public string this[int index] { get { return this.itemStrings[index]; } }

        /// <see cref="UIListBox.CreateScrollbar"/>
        protected override UIScrollBar CreateScrollbar(int intervalLength, int selectedValue)
        {
            RCScrollBar scrollBar = new RCScrollBar(new RCIntVector(0, 0), this.Range.Height, intervalLength);
            scrollBar.Position = new RCIntVector(this.Range.Width - scrollBar.Range.Width, 0);
            scrollBar.SelectedValue = selectedValue;
            return scrollBar;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            /// Render the upper part of the control.
            renderContext.RenderSprite(this.controlSprite, new RCIntVector(0, 0), new RCIntRectangle(0, 0, TEXT_PADDING_LEFT, HEIGHT));
            for (int drawnWidth = 0; drawnWidth < this.textPartWidth; )
            {
                int toBeDrawnNow = Math.Min(this.textPartWidth - drawnWidth, SPRITE_BACKGROUND_WIDTH);

                renderContext.RenderSprite(
                    this.controlSprite,
                    new RCIntVector(TEXT_PADDING_LEFT + drawnWidth, 0),
                    new RCIntRectangle(TEXT_PADDING_LEFT, 0, toBeDrawnNow, HEIGHT));

                drawnWidth += toBeDrawnNow;
            }
            renderContext.RenderSprite(this.controlSprite, new RCIntVector(TEXT_PADDING_LEFT + textPartWidth, 0), new RCIntRectangle(TEXT_PADDING_LEFT + SPRITE_BACKGROUND_WIDTH, 0, TEXT_PADDING_RIGHT, HEIGHT));

            /// Render the middle and bottom part of the control.
            for (int i = 1; i < this.VisibleItemCount; i++)
            {
                renderContext.RenderSprite(this.controlSprite, new RCIntVector(0, i * HEIGHT), new RCIntRectangle(0, i < this.VisibleItemCount - 1 ? HEIGHT : 2 * HEIGHT, TEXT_PADDING_LEFT, HEIGHT));
                for (int drawnWidth = 0; drawnWidth < this.textPartWidth; )
                {
                    int toBeDrawnNow = Math.Min(this.textPartWidth - drawnWidth, SPRITE_BACKGROUND_WIDTH);

                    renderContext.RenderSprite(
                        this.controlSprite,
                        new RCIntVector(TEXT_PADDING_LEFT + drawnWidth, i * HEIGHT),
                        new RCIntRectangle(TEXT_PADDING_LEFT, i < this.VisibleItemCount - 1 ? HEIGHT : 2 * HEIGHT, toBeDrawnNow, HEIGHT));

                    drawnWidth += toBeDrawnNow;
                }
                renderContext.RenderSprite(this.controlSprite, new RCIntVector(TEXT_PADDING_LEFT + textPartWidth, i * HEIGHT), new RCIntRectangle(TEXT_PADDING_LEFT + SPRITE_BACKGROUND_WIDTH, i < this.VisibleItemCount - 1 ? HEIGHT : 2 * HEIGHT, TEXT_PADDING_RIGHT, HEIGHT));
            }

            /// Render the items if necessary.
            if (this.ItemCount > 0)
            {
                for (int i = this.FirstVisibleIndex, j = 0; i < this.FirstVisibleIndex + this.VisibleItemCount && i < this.ItemCount; i++, j++)
                {
                    if (this.IsEnabled)
                    {
                        renderContext.RenderString(i == this.HighlightedIndex ? this.highlightedItems[i] : this.items[i],
                                                   new RCIntVector(TEXT_PADDING_LEFT, (j + 1) * HEIGHT - BASELINE - 1),
                                                   this.textPartWidth);
                    }
                    else
                    {
                        renderContext.RenderString(this.disabledItems[i],
                                                   new RCIntVector(TEXT_PADDING_LEFT, (j + 1) * HEIGHT - BASELINE - 1),
                                                   this.textPartWidth);
                    }
                }
            }
        }

        /// <summary>
        /// List of the item strings.
        /// </summary>
        private UIString[] items;

        /// <summary>
        /// List of the highlighted item strings.
        /// </summary>
        private UIString[] highlightedItems;

        /// <summary>
        /// List of the disabled item strings.
        /// </summary>
        private UIString[] disabledItems;

        /// <summary>
        /// List of the item strings.
        /// </summary>
        private string[] itemStrings;

        /// <summary>
        /// The sprite of the control.
        /// </summary>
        private UISprite controlSprite;

        /// <summary>
        /// The width of the text part of the control.
        /// </summary>
        private int textPartWidth;

        /// <summary>
        /// Constant definitions.
        /// </summary>
        private static int HEIGHT = 13;
        private static int TEXT_PADDING_LEFT = 3;
        private static int BASELINE = 4;
        private static int TEXT_PADDING_RIGHT = 3;
        private static int SPRITE_BACKGROUND_WIDTH = 216;
    }
}
