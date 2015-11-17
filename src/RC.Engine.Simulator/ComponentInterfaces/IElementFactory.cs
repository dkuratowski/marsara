using System;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.ComponentInterfaces
{
    /// <summary>
    /// Interface of the element factory component that is responsible for creating scenario elements and initializing players.
    /// </summary>
    [ComponentInterface]
    public interface IElementFactory
    {
        /// <summary>
        /// Initializes the given player with the given race.
        /// </summary>
        /// <param name="player">The player to initialize.</param>
        /// <param name="race">The race of the player.</param>
        void InitializePlayer(Player player, RaceEnum race);

        /// <summary>
        /// Creates an element of the given type.
        /// </summary>
        /// <param name="typeName">The name of the type of the element to be created.</param>
        /// <returns>True if the element has been successfully created.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no factory method registered for the given element type.
        /// </exception>
        bool CreateElement(string typeName);

        /// <summary>
        /// Creates an element of the given type.
        /// </summary>
        /// <typeparam name="TParam">The type of the input parameter that is needed to create the element.</typeparam>
        /// <param name="typeName">The name of the type of the element to be created.</param>
        /// <param name="param">The input parameter that is needed to create the element.</param>
        /// <returns>True if the element created successfully; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no factory method registered for the given element type.
        /// In case of mismatch with the parameter list of the factory method registered for the given element type.
        /// </exception>
        bool CreateElement<TParam>(string typeName, TParam param);

        /// <summary>
        /// Creates an element of the given type.
        /// </summary>
        /// <typeparam name="TParam0">The type of the first input parameter that is needed to create the element.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter that is needed to create the element.</typeparam>
        /// <param name="typeName">The name of the type of the element to be created.</param>
        /// <param name="param0">The first input parameter that is needed to create the element.</param>
        /// <param name="param1">The second input parameter that is needed to create the element.</param>
        /// <returns>True if the element created successfully; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no factory method registered for the given element type.
        /// In case of mismatch with the parameter list of the factory method registered for the given element type.
        /// </exception>
        bool CreateElement<TParam0, TParam1>(string typeName, TParam0 param0, TParam1 param1);

        /// <summary>
        /// Creates an element of the given type.
        /// </summary>
        /// <typeparam name="TParam0">The type of the first input parameter that is needed to create the element.</typeparam>
        /// <typeparam name="TParam1">The type of the second input parameter that is needed to create the element.</typeparam>
        /// <typeparam name="TParam2">The type of the third input parameter that is needed to create the element.</typeparam>
        /// <param name="typeName">The name of the type of the element to be created.</param>
        /// <param name="param0">The first input parameter that is needed to create the element.</param>
        /// <param name="param1">The second input parameter that is needed to create the element.</param>
        /// <param name="param2">The third input parameter that is needed to create the element.</param>
        /// <returns>True if the element created successfully; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// If there is no factory method registered for the given element type.
        /// In case of mismatch with the parameter list of the factory method registered for the given element type.
        /// </exception>
        bool CreateElement<TParam0, TParam1, TParam2>(string typeName, TParam0 param0, TParam1 param1, TParam2 param2);
    }
}
