using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace RC.RenderSystem
{
    /// <summary>
    /// This is a Display that can be used for drawing using ViewPort objects.
    /// </summary>
    public class Display : IDisposable
    {
        #region Static members

        /// <summary>
        /// Creates a Display object with the given width, height, vertical and horizontal scale and background color.
        /// </summary>
        /// <param name="width">The width of the Display (in logical pixels).</param>
        /// <param name="height">The height of the Display (in logical pixels).</param>
        /// <param name="horzScale">The horizontal scaling factor of the Display.</param>
        /// <param name="vertScale">The vertical scaling factor of the Display.</param>
        /// <param name="background">The background color of the created Display.</param>
        /// <returns>
        /// A reference to the newly created Display or a null pointer if the Display already exists.
        /// </returns>
        /// <remarks>
        /// If the Display already exists, you can get a reference to it with the Display.Instance property.
        /// </remarks>
        public static Display Create(int width, int height, int horzScale, int vertScale, Color background)
        {
            if (null == instance)
            {
                instance = new Display(width, height, horzScale, vertScale, background);

                /// Initialize the frame buffer only after the instance has been successfully created.
                /// See the comment in the private constructor of the Display for more information.
                instance.frameBuffer = new ScaledBitmap(width, height);
                instance.frameBufferAccess = BitmapAccess.FromBitmap(instance.frameBuffer);
                instance.frameBufferAccess.Clear(instance.backgroundColor);

                return instance;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the horizontal scaling factor of the Display object. If it doesn't exists then this
        /// function will simply return 1.
        /// </summary>
        public static int HorizontalScale
        {
            get { return (null != Display.Instance) ? (Display.Instance.horizontalScale) : (1); }
        }

        /// <summary>
        /// Gets the vertical scaling factor of the Display object. If it doesn't exists then this
        /// function will simply return 1.
        /// </summary>
        public static int VerticalScale
        {
            get { return (null != Display.Instance) ? (Display.Instance.verticalScale) : (1); }
        }

        /// <summary>
        /// Gets the singleton instance of the Display or a null reference if the display doesn't exist.
        /// </summary>
        /// <remarks>
        /// You can create the Display with the Display.Create() function.
        /// </remarks>
        public static Display Instance { get { return instance; } }

        /// <summary>
        /// The singleton instance of the Display.
        /// </summary>
        private static Display instance = null;

        #endregion

        #region Rendering interface

        /// <summary>
        /// Starts rendering the next frame of the Display.
        /// </summary>
        /// <param name="refreshedArea">
        /// The bounding box of the regions on the Display that has been refreshed (the values of this
        /// rectangle is in logical pixels).
        /// </param>
        /// <returns>
        /// If another thread is currently executing a render operation or render loop then this function
        /// returns immediately with false. Otherwise it returns true after the render operation finished.
        /// </returns>
        public bool RenderOneFrame(out Rectangle refreshedArea)
        {
            /// Test the state of the semaphore but don't block the thread.
            if (this.renderSemaphore.WaitOne(0))
            {
                /// If one of the ViewPorts has been moved or changed its position in the Z-order or a ViewPort
                /// has been registered at or unregistered from the Display then this is the point where we can apply
                /// these changes and invalidate the Display if necessary.
                ApplyViewPortChanges();

                CollectDirtyRectsFromViewPorts();
                ApplyZOrderOnViewPorts();

                lock (this.frameBuffer)
                {
                    if (this.invalidate)
                    {
                        this.frameBufferAccess.ClipBounds = new Rectangle(0, 0, this.width, this.height);
                        this.frameBufferAccess.Clear(instance.backgroundColor);
                    }
                    CallDrawFunctionOnViewPorts();
                    refreshedArea = (!this.invalidate) ? (ComputeRefreshedArea())
                                                       : new Rectangle(0, 0, this.width, this.height);
                    this.invalidate = false;
                }

                //FramePostProcessing();

                /// Render operation finished, release the semaphore.
                this.renderSemaphore.Release();
                return true;
            }
            else
            {
                /// If the semaphore is not signaled, we return false indicating that another
                /// thread has already started a render operation or a render loop.
                refreshedArea = new Rectangle();
                return false;
            }
        }

        /// <summary>
        /// Starts an infinite render loop with the thread that called this function. The function will
        /// block the calling thread while the render loop is running. The loop can be stopped from the
        /// event handler functions in a registered IRenderLoopListener.
        /// </summary>
        /// <returns>
        /// If another thread is currently executing a render operation or render loop then this function
        /// returns immediately with false. Otherwise it returns true after the render loop finished.
        /// </returns>
        public bool StartRenderLoop()
        {
            /// Test the state of the semaphore but doesn't block the thread.
            if (this.renderSemaphore.WaitOne(0))
            {
                long lastFrameEndTime = 0;
                long currFrameBeginTime = 0;
                long currDrawBeginTime = 0;
                long currDrawFinishTime = 0;
                bool firstFrame = true;
                Rectangle refreshedArea;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    /// If one of the ViewPorts has been moved or changed its position in the Z-order or a ViewPort
                    /// has been registered at or unregistered from the Display then this is the point where we can apply
                    /// these changes and invalidate the Display if necessary.
                    ApplyViewPortChanges();

                    currFrameBeginTime = (firstFrame) ? (0) : (watch.ElapsedMilliseconds);
                    if (!CallListenersCreateFrameBegin((firstFrame) ? (0) : (currFrameBeginTime - lastFrameEndTime)))
                    {
                        /// One of the listeners returned 0, so finish the render loop.
                        break;
                    }

                    CollectDirtyRectsFromViewPorts();
                    ApplyZOrderOnViewPorts();

                    currDrawBeginTime = watch.ElapsedMilliseconds;
                    CallListenersDrawBegin(currDrawBeginTime - currFrameBeginTime);
                    lock (this.frameBuffer)
                    {
                        if (this.invalidate)
                        {
                            this.frameBufferAccess.ClipBounds = new Rectangle(0, 0, this.width, this.height);
                            this.frameBufferAccess.Clear(instance.backgroundColor);
                        }
                        CallDrawFunctionOnViewPorts();
                        refreshedArea = (!this.invalidate) ? (ComputeRefreshedArea())
                                                           : new Rectangle(0, 0, this.width, this.height);
                        this.invalidate = false;
                    }
                    currDrawFinishTime = watch.ElapsedMilliseconds;
                    CallListenersDrawFinish(currDrawFinishTime - currDrawBeginTime, refreshedArea);

                    //FramePostProcessing();

                    long currFrameFinishTime = watch.ElapsedMilliseconds;
                    if (!CallListenersCreateFrameFinish((firstFrame) ? (0) : (currFrameFinishTime - lastFrameEndTime),
                                                        currFrameFinishTime - currFrameBeginTime,
                                                        currFrameFinishTime - currDrawFinishTime))
                    {
                        /// One of the listeners returned 0, so finish the render loop.
                        break;
                    }
                    lastFrameEndTime = currFrameFinishTime;
                    firstFrame = false;
                }

                /// Render loop finished, release the semaphore.
                this.renderSemaphore.Release();
                return true;
            }
            else
            {
                /// If the semaphore is not signaled, we return false indicating that another
                /// thread has already started a render operation or a render loop.
                return false;
            }
        }

        /// <summary>
        /// You can only gain access to the contents of the frame buffer if you ask the Display to
        /// copy a region of it to a graphic context using this function.
        /// </summary>
        /// <param name="gc">
        /// The graphic context to which you want to copy the given region of the frame buffer.
        /// </param>
        /// <param name="clipRect">
        /// This rectangle defines the region of the frame buffer you want to copy (in physical pixels).
        /// </param>
        /// <remarks>
        /// This function blocks the caller thread during the rendering periods (while the frame buffer is
        /// being written by the render thread(s)).
        /// </remarks>
        public void AccessCurrentFrame(Graphics gc, Rectangle clipRect)
        {
            gc.Clip = new Region(clipRect);

            /// Gain exclusive access to the frame buffer.
            lock (this.frameBuffer)
            {
                gc.DrawImageUnscaled(this.frameBuffer.RawBitmap, 0, 0);
            }
        }

        #endregion

        #region RenderLoopListener management

        /// <summary>
        /// Registers the given render loop listener object to this Display.
        /// </summary>
        /// <param name="listener">The listener object you want to register.</param>
        /// <remarks>
        /// If the given listener has been already registered then this function has no effect.
        /// Frame listeners can be used when you want to be notified about events during a render loop.
        /// If you call RenderOneFrame() for rendering a frame then the event handlers of the frame
        /// listeners won't be called. They will be only called if you are in a render loop with the
        /// StartRenderLoop() function.
        /// </remarks>
        public void RegisterListener(IRenderLoopListener listener)
        {
            lock (this.renderLoopListeners)
            {
                if (!this.renderLoopListeners.Contains(listener))
                {
                    this.renderLoopListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Unregisters the given render loop listener object from this Display.
        /// </summary>
        /// <param name="listener">The listener object you want to unregister.</param>
        /// <remarks>
        /// If the given listener was not registered then this function has no effect.
        /// </remarks>
        public void UnregisterListener(IRenderLoopListener listener)
        {
            lock (this.renderLoopListeners)
            {
                if (this.renderLoopListeners.Contains(listener))
                {
                    this.renderLoopListeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Unregisters every render loop listeners from the Display.
        /// </summary>
        public void UnregisterAllListeners()
        {
            lock (this.renderLoopListeners)
            {
                this.renderLoopListeners.Clear();
            }
        }

        #endregion

        #region ViewPort management

        /// <summary>
        /// Registers the given ViewPort to the Display.
        /// </summary>
        /// <param name="vp">The ViewPort you want to register.</param>
        /// <remarks>If the given ViewPort is already registered then this function has no effect.</remarks>
        public void RegisterViewPort(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.RegisterViewPort_i);
                this.manipulatorParams.Add(vp);
            }
        }

        /// <summary>
        /// Unregisters the given ViewPort from the Display.
        /// </summary>
        /// <param name="vp">The ViewPort you want to unregister.</param>
        /// <remarks>If the given ViewPort is not registered then this function has no effect.</remarks>
        public void UnregisterViewPort(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.UnregisterViewPort_i);
                this.manipulatorParams.Add(vp);
            }
        }

        /// <summary>
        /// Unregisters every ViewPorts from the Display.
        /// </summary>
        public void UnregisterAllViewPorts()
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.UnregisterAllViewPorts_i);
                this.manipulatorParams.Add(null); /// Unused parameter...
            }
        }

        /// <summary>
        /// Brings the given ViewPort forward one step in the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to bring forward.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the topmost ViewPort of the Display then this function
        /// has no effect.
        /// </remarks>
        public void BringViewPortForward(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.BringViewPortForward_i);
                this.manipulatorParams.Add(vp);
            }
        }

        /// <summary>
        /// Sends the given ViewPort backward one step in the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to send backward.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the bottommost ViewPort of the Display then this function
        /// has no effect.
        /// </remarks>
        public void SendViewPortBackward(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.SendViewPortBackward_i);
                this.manipulatorParams.Add(vp);
            }
        }

        /// <summary>
        /// Brings the given ViewPort to the top of the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to bring to the top.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the topmost ViewPort of the Display then this function
        /// has no effect.
        /// </remarks>
        public void BringViewPortTop(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.BringViewPortTop_i);
                this.manipulatorParams.Add(vp);
            }
        }

        /// <summary>
        /// Sends the given ViewPort to the bottom of the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to send to the bottom.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the bottommost ViewPort of the Display then this function
        /// has no effect.
        /// </remarks>
        public void SendViewPortBottom(ViewPort vp)
        {
            lock (this.manipulators)
            {
                this.manipulators.Add(this.SendViewPortBottom_i);
                this.manipulatorParams.Add(vp);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.frameBufferAccess.Dispose();
            this.frameBuffer.Dispose();
            this.renderSemaphore.Close();
        }

        #endregion

        #region Private members

        /// <summary>
        /// Constructs a Display object.
        /// </summary>
        /// <param name="width">The width of the Display (in logical pixels).</param>
        /// <param name="height">The height of the Display (in logical pixels).</param>
        /// <param name="horzScale">The horizontal scaling factor of the Display.</param>
        /// <param name="vertScale">The vertical scaling factor of the Display.</param>
        /// <param name="background">The background color of the Display.</param>
        private Display(int width, int height, int horzScale, int vertScale, Color background)
        {
            if (horzScale <= 0 || vertScale <= 0)
            {
                throw new ArgumentException("Horizontal and vertical scaling factors have to be greater than 0!");
            }
            else if (background.IsEmpty)
            {
                throw new ArgumentException("Background color not specified!", "background");
            }
            else
            {
                this.width = width;
                this.height = height;
                this.horizontalScale = horzScale;
                this.verticalScale = vertScale;
                this.backgroundColor = background;

                /// We cannot initialize the frame buffer here because at this point Display.Instance is still null
                /// and the horizontal and vertical scaling factor is 1. Instead we will initialize it at the static
                /// method Display.Create after the instance has been created.
                this.frameBuffer = null;
                this.frameBufferAccess = null;

                this.renderSemaphore = new Semaphore(1, 1);
                this.renderLoopListeners = new HashSet<IRenderLoopListener>();
                this.viewPorts = new List<ViewPort>();
                this.vpRefreshers = new List<ViewPortRefreshMgr>();
                this.invalidate = true;
                this.manipulators = new List<ViewPortManipulator>();
                this.manipulatorParams = new List<ViewPort>();
            }
        }

        #region Render helper functions

        /// <summary>
        /// This function asks the refresh managers to collect those rectangles from the ViewPorts which
        /// they want to refresh.
        /// </summary>
        private void CollectDirtyRectsFromViewPorts()
        {
            for (int idx = 0; idx < this.vpRefreshers.Count; idx++)
            {
                this.vpRefreshers[idx].CollectDirtyRects();
            }
        }

        /// <summary>
        /// This function applies the current Z-order on the ViewPorts. This means that all refresh manager
        /// will be notified if any parts of it's corresponding ViewPort is covered by another ViewPort.
        /// </summary>
        private void ApplyZOrderOnViewPorts()
        {
            for (int currIdx = 0; currIdx < this.viewPorts.Count - 1; currIdx++)
            {
                ViewPort currVP = this.viewPorts[currIdx];
                ViewPortRefreshMgr currVPR = this.vpRefreshers[currIdx];

                for (int otherIdx = currIdx + 1; otherIdx < this.viewPorts.Count; otherIdx++)
                {
                    ViewPort otherVP = this.viewPorts[otherIdx];
                    ViewPortRefreshMgr otherVPR = this.vpRefreshers[otherIdx];
                    Rectangle intersection = Rectangle.Intersect(currVP.Position, otherVP.Position);
                    if (!intersection.IsEmpty)
                    {
                        currVPR.AddHiddenArea(otherVPR, intersection);
                    }
                }
            }
        }

        /// <summary>
        /// This function asks the refresh managers to call the draw functions of the corresponding ViewPorts.
        /// </summary>
        private void CallDrawFunctionOnViewPorts()
        {
            foreach (ViewPortRefreshMgr vpr in this.vpRefreshers)
            {
                vpr.CallDrawFunction();
            }
        }

        /// <summary>
        /// Computes the bounding box of all refreshed areas in the current frame.
        /// </summary>
        /// <returns>
        /// The bounding box of all refreshed areas in the current frame.
        /// </returns>
        private Rectangle ComputeRefreshedArea()
        {
            Rectangle boundingBox = new Rectangle();
            foreach (ViewPortRefreshMgr vpr in this.vpRefreshers)
            {
                Rectangle r = vpr.ComputeRefreshedArea();
                if (!r.IsEmpty)
                {
                    if (!boundingBox.IsEmpty)
                    {
                        boundingBox = Rectangle.Union(boundingBox, r);
                    }
                    else
                    {
                        boundingBox = r;
                    }
                }
            }
            return boundingBox;
        }

        /// <summary>
        /// Applies any changes in the ViewPort positions and Z-order at the end of a render operation.
        /// </summary>
        /// <remarks>
        /// You have to call this function at the end of a render operation from the same thread that is
        /// actually rendering.
        /// </remarks>
        private void ApplyViewPortChanges()
        {
            foreach (ViewPortRefreshMgr vpr in this.vpRefreshers)
            {
                if (vpr.ApplyMoveViewPort())
                {
                    this.invalidate = true;
                }
            }
            if (ApplyViewPortManipulators())
            {
                this.invalidate = true;
            }
            if (this.invalidate)
            {
                foreach (ViewPortRefreshMgr vpr in this.vpRefreshers)
                {
                    vpr.Invalidate();
                }
            }
        }

        #endregion

        #region IRenderLoopListener notification functions

        /// <summary>
        /// This function calls the IRenderLoopListener.CreateFrameBegin event handler for every registered
        /// render loop listener.
        /// </summary>
        /// <seealso cref="IRenderLoopListener.CreateFrameBegin"/>
        /// <returns>True if every listener returned true, false otherwise.</returns>
        /// <remarks>
        /// It is guaranteed that all listener will get the message even if one of them returns false.
        /// </remarks>
        private bool CallListenersCreateFrameBegin(long timeSinceLastFrameFinish)
        {
            bool retVal = true;
            lock (this.renderLoopListeners)
            {
                foreach (IRenderLoopListener listener in this.renderLoopListeners)
                {
                    bool currRetVal = listener.CreateFrameBegin(timeSinceLastFrameFinish);
                    retVal = retVal && currRetVal;
                }
            }
            return retVal;
        }

        /// <summary>
        /// This function calls the IRenderLoopListener.DrawBegin event handler for every registered
        /// render loop listener.
        /// </summary>
        /// <seealso cref="IRenderLoopListener.DrawBegin"/>
        private void CallListenersDrawBegin(long timeSinceFrameBegin)
        {
            lock (this.renderLoopListeners)
            {
                foreach (IRenderLoopListener listener in this.renderLoopListeners)
                {
                    listener.DrawBegin(timeSinceFrameBegin);
                }
            }
        }

        /// <summary>
        /// This function calls the IRenderLoopListener.DrawFinish event handler for every registered
        /// render loop listener.
        /// </summary>
        /// <seealso cref="IRenderLoopListener.DrawFinish"/>
        private void CallListenersDrawFinish(long timeSinceDrawBegin, Rectangle refreshedArea)
        {
            lock (this.renderLoopListeners)
            {
                foreach (IRenderLoopListener listener in this.renderLoopListeners)
                {
                    listener.DrawFinish(timeSinceDrawBegin, refreshedArea);
                }
            }
        }

        /// <summary>
        /// This function calls the IRenderLoopListener.CreateFrameFinish event handler for every registered
        /// render loop listener.
        /// </summary>
        /// <seealso cref="IRenderLoopListener.CreateFrameFinish"/>
        /// <returns>True if every listener returned true, false otherwise.</returns>
        /// <remarks>
        /// It is guaranteed that all listener will get the message even if one of them returns false.
        /// </remarks>
        private bool CallListenersCreateFrameFinish(long timeSinceLastFrameFinish,
                                                    long timeSinceThisFrameBegin,
                                                    long timeSinceDrawFinish)
        {
            bool retVal = true;
            lock (this.renderLoopListeners)
            {
                foreach (IRenderLoopListener listener in this.renderLoopListeners)
                {
                    bool currRetVal = listener.CreateFrameFinish(timeSinceLastFrameFinish,
                                                                 timeSinceThisFrameBegin,
                                                                 timeSinceDrawFinish);
                    retVal = retVal && currRetVal;
                }
            }
            return retVal;
        }

        #endregion

        #region Internal ViewPort manipulators

        /// <summary>
        /// Delegate for functions that can manipulate ViewPorts.
        /// </summary>
        /// <param name="vp">The target ViewPort of the manipulation.</param>
        /// <remarks>
        /// For example: A client of the Display wants to register a new ViewPort but the current frame is still being
        /// rendered. He calls RegisterViewPort() on the public interface. Registering the new ViewPort is not
        /// executed immediately. Instead the manipulator function RegisterViewPort_i (that matches with this
        /// delegate) will be put into the list this.manipulators and the given ViewPort will be put into the
        /// list this.manipulatorParams. These manipulator functions will then be called at the beginning of the next
        /// frame by the render thread.
        /// </remarks>
        private delegate void ViewPortManipulator(ViewPort vp);

        /// <summary>
        /// Registers the given ViewPort to the Display.
        /// </summary>
        /// <param name="vp">The ViewPort you want to register.</param>
        /// <remarks>
        /// If the given ViewPort is already registered then this function has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void RegisterViewPort_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                if (!this.viewPorts.Contains(vp))
                {
                    this.viewPorts.Add(vp);
                    this.vpRefreshers.Add(new ViewPortRefreshMgr(vp, this.frameBufferAccess));
                }
            }
        }

        /// <summary>
        /// Unregisters the given ViewPort from the Display.
        /// </summary>
        /// <param name="vp">The ViewPort you want to unregister.</param>
        /// <remarks>
        /// If the given ViewPort is not registered then this function has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void UnregisterViewPort_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                if (this.viewPorts.Contains(vp))
                {
                    int vpIdx = this.viewPorts.IndexOf(vp);
                    this.viewPorts.Remove(vp);
                    this.vpRefreshers.RemoveAt(vpIdx);
                }
            }
        }

        /// <summary>
        /// Unregisters every ViewPorts from the Display.
        /// </summary>
        /// <param name="hasToBeNull">
        /// This parameter is not used and is only needed for matching with the delegate ViewPortManipulator.
        /// </param>
        /// <remarks>Call this from the render thread.</remarks>
        private void UnregisterAllViewPorts_i(ViewPort hasToBeNull)
        {
            lock (this.viewPorts)
            {
                this.viewPorts.Clear();
                this.vpRefreshers.Clear();
            }
        }

        /// <summary>
        /// Brings the given ViewPort forward one step in the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to bring forward.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the topmost ViewPort of the Display then this function
        /// has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void BringViewPortForward_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                int vpIndex = this.viewPorts.IndexOf(vp);
                if (-1 != vpIndex && vpIndex < this.viewPorts.Count - 1)
                {
                    /// Swap the given ViewPort and the ViewPort in front of it.
                    ViewPortRefreshMgr vpr = this.vpRefreshers[vpIndex];
                    this.viewPorts[vpIndex] = this.viewPorts[vpIndex + 1];
                    this.viewPorts[vpIndex + 1] = vp;
                    this.vpRefreshers[vpIndex] = this.vpRefreshers[vpIndex + 1];
                    this.vpRefreshers[vpIndex + 1] = vpr;
                }
            }
        }

        /// <summary>
        /// Sends the given ViewPort backward one step in the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to send backward.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the bottommost ViewPort of the Display then this function
        /// has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void SendViewPortBackward_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                int vpIndex = this.viewPorts.IndexOf(vp);
                if (vpIndex > 0)
                {
                    /// Swap the given ViewPort and the ViewPort behind it.
                    ViewPortRefreshMgr vpr = this.vpRefreshers[vpIndex];
                    this.viewPorts[vpIndex] = this.viewPorts[vpIndex - 1];
                    this.viewPorts[vpIndex - 1] = vp;
                    this.vpRefreshers[vpIndex] = this.vpRefreshers[vpIndex - 1];
                    this.vpRefreshers[vpIndex - 1] = vpr;
                }
            }
        }

        /// <summary>
        /// Brings the given ViewPort to the top of the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to bring to the top.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the topmost ViewPort of the Display then this function
        /// has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void BringViewPortTop_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                int vpIndex = this.viewPorts.IndexOf(vp);
                if (-1 != vpIndex && vpIndex < this.viewPorts.Count - 1)
                {
                    ViewPortRefreshMgr vpr = this.vpRefreshers[vpIndex];
                    for (int i = vpIndex; i < this.viewPorts.Count - 1; i++)
                    {
                        this.viewPorts[i] = this.viewPorts[i + 1];
                        this.vpRefreshers[i] = this.vpRefreshers[i + 1];
                    }
                    this.viewPorts[this.viewPorts.Count - 1] = vp;
                    this.vpRefreshers[this.vpRefreshers.Count - 1] = vpr;
                }
            }
        }

        /// <summary>
        /// Sends the given ViewPort to the bottom of the Z-order.
        /// </summary>
        /// <param name="vp">The ViewPort you want to send to the bottom.</param>
        /// <remarks>
        /// If the given ViewPort is not registered or is the bottommost ViewPort of the Display then this function
        /// has no effect.
        /// Call this from the render thread.
        /// </remarks>
        private void SendViewPortBottom_i(ViewPort vp)
        {
            lock (this.viewPorts)
            {
                int vpIndex = this.viewPorts.IndexOf(vp);
                if (vpIndex > 0)
                {
                    ViewPortRefreshMgr vpr = this.vpRefreshers[vpIndex];
                    for (int i = vpIndex; i > 0; i--)
                    {
                        this.viewPorts[i] = this.viewPorts[i - 1];
                        this.vpRefreshers[i] = this.vpRefreshers[i - 1];
                    }
                    this.viewPorts[0] = vp;
                    this.vpRefreshers[0] = vpr;
                }
            }
        }

        /// <summary>
        /// Calls the functions in the list this.manipulators with the parameters in the list this.manipulatorParams.
        /// </summary>
        /// <returns>
        /// True if there is at least 1 manipulator function to call.
        /// </returns>
        private bool ApplyViewPortManipulators()
        {
            lock (this.manipulators)
            {
                if (0 != this.manipulators.Count)
                {
                    if (this.manipulators.Count == this.manipulatorParams.Count)
                    {
                        for (int i = 0; i < this.manipulators.Count; i++)
                        {
                            this.manipulators[i](this.manipulatorParams[i]);
                        }
                        this.manipulators.Clear();
                        this.manipulatorParams.Clear();
                        return true;
                    }
                    else
                    {
                        throw new RenderSystemException("Inconsistence in ViewPortManipulator FIFO!");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// List of the ViewPortManipulator functions that has to be called by the render thread at the beginning
        /// of the next frame.
        /// </summary>
        private List<ViewPortManipulator> manipulators;

        /// <summary>
        /// List of the parameters for each manipulator function call.
        /// </summary>
        private List<ViewPort> manipulatorParams;

        #endregion

        #region Data members

        /// <summary>
        /// The horizontal scaling factor of the Display.
        /// </summary>
        private int horizontalScale;

        /// <summary>
        /// The vertical scaling factor of the Display.
        /// </summary>
        private int verticalScale;

        /// <summary>
        /// The background color of the Display.
        /// </summary>
        private Color backgroundColor;

        /// <summary>
        /// This is the frame buffer of the render system. Every draw operation happens in this buffer
        /// while the contents of primary buffer is displayed on the screen. After the draw operations finished
        /// the contents of the primary buffer will be overwritten with the contents of this buffer in one step.
        /// </summary>
        private ScaledBitmap frameBuffer;

        /// <summary>
        /// This is the interface that is used by the ViewPortRefreshMgrs to draw the frame buffer.
        /// </summary>
        private BitmapAccess frameBufferAccess;

        /// <summary>
        /// The width of the Display.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the Display.
        /// </summary>
        private int height;

        /// <summary>
        /// Only one thread can call RenderOneFrame() or StartRenderLoop() at a time.
        /// </summary>
        private Semaphore renderSemaphore;

        /// <summary>
        /// List of the frame listeners registered to the Display object.
        /// </summary>
        private HashSet<IRenderLoopListener> renderLoopListeners;

        /// <summary>
        /// List of the ViewPort added to this Display.
        /// </summary>
        private List<ViewPort> viewPorts;

        /// <summary>
        /// List of the refresh managers. The nth refresher in this list belongs to the nth ViewPort
        /// in this.viewPorts.
        /// </summary>
        private List<ViewPortRefreshMgr> vpRefreshers;

        /// <summary>
        /// This flag is true if we have to redraw the whole frame buffer in the next frame.
        /// </summary>
        private bool invalidate;

        #endregion

        #endregion
    }
}
