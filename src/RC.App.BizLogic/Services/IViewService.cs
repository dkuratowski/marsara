using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Services
{
    /// <summary>
    /// Interface of the view service that is used to create the appropriate views for the presentation layer.
    /// </summary>
    [ComponentInterface]
    public interface IViewService
    {
        /// <summary>
        /// Creates a view of the given type.
        /// </summary>
        /// <typeparam name="TView">The type of the view to be created.</typeparam>
        /// <returns>
        /// The created view of the given type or null if no factory method is registered for the given type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// In case of mismatch with the parameter list of the factory method registered for the given view type.
        /// </exception>
        TView CreateView<TView>() where TView : class;

        /// <summary>
        /// Creates a view of the given type.
        /// </summary>
        /// <typeparam name="TView">The type of the view to be created.</typeparam>
        /// <typeparam name="TParam">The type of the input parameter that is needed to create the view.</typeparam>
        /// <param name="param">The input parameter that is needed to create the view.</param>
        /// <returns>
        /// The created view of the given type or null if no factory method is registered for the given type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// In case of mismatch with the parameter list of the factory method registered for the given view type.
        /// </exception>
        TView CreateView<TView, TParam>(TParam param) where TView : class;

        /// <summary>
        /// Creates a view of the given type.
        /// </summary>
        /// <typeparam name="TView">The type of the view to be created.</typeparam>
        /// <typeparam name="TParam0">The type of the first input parameter that is needed to create the view.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter that is needed to create the view.</typeparam>
        /// <param name="param0">The first input parameter that is needed to create the view.</param>
        /// <param name="param1">The second input parameter that is needed to create the view.</param>
        /// <returns>
        /// The created view of the given type or null if no factory method is registered for the given type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// In case of mismatch with the parameter list of the factory method registered for the given view type.
        /// </exception>
        TView CreateView<TView, TParam0, TParam1>(TParam0 param0, TParam1 param1) where TView : class;

        /// <summary>
        /// Creates a view of the given type.
        /// </summary>
        /// <typeparam name="TView">The type of the view to be created.</typeparam>
        /// <typeparam name="TParam0">The type of the first input parameter that is needed to create the view.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter that is needed to create the view.</typeparam>
        /// <typeparam name="TParam2">The type of the third input parameter that is needed to create the view.</typeparam>
        /// <param name="param0">The first input parameter that is needed to create the view.</param>
        /// <param name="param1">The second input parameter that is needed to create the view.</param>
        /// <param name="param2">The third input parameter that is needed to create the view.</param>
        /// <returns>
        /// The created view of the given type or null if no factory method is registered for the given type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// In case of mismatch with the parameter list of the factory method registered for the given view type.
        /// </exception>
        TView CreateView<TView, TParam0, TParam1, TParam2>(TParam0 param0, TParam1 param1, TParam2 param2) where TView : class;
    }
}
