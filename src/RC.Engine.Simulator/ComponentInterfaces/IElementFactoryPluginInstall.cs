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
    /// This interface is used by the plugins of the element factory to install themselves.
    /// </summary>
    [PluginInstallInterface]
    public interface IElementFactoryPluginInstall
    {
        /// <summary>
        /// Registers a player initializer method for the given race.
        /// </summary>
        /// <param name="race">The race that the initializer belongs to.</param>
        /// <param name="initializer">The initializer method.</param>
        void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer);

        /// <summary>
        /// Registers the given factory method for the given element type.
        /// </summary>
        /// <param name="typeName">The name of the element type.</param>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given element type has already been registered.
        /// </exception>
        void RegisterElementFactory(string typeName, Func<bool> factoryMethod);

        /// <summary>
        /// Registers the given factory method for the given element type.
        /// </summary>
        /// <typeparam name="TParam">The type of the input parameter of the factory method.</typeparam>
        /// <param name="typeName">The name of the element type.</param>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given element type has already been registered.
        /// </exception>
        void RegisterElementFactory<TParam>(string typeName, Func<TParam, bool> factoryMethod);

        /// <summary>
        /// Registers the given factory method for the given element type.
        /// </summary>
        /// <typeparam name="TParam0">The type of the first input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter of the factory method.</typeparam>
        /// <param name="typeName">The name of the element type.</param>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given element type has already been registered.
        /// </exception>
        void RegisterElementFactory<TParam0, TParam1>(string typeName, Func<TParam0, TParam1, bool> factoryMethod);

        /// <summary>
        /// Registers the given factory method for the given element type.
        /// </summary>
        /// <typeparam name="TParam0">The type of the first input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam2">The type of the third input parameter of the factory method.</typeparam>
        /// <param name="typeName">The name of the element type.</param>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given element type has already been registered.
        /// </exception>
        void RegisterElementFactory<TParam0, TParam1, TParam2>(string typeName, Func<TParam0, TParam1, TParam2, bool> factoryMethod);
    }
}
