using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Helper class for calculating a step response function for displaying values delayed on the UI.
    /// </summary>
    class StepResponse
    {
        /// <summary>
        /// Constructs a StepResponse instance with the given delta value.
        /// </summary>
        /// <param name="delta">The delta value of the tracked original function.</param>
        public StepResponse(int delta)
        {
            if (delta <= 0) { throw new ArgumentOutOfRangeException("delta", "The delta value shall be greater than 0!"); }

            /// Calculate the initial speed and the step response finish time.
            this.delta = delta;
            double v0 = -1;
            if (delta <= DELTA_MIN)
            {
                /// Trivial case A
                v0 = 2 * delta / T_MIN;
                this.t0 = T_MIN;
            }
            else if (delta >= DELTA_MAX)
            {
                /// Trivial case B
                v0 = 2 * delta /T_MAX;
                this.t0 = T_MAX;
            }
            else
            {
                /// Normal case -> we have to solve a second degree polynomial equation.
                double a = V_MAX - V_MIN;
                double b = T_MAX * V_MIN - V_MAX * T_MIN;
                double c = 2 * delta * (T_MIN - T_MAX);
                double b2_4ac_sqrt = Math.Sqrt(b * b - 4 * a * c);
                double t0_1 = (-b + b2_4ac_sqrt) / (2 * a);
                this.t0 = t0_1 >= 0 ? t0_1 : (-b - b2_4ac_sqrt) / (2 * a);
                v0 = 2 * delta / this.t0;
            }

            /// Calculate the coefficients of the step response function.
            this.coefficientA = -v0 / (2 * this.t0);
            this.coefficientB = v0;
        }

        /// <summary>
        /// Gets the value of this step response function at the given timepoint.
        /// </summary>
        /// <param name="t">The timepoint given in millisecs.</param>
        /// <returns>The value of this step response function at the given timepoint.</returns>
        public int GetValue(int t)
        {
            if (t < 0) { throw new ArgumentOutOfRangeException("t", "Unable to calculate the value of the function before timepoint 0!"); }
            if (t >= this.t0) { return (int)this.delta; }
            return (int)((this.coefficientA * t + coefficientB) * t);
        }

        /// <summary>
        /// The coefficients A and B of the step response function s(t) = At^2 + Bt.
        /// </summary>
        private readonly double coefficientA;
        private readonly double coefficientB;

        /// <summary>
        /// The step response time in millisecs.
        /// </summary>
        private readonly double t0;

        /// <summary>
        /// The delta value of the tracked original function.
        /// </summary>
        private readonly double delta;

        /// <summary>
        /// The minimum step response time in millisecs.
        /// </summary>
        private static readonly double T_MIN = 200;

        /// <summary>
        /// The maximum step response time in millisecs.
        /// </summary>
        private static readonly double T_MAX = 3000;

        /// <summary>
        /// The delta value of the tracked original function that corresponds to the minimum time.
        /// </summary>
        private static readonly double DELTA_MIN = 10;

        /// <summary>
        /// The delta value of the tracked original function that corresponds to the maximum time.
        /// </summary>
        private static readonly double DELTA_MAX = 3000;

        /// <summary>
        /// The initial velocity that corresponds to the maximum time.
        /// </summary>
        private static readonly double V_MAX = 2 * DELTA_MAX / T_MAX;

        /// <summary>
        /// The initial velocity that corresponds to the minimum time.
        /// </summary>
        private static readonly double V_MIN = 2 * DELTA_MIN / T_MIN;
    }
}
