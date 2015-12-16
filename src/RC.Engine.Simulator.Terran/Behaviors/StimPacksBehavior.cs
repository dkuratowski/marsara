using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Engine.Behaviors;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.Behaviors
{
    /// <summary>
    /// Applies the effect of the StimPacks for the given entity.
    /// </summary>
    class StimPacksBehavior : EntityBehavior
    {
        /// <summary>
        /// Constructs an AddonBehavior instance.
        /// </summary>
        public StimPacksBehavior()
        {
            this.dummyField = this.ConstructField<byte>("dummyField");
        }

        #region Overrides

        /// <see cref="EntityBehavior.UpdateState"/>
        public override void UpdateState(Entity entity)
        {
            Marine entityAsMarine = entity as Marine;
            if (entityAsMarine != null)
            {
                entityAsMarine.UpdateStimPacksStatus();
                return;
            }

            /// TODO: handle other entity types here!
        }

        #endregion Overrides

        /// <summary>
        /// Dummy heaped value because we are deriving from HeapedObject.
        /// TODO: change HeapedObject to be possible to derive from it without heaped values.
        /// </summary>
        private HeapedValue<byte> dummyField;
    }
}
