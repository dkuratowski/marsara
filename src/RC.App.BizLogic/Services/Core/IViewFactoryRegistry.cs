using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services.Core
{
    /// <summary>
    /// Internal interface for registering/unregistering view factory methods.
    /// </summary>
    [ComponentInterface]
    interface IViewFactoryRegistry
    {
        /// <summary>
        /// Registers the given factory method for the given view type.
        /// </summary>
        /// <typeparam name="TView">The type of the views created by the factory method.</typeparam>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given view type has already been registered.
        /// </exception>
        void RegisterViewFactory<TView>(Func<TView> factoryMethod) where TView : class;

        /// <summary>
        /// Registers the given factory method for the given view type.
        /// </summary>
        /// <typeparam name="TView">The type of the views created by the factory method.</typeparam>
        /// <typeparam name="TParam">The type of the input parameter of the factory method.</typeparam>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given view type has already been registered.
        /// </exception>
        void RegisterViewFactory<TView, TParam>(Func<TParam, TView> factoryMethod) where TView : class;

        /// <summary>
        /// Registers the given factory method for the given view type.
        /// </summary>
        /// <typeparam name="TView">The type of the views created by the factory method.</typeparam>
        /// <typeparam name="TParam0">The type of the first input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter of the factory method.</typeparam>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given view type has already been registered.
        /// </exception>
        void RegisterViewFactory<TView, TParam0, TParam1>(Func<TParam0, TParam1, TView> factoryMethod) where TView : class;

        /// <summary>
        /// Registers the given factory method for the given view type.
        /// </summary>
        /// <typeparam name="TView">The type of the views created by the factory method.</typeparam>
        /// <typeparam name="TParam0">The type of the first input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter of the factory method.</typeparam>
        /// <typeparam name="TParam2">The type of the third input parameter of the factory method.</typeparam>
        /// <param name="factoryMethod">The factory method to be registered.</param>
        /// <exception cref="InvalidOperationException">
        /// If a factory method for the given view type has already been registered.
        /// </exception>
        void RegisterViewFactory<TView, TParam0, TParam1, TParam2>(Func<TParam0, TParam1, TParam2, TView> factoryMethod) where TView : class;

        /// <summary>
        /// Unregisters the factory method that is currently registered for the given view type.
        /// </summary>
        /// <typeparam name="TView">The type of the view whose factory method needs to be unregistered.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// If there is no factory method registered for the given view type.
        /// </exception>
        void UnregisterViewFactory<TView>() where TView : class;
    }
}
