using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common.Configuration;
using RC.Common.Diagnostics;
using RC.Engine.Simulator.Scenarios;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// General helper methods.
    /// </summary>
    static class BizLogicHelpers
    {
        /// <summary>
        /// Gets the owner of the given entity.
        /// </summary>
        /// <param name="entity">Reference to the entity.</param>
        /// <returns>The owner of the given entity.</returns>
        public static PlayerEnum GetEntityOwner(Entity entity)
        {
            StartLocation entityAsStartLoc = entity as StartLocation;
            PlayerEnum owner = entityAsStartLoc != null
                             ? (PlayerEnum)entityAsStartLoc.PlayerIndex
                             : (entity.Owner != null ? (PlayerEnum)entity.Owner.PlayerIndex : PlayerEnum.Neutral);
            return owner;
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
