using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Commands
{
    /// <summary>
    /// Implementation of the ICommandExecutor interface.
    /// </summary>
    [Component("RC.Engine.Simulator.CommandExecutor")]
    class CommandExecutor : ICommandExecutor, IPlayerInitializer, ICommandExecutorPluginInstall
    {
        /// <summary>
        /// Constructs a CommandExecutor instance.
        /// </summary>
        public CommandExecutor()
        {
            this.playerInitializers = new Dictionary<RaceEnum, Action<Player>>();
            this.commandExecutionFactories = new Dictionary<string, Dictionary<string, ICommandExecutionFactory>>();
        }

        #region ICommandExecutor methods

        /// <see cref="ICommandExecutor.GetCommandAvailability"/>
        public AvailabilityEnum GetCommandAvailability(Scenario scenario, string commandType, string commandParameter, IEnumerable<int> entityIDs)
        {
            /// Get all the existing entities from the scenario that are currently attached to the map.
            RCSet<Entity> entitySet = this.GetEntitiesOnMap(scenario, entityIDs);
            if (entitySet.Count == 0) { return AvailabilityEnum.Unavailable; }

            /// Select the factories to be used and get the availability of the command from the selected factories
            /// and combine the results.
            bool isAnyUnavailable = true;
            bool isAnyDisabled = false;
            foreach (ICommandExecutionFactory factory in this.SelectFactories(commandType, entitySet))
            {
                isAnyUnavailable = false;
                AvailabilityEnum availability = factory.GetCommandAvailability(entitySet, commandParameter);
                if (availability == AvailabilityEnum.Unavailable)
                {
                    /// If any of the factories says that the command execution is unavailable then the overall result is unavailable.
                    isAnyUnavailable = true;
                    break;
                }
                else if (availability == AvailabilityEnum.Disabled)
                {
                    /// If every factories says that the command execution is available but any of them says that its
                    /// currently disabled, then the overall result is disabled.
                    isAnyDisabled = true;
                }
            }

            /// Return the result based on the collected flags.
            if (isAnyUnavailable) { return AvailabilityEnum.Unavailable; }
            if (isAnyDisabled) { return AvailabilityEnum.Disabled; }
            return AvailabilityEnum.Enabled;
        }

        /// <see cref="ICommandExecutor.StartExecution"/>
        public void StartExecution(Scenario scenario, RCCommand command)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (command == null) { throw new ArgumentNullException("command"); }

            RCSet<Entity> entitySet = this.GetEntitiesOnMap(scenario, command.RecipientEntities);
            foreach (ICommandExecutionFactory factory in this.SelectFactories(command.CommandType, entitySet))
            {
                factory.StartCommandExecution(entitySet, command.TargetPosition, command.TargetEntity, command.Parameter);
            }
        }

        /// <see cref="ICommandExecutor.GetCommandsBeingExecuted"/>
        public RCSet<string> GetCommandsBeingExecuted(Scenario scenario, IEnumerable<int> entityIDs)
        {
            if (scenario == null) { throw new ArgumentNullException("scenario"); }

            RCSet<string> retList = new RCSet<string>();
            RCSet<Entity> entitySet = this.GetEntitiesOnMap(scenario, entityIDs);
            foreach (Entity entity in entitySet)
            {
                if (entity.CommandBeingExecuted != null) { retList.Add(entity.CommandBeingExecuted); }
            }
            return retList;
        }

        #endregion ICommandExecutor methods

        #region IPlayerInitializer methods

        /// <see cref="IPlayerInitializer.Initialize"/>
        public void Initialize(Player player, RaceEnum race)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (!this.playerInitializers.ContainsKey(race)) { throw new SimulatorException(string.Format("Player initializer not found for race '{0}'!", race)); }
            this.playerInitializers[race](player);
        }

        #endregion IPlayerInitializer methods

        #region ICommandExecutorPluginInstall methods

        /// <see cref="ICommandExecutorPluginInstall.RegisterPlayerInitializer"/>
        public void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer)
        {
            if (initializer == null) { throw new ArgumentNullException("initializer"); }
            if (this.playerInitializers.ContainsKey(race)) { throw new InvalidOperationException(string.Format("Player initializer has already been registered for race '{0}'!", race)); }
            this.playerInitializers[race] = initializer;
        }

        /// <see cref="ICommandExecutorPluginInstall.RegisterCommandExecutionFactory"/>
        public void RegisterCommandExecutionFactory(ICommandExecutionFactory factory)
        {
            if (factory == null) { throw new ArgumentNullException("factory"); }
            if (factory.CommandType != null && string.IsNullOrWhiteSpace(factory.CommandType)) { throw new ArgumentException("The command type of the factory shall be null or a non-empty string!", "factory"); }
            if (string.IsNullOrWhiteSpace(factory.EntityType)) { throw new ArgumentException("The entity type of the factory cannot be null or whitespace!", "factory"); }

            this.RegisterCommandExecutionFactoryImpl(factory.CommandType ?? string.Empty, factory.EntityType, factory);
        }

        #endregion ICommandExecutorPluginInstall methods

        #region Internal methods

        /// <summary>
        /// The internal implementation of the RegisterCommandExecutionFactory method.
        /// </summary>
        private void RegisterCommandExecutionFactoryImpl(string commandType, string entityType, ICommandExecutionFactory factory)
        {
            /// Create a new group of command execution factories for the given command type if doesn't exist.
            if (!this.commandExecutionFactories.ContainsKey(commandType))
            {
                this.commandExecutionFactories[commandType] = new Dictionary<string, ICommandExecutionFactory>();
            }

            /// Check if another factory has already been registered for the given command and entity type.
            if (this.commandExecutionFactories[commandType].ContainsKey(entityType))
            {
                throw new InvalidOperationException(string.Format("Command execution factory for command type '{0}' and entity type '{1}' has already been registered!", commandType, entityType));
            }

            /// Register the new factory.
            this.commandExecutionFactories[commandType][entityType] = factory;
        }

        /// <summary>
        /// Gets all the existing entities that are currently attached to the map.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <param name="entityIDs">The IDs of the entities to get.</param>
        /// <returns>All the existing entities that are currently attached to the map.</returns>
        private RCSet<Entity> GetEntitiesOnMap(Scenario scenario, IEnumerable<int> entityIDs)
        {
            RCSet<Entity> entitySet = new RCSet<Entity>();
            foreach (int entityId in entityIDs)
            {
                Entity entity = scenario.GetElementOnMap<Entity>(entityId);
                if (entity != null) { entitySet.Add(entity); }
            }
            return entitySet;
        }

        /// <summary>
        /// Selects the command execution factories to be used for the given entity set.
        /// </summary>
        /// <param name="commandType">The type of the command.</param>
        /// <param name="entitySet">The set of entities.</param>
        /// <returns>
        /// The list of the command execution factories to be used or an empty list if there is at least 1 entity that is not
        /// able to execute the given command.
        /// </returns>
        private IEnumerable<ICommandExecutionFactory> SelectFactories(string commandType, IEnumerable<Entity> entitySet)
        {
            if (!this.commandExecutionFactories.ContainsKey(commandType)) { return new RCSet<ICommandExecutionFactory>(); }

            Dictionary<string, ICommandExecutionFactory> factoriesPerTypes = this.commandExecutionFactories[commandType];
            RCSet<ICommandExecutionFactory> retList = new RCSet<ICommandExecutionFactory>();
            foreach (Entity entity in entitySet)
            {
                if (!factoriesPerTypes.ContainsKey(entity.ElementType.Name)) { return new RCSet<ICommandExecutionFactory>(); }
                ICommandExecutionFactory factory = factoriesPerTypes[entity.ElementType.Name];
                retList.Add(factory);
            }
            return retList;
        }

        #endregion Internal methods

        /// <summary>
        /// List of the registered player initializers mapped by the corresponding races.
        /// </summary>
        private readonly Dictionary<RaceEnum, Action<Player>> playerInitializers;

        /// <summary>
        /// List of the command execution factories registered by the engine plugins mapped by the corresponding
        /// command and entity types.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, ICommandExecutionFactory>> commandExecutionFactories;
    }
}
