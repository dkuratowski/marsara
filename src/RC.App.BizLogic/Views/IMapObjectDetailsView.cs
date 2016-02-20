using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Interface of views that provide detailed informations about map objects.
    /// </summary>
    public interface IMapObjectDetailsView
    {
        /// <summary>
        /// Gets the type ID of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The type ID of the given map object.</returns>
        int GetObjectTypeID(int objectID);

        /// <summary>
        /// Gets the amount of vespene gas in the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The amount of vespene gas in the given map object or -1 if the given map object has no vespene gas or if the vespene gas amount is
        /// not accessible by the local player.
        /// </returns>
        int GetVespeneGasAmount(int objectID);

        /// <summary>
        /// The amount of minerals in the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The amount of minerals in the given map object or -1 if the given map object has no minerals or if the mineral amount is
        /// not accessible by the local player.
        /// </returns>
        int GetMineralsAmount(int objectID);

        /// <summary>
        /// Gets the big HP indicator icon of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The big HP indicator icon of the given map object.</returns>
        SpriteRenderInfo GetBigHPIcon(int objectID);

        /// <summary>
        /// Gets the small HP indicator icon of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The small HP indicator icon of the given map object.</returns>
        SpriteRenderInfo GetSmallHPIcon(int objectID);

        /// <summary>
        /// Gets the condition of the HP of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The condition of the HP of the given map object.</returns>
        MapObjectConditionEnum GetHPCondition(int objectID);

        /// <summary>
        /// Gets the current HP value of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The current HP value of the given map object or -1 if the given map object doesn't have HP value.</returns>
        int GetCurrentHP(int objectID);

        /// <summary>
        /// Gets the maximum HP value of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>The maximum HP value of the given map object or -1 if the given map object doesn't have HP value.</returns>
        int GetMaxHP(int objectID);

        /// <summary>
        /// Gets the current energy value of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The current energy value of the given map object or -1 if the given map object doesn't have energy value or if the current energy value is
        /// not accessible by the local player.
        /// </returns>
        int GetCurrentEnergy(int objectID);

        /// <summary>
        /// Gets the maximum energy value of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The maximum energy value of the given map object or -1 if the given map object doesn't have energy value or if the maximum energy value is
        /// not accessible by the local player.
        /// </returns>
        int GetMaxEnergy(int objectID);

        /// <summary>
        /// Gets the amount of supplies that the given map object provides.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The amount of supplies that the given map object provides or -1 if the given map object is not providing supplies or if the information
        /// is not accessible by the local player.
        /// </returns>
        int GetSuppliesProvided(int objectID);

        /// <summary>
        /// Gets informations about the armor of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The armor informations of the given map object or null if the given map object doesn't have armor informations or if the information
        /// is not accessible by the local player.
        /// The first item in the returned tuple is the original armor and the second item is the amount of upgrade on the armor.
        /// </returns>
        Tuple<int, int> GetArmorInfo(int objectID);

        /// <summary>
        /// Gets informations about the weapons of the given map object.
        /// </summary>
        /// <param name="objectID">The ID of the map object.</param>
        /// <returns>
        /// The weapon informations of the given map object or and empty list if the given map object doesn't have weapons or if the information
        /// is not accessible by the local player.
        /// Each item in the returned list is a 3-tuple. The first item in these tuples is the name, the second item is
        /// original damage and the third item is the amount of upgrade on the damage of the corresponding weapon.
        /// </returns>
        List<Tuple<string, int, int>> GetWeaponInfo(int objectID);
    }
}
