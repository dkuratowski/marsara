using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// Defines constants and methods for the special abilities of the Terran race.
    /// </summary>
    static class TerranAbilities
    {
        /// <summary>
        /// Command execution factory method for applying StimPacks for Terran Marines.
        /// </summary>
        /// <param name="recipientMarines">The recipient marines of the StimPacks.</param>
        /// <returns>
        /// This method only activates the StimPacks for the Marines and always returns an empty list because we don't want
        /// to interrupt the current command execution.
        /// </returns>
        public static IEnumerable<CmdExecutionBase> MarineStimPacksFactoryMethod(RCSet<Marine> recipientMarines)
        {
            foreach (Marine recipientMarine in recipientMarines) { recipientMarine.ActivateStimPacks(); }
            return new List<CmdExecutionBase>();
        }

        /// <summary>
        /// Constants for the StimPacks ability.
        /// </summary>
        public const string STIMPACKS = "StimPacks";
        public const int STIMPACKS_DAMAGE = 10;
        public const int STIMPACKS_TIME = 300;
        public static readonly RCNumber STIMPACKS_SPEED_UPGRADE = (RCNumber)1 / (RCNumber)4;
        public const int STIMPACKS_COOLDOWN_UPGRADE = -7;

        /// <summary>
        /// Constants for the SiegeTech ability.
        /// </summary>
        public const string SIEGETECH = "SiegeTech";
    }
}
