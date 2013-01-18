using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.Configuration;

namespace RC.UI
{
    /// <summary>
    /// Represents a method that handles UIPage events.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    public delegate void UIPageEventHdl(UIPage sender);

    /// <summary>
    /// The UIWorkspace is the main access point of the graphical user interface. UIWorkspace is a singleton
    /// class, and it's instance can be globally accessible with the UIWorkspace.Instance property.
    /// </summary>
    public class UIWorkspace : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance of the UIWorkspace class.
        /// </summary>
        public static UIWorkspace Instance
        {
            get
            {
                if (theInstance == null) { throw new UIException("No instance of UIWorkspace exists!"); }
                return theInstance;
            }
        }

        /// <summary>
        /// Occurs when the active page has been inactivated.
        /// </summary>
        public event UIPageEventHdl PageInactivated;

        /// <summary>
        /// Creates a UIWorkspace instance. If an instance already exists, a UIException is thrown.
        /// </summary>
        /// <param name="displaySize">The size of the surrounding display in screen coordinates.</param>
        /// <param name="workspaceSize">
        /// The size of the workspace in workspace coordinates.
        /// </param>
        /// <remarks>
        /// The workspace size must be less than or equals with the display size. The position of the workspace
        /// will be automatically set to the middle of the display. The pixel scaling of the workspace will be
        /// automatically set to the greatest possible values in both dimensions such that it is entirely contained
        /// by the display.
        /// </remarks>
        public UIWorkspace(RCIntVector displaySize, RCIntVector workspaceSize)
        {
            if (theInstance != null) { throw new UIException("An instance of UIWorkspace already exists!"); }
            if (displaySize == RCIntVector.Undefined) { throw new ArgumentNullException("displaySize"); }
            if (displaySize.X <= 0 || displaySize.Y <= 0) { throw new ArgumentOutOfRangeException("displaySize"); }
            if (workspaceSize == RCIntVector.Undefined) { throw new ArgumentNullException("workspaceSize"); }
            if (workspaceSize.X <= 0 || workspaceSize.Y <= 0) { throw new ArgumentOutOfRangeException("workspaceSize"); }
            if (workspaceSize.X > displaySize.X || workspaceSize.Y > displaySize.Y) { throw new ArgumentException("displaySize must be greater than or equal with workspaceSize!"); }

            this.activePage = null;
            this.openedDialog = null;
            this.registeredPages = new HashSet<UIPage>();

            /// Construct the underlying objects.
            RCIntVector pixelScaling = new RCIntVector(displaySize.X / workspaceSize.X, displaySize.Y / workspaceSize.Y);
            this.display = new UIObject(new RCIntVector(1, 1), new RCIntVector(0, 0), new RCIntRectangle(0, 0, displaySize.X, displaySize.Y));
            RCIntVector scaledAreaPos = (displaySize - workspaceSize * pixelScaling) / 2;
            this.scaledArea = new UIObject(pixelScaling, scaledAreaPos, new RCIntRectangle(0, 0, workspaceSize.X, workspaceSize.Y));
            this.workspace = new UISensitiveObject(new RCIntVector(0, 0), new RCIntRectangle(0, 0, workspaceSize.X, workspaceSize.Y));
            
            /// Construct the root of the object tree.
            this.display.Attach(this.scaledArea);
            this.scaledArea.Attach(this.workspace);
            this.mouseManager = new UIMouseManager(this.workspace);

            /// Subscribe for the illegal events of the display and the scaled area.
            this.SubscribeForInvalidTreeOperations(this.display);
            this.SubscribeForInvalidTreeOperations(this.scaledArea);

            /// Attach the display to the render manager.
            UIRoot.Instance.GraphicsPlatform.RenderManager.Attach(this.display);

            /// Register the "RC.UI.UIWorkspace.PixelScaling" resolver method for the configuration files
            DynamicString.RegisterResolver(
                "RC.UI.UIWorkspace.PixelScaling",
                delegate() { return string.Format("{0};{1}", this.PixelScaling.X, this.PixelScaling.Y); });

            theInstance = this;

            ///// TODO: Implement an interface for changing the mouse cursor.
            //UISprite mouseIcon = UIResourceManager.GetResource<UISprite>("RC.Sprites.TestPointerSprite");
            //mouseIcon.TransparentColor = new UIColor(255, 0, 255);
            //this.mouseManager.Pointer = new UIBasicPointer(mouseIcon, new RCIntVector(4, 4));

            TraceManager.WriteAllTrace("UIWorkspace.Instance created", UITraceFilters.INFO);
        }

        #region Public properties

