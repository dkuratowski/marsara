using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Engine.Behaviors;

namespace RC.Engine.Simulator.Terran.Units
{
    /// <summary>
    /// Represents a Terran Goliath.
    /// </summary>
    class Goliath : Unit
    {
        /// <summary>
        /// Constructs a Terran Goliath instance.
        /// </summary>
        public Goliath()
            : base(GOLIATH_TYPE_NAME, false, new BasicAnimationsBehavior("Walking", "Standing", "Standing"))
        {
        }

        /// <see cref="Entity.DestructionAnimationName"/>
        protected override string DestructionAnimationName { get { return "Dying"; } }

        /// <summary>
        /// The name of the Goliath element type.
        /// </summary>
        public const string GOLIATH_TYPE_NAME = "Goliath";
    }
}
