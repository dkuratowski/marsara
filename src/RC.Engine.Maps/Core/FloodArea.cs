using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using System.Collections;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This is a helper class that is used for drawing terrain on the map.
    /// </summary>
    public class FloodArea : IEnumerable<FloodItem> // TODO: make private
    {
        /// <summary>
        /// Constructs a FloodArea object.
        /// </summary>
        public FloodArea()
        {
            this.coordsInside = new RCSet<RCIntVector>();
            this.enlargements = new List<int>();
            this.currentRadius = 1;
            this.tmpFloodItems = null;

            this.northCorner = new RCIntVector(-1, -1);
            this.eastCorner = new RCIntVector(1, -1);
            this.westCorner = new RCIntVector(-1, 1);
            this.southCorner = new RCIntVector(1, 1);

            this.coordsInside.Add(this.northCorner);
            this.coordsInside.Add(this.eastCorner);
            this.coordsInside.Add(this.westCorner);
            this.coordsInside.Add(this.southCorner);

            this.coordsInside.Add(new RCIntVector(0, -1));
            this.coordsInside.Add(new RCIntVector(-1, 0));
            this.coordsInside.Add(new RCIntVector(0, 0));
            this.coordsInside.Add(new RCIntVector(1, 0));
            this.coordsInside.Add(new RCIntVector(0, 1));
        }

        #region Public methods

        /// <summary>
        /// Enlarges this flood with the given amount.
        /// </summary>
        /// <param name="amount">The amount to enlarge with.</param>
        public void Enlarge(int amount)
        {
            if (amount <= 0) { throw new ArgumentOutOfRangeException("amount", "Amount of flood area enlargement must be greater than 0!"); }

            /// Calculate the corners.
            RCIntVector northCorner = new RCIntVector(-(this.enlargements.Count + 2), -(this.enlargements.Count + 2));
            RCIntVector eastCorner = new RCIntVector((this.enlargements.Count + 2), -(this.enlargements.Count + 2));
            RCIntVector westCorner = new RCIntVector(-(this.enlargements.Count + 2), (this.enlargements.Count + 2));
            RCIntVector southCorner = new RCIntVector((this.enlargements.Count + 2), (this.enlargements.Count + 2));

            /// Add the new vertical borders.
            this.AddVertical(-(this.currentRadius + amount), northCorner.Y, northCorner.X);
            this.AddVertical(-(this.currentRadius + amount), eastCorner.Y, eastCorner.X);
            this.AddVertical(westCorner.Y, this.currentRadius + amount, westCorner.X);
            this.AddVertical(southCorner.Y, this.currentRadius + amount, southCorner.X);

            /// Add the new horizontal borders.
            this.AddHorizontal(-(this.currentRadius + amount), northCorner.X, northCorner.Y);
            this.AddHorizontal(-(this.currentRadius + amount), westCorner.X, westCorner.Y);
            this.AddHorizontal(eastCorner.X, this.currentRadius + amount, eastCorner.Y);
            this.AddHorizontal(southCorner.X, this.currentRadius + amount, southCorner.Y);

            /// Add the new internal areas.
            this.AddArea(new RCIntRectangle(northCorner.X + 1, -(this.currentRadius + amount), eastCorner.X - northCorner.X - 1, amount));
            this.AddArea(new RCIntRectangle(westCorner.X + 1, this.currentRadius + 1, southCorner.X - westCorner.X - 1, amount));
            this.AddArea(new RCIntRectangle(-(this.currentRadius + amount), northCorner.Y + 1, amount, westCorner.Y - northCorner.Y - 1));
            this.AddArea(new RCIntRectangle(this.currentRadius + 1, eastCorner.Y + 1, amount, southCorner.Y - eastCorner.Y - 1));

            /// Save this enlargement and the corners.
            this.enlargements.Add(amount);
            this.currentRadius += amount;
            this.tmpFloodItems = null;

            this.northCorner = northCorner;
            this.eastCorner = eastCorner;
            this.westCorner = westCorner;
            this.southCorner = southCorner;
        }

        /// <summary>
        /// Reduces this flood with the amount of the last enlargement.
        /// </summary>
        public void Reduce()
        {
            if (this.enlargements.Count == 0) { throw new InvalidOperationException("FloodArea area has reached its minimum size!"); }

            /// Remove the last enlargement.
            int amount = this.enlargements[this.enlargements.Count - 1];
            this.enlargements.RemoveAt(this.enlargements.Count - 1);
            this.currentRadius -= amount;
            this.tmpFloodItems = null;

            /// Calculate the corners.
            RCIntVector northCorner = new RCIntVector(-(this.enlargements.Count + 2), -(this.enlargements.Count + 2));
            RCIntVector eastCorner = new RCIntVector((this.enlargements.Count + 2), -(this.enlargements.Count + 2));
            RCIntVector westCorner = new RCIntVector(-(this.enlargements.Count + 2), (this.enlargements.Count + 2));
            RCIntVector southCorner = new RCIntVector((this.enlargements.Count + 2), (this.enlargements.Count + 2));
            
            /// Remove the old vertical borders.
            this.RemoveVertical(-(this.currentRadius + amount), northCorner.Y, northCorner.X);
            this.RemoveVertical(-(this.currentRadius + amount), eastCorner.Y, eastCorner.X);
            this.RemoveVertical(westCorner.Y, this.currentRadius + amount, westCorner.X);
            this.RemoveVertical(southCorner.Y, this.currentRadius + amount, southCorner.X);

            /// Remove the old horizontal borders.
            this.RemoveHorizontal(-(this.currentRadius + amount), northCorner.X, northCorner.Y);
            this.RemoveHorizontal(-(this.currentRadius + amount), westCorner.X, westCorner.Y);
            this.RemoveHorizontal(eastCorner.X, this.currentRadius + amount, eastCorner.Y);
            this.RemoveHorizontal(southCorner.X, this.currentRadius + amount, southCorner.Y);

            /// Remove the old internal areas.
            this.RemoveArea(new RCIntRectangle(northCorner.X + 1, -(this.currentRadius + amount), eastCorner.X - northCorner.X - 1, amount));
            this.RemoveArea(new RCIntRectangle(westCorner.X + 1, this.currentRadius + 1, southCorner.X - westCorner.X - 1, amount));
            this.RemoveArea(new RCIntRectangle(-(this.currentRadius + amount), northCorner.Y + 1, amount, westCorner.Y - northCorner.Y - 1));
            this.RemoveArea(new RCIntRectangle(this.currentRadius + 1, eastCorner.Y + 1, amount, southCorner.Y - eastCorner.Y - 1));

            this.northCorner = new RCIntVector(-(this.enlargements.Count + 1), -(this.enlargements.Count + 1));
            this.eastCorner = new RCIntVector((this.enlargements.Count + 1), -(this.enlargements.Count + 1));
            this.westCorner = new RCIntVector(-(this.enlargements.Count + 1), (this.enlargements.Count + 1));
            this.southCorner = new RCIntVector((this.enlargements.Count + 1), (this.enlargements.Count + 1));
        }

        #endregion Public methods

        #region IEnumerable<FloodItem> members

        /// <see cref="IEnumerable&lt;T&gt;.GetEnumerator"/>
        public IEnumerator<FloodItem> GetEnumerator()
        {
            if (this.tmpFloodItems == null)
            {
                this.UpdateFloodItemList();
            }

            return this.tmpFloodItems.GetEnumerator();
        }

        #endregion IEnumerable<FloodItem> members

        #region IEnumerable Members

        /// <see cref="IEnumerable.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion IEnumerable Members

        #region Internal methods

        /// <summary>
        /// Adds the coordinates in a given horizontal section.
        /// </summary>
        private void AddHorizontal(int firstX, int lastX, int y)
        {
            if (firstX > lastX) { throw new ArgumentException("firstX must be less than or equal with lastX!"); }
            for (int x = firstX; x <= lastX; x++) { this.coordsInside.Add(new RCIntVector(x, y)); }
        }

        /// <summary>
        /// Adds the coordinates in a given vertical section.
        /// </summary>
        private void AddVertical(int firstY, int lastY, int x)
        {
            if (firstY > lastY) { throw new ArgumentException("firstY must be less than or equal with lastY!"); }
            for (int y = firstY; y <= lastY; y++) { this.coordsInside.Add(new RCIntVector(x, y)); }
        }

        /// <summary>
        /// Removes the coordinates in a given horizontal section.
        /// </summary>
        private void RemoveHorizontal(int firstX, int lastX, int y)
        {
            if (firstX > lastX) { throw new ArgumentException("firstX must be less than or equal with lastX!"); }
            for (int x = firstX; x <= lastX; x++) { this.coordsInside.Remove(new RCIntVector(x, y)); }
        }

        /// <summary>
        /// Removes the coordinates in a given vertical section.
        /// </summary>
        private void RemoveVertical(int firstY, int lastY, int x)
        {
            if (firstY > lastY) { throw new ArgumentException("firstY must be less than or equal with lastY!"); }
            for (int y = firstY; y <= lastY; y++) { this.coordsInside.Remove(new RCIntVector(x, y)); }
        }

        /// <summary>
        /// Adds the coordinates in the given area.
        /// </summary>
        private void AddArea(RCIntRectangle area)
        {
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }
            for (int row = 0; row < area.Height; row++) { this.AddHorizontal(area.Left, area.Right - 1, area.Y + row); }
        }

        /// <summary>
        /// Removes the coordinates in the given area.
        /// </summary>
        private void RemoveArea(RCIntRectangle area)
        {
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }
            for (int row = 0; row < area.Height; row++) { this.RemoveHorizontal(area.Left, area.Right - 1, area.Y + row); }
        }

        /// <summary>
        /// Updates the temporary list of FloodItems.
        /// </summary>
        private void UpdateFloodItemList()
        {
            this.tmpFloodItems = new List<FloodItem>();
            foreach (RCIntVector item in this.coordsInside)
            {
                if (item == this.northCorner)
                {
                    this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = this.currentRadius == this.enlargements.Count + 1 ? TerrainCombination.AABA : TerrainCombination.ABBB });
                }
                else if (item == this.eastCorner)
                {
                    this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = this.currentRadius == this.enlargements.Count + 1 ? TerrainCombination.AAAB : TerrainCombination.BABB });
                }
                else if (item == this.southCorner)
                {
                    this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = this.currentRadius == this.enlargements.Count + 1 ? TerrainCombination.BAAA : TerrainCombination.BBAB });
                }
                else if (item == this.westCorner)
                {
                    this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = this.currentRadius == this.enlargements.Count + 1 ? TerrainCombination.ABAA : TerrainCombination.BBBA });
                }
                else if (this.currentRadius == this.enlargements.Count + 1)
                {
                    if (item.Y == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AABB });
                    }
                    else if (item.Y == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BBAA });
                    }
                    else if (item.X == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.ABBA });
                    }
                    else if (item.X == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BAAB });
                    }
                    else
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.Simple });
                    }
                }
                else
                {
                    if (item.X == this.northCorner.X && item.Y < this.northCorner.Y && item.Y > -this.currentRadius ||
                        item.X == this.westCorner.X && item.Y > this.westCorner.Y && item.Y < this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.ABBA });
                    }
                    else if (item.X == this.eastCorner.X && item.Y < this.eastCorner.Y && item.Y > -this.currentRadius ||
                             item.X == this.southCorner.X && item.Y > this.southCorner.Y && item.Y < this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BAAB });
                    }
                    else if (item.Y == this.northCorner.Y && item.X < this.northCorner.X && item.X > -this.currentRadius ||
                             item.Y == this.eastCorner.Y && item.X > this.eastCorner.X && item.X < this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AABB });
                    }
                    else if (item.Y == this.westCorner.Y && item.X < this.westCorner.X && item.X > -this.currentRadius ||
                             item.Y == this.southCorner.Y && item.X > this.southCorner.X && item.X < this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BBAA });
                    }
                    else if (item.X == this.northCorner.X && item.Y == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AABA });
                    }
                    else if (item.X == this.eastCorner.X && item.Y == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AAAB });
                    }
                    else if (item.X == this.westCorner.X && item.Y == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.ABAA });
                    }
                    else if (item.X == this.southCorner.X && item.Y == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BAAA });
                    }
                    else if (item.Y == this.northCorner.Y && item.X == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AABA });
                    }
                    else if (item.Y == this.eastCorner.Y && item.X == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AAAB });
                    }
                    else if (item.Y == this.westCorner.Y && item.X == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.ABAA });
                    }
                    else if (item.Y == this.southCorner.Y && item.X == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BAAA });
                    }
                    else if (item.Y == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.AABB });
                    }
                    else if (item.Y == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BBAA });
                    }
                    else if (item.X == -this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.ABBA });
                    }
                    else if (item.X == this.currentRadius)
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.BAAB });
                    }
                    else
                    {
                        this.tmpFloodItems.Add(new FloodItem() { Coordinates = item, Combination = TerrainCombination.Simple });
                    }
                }
            }
        }

        #endregion Internal methods

        /// <summary>
        /// List of enlargement amounts.
        /// </summary>
        private List<int> enlargements;

        /// <summary>
        /// List of the coordinates of the isometric tiles inside the flood area relative to the center.
        /// </summary>
        private RCSet<RCIntVector> coordsInside;

        /// <summary>
        /// The current radius of the flood area.
        /// </summary>
        private int currentRadius;

        /// <summary>
        /// The corners of the flood area.
        /// </summary>
        RCIntVector northCorner;
        RCIntVector eastCorner;
        RCIntVector westCorner;
        RCIntVector southCorner;

        /// <summary>
        /// Temporary list of flood items.
        /// </summary>
        private List<FloodItem> tmpFloodItems;

        /// TODO: only for debugging
        public int CurrentRadius { get { return this.currentRadius; } }
    }

    /// <summary>
    /// Represents a tile in a FloodArea.
    /// </summary>
    public struct FloodItem // TODO: make private
    {
        /// <summary>
        /// The coordinates of the tile relative to the center of the FloodArea.
        /// </summary>
        public RCIntVector Coordinates;

        /// <summary>
        /// The terrain combination of the tile between terrain inside and outside the FloodArea.
        /// </summary>
        public TerrainCombination Combination;
    }
}
