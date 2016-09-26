using RC.Engine.Simulator.Behaviors;
using RC.Engine.Simulator.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran Wraith.
    /// </summary>
    class Wraith : Unit
    {
        /// <summary>
        /// Constructs a Terran Wraith instance.
        /// </summary>
        public Wraith()
            : base(WRAITH_TYPE_NAME, true, new BasicAnimationsBehavior("Moving", "Attacking", "Stopped"))
        {
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <summary>
        /// The name of the Wraith element type.
        /// </summary>
        public const string WRAITH_TYPE_NAME = "Wraith";
    }
}
