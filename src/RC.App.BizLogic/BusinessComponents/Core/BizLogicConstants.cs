using RC.App.BizLogic.Views;
using RC.Common.Configuration;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// General helper methods.
    /// </summary>
    static class BizLogicHelpers
    {
        /// <summary>
        /// Gets the owner of the given map object.
        /// </summary>
        /// <param name="mapObject">Reference to the map object.</param>
        /// <returns>The owner of the given map object or PlayerEnum.Neutral if the map object has no owner.</returns>
        public static PlayerEnum GetMapObjectCurrentOwner(MapObject mapObject)
        {
            Entity entity = mapObject.Owner as Entity;
            if (entity != null)
            {
                StartLocation entityAsStartLoc = entity as StartLocation;
                PlayerEnum owner = entityAsStartLoc != null
                    ? (PlayerEnum) entityAsStartLoc.PlayerIndex
                    : (entity.Owner != null ? (PlayerEnum) entity.Owner.PlayerIndex : PlayerEnum.Neutral);
                return owner;
            }

            EntityWreck entityWreck = mapObject.Owner as EntityWreck;
            if (entityWreck != null) { return (PlayerEnum)entityWreck.PlayerIndex; }

            return PlayerEnum.Neutral;
        }

        /// <summary>
        /// Gets the last known owner of the given map object.
        /// </summary>
        /// <param name="mapObject">Reference to the map object.</param>
        /// <returns>The last known owner of the given map object or PlayerEnum.Neutral if the map object hasn't had owner yet.</returns>
        public static PlayerEnum GetMapObjectLastOwner(MapObject mapObject)
        {
            Entity entity = mapObject.Owner as Entity;
            if (entity != null)
            {
                StartLocation entityAsStartLoc = entity as StartLocation;
                PlayerEnum owner = entityAsStartLoc != null
                    ? (PlayerEnum)entityAsStartLoc.PlayerIndex
                    : (PlayerEnum)entity.LastOwnerIndex;
                return owner;
            }

            EntityWreck entityWreck = mapObject.Owner as EntityWreck;
            if (entityWreck != null) { return (PlayerEnum)entityWreck.PlayerIndex; }

            return PlayerEnum.Neutral;
        }
    }

    /// <summary>
    /// This static class is used to access the constants of the RC.App.BizLogic module.
    /// </summary>
    static class BizLogicConstants
    {
        /// <summary>
        /// The directory of the tilesets.
        /// </summary>
        public static readonly string TILESET_DIR = ConstantsTable.Get<string>("RC.App.BizLogic.TileSetDir");

        /// <summary>
        /// The directory of the command workflow definitions.
        /// </summary>
        public static readonly string COMMAND_DIR = ConstantsTable.Get<string>("RC.App.BizLogic.CommandDir");
    }

    /// <summary>
    /// This static class is used to access the trace filters defined for the RC.App.BizLogic module.
    /// </summary>
    static class BizLogicTraceFilters
    {
        public static readonly int ERROR = TraceManager.GetTraceFilterID("RC.App.BizLogic.Error");
        public static readonly int WARNING = TraceManager.GetTraceFilterID("RC.App.BizLogic.Warning");
        public static readonly int INFO = TraceManager.GetTraceFilterID("RC.App.BizLogic.Info");
        public static readonly int DETAILS = TraceManager.GetTraceFilterID("RC.App.BizLogic.Details");
    }
}
