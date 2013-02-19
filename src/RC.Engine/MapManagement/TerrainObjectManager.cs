using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    class TerrainObjectManager : BspMapContentManager<ITerrainObject>, ITerrainObjectEdit
    {
        public TerrainObjectManager(Map parentMap)
            : base(new RCNumRectangle((RCNumber)(-1) / (RCNumber)2,
                                      (RCNumber)(-1) / (RCNumber)2,
                                      parentMap.NavSize.X,
                                      parentMap.NavSize.Y),
                   EngineConstants.BSP_NODE_CAPACITY,
                   EngineConstants.BSP_MIN_NODE_SIZE)
        {
        }

        #region ITerrainObjectEdit methods

        /// <see cref="ITerrainObjectEdit.CreateTerrainObject"/>
        public ITerrainObject CreateTerrainObject(TerrainObjectType type)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ITerrainObjectEdit.AttachTerrainObject"/>
        public void AttachTerrainObject(ITerrainObject terrainObject, RCIntVector mapCoords)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ITerrainObjectEdit.DetachTerrainObject"/>
        public void DetachTerrainObject(ITerrainObject terrainObject)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ITerrainObjectEdit.CheckConstraints"/>
        public IEnumerable<RCIntVector> CheckConstraints(ITerrainObject terrainObject, RCIntVector mapCoords)
        {
            throw new NotImplementedException();
        }

        #endregion ITerrainObjectEdit methods


    }
}
