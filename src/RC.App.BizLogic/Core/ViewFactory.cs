using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.ComponentInterfaces;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// The implementation of the component that is responsible for creating views for the presentation layer.
    /// </summary>
    [Component("RC.App.BizLogic.ViewFactory")]
    class ViewFactory : IViewFactory, IViewFactoryRegistry
    {
        /// <summary>
        /// Constructs a ViewFactory instance.
        /// </summary>
        public ViewFactory()
        {
            this.factoryMethods = new Dictionary<Type, Delegate>();
        }

        #region IViewFactory members

        /// <see cref="IViewFactory.CreateView<TView>"/>
        TView IViewFactory.CreateView<TView>()
        {
            Delegate factoryMethod = this.GetFactoryMethod_i<TView>();
            if (factoryMethod == null) { return null; }

            Func<TView> factoryMethodCasted = factoryMethod as Func<TView>;
            if (factoryMethodCasted == null) { throw new InvalidOperationException("View factory method parameter list mismatch!"); }
            return factoryMethodCasted();
        }

        /// <see cref="IViewFactory.CreateView<TView, TParam>"/>
        TView IViewFactory.CreateView<TView, TParam>(TParam param)
        {
            Delegate factoryMethod = this.GetFactoryMethod_i<TView>();
            if (factoryMethod == null) { return null; }

            Func<TParam, TView> factoryMethodCasted = factoryMethod as Func<TParam, TView>;
            if (factoryMethodCasted == null) { throw new InvalidOperationException("View factory method parameter list mismatch!"); }
            return factoryMethodCasted(param);
        }

        /// <see cref="IViewFactory.CreateView<TView, TParam0, TParam1>"/>
        TView IViewFactory.CreateView<TView, TParam0, TParam1>(TParam0 param0, TParam1 param1)
        {
            Delegate factoryMethod = this.GetFactoryMethod_i<TView>();
            if (factoryMethod == null) { return null; }

            Func<TParam0, TParam1, TView> factoryMethodCasted = factoryMethod as Func<TParam0, TParam1, TView>;
            if (factoryMethodCasted == null) { throw new InvalidOperationException("View factory method parameter list mismatch!"); }
            return factoryMethodCasted(param0, param1);
        }

        /// <see cref="IViewFactory.CreateView<TView, TParam0, TParam1, TParam2>"/>
        TView IViewFactory.CreateView<TView, TParam0, TParam1, TParam2>(TParam0 param0, TParam1 param1, TParam2 param2)
        {
            Delegate factoryMethod = this.GetFactoryMethod_i<TView>();
            if (factoryMethod == null) { return null; }

            Func<TParam0, TParam1, TParam2, TView> factoryMethodCasted = factoryMethod as Func<TParam0, TParam1, TParam2, TView>;
            if (factoryMethodCasted == null) { throw new InvalidOperationException("View factory method parameter list mismatch!"); }
            return factoryMethodCasted(param0, param1, param2);
        }

        #endregion IViewFactory members

        #region IViewFactoryRegistry members

        /// <see cref="IViewFactory.RegisterViewFactory<TView>"/>
        void IViewFactoryRegistry.RegisterViewFactory<TView>(Func<TView> factoryMethod)
        {
            this.RegisterViewFactory_i<TView>(factoryMethod);
        }

        /// <see cref="IViewFactory.RegisterViewFactory<TView, TParam>"/>
        void IViewFactoryRegistry.RegisterViewFactory<TView, TParam>(Func<TParam, TView> factoryMethod)
        {
            this.RegisterViewFactory_i<TView>(factoryMethod);
        }

        /// <see cref="IViewFactory.RegisterViewFactory<TView, TParam0, TParam1>"/>
        void IViewFactoryRegistry.RegisterViewFactory<TView, TParam0, TParam1>(Func<TParam0, TParam1, TView> factoryMethod)
        {
            this.RegisterViewFactory_i<TView>(factoryMethod);
        }

        /// <see cref="IViewFactory.RegisterViewFactory<TView, TParam0, TParam1, TParam2>"/>
        void IViewFactoryRegistry.RegisterViewFactory<TView, TParam0, TParam1, TParam2>(Func<TParam0, TParam1, TParam2, TView> factoryMethod)
        {
            this.RegisterViewFactory_i<TView>(factoryMethod);
        }

        /// <see cref="IViewFactory.UnregisterViewFactory<TView>"/>
        void IViewFactoryRegistry.UnregisterViewFactory<TView>()
        {
            Type viewType = typeof(TView);
            if (!this.factoryMethods.ContainsKey(viewType)) { throw new InvalidOperationException(string.Format("No factory method for view type '{0}' has been registered!", viewType.Name)); }
            this.factoryMethods.Remove(viewType);
        }

        #endregion IViewFactoryRegistry members

        #region Internal methods

        /// <summary>
        /// The internal implementation of factory method registration.
        /// </summary>
        private void RegisterViewFactory_i<TView>(Delegate factoryMethod)
        {
            if (factoryMethod == null) { throw new ArgumentNullException("factoryMethod"); }

            Type viewType = typeof(TView);
            if (this.factoryMethods.ContainsKey(viewType)) { throw new InvalidOperationException(string.Format("Factory method for view type '{0}' has already been registered!", viewType.Name)); }
            this.factoryMethods[viewType] = factoryMethod;
        }

        /// <summary>
        /// Gets the factory method registered for the given view type.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <returns>
        /// The factory method registered for the given view type or null if no factory method for that type has been
        /// registered.
        /// </returns>
        private Delegate GetFactoryMethod_i<TView>()
        {
            Type viewType = typeof(TView);
            return this.factoryMethods.ContainsKey(viewType) ? this.factoryMethods[viewType] : null;
        }

        #endregion Internal methods

        /// <summary>
        /// List of the registered factory methods mapped by the view types they create.
        /// </summary>
        private readonly Dictionary<Type, Delegate> factoryMethods;
    }
}
