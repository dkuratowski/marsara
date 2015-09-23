using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// This interface is used by the plugins of the entity factory to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface IEntityFactoryPluginInstall
    {
        /// <summary>
        /// Registers a player initializer method for the given race.
        /// </summary>
        /// <param name="race">The race that the initializer belongs to.</param>
        /// <param name="initializer">The initializer method.</param>
        void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer);

        /// <summary>
        /// Registers an entity creator method for the given type.
        /// </summary>
        /// <param name="typeName">The name of the type of the entity to create.</param>
        /// <param name="creator">The creator method that returns true if the creation was successful; otherwise false.</param>
        void RegisterEntityCreator(string typeName, Func<Player, Entity, bool> creator);
    }
}
