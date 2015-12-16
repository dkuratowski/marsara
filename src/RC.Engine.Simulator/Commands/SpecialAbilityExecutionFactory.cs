using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Factory for special ability command executions.
    /// </summary>
    public class SpecialAbilityExecutionFactory<T> : CommandExecutionFactoryBase<T> where T : Entity
    {
        /// <summary>
        /// Constructs a SpecialAbilityExecutionFactory instance.
        /// </summary>
        /// <param name="specialAbility">The name of the special ability that is necessary for the command.</param>
        /// <param name="entityType">The type of the recipient entities.</param>
        public SpecialAbilityExecutionFactory(string specialAbility, string entityType,
            Func<RCSet<T>, RCNumVector, int, string, IEnumerable<CmdExecutionBase>> factoryMethod)
            : base(specialAbility, entityType)
        {
            if (factoryMethod == null) { throw new ArgumentNullException("factoryMethod"); }
            this.factoryMethod = factoryMethod;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override AvailabilityEnum GetCommandAvailability(RCSet<T> entitiesToHandle, RCSet<Entity> fullEntitySet, string parameter)
        {
            /// Check if all the entities to handle are owned by the same player.
            Player owner = entitiesToHandle.First().Owner;
            if (entitiesToHandle.Any(entity => entity.Owner != owner)) { throw new InvalidOperationException("Entities with different player sent to SpecialAbilityExecutionFactory!"); }

            /// If the entities to handle are neutral -> the command is unavailable.
            if (owner == null) { return AvailabilityEnum.Unavailable; }

            /// Enable the special ability execution if the owner player has the necessary research.
            return owner.GetUpgradeStatus(this.CommandType) == UpgradeStatus.Researched ? AvailabilityEnum.Enabled : AvailabilityEnum.Disabled;
        }

        /// <see cref="CommandExecutionFactoryBase.GetCommandAvailability"/>
        protected override IEnumerable<CmdExecutionBase> CreateCommandExecutions(RCSet<T> entitiesToHandle, RCSet<Entity> fullEntitySet, RCNumVector targetPosition, int targetEntityID, string parameter)
        {
            return this.factoryMethod(entitiesToHandle, targetPosition, targetEntityID, parameter);
        }

        /// <summary>
        /// Reference to the factory method that will create the command executions for the special ability.
        /// </summary>
        private readonly Func<RCSet<T>, RCNumVector, int, string, IEnumerable<CmdExecutionBase>> factoryMethod;
    }
}
