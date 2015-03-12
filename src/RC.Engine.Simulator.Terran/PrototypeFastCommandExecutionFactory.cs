using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Scenarios;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran
{
    /// TODO: this is only a temprary solution for testing motion control.
    class PrototypeFastCommandExecutionFactory : CommandExecutionFactoryBase<SCV>
    {
        public PrototypeFastCommandExecutionFactory() : base(null, SCV.SCV_TYPE_NAME)
        {
        }

        protected override AvailabilityEnum GetCommandAvailability(HashSet<SCV> entitiesToHandle, HashSet<Entity> fullEntitySet)
        {
            throw new NotImplementedException();
        }

        protected override void StartCommandExecution(HashSet<SCV> entitiesToHandle, HashSet<Entity> fullEntitySet, RCNumVector targetPosition,
            Entity targetEntity, string parameter)
        {
            MagicBox magicBox = new MagicBox(fullEntitySet, targetPosition);
            foreach (SCV scv in entitiesToHandle) { scv.Move(magicBox.GetTargetPosition(scv)); }
        }
    }
}