        /// <summary>
        /// Gets the pixel scaling of the UIWorkspace.
        /// </summary>
        public RCIntVector PixelScaling { get { return this.workspace.AbsolutePixelScaling; } }

        /// <summary>
        /// Gets the size of the display.
        /// </summary>
        public RCIntVector DisplaySize { get { return this.display.Range.Size; } }

        /// <summary>
        /// Gets the size of the workspace.
        /// </summary>
        public RCIntVector WorkspaceSize { get { return this.workspace.Range.Size; } }

        /// <summary>
        /// Gets the currently active page.
        /// </summary>
        public UIPage ActivePage
        {
            get
            {
                if (this.activePage.CurrentStatus != UIPage.Status.Active)
                {
                    throw new InvalidOperationException("The active UIPage is currently inactivating!");
                }
                return this.activePage;
            }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Registers the given page to the UIWorkspace.
        /// </summary>
        /// <param name="page">The page to register.</param>
        public void RegisterPage(UIPage page)
        {
            if (page == null) { throw new ArgumentNullException("page"); }
            if (this.registeredPages.Contains(page)) { throw new InvalidOperationException("The given UIPage is already registered at the UIWorkspace!"); }
            if (page.CurrentStatus != UIPage.Status.Inactive) { throw new InvalidOperationException("The given UIPage is not in Inactive status!"); }
            this.registeredPages.Add(page);
            page.StatusChanged += this.OnPageStatusChanged;
        }

        /// <summary>
        /// Unregisters the given page from the UIWorkspace.
        /// </summary>
        /// <param name="page">The page to unregister.</param>
        public void UnregisterPage(UIPage page)
        {
            if (page == null) { throw new ArgumentNullException("page"); }
            if (!this.registeredPages.Contains(page)) { throw new InvalidOperationException("The given UIPage is not registered at the UIWorkspace!"); }
            if (page.CurrentStatus != UIPage.Status.Inactive) { throw new InvalidOperationException("The given UIPage is not in Inactive status!"); }
            this.registeredPages.Remove(page);
            page.StatusChanged -= this.OnPageStatusChanged;
        }

        /// <summary>
        /// Opens the given UIDialog as a modal dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open.</param>
        public void OpenDialog(UIDialog dialog)
        {
        }

        /// <summary>
        /// Closes the currently opened dialog.
        /// </summary>
        public void CloseDialog()
        {
        }

        /// <summary>
        /// Sets the mouse pointer of the UIWorkspace.
        /// </summary>
        /// <param name="ptr">The new pointer or null to turn off the pointer.</param>
        /// <remarks>TODO: define mouse pointers as resources in the future.</remarks>
        public void SetMousePointer(IUIMousePointer ptr)
        {
            this.mouseManager.Pointer = ptr;
        }

        #endregion Public methods

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.activePage != null) { throw new InvalidOperationException("Unable to dispose the UIWorkspace while there is an active page!"); }
            if (this.openedDialog != null) { throw new InvalidOperationException("Unable to dispose the UIWorkspace while there is an opened dialog!"); }

            /// Unregister the "RC.UI.UIWorkspace.PixelScaling" resolver method
            DynamicString.UnregisterResolver("RC.UI.UIWorkspace.PixelScaling");

            this.registeredPages.Clear();

            /// Detach the display from the render manager.
            if (UIRoot.Instance.GraphicsPlatform.RenderManager.AttachedObject == this.display)
            {
                UIRoot.Instance.GraphicsPlatform.RenderManager.Detach();
            }

            /// Unsubscribe from the illegal events of the display and the scaled area.
            this.UnsubscribeFromInvalidTreeOperations(this.scaledArea);
            this.UnsubscribeFromInvalidTreeOperations(this.display);

            /// Kill the mouse manager.
            this.mouseManager.Dispose();
            this.mouseManager = null;

            /// Destroy the root of the object tree.
            this.scaledArea.Detach(this.workspace);
            this.display.Detach(this.scaledArea);
            this.workspace = null;
            this.scaledArea = null;
            this.display = null;

