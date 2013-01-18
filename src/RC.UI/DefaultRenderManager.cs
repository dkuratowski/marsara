using System;
using System.Collections.Generic;

namespace RC.UI
{
    /// <summary>
    /// This is the default implementation of the IRenderManager.
    /// </summary>
    public class DefaultRenderManager : UIRenderManagerBase
    {
        /// <summary>
        /// Constructs a DefaultRenderManager object.
        /// </summary>
        public DefaultRenderManager()
        {
            this.renderQueue = new List<UIObject>();
            this.contexts = new Dictionary<UIObject, DefaultRenderContext>();
        }

        /// <summary>
        /// Gets the string representation of the current render queue.
        /// </summary>
        /// <returns>The string representation of the current render queue.</returns>
        /// <remarks>Use this method for debugging.</remarks>
        public override string ToString()
        {
            string retString = string.Empty;
            int i = 0;
            foreach (UIObject obj in this.renderQueue)
            {
                retString += obj.ToString();
                if (i < this.renderQueue.Count - 1)
                {
                    retString += "-";
                }
                i++;
            }
            return retString;
        }

        #region Overriden methods of RenderManagerBase

        /// <see cref="UIRenderManagerBase.PostAttach_i"/>
        protected override void PostAttach_i()
        {
            this.AttachedObject.WalkTreeDFS(ref this.renderQueue);
            this.Subscribe(this.renderQueue);
            this.CreateRenderContexts(this.renderQueue);
        }

        /// <see cref="UIRenderManagerBase.PreDetach_i"/>
        protected override void PreDetach_i()
        {
            this.Unsubscribe(this.renderQueue);
            this.contexts.Clear();
            this.renderQueue.Clear();
        }

        /// <see cref="UIRenderManagerBase.Render_i"/>
        protected override void Render_i(IUIRenderContext screenContext)
        {
            /// Call the UIObject.Render on each UIObject in the render queue.
            foreach (UIObject obj in this.renderQueue)
            {
                DefaultRenderContext renderContext = this.contexts[obj]; /// TODO: performance issue
                renderContext.Reset(screenContext);
                renderContext.Enabled = true;
                obj.Render(renderContext);
                //obj.Render(screenContext); // TODO: just for testing
                renderContext.Enabled = false;
            }
        }

        #endregion Overriden methods of RenderManagerBase

        #region Event handlers

        /// <summary>
        /// Called when a new child of an enqueued UIObject has been attached.
        /// </summary>
        /// <param name="parentObj">The parent object.</param>
        /// <param name="childObj">The child object.</param>
        private void OnChildAttached(UIObject parentObj, UIObject childObj)
        {
            /// Get the subtree of childObj.
            List<UIObject> childWalk = new List<UIObject>();
            childObj.WalkTreeDFS(ref childWalk);

            /// Insert the UIObjects to the render queue, subscribe to their events and create
            /// the corresponding render contexts.
            UIObject nextSubTree = parentObj.NextSubTree;
            if (nextSubTree != null)
            {
                int idxNextSubTree = this.renderQueue.IndexOf(nextSubTree);
                if (-1 != idxNextSubTree)
                {
                    this.CreateRenderContexts(childWalk);
                    this.Subscribe(childWalk);
                    this.renderQueue.InsertRange(idxNextSubTree, childWalk);
                }
                else
                {
                    throw new UIException("Unexpected error when inserting object-subtree to the render queue!");
                }
            }
            else
            {
                this.CreateRenderContexts(childWalk);
                this.Subscribe(childWalk);
                this.renderQueue.AddRange(childWalk);
            }
        }

        /// <summary>
        /// Called when a child of an enqueued UIObject has been detached.
        /// </summary>
        /// <param name="parentObj">The parent object.</param>
        /// <param name="childObj">The child object.</param>
        private void OnChildDetached(UIObject parentObj, UIObject childObj)
        {
            /// Get the subtree of childObj.
            List<UIObject> childWalk = new List<UIObject>();
            childObj.WalkTreeDFS(ref childWalk);

            /// Remove the UIObjects from the render queue, unsubscribe from their events and
            /// destroy the corresponding render contexts.
            int idxChild = this.renderQueue.IndexOf(childObj);
            if (idxChild != -1)
            {
                this.DestroyRenderContexts(childWalk);
                this.Unsubscribe(childWalk);
                this.renderQueue.RemoveRange(idxChild, childWalk.Count);
            }
            else
            {
                throw new UIException("Unexpected error when removing object-subtree from the render queue!");
            }
        }

        /// <summary>
        /// Called when a UIObject has been brought forward in the Z-order.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnBroughtForward(UIObject sender)
        {
            /// Get the subtree of the sender.
            List<UIObject> senderWalk = new List<UIObject>();
            sender.WalkTreeDFS(ref senderWalk);

            /// Swap the UIObjects in the render queue.
            UIObject prevSibling = sender.PreviousSibling;
            if (prevSibling != null)
            {
                List<UIObject> prevSiblingWalk = new List<UIObject>();
                prevSibling.WalkTreeDFS(ref prevSiblingWalk);

                int idxPrevSibling = this.renderQueue.IndexOf(prevSibling);
                int idxSender = this.renderQueue.IndexOf(sender);
                if (idxPrevSibling != -1 && idxSender != -1)
                {
                    this.renderQueue.RemoveRange(idxPrevSibling, prevSiblingWalk.Count);
                    this.renderQueue.InsertRange(idxSender, prevSiblingWalk);
                }
                else
                {
                    throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
                }
            }
            else
            {
                throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
            }
        }

