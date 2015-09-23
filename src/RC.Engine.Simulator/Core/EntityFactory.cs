using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// The implementation of the entity factory component.
    /// </summary>
    [Component("RC.Engine.Simulator.EntityFactory")]
    class EntityFactory : IEntityFactory, IEntityFactoryPluginInstall
    {
        /// <summary>
        /// Constructs an EntityFactory instance.
        /// </summary>
        public EntityFactory()
        {
            this.playerInitializers = new Dictionary<RaceEnum, Action<Player>>();
            this.entityCreators = new Dictionary<string, Func<Player, Entity, bool>>();
        }

        #region IEntityFactory methods

        /// <see cref="IEntityFactory.InitializePlayer"/>
        public void InitializePlayer(Player player, RaceEnum race)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (!this.playerInitializers.ContainsKey(race)) { throw new SimulatorException(string.Format("Player initializer not found for race '{0}'!", race)); }
            this.playerInitializers[race](player);
        }

        /// <see cref="IEntityFactory.CreateEntity"/>
        public bool CreateEntity(IScenarioElementType entityType, Player player, Entity producer)
        {
            if (entityType == null) { throw new ArgumentNullException("entityType"); }
            if (player == null) { throw new ArgumentNullException("player"); }
            if (producer == null) { throw new ArgumentNullException("producer"); }
            if (!this.entityCreators.ContainsKey(entityType.Name)) { throw new SimulatorException(string.Format("Entity creator method not found for type '{0}'!", entityType.Name)); }

            return this.entityCreators[entityType.Name](player, producer);
        }

        #endregion IEntityFactory methods

        #region IEntityFactoryPluginInstall methods

        /// <see cref="IEntityFactoryPluginInstall.RegisterPlayerInitializer"/>
        public void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer)
        {
            if (initializer == null) { throw new ArgumentNullException("initializer"); }
            if (this.playerInitializers.ContainsKey(race)) { throw new InvalidOperationException(string.Format("Player initializer has already been registered for race '{0}'!", race)); }
            this.playerInitializers[race] = initializer;
        }

        /// <see cref="IEntityFactoryPluginInstall.RegisterEntityCreator"/>
        public void RegisterEntityCreator(string typeName, Func<Player, Entity, bool> creator)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (creator == null) { throw new ArgumentNullException("creator"); }
            if (this.entityCreators.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Entity creator method has already been registered for type '{0}'!", typeName)); }
            this.entityCreators[typeName] = creator;
        }

        #endregion IEntityFactoryPluginInstall methods

        /// <summary>
        /// List of the registered player initializers mapped by the corresponding races.
        /// </summary>
        private readonly Dictionary<RaceEnum, Action<Player>> playerInitializers;

        /// <summary>
        /// List of the registered entity creator methods mapped by the corresponding types.
        /// </summary>
        private readonly Dictionary<string, Func<Player, Entity, bool>> entityCreators;
    }
}
