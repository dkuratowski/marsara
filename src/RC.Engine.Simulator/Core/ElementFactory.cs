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
    /// The implementation of the element factory component.
    /// </summary>
    [Component("RC.Engine.Simulator.ElementFactory")]
    class ElementFactory : IElementFactory, IElementFactoryPluginInstall
    {
        /// <summary>
        /// Constructs an EntityFactory instance.
        /// </summary>
        public ElementFactory()
        {
            this.playerInitializers = new Dictionary<RaceEnum, Action<Player>>();
            this.factoryMethods = new Dictionary<string, Delegate>();
        }

        #region IElementFactory methods

        /// <see cref="IElementFactory.InitializePlayer"/>
        public void InitializePlayer(Player player, RaceEnum race)
        {
            if (player == null) { throw new ArgumentNullException("player"); }
            if (!this.playerInitializers.ContainsKey(race)) { throw new SimulatorException(string.Format("Player initializer not found for race '{0}'!", race)); }
            this.playerInitializers[race](player);
        }

        /// <see cref="IElementFactory.CreateElement"/>
        public bool CreateElement(string typeName)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.factoryMethods.ContainsKey(typeName)) { throw new SimulatorException(string.Format("Factory method not found for scenario element type '{0}'!", typeName)); }

            Func<bool> factoryMethod = this.factoryMethods[typeName] as Func<bool>;
            if (factoryMethod == null) { throw new InvalidOperationException("Factory method parameter list mismatch!"); }

            return factoryMethod();
        }

        /// <see cref="IElementFactory.CreateElement"/>
        public bool CreateElement<TParam>(string typeName, TParam param)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.factoryMethods.ContainsKey(typeName)) { throw new SimulatorException(string.Format("Factory method not found for scenario element type '{0}'!", typeName)); }

            Func<TParam, bool> factoryMethod = this.factoryMethods[typeName] as Func<TParam, bool>;
            if (factoryMethod == null) { throw new InvalidOperationException("Factory method parameter list mismatch!"); }

            return factoryMethod(param);
        }

        /// <see cref="IElementFactory.CreateElement"/>
        public bool CreateElement<TParam0, TParam1>(string typeName, TParam0 param0, TParam1 param1)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.factoryMethods.ContainsKey(typeName)) { throw new SimulatorException(string.Format("Factory method not found for scenario element type '{0}'!", typeName)); }

            Func<TParam0, TParam1, bool> factoryMethod = this.factoryMethods[typeName] as Func<TParam0, TParam1, bool>;
            if (factoryMethod == null) { throw new InvalidOperationException("Factory method parameter list mismatch!"); }

            return factoryMethod(param0, param1);
        }

        /// <see cref="IElementFactory.CreateElement"/>
        public bool CreateElement<TParam0, TParam1, TParam2>(string typeName, TParam0 param0, TParam1 param1, TParam2 param2)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (!this.factoryMethods.ContainsKey(typeName)) { throw new SimulatorException(string.Format("Factory method not found for scenario element type '{0}'!", typeName)); }

            Func<TParam0, TParam1, TParam2, bool> factoryMethod = this.factoryMethods[typeName] as Func<TParam0, TParam1, TParam2, bool>;
            if (factoryMethod == null) { throw new InvalidOperationException("Factory method parameter list mismatch!"); }

            return factoryMethod(param0, param1, param2);
        }

        #endregion IElementFactory methods

        #region IElementFactoryPluginInstall methods

        /// <see cref="IElementFactoryPluginInstall.RegisterPlayerInitializer"/>
        public void RegisterPlayerInitializer(RaceEnum race, Action<Player> initializer)
        {
            if (initializer == null) { throw new ArgumentNullException("initializer"); }
            if (this.playerInitializers.ContainsKey(race)) { throw new InvalidOperationException(string.Format("Player initializer has already been registered for race '{0}'!", race)); }
            this.playerInitializers[race] = initializer;
        }

        /// <see cref="IElementFactoryPluginInstall.RegisterElementFactory"/>
        public void RegisterElementFactory(string typeName, Func<bool> creator)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (creator == null) { throw new ArgumentNullException("creator"); }
            if (this.factoryMethods.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Factory method has already been registered for scenario element type '{0}'!", typeName)); }

            this.factoryMethods[typeName] = creator;
        }

        /// <see cref="IElementFactoryPluginInstall.RegisterElementFactory"/>
        public void RegisterElementFactory<TParam>(string typeName, Func<TParam, bool> creator)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (creator == null) { throw new ArgumentNullException("creator"); }
            if (this.factoryMethods.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Factory method has already been registered for scenario element type '{0}'!", typeName)); }

            this.factoryMethods[typeName] = creator;
        }

        /// <see cref="IElementFactoryPluginInstall.RegisterElementFactory"/>
        public void RegisterElementFactory<TParam0, TParam1>(string typeName, Func<TParam0, TParam1, bool> creator)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (creator == null) { throw new ArgumentNullException("creator"); }
            if (this.factoryMethods.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Factory method has already been registered for scenario element type '{0}'!", typeName)); }

            this.factoryMethods[typeName] = creator;
        }

        /// <see cref="IElementFactoryPluginInstall.RegisterElementFactory"/>
        public void RegisterElementFactory<TParam0, TParam1, TParam2>(string typeName, Func<TParam0, TParam1, TParam2, bool> creator)
        {
            if (typeName == null) { throw new ArgumentNullException("typeName"); }
            if (creator == null) { throw new ArgumentNullException("creator"); }
            if (this.factoryMethods.ContainsKey(typeName)) { throw new InvalidOperationException(string.Format("Factory method has already been registered for scenario element type '{0}'!", typeName)); }

            this.factoryMethods[typeName] = creator;
        }

        #endregion IElementFactoryPluginInstall methods

        /// <summary>
        /// List of the registered player initializers mapped by the corresponding races.
        /// </summary>
        private readonly Dictionary<RaceEnum, Action<Player>> playerInitializers;

        /// <summary>
        /// List of the registered factory methods mapped by the element types they create.
        /// </summary>
        private readonly Dictionary<string, Delegate> factoryMethods;
    }
}
