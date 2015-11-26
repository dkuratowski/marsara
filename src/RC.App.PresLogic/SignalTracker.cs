using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Helper class for displaying values delayed on the UI.
    /// </summary>
    class SignalTracker
    {
        /// <summary>
        /// Constructs a SignalTracker instance.
        /// </summary>
        public SignalTracker()
        {
            this.lastKnownDelayedValue = 0;
            this.lastKnownTimepoint = 0;
            this.stepResponseStartTime = 0;
            this.stepResponseStartValue = 0;
            this.stepResponseEndValue = 0;
            this.stepResponse = null;
        }

        /// <summary>
        /// Gets the delayed value at the given timepoint produced by this tracker.
        /// </summary>
        /// <param name="t">The timepoint in millisecs.</param>
        /// <returns>The delayed value at the given timepoint.</returns>
        public int GetDelayedValue(int t)
        {
            if (t < this.lastKnownTimepoint) { throw new InvalidOperationException(string.Format("Unable to calculate the delayed value before timepoint {0}!", this.lastKnownTimepoint)); }

            this.lastKnownTimepoint = t;
            if (this.stepResponse == null)
            {
                /// Check if we have to calculate a step response or not.
                if (this.stepResponseEndValue != this.stepResponseStartValue)
                {
                    /// Calculate the new step response starting from t.
                    this.stepResponse = new StepResponse(Math.Abs(this.stepResponseEndValue - this.stepResponseStartValue));
                    this.stepResponseStartTime = t;
                }
                else
                {
                    /// We don't have to calculate step response.
                    this.lastKnownDelayedValue = this.stepResponseEndValue;
                    return this.stepResponseEndValue;
                }
            }

            /// Calculate the delayed value at t using the underlying step response.
            int delayedDeltaAbs = this.stepResponse.GetValue(t - this.stepResponseStartTime);
            this.lastKnownDelayedValue = this.stepResponseEndValue >= this.stepResponseStartValue
                                       ? this.stepResponseStartValue + delayedDeltaAbs
                                       : this.stepResponseStartValue - delayedDeltaAbs;
            return this.lastKnownDelayedValue;
        }

        /// <summary>
        /// Sets the value of the signal.
        /// </summary>
        /// <param name="signalValue">The value of the signal.</param>
        public void SetSignalValue(int signalValue)
        {
            if (signalValue != this.stepResponseEndValue)
            {
                /// We will have to recalculate the underlying step response when the GetDelayedValue method will be called next time.
                this.stepResponseStartValue = this.lastKnownDelayedValue;
                this.stepResponseEndValue = signalValue;
                this.stepResponseStartTime = this.lastKnownTimepoint;
                this.stepResponse = null;
            }
        }

        /// <summary>
        /// The delayed value that was calculated when the GetDelayedValue method was called last time.
        /// </summary>
        private int lastKnownDelayedValue;

        /// <summary>
        /// The timepoint that was given when the GetDelayedValue method was called last time.
        /// </summary>
        private int lastKnownTimepoint;

        /// <summary>
        /// The timepoint at the start of the actual step response.
        /// </summary>
        private int stepResponseStartTime;

        /// <summary>
        /// The value at the start of the actual step response.
        /// </summary>
        private int stepResponseStartValue;

        /// <summary>
        /// The value at the end of the actual step response.
        /// </summary>
        private int stepResponseEndValue;

        /// <summary>
        /// The actual step response.
        /// </summary>
        private StepResponse stepResponse;
    }
}
