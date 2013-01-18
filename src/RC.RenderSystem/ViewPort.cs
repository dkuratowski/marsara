using System.Collections.Generic;
using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>
    /// This class represents a part of the Display that can be used for drawing. Every ViewPort has an own
    /// coordinate system which is based on the upper-left corner of the corresponding ViewPort. The ViewPorts
    /// of a Display has a Z-order. The last ViewPort in the Z-order is the topmost ViewPort, the first ViewPort
    /// in the Z-order is the bottommost ViewPort. The RenderSystem is responsible for handle the cases when
    /// one or more ViewPorts are hiding a whole or a part of one or more ViewPorts.
    /// </summary>
    public class ViewPort
    {
        /// <summary>
        /// Constructs a ViewPort object.
        /// </summary>
        /// <param name="x">The X coordinate of this ViewPort inside the Display (in logical pixels).</param>
        /// <param name="y">The Y coordinate of this ViewPort inside the Display (in logical pixels).</param>
        /// <param name="width">The width of this ViewPort.</param>
        /// <param name="height">The height of this ViewPort.</param>
        public ViewPort(int x, int y, int width, int height)
        {
            this.position = new Rectangle(x, y, width, height);
            this.viewPortMoved = false;
            this.newPos = Rectangle.Empty;
            this.moveMutex = new object();
        }

        #region Overridable members

        /// <summary>
        /// This function is called by the render system if this ViewPort is registered to the Display.
        /// This function should return a list of rectangles that has invalid contents at the moment the
        /// render system calls this function.
        /// You can implement this function in any ViewPort derived classes. The default implementation
        /// returns an empty list.
        /// </summary>
        /// <returns>
        /// The list of the invalid rectangles of this ViewPort (in the ViewPort's own coordinate system).
        /// </returns>
        public virtual List<Rectangle> GetDirtyRects()
        {
            List<Rectangle> retList = new List<Rectangle>();
            //retList.Add(new Rectangle(0, 0, this.Width, this.Height));
            return retList;
        }

        /// <summary>
        /// This function is called by the render system if this ViewPort is registered to the Display.
        /// You can implement this function in any ViewPort derived classes. The default implementation
        /// do nothing.
        /// </summary>
        /// <param name="drawTarget">The target of the draw operations.</param>
        /// <param name="drawRect">The rectangle in which to draw.</param>
        public virtual void Draw(IDrawTarget drawTarget, Rectangle drawRect)
        {
        }

        #endregion

        /// <summary>
        /// Moves this ViewPort to the given position.
        /// </summary>
        /// <param name="newX">The X coordinate of the new position of the upper-left corner of this ViewPort.</param>
        /// <param name="newY">The Y coordinate of the new position of the upper-left corner of this ViewPort.</param>
        /// <remarks>This is a thread safe method.</remarks>
        public void MoveViewPort(int newX, int newY)
        {
            MoveViewPort(newX, newY, this.position.Width, this.position.Height);
        }

        /// <summary>
        /// Moves this ViewPort to the given position and resize it.
        /// </summary>
        /// <param name="newX">The X coordinate of the new position of the upper-left corner of this ViewPort.</param>
        /// <param name="newY">The Y coordinate of the new position of the upper-left corner of this ViewPort.</param>
        /// <param name="newWidth">The new width of the resized ViewPort.</param>
        /// <param name="newHeight">The new height of the resized ViewPort.</param>
        /// <remarks>This is a thread safe method.</remarks>
        public void MoveViewPort(int newX, int newY, int newWidth, int newHeight)
        {
            lock (this.moveMutex)
            {
                this.viewPortMoved = true;
                this.newPos = new Rectangle(newX, newY, newWidth, newHeight);
            }
        }

        /// <summary>Applies any move on this ViewPort.</summary>
        /// <returns>True if move has been appied, false otherwise.</returns>
        /// <remarks>For internal use only.</remarks>
        public bool ApplyMove()
        {
            lock (this.moveMutex)
            {
                if (this.viewPortMoved)
                {
                    this.position = this.newPos;
                    this.viewPortMoved = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the X coordinate of this ViewPort inside the Display (in logical pixels).
        /// </summary>
        public int X { get { return this.position.X; } }

        /// <summary>
        /// Gets the Y coordinate of this ViewPort inside the Display (in logical pixels).
        /// </summary>
        public int Y { get { return this.position.Y; } }

        /// <summary>
        /// Gets the width of this ViewPort (in logical pixels).
        /// </summary>
        public int Width { get { return this.position.Width; } }

        /// <summary>
        /// Gets the height of this ViewPort (in logical pixels).
        /// </summary>
        public int Height { get { return this.position.Height; } }

        /// <summary>
        /// Gets the position of this ViewPort (in logical pixels).
        /// </summary>
        public Rectangle Position { get { return this.position; } }

        /// <summary>
        /// The position of this ViewPort inside the Display (in logical pixels).
        /// </summary>
        private Rectangle position;

        /// <summary>
        /// This flag is true if the position of this ViewPort has been changed since last check.
        /// </summary>
        private bool viewPortMoved;

        /// <summary>
        /// This is the new position of this ViewPort. Only valid if this.viewPortMoved == true.
        /// </summary>
        private Rectangle newPos;

        /// <summary>
        /// A mutex object that is locked when the ViewPort is about to move.
        /// </summary>
        private object moveMutex;
    }
}