            theInstance = null;
            TraceManager.WriteAllTrace("UIWorkspace.Instance destroyed", UITraceFilters.INFO);
        }

        #endregion IDisposable members

        #region Internal methods

        /// <summary>
        /// Subscribes for the illegal events of the given object.
        /// </summary>
        private void SubscribeForInvalidTreeOperations(UIObject targetObject)
        {
            targetObject.Attached += this.OnInvalidTreeOperation;
            targetObject.Detached += this.OnInvalidTreeOperation;
            targetObject.BroughtForward += this.OnInvalidTreeOperation;
            targetObject.BroughtToTop += this.OnInvalidTreeOperation;
            targetObject.SentBackward += this.OnInvalidTreeOperation;
            targetObject.SentToBottom += this.OnInvalidTreeOperation;
            targetObject.ClipRectChanged += this.OnInvalidTreeOperation;
            targetObject.CloakRectChanged += this.OnInvalidTreeOperation;
            targetObject.RangeRectChanged += this.OnInvalidTreeOperation;
            targetObject.PositionChanged += this.OnInvalidTreeOperation;
            targetObject.ChildAttached += this.OnInvalidTreeOperation;
            targetObject.ChildDetached += this.OnInvalidTreeOperation;
        }

        /// <summary>
        /// Unsubscribes from the illegal events of the given object.
        /// </summary>
        private void UnsubscribeFromInvalidTreeOperations(UIObject targetObject)
        {
            targetObject.Attached -= this.OnInvalidTreeOperation;
            targetObject.Detached -= this.OnInvalidTreeOperation;
            targetObject.BroughtForward -= this.OnInvalidTreeOperation;
            targetObject.BroughtToTop -= this.OnInvalidTreeOperation;
            targetObject.SentBackward -= this.OnInvalidTreeOperation;
            targetObject.SentToBottom -= this.OnInvalidTreeOperation;
            targetObject.ClipRectChanged -= this.OnInvalidTreeOperation;
            targetObject.CloakRectChanged -= this.OnInvalidTreeOperation;
            targetObject.RangeRectChanged -= this.OnInvalidTreeOperation;
            targetObject.PositionChanged -= this.OnInvalidTreeOperation;
            targetObject.ChildAttached -= this.OnInvalidTreeOperation;
            targetObject.ChildDetached -= this.OnInvalidTreeOperation;
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject parentObj, UIObject childObj)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender, RCIntVector prev, RCIntVector curr)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        /// <summary>
        /// Called on invalid tree operations.
        /// </summary>
        private void OnInvalidTreeOperation(UIObject sender, RCIntRectangle prev, RCIntRectangle curr)
        {
            throw new InvalidOperationException("Invalid tree operation!");
        }

        #endregion Internal methods

        #region UIPage event handlers

        /// <summary>
        /// Called when the status of a registered UIPage has been changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="newStatus">The new status of the sender.</param>
        private void OnPageStatusChanged(UIPage sender, UIPage.Status newStatus)
        {
            if (!this.registeredPages.Contains(sender)) { throw new InvalidOperationException("Unregistered UIPage!"); }

            if (sender == this.activePage && newStatus == UIPage.Status.Inactivating)
            {
                TraceManager.WriteAllTrace("UIPage inactivating", UITraceFilters.INFO);
            }
            else if (sender == this.activePage && newStatus == UIPage.Status.Inactive)
            {
                this.workspace.DetachSensitive(this.activePage);
                this.workspace.Detach(this.activePage);
                UIPage inactivatedPage = this.activePage;
                this.activePage = null;
                TraceManager.WriteAllTrace("UIPage inactivated", UITraceFilters.INFO);
                if (this.PageInactivated != null) { this.PageInactivated(inactivatedPage); }
            }
            else if (sender != this.activePage && this.activePage == null && newStatus == UIPage.Status.Active)
            {
                this.activePage = sender;
                this.workspace.Attach(this.activePage);
                this.workspace.AttachSensitive(this.activePage);
                TraceManager.WriteAllTrace("UIPage activated", UITraceFilters.INFO);
            }
            else
            {
                throw new InvalidOperationException("Invalid UIPage operation!");
            }
        }

        #endregion UIPage event handlers

        /// <summary>
        /// Reference to the singleton instance of the UIWorkspace class.
        /// </summary>
        private static UIWorkspace theInstance = null;
        
        /// <summary>
        /// The surrounding display object.
        /// </summary>
        private UIObject display;

        /// <summary>
        /// The scaled area of the display object.
        /// </summary>
        private UIObject scaledArea;

        /// <summary>
        /// The root object of the workspace.
        /// </summary>
        private UISensitiveObject workspace;

        /// <summary>
        /// The mouse manager of this UIWorkspace.
        /// </summary>
        private UIMouseManager mouseManager;

        /// <summary>
        /// Reference to the currently active page.
        /// </summary>
        private UIPage activePage;

        /// <summary>
        /// List of the registered pages.
        /// </summary>
        private HashSet<UIPage> registeredPages;

        /// <summary>
        /// Reference to the currently opened UIDialog.
        /// </summary>
        private UIDialog openedDialog;
    }
}
