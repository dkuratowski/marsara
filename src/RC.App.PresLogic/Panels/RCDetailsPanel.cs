using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.App.PresLogic.SpriteGroups;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;
using RC.Common.Diagnostics;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The details panel on the gameplay page
    /// </summary>
    public class RCDetailsPanel : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a details panel.
        /// </summary>
        /// <param name="productIconSprites">The product icon sprite group.</param>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCDetailsPanel(ISpriteGroup productIconSprites, RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            if (productIconSprites == null) { throw new ArgumentNullException("productIconSprites"); }

            this.textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.objectTypeTexts = new Dictionary<int, UIString>();

            this.isConnected = false;
            this.backgroundTask = null;
            this.hpIndicatorSprites = new Dictionary<MapObjectConditionEnum, SpriteGroup>();
            this.productIconSprites = productIconSprites;
            this.currentCustomContent = null;
            this.buttonArray = new RCSelectionButton[MAX_SELECTION_SIZE];
            this.productionLineDisplay = null;
            this.multiplayerService = null;
            this.selectionDetailsView = null;
            this.mapObjectDetailsView = null;
            this.productionLineView = null;
            this.selectionButtonsAdded = false;
            this.hpTexts = new Dictionary<MapObjectConditionEnum, UIString>();
            this.energyText = null;
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The details panel has been connected or is currently being connected!"); }

            /// UI-thread connection procedure
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.selectionDetailsView = viewService.CreateView<ISelectionDetailsView>();
            this.mapObjectDetailsView = viewService.CreateView<IMapObjectDetailsView>();
            this.productionLineView = viewService.CreateView<IProductionLineView>();
            this.metadataView = viewService.CreateView<IMetadataView>();
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Excellent, new HPIconSpriteGroup(this.metadataView, MapObjectConditionEnum.Excellent));
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Moderate, new HPIconSpriteGroup(this.metadataView, MapObjectConditionEnum.Moderate));
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Critical, new HPIconSpriteGroup(this.metadataView, MapObjectConditionEnum.Critical));
            this.hpIndicatorSprites.Add(MapObjectConditionEnum.Undefined, new HPIconSpriteGroup(this.metadataView, MapObjectConditionEnum.Undefined));

            this.backgroundTask = UITaskManager.StartParallelTask(this.ConnectBackgroundProc, "RCDetailsPanel.Connect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (!this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The command panel has been connected or is currently being connected!"); }

            /// Unsubscribe from the FrameUpdate event.
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnFrameUpdate;

            /// Destroy the controls that display the custom contents.
            //this.productionDisplay.Dispose(); TODO: dispose if necessary!
            this.productionLineDisplay = null;
            this.currentCustomContent = null;

            /// Destroy the selection buttons.
            for (int i = 0; i < MAX_SELECTION_SIZE; i++)
            {
                if (this.selectionButtonsAdded) { this.RemoveControl(this.buttonArray[i]); }
                this.buttonArray[i].Dispose();
                this.buttonArray[i] = null;
            }
            this.selectionButtonsAdded = false;

            this.backgroundTask = UITaskManager.StartParallelTask(this.DisconnectBackgroundProc, "RCDetailsPanel.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get
            {
                if (this.backgroundTask == null) { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
                else { return this.isConnected ? ConnectionStatusEnum.Disconnecting : ConnectionStatusEnum.Connecting; }
            }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        #region Overrides

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);

            /// Check if we are online and in single selection mode.
            if (this.ConnectionStatus != ConnectionStatusEnum.Online) { return; }
            if (this.selectionDetailsView.SelectionCount != 1) { return; }

            /// Retrieve the ID and the HP condition of the object.
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);
            MapObjectConditionEnum hpCondition = this.mapObjectDetailsView.GetHPCondition(mapObjectID);
            if (!this.hpIndicatorSprites.ContainsKey(hpCondition)) { return; }

            /// Render the big icon of the selected object.
            SpriteInst hpSprite = this.mapObjectDetailsView.GetBigHPIcon(mapObjectID);
            renderContext.RenderSprite(this.hpIndicatorSprites[hpCondition][hpSprite.Index],
                                       RCDetailsPanel.ICON_POS + hpSprite.DisplayCoords,
                                       hpSprite.Section);

            /// Render the typename of the selected object.
            UIString typeTextToRender = this.objectTypeTexts[this.mapObjectDetailsView.GetObjectTypeID(mapObjectID)];
            RCIntVector typeNameStringPos = TYPENAME_STRING_MIDDLE_POS - new RCIntVector(typeTextToRender.Width / 2, 0);
            renderContext.RenderString(typeTextToRender, typeNameStringPos);

            /// Render the HP of the selected object.
            int currentHP = this.mapObjectDetailsView.GetCurrentHP(mapObjectID);
            if (currentHP != -1)
            {
                int maxHP = this.mapObjectDetailsView.GetMaxHP(mapObjectID);
                UIString hpTextToRender = this.hpTexts[hpCondition];
                hpTextToRender[0] = currentHP;
                hpTextToRender[1] = maxHP;

                RCIntVector hpStringPos = HP_STRING_MIDDLE_POS - new RCIntVector(hpTextToRender.Width / 2, 0);
                renderContext.RenderString(hpTextToRender, hpStringPos);
            }

            /// Render the energy of the selected object.
            int currentEnergy = this.mapObjectDetailsView.GetCurrentEnergy(mapObjectID);
            if (currentEnergy != -1)
            {
                int maxEnergy = this.mapObjectDetailsView.GetMaxEnergy(mapObjectID);
                this.energyText[0] = currentEnergy;
                this.energyText[1] = maxEnergy;

                RCIntVector energyStringPos = ENERGY_STRING_MIDDLE_POS - new RCIntVector(this.energyText.Width / 2, 0);
                renderContext.RenderString(this.energyText, energyStringPos);
            }
        }

        #endregion Overrides

        #region Internal members

        /// <summary>
        /// Called when the currently running background task has been finished.
        /// </summary>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object message)
        {
            this.backgroundTask.Finished -= this.OnBackgroundTaskFinished;
            this.backgroundTask = null;
            if (!this.isConnected)
            {
                /// Create the selection buttons.
                for (int i = 0; i < MAX_SELECTION_SIZE; i++)
                {
                    this.buttonArray[i] = new RCSelectionButton(i, this.hpIndicatorSprites);
                }

                /// Create the controls that display the custom content.
                this.productionLineDisplay = new RCProductionLineDisplay(this.productIconSprites, CUSTOM_CONTENT_RECT.Location, CUSTOM_CONTENT_RECT.Size);

                /// Subscribe to the FrameUpdate event.
                UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnFrameUpdate;

                this.isConnected = true;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
            else
            {
                this.multiplayerService = null;
                this.selectionDetailsView = null;
                this.mapObjectDetailsView = null;
                this.productionLineView = null;
                this.isConnected = false;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
        }

        /// <summary>
        /// Executes connection procedures on a background thread.
        /// </summary>
        private void ConnectBackgroundProc(object parameter)
        {
            /// Load the HP indicator sprites.
            foreach (SpriteGroup spriteGroup in this.hpIndicatorSprites.Values) { spriteGroup.Load(); }

            /// Load the UIStrings for displaying the map object type names.
            foreach (KeyValuePair<int, string> typeName in this.metadataView.GetMapObjectDisplayedTypeNames())
            {
                this.objectTypeTexts.Add(typeName.Key,
                    new UIString(typeName.Value, this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.White));
            }

            /// Load the UIStrings for displaying the HP and the energy of the selected object in single selection mode.
            this.hpTexts.Add(MapObjectConditionEnum.Excellent,
                new UIString("{0}/{1}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.LightGreen));
            this.hpTexts.Add(MapObjectConditionEnum.Moderate,
                new UIString("{0}/{1}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.Yellow));
            this.hpTexts.Add(MapObjectConditionEnum.Critical,
                new UIString("{0}/{1}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.Red));
            this.energyText = new UIString("{0}/{1}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.WhiteHigh);
        }

        /// <summary>
        /// Executes disconnection procedures on a background thread.
        /// </summary>
        private void DisconnectBackgroundProc(object parameter)
        {
            /// Unload the HP indicator sprites.
            foreach (SpriteGroup spriteGroup in this.hpIndicatorSprites.Values) { spriteGroup.Unload(); }
            this.hpIndicatorSprites.Clear();

            /// Unload the UIString that displayed the map object type names.
            foreach (UIString objectTypeText in this.objectTypeTexts.Values) { objectTypeText.Dispose(); }
            this.objectTypeTexts.Clear();

            /// Unload the UIStrings that displayed the HP and the energy of the selected object in single selection mode.
            foreach (UIString hpText in this.hpTexts.Values) { hpText.Dispose(); }
            this.hpTexts.Clear();
            this.energyText.Dispose();
            this.energyText = null;
        }

        /// <summary>
        /// This method is called on each frame update.
        /// </summary>
        private void OnFrameUpdate()
        {
            if (this.selectionDetailsView.SelectionCount >= 2 && !this.selectionButtonsAdded)
            {
                /// Activate multi-select mode: add the selection buttons to the panel.
                for (int i = 0; i < MAX_SELECTION_SIZE; i++)
                {
                    this.AddControl(this.buttonArray[i]);
                }

                this.selectionButtonsAdded = true;
                TraceManager.WriteAllTrace("SelectionButtons added", PresLogicTraceFilters.INFO);
            }
            else if (this.selectionDetailsView.SelectionCount < 2 && this.selectionButtonsAdded)
            {
                /// Deactivate multi-select mode: remove the selection buttons from the panel.
                for (int i = 0; i < MAX_SELECTION_SIZE; i++)
                {
                    this.RemoveControl(this.buttonArray[i]);
                }

                this.selectionButtonsAdded = false;
                TraceManager.WriteAllTrace("SelectionButtons removed", PresLogicTraceFilters.INFO);
            }

            /// Update the custom content to be displayed.
            if (this.selectionDetailsView.SelectionCount == 1)
            {
                UIControl newCustomContent = this.SelectCustomContent();
                if (this.currentCustomContent != newCustomContent)
                {
                    if (this.currentCustomContent != null) { this.RemoveControl(this.currentCustomContent); }
                    if (newCustomContent != null) { this.AddControl(newCustomContent); }
                    this.currentCustomContent = newCustomContent;
                }
            }
            else if (this.currentCustomContent != null)
            {
                this.RemoveControl(this.currentCustomContent);
                this.currentCustomContent = null;
            }
        }

        /// <summary>
        /// Selects the custom content to be displayed or null if no custom content shall be displayed.
        /// </summary>
        /// <remarks>Note that this method is called only in single selection mode.</remarks>
        private UIControl SelectCustomContent()
        {
            // TODO: implement this method accordingly!
            if (this.productionLineView.Capacity == 0) { return null; }

            return this.productionLineDisplay;
        }

        #endregion Internal members

        /// <summary>
        /// This flag indicates whether this details panel has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// An array that stores the selection buttons on the details panel in layout order.
        /// </summary>
        private readonly RCSelectionButton[] buttonArray;

        /// <summary>
        /// Reference to the control that currently displays custom content or null if custom content is currently
        /// not displayed.
        /// </summary>
        private UIControl currentCustomContent;

        /// <summary>
        /// Reference to the production line display control.
        /// </summary>
        private RCProductionLineDisplay productionLineDisplay;

        /// <summary>
        /// Reference to the currently executed connecting/disconnecting task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// List of the HP indicator sprite groups for each possible conditions.
        /// </summary>
        private readonly Dictionary<MapObjectConditionEnum, SpriteGroup> hpIndicatorSprites;

        /// <summary>
        /// The product icon sprite group.
        /// </summary>
        private readonly ISpriteGroup productIconSprites;

        /// <summary>
        /// Reference to the multiplayer service.
        /// </summary>
        private IMultiplayerService multiplayerService;

        /// <summary>
        /// Reference to the selection details view.
        /// </summary>
        private ISelectionDetailsView selectionDetailsView;

        /// <summary>
        /// Reference to the map object details view.
        /// </summary>
        private IMapObjectDetailsView mapObjectDetailsView;

        /// <summary>
        /// Reference to the production line view.
        /// </summary>
        private IProductionLineView productionLineView;

        /// <summary>
        /// Reference to the metadata view.
        /// </summary>
        private IMetadataView metadataView;

        /// <summary>
        /// This flag indicates whether the selection buttons are currently added to the details panel.
        /// </summary>
        private bool selectionButtonsAdded;

        /// <summary>
        /// The strings for displaying the type name of the selected map object in single selection mode mapped by the
        /// IDs of the corresponding types.
        /// </summary>
        private readonly Dictionary<int, UIString> objectTypeTexts;

        /// <summary>
        /// The string for displaying the HP of the selected object in single selection mode.
        /// </summary>
        private readonly Dictionary<MapObjectConditionEnum, UIString> hpTexts;

        /// <summary>
        /// The string for displaying the energy of the selected object in single selection mode.
        /// </summary>
        private UIString energyText;

        /// <summary>
        /// The font that is used for rendering texts on the details panel.
        /// </summary>
        private readonly UIFont textFont;

        /// <summary>
        /// The maximum number of objects can be selected.
        /// </summary>
        private const int MAX_SELECTION_SIZE = 12;

        /// <summary>
        /// The position of the object icon in single selection mode.
        /// </summary>
        private static readonly RCIntVector ICON_POS = new RCIntVector(22, 1);

        /// <summary>
        /// The position of the center of the string that displays the object's typename in single selection mode.
        /// </summary>
        private static readonly RCIntVector TYPENAME_STRING_MIDDLE_POS = new RCIntVector(125, 6);

        /// <summary>
        /// The position of the center of the string that displays the object's HP in single selection mode.
        /// </summary>
        private static readonly RCIntVector HP_STRING_MIDDLE_POS = new RCIntVector(37, 38);

        /// <summary>
        /// The position of the center of the string that displays the object's energy in single selection mode.
        /// </summary>
        private static readonly RCIntVector ENERGY_STRING_MIDDLE_POS = new RCIntVector(37, 47);

        /// <summary>
        /// The position of the custom content.
        /// </summary>
        private static readonly RCIntRectangle CUSTOM_CONTENT_RECT = new RCIntRectangle(76, 10, 100, 40);
    }
}