        /// <summary>
        /// Called when a UIObject has been brought to the top of the Z-order.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnBroughtToTop(UIObject sender)
        {
            /// Get the subtree of the sender.
            List<UIObject> senderWalk = new List<UIObject>();
            sender.WalkTreeDFS(ref senderWalk);

            /// Remove the sender from the render queue.
            int idxSender = this.renderQueue.IndexOf(sender);
            if (idxSender != -1)
            {
                this.renderQueue.RemoveRange(idxSender, senderWalk.Count);
            }
            else
            {
                throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
            }

            /// Insert the sender back to the render queue just before it's next subtree.
            UIObject nextSubTree = sender.NextSubTree;
            if (nextSubTree != null)
            {
                int idxNextSubTree = this.renderQueue.IndexOf(nextSubTree);
                if (-1 != idxNextSubTree)
                {
                    this.renderQueue.InsertRange(idxNextSubTree, senderWalk);
                }
                else
                {
                    throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
                }
            }
            else
            {
                this.renderQueue.AddRange(senderWalk);
            }
        }

        /// <summary>
        /// Called when a UIObject has been sent backward in the Z-order.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnSentBackward(UIObject sender)
        {
            /// Get the subtree of the sender.
            List<UIObject> senderWalk = new List<UIObject>();
            sender.WalkTreeDFS(ref senderWalk);

            /// Swap the UIObjects in the render queue.
            UIObject nextSibling = sender.NextSibling;
            if (nextSibling != null)
            {
                List<UIObject> nextSiblingWalk = new List<UIObject>();
                nextSibling.WalkTreeDFS(ref nextSiblingWalk);

                int idxNextSibling = this.renderQueue.IndexOf(nextSibling);
                int idxSender = this.renderQueue.IndexOf(sender);
                if (idxNextSibling != -1 && idxSender != -1)
                {
                    this.renderQueue.RemoveRange(idxSender, senderWalk.Count);
                    this.renderQueue.InsertRange(idxNextSibling, senderWalk);
                }
                else
                {
                    throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
                }
            }
            else
            {
                throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
            }
        }

        /// <summary>
        /// Called when a UIObject has been sent to the bottom of the Z-order.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnSentToBottom(UIObject sender)
        {
            /// Get the subtree of the sender.
            List<UIObject> senderWalk = new List<UIObject>();
            sender.WalkTreeDFS(ref senderWalk);

            /// Insert the senderWalk just before sender's next sibling.
            UIObject nextSibling = sender.NextSibling;
            if (nextSibling != null)
            {
                int idxSender = this.renderQueue.IndexOf(sender);
                int idxNextSibling = this.renderQueue.IndexOf(nextSibling);
                if (idxNextSibling != -1)
                {
                    this.renderQueue.RemoveRange(idxSender, senderWalk.Count);
                    this.renderQueue.InsertRange(idxNextSibling, senderWalk);
                }
                else
                {
                    throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
                }
            }
            else
            {
                throw new UIException("Unexpected error when swapping object-subtree in the render queue!");
            }
        }

        #endregion Event handlers

        /// <summary>
        /// Creates the render contexts for the given objects.
        /// </summary>
        /// <param name="objects">The objects to create render contexts for.</param>
        private void CreateRenderContexts(List<UIObject> objects)
        {
            foreach (UIObject obj in objects)
            {
                DefaultRenderContext newContext = new DefaultRenderContext(obj);
                this.contexts.Add(obj, newContext);
            }
        }

        /// <summary>
        /// Destroys the render contexts of the given objects.
        /// </summary>
        /// <param name="objects">The objects whose corresponding render contexts shall be destroyed.</param>
        private void DestroyRenderContexts(List<UIObject> objects)
        {
            foreach (UIObject obj in objects)
            {
                this.contexts.Remove(obj);
            }
        }

        /// <summary>
        /// Subscribes to the events of the given UIObjects.
        /// </summary>
        /// <param name="toObjs">The UIObjects to subscribe.</param>
        private void Subscribe(List<UIObject> toObjs)
        {
            foreach (UIObject obj in toObjs)
            {
                obj.ChildAttached += this.OnChildAttached;
                obj.ChildDetached += this.OnChildDetached;
                obj.BroughtForward += this.OnBroughtForward;
                obj.BroughtToTop += this.OnBroughtToTop;
                obj.SentBackward += this.OnSentBackward;
                obj.SentToBottom += this.OnSentToBottom;
            }
        }

        /// <summary>
        /// Unubscribes from the events of the given UIObjects.
        /// </summary>
        /// <param name="fromObjs">The UIObjects to unsubscribe from.</param>
        private void Unsubscribe(List<UIObject> fromObjs)
        {
            foreach (UIObject obj in fromObjs)
            {
                obj.ChildAttached -= this.OnChildAttached;
                obj.ChildDetached -= this.OnChildDetached;
                obj.BroughtForward -= this.OnBroughtForward;
                obj.BroughtToTop -= this.OnBroughtToTop;
                obj.SentBackward -= this.OnSentBackward;
                obj.SentToBottom -= this.OnSentToBottom;
            }
        }

        /// <summary>
        /// List of the UIObjects in order of rendering.
        /// </summary>
        private List<UIObject> renderQueue;

        /// <summary>
        /// List of the render contexts mapped by the corresponding UIObjects.
        /// </summary>
        private Dictionary<UIObject, DefaultRenderContext> contexts;
    }
}
