using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.Core
{
    /// <summary>
    /// Implements a min-heap that can be used as the priority queue of an A* pathfinding algorithm.
    /// </summary>
    /// <typeparam name="TNode">The type of the nodes of the graph on which the pathfinding is searching.</typeparam>
    class PriorityQueue<TNode>
    {
        /// <summary>
        /// Constructs a PriorityQueue instance.
        /// </summary>
        public PriorityQueue()
        {
            this.itemCount = 0;
            this.heapArray = new List<PathNode<TNode>>();
        }

        /// <summary>
        /// Inserts a new pathnode into the heap.
        /// </summary>
        /// <param name="pathNode">The pathnode to insert.</param>
        public void Insert(PathNode<TNode> pathNode)
        {
            /// Add the new pathnode to the bottom level of the heap.
            this.heapArray.Add(pathNode);

            /// Restore the heap-property if necessary.
            this.Upheap(this.itemCount);

            /// Increment the nextIndex pointer to the end of the heap array.
            this.itemCount++;
        }

        /// <summary>
        /// Deletes the item at the top of the heap.
        /// </summary>
        public void DeleteTopItem()
        {
            if (this.itemCount == 0) { throw new InvalidOperationException("The heap is empty!"); }

            /// If we have only 1 more element left, just clear the array and return.
            if (this.itemCount == 1)
            {
                this.heapArray.Clear();
                this.itemCount = 0;
                return;
            }

            /// Replace the root of the heap with the last element.
            PathNode<TNode> tmp = this.heapArray[this.itemCount - 1];
            this.heapArray.RemoveAt(this.itemCount - 1);
            this.heapArray[0] = tmp;

            /// Decrement the nextIndex pointer to the end of the heap array.
            this.itemCount--;

            /// Restore the heap-property if necessary.
            this.Downheap(0);
        }

        /// <summary>
        /// Gets the item at the top of the heap.
        /// </summary>
        public PathNode<TNode> TopItem
        {
            get
            {
                if (this.itemCount == 0) { throw new InvalidOperationException("The heap is empty!"); }
                return this.heapArray[0];
            }
        }

        /// <summary>
        /// Gets the number of items in the heap.
        /// </summary>
        public int Count { get { return this.itemCount; } }

        /// <summary>
        /// Internal method to restore the heap-property after an Insert operation.
        /// </summary>
        private void Upheap(int fromIdx)
        {
            int currIdx = fromIdx;
            while (currIdx != 0)
            {
                int parentIdx = (currIdx - 1) / 2;
                if (this.heapArray[currIdx].Priority < this.heapArray[parentIdx].Priority)
                {
                    /// Swap the current item with its parent.
                    this.SwapItems(parentIdx, currIdx);

                    /// Go to the parent item and continue the upheap.
                    currIdx = parentIdx;
                }
                else
                {
                    /// The heap-property is now OK.
                    break;
                }
            }
        }

        /// <summary>
        /// Internal method to restore the heap-property after a DeleteTop operation.
        /// </summary>
        private void Downheap(int fromIdx)
        {
            /// If the heap is empty, we have nothing to do
            if (this.itemCount == 0) { return; }

            int currIdx = fromIdx;
            while (true)
            {
                int leftIdx = 2 * currIdx + 1;
                int rightIdx = 2 * currIdx + 2;

                /// If there is not even a left child, the heap-property is OK.
                if (leftIdx >= this.itemCount) { break; }

                /// If there is no right child, we only have to compare with the left.
                if (rightIdx >= this.itemCount)
                {
                    if (this.heapArray[leftIdx].Priority < this.heapArray[currIdx].Priority)
                    {
                        /// Swap the current item with its left child.
                        this.SwapItems(leftIdx, currIdx);

                        currIdx = leftIdx;
                        continue;
                    }
                    else
                    {
                        /// The heap-property is now OK.
                        break;
                    }
                }

                /// If there is a right child, we have to compare with both.
                if (this.heapArray[leftIdx].Priority < this.heapArray[currIdx].Priority && this.heapArray[rightIdx].Priority < this.heapArray[currIdx].Priority)
                {
                    /// If the heap-property is wrong with both of the children, we have to swap the current item with its smaller child.
                    int childIdx = this.heapArray[leftIdx].Priority > this.heapArray[rightIdx].Priority ? rightIdx : leftIdx;
                    this.SwapItems(childIdx, currIdx);

                    currIdx = childIdx;
                    continue;
                }
                else if (this.heapArray[leftIdx].Priority < this.heapArray[currIdx].Priority && this.heapArray[rightIdx].Priority >= this.heapArray[currIdx].Priority)
                {
                    /// If the heap-property is wrong only with the left child, we have to swap the current item with the left child.
                    this.SwapItems(leftIdx, currIdx);

                    currIdx = leftIdx;
                    continue;
                }
                else if (this.heapArray[leftIdx].Priority >= this.heapArray[currIdx].Priority && this.heapArray[rightIdx].Priority < this.heapArray[currIdx].Priority)
                {
                    /// If the heap-property is wrong only with the right child, we have to swap the current item with the right child.
                    this.SwapItems(rightIdx, currIdx);

                    currIdx = rightIdx;
                    continue;
                }
                else
                {
                    /// The heap-property is now OK.
                    break;
                }
            }
        }

        /// <summary>
        /// Swaps two items in the underlying heap array.
        /// </summary>
        /// <param name="idxA">The index of the first item.</param>
        /// <param name="idxB">The index of the second item.</param>
        private void SwapItems(int idxA, int idxB)
        {
            PathNode<TNode> tmp = this.heapArray[idxA];
            this.heapArray[idxA] = this.heapArray[idxB];
            this.heapArray[idxB] = tmp;
        }

        /// <summary>
        /// The underlying array that stores the items of the heap.
        /// </summary>
        private readonly List<PathNode<TNode>> heapArray;

        /// <summary>
        /// The number of items in the heap array.
        /// </summary>
        private int itemCount;
    }
}
