using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// This is a helper class that can be used to calculate the average of the last N items
    /// of a series of integer numbers.
    /// </summary>
    public class AverageCalculator
    {
        /// <summary>
        /// Constructs an average calculator object.
        /// </summary>
        /// <param name="n">
        /// The number of the items to compute the average value from.
        /// </param>
        public AverageCalculator(int N, int initialValue)
        {
            if (N < 1) { throw new Exception("AverageCalculator cannot be initialized with an empty array!"); }

            this.items = new int[N];
            for (int i = 0; i < N; ++i)
            {
                this.items[i] = initialValue;
            }
            this.average = initialValue;
            this.N = N;
            this.writeIdx = 0;
            this.averageDirty = false;
        }

        /// <summary>
        /// Overwrites the oldest item in this.items with the item given in the parameter.
        /// </summary>
        /// <param name="item">The new item that will be inserted to this.items.</param>
        public void NewItem(int item)
        {
            this.items[this.writeIdx] = item;
            this.writeIdx++;
            if (this.writeIdx == this.N) { this.writeIdx = 0; }
            this.averageDirty = true;
        }

        /// <summary>
        /// Gets the computed average value.
        /// </summary>
        public int Average
        {
            get
            {
                if (this.averageDirty)
                {
                    this.average = ComputeAverage();
                    this.averageDirty = false;
                }
                return this.average;
            }
        }

        /// <summary>
        /// Gets the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            string retStr = "items: ";
            for (int i = 0; i < this.N; ++i)
            {
                retStr += this.items[i];
                if (i < N - 1) { retStr += ", "; }
            }
            retStr += " average: " + this.Average;
            return retStr;
        }

        /// <summary>
        /// Computes the average of this.items.
        /// </summary>
        /// <returns>The computed average.</returns>
        private int ComputeAverage()
        {
            int summa = 0;
            for (int i = 0; i < N; ++i)
            {
                summa += this.items[i];
            }
            return summa / N;
        }

        /// <summary>
        /// The last N items of the input series.
        /// </summary>
        private int[] items;

        /// <summary>
        /// Index of the item in this.items that will be overwritten next time when NewItem() is called.
        /// </summary>
        private int writeIdx;

        /// <summary>
        /// The length of this.items.
        /// </summary>
        private int N;

        /// <summary>
        /// This flag indicates whether the value of this.currentAverage is out of date.
        /// </summary>
        private bool averageDirty;

        /// <summary>
        /// The currently computed average value.
        /// </summary>
        private int average;
    }
}
