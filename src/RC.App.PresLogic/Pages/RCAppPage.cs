using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// Represents a page in the RC application.
    /// </summary>
    public class RCAppPage : UIPage
    {
        /// <summary>
        /// Constructs an RCAppPage object.
        /// </summary>
        public RCAppPage() : base()
        {
            this.pageReferences = new Dictionary<string, RCAppPage>();
            this.navigatingTo = null;
            this.StatusChanged += this.OnPageStatusChanged;
            UIWorkspace.Instance.PageInactivated += this.OnActivePageInactivated;
        }

        /// <summary>
        /// Navigate to the a referred page.
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        public void NavigateToPage(string referenceName)
        {
            if (this.navigatingTo != null) { throw new InvalidOperationException("Another page navigation is in progress!"); }
            if (referenceName == null) { throw new ArgumentNullException("referenceName"); }
            if (!this.pageReferences.ContainsKey(referenceName)) { throw new UIException(string.Format("Page reference with name '{0}' doesn't exist!", referenceName)); }

            this.navigatingTo = this.pageReferences[referenceName];
            this.Deactivate();
        }

        #region Methods for build up the page-graph

        /// <summary>
        /// Adds a reference to another RCAppPage.
        /// </summary>
        /// <param name="name">The name of the reference.</param>
        /// <param name="page">The referred RCAppPage.</param>
        public void AddReference(string name, RCAppPage page)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (page == null) { throw new ArgumentNullException("page"); }
            if (this.pageReferences.ContainsKey(name)) { throw new UIException(string.Format("Page reference with name '{0}' already exists!", name)); }

            this.pageReferences.Add(name, page);
        }

        /// <summary>
        /// Removes the reference with the given name.
        /// </summary>
        /// <param name="name">The name of the reference to remove.</param>
        public void RemoveReference(string name)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (!this.pageReferences.ContainsKey(name)) { throw new UIException(string.Format("Page reference with name '{0}' doesn't exist!", name)); }

            this.pageReferences.Remove(name);
        }

        #endregion Methods for build up the page-graph
        
        /// <summary>
        /// Called when this page has been activated. This method can be overriden in the derived classes and
        /// can perform operations after activation (for example: showing some panels of the page).
        /// </summary>
        protected virtual void OnActivated() { }

        /// <summary>
        /// Called when this page is being inactivated. This method can be overriden in the derived classes and
        /// can perform operations before inactivation (for example: send informations from the page to the
        /// business layer).
        /// </summary>
        protected virtual void OnInactivating() { }

        /// <summary>
        /// Called when the state of this RCAppPage has been changed.
        /// </summary>
        private void OnPageStatusChanged(UIPage sender, Status newState)
        {
            if (newState == Status.Active)
            {
                /// This page has been activated.
                this.OnActivated();                
            }
            else if (newState == Status.Inactivating)
            {
                /// This page is being inactivated.
                this.OnInactivating();
            }
        }

        /// <summary>
        /// Called when the active page of the UIWorkspace has been inactivated.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnActivePageInactivated(UIPage sender)
        {
            if (sender == this && this.navigatingTo != null)
            {
                this.navigatingTo.Activate();
                this.navigatingTo = null;
            }
        }

        /// <summary>
        /// List of the references to other RCAppPages mapped by their names.
        /// </summary>
        private Dictionary<string, RCAppPage> pageReferences;

        /// <summary>
        /// Reference to the page that we are currently navigating to.
        /// </summary>
        private RCAppPage navigatingTo;
    }
}
