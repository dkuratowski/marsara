using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// Contains event data of update procedures.
    /// </summary>
    public class UIUpdateSystemEventArgs : UIEventArgs
    {
        /// <summary>
        /// Constructs a UIUpdateSystemEventArgs object.
        /// </summary>
        /// <param name="timeSinceLastUpdate">The elapsed time since update in the previous frame in milliseconds.</param>
        /// <param name="timeSinceStart">The elapsed time since the start of the render loop in milliseconds.</param>
        public UIUpdateSystemEventArgs(int timeSinceLastUpdate, int timeSinceStart)
        {
            if (timeSinceLastUpdate < 0) { throw new ArgumentOutOfRangeException("timeSinceLastUpdate"); }
            if (timeSinceStart < 0) { throw new ArgumentOutOfRangeException("timeSinceStart"); }

            this.timeSinceLastUpdate = timeSinceLastUpdate;
            this.timeSinceStart = timeSinceStart;
        }

        /// <summary>
        /// Gets the elapsed time since update in the previous frame in milliseconds.
        /// </summary>
        public int TimeSinceLastUpdate { get { return this.timeSinceLastUpdate; } }

        /// <summary>
        /// Gets the elapsed time since the start of the render loop in milliseconds.
        /// </summary>
        public int TimeSinceStart { get { return this.timeSinceStart; } }

        /// <summary>
        /// The elapsed time since update in the previous frame in milliseconds.
        /// </summary>
        private int timeSinceLastUpdate;

        /// <summary>
        /// The elapsed time since the start of the render loop in milliseconds.
        /// </summary>
        private int timeSinceStart;
    }
}
