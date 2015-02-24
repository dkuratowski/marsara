using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on the Fog Of War of the currently opened map.
    /// </summary>
    class FogOfWarView : MapViewBase, IFogOfWarView
    {
        /// <summary>
        /// Constructs a FogOfWarView instance.
        /// </summary>
        public FogOfWarView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region IFogOfWarView methods

        /// <see cref="IFogOfWarView.GetPartialFOWTiles"/>
        public List<SpriteInst> GetPartialFOWTiles()
        {
            List<SpriteInst> retList = new List<SpriteInst>();
            foreach (IQuadTile quadTile in this.fogOfWarBC.GetQuadTilesToUpdate())
            {
                FOWTileFlagsEnum partialFowFlags = this.fogOfWarBC.GetPartialFowTileFlags(quadTile.MapCoords);
                if (partialFowFlags != FOWTileFlagsEnum.None)
                {
                    retList.Add(
                        new SpriteInst()
                        {
                            Index = (int)partialFowFlags,
                            DisplayCoords = this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(quadTile.MapCoords, new RCIntVector(1, 1))).Location,
                            Section = RCIntRectangle.Undefined
                        });
                }
            }
            return retList;
        }

        /// <see cref="IFogOfWarView.GetFullFOWTiles"/>
        public List<SpriteInst> GetFullFOWTiles()
        {
            List<SpriteInst> retList = new List<SpriteInst>();
            foreach (IQuadTile quadTile in this.fogOfWarBC.GetQuadTilesToUpdate())
            {
                FOWTileFlagsEnum fullFowFlags = this.fogOfWarBC.GetFullFowTileFlags(quadTile.MapCoords);
                if (fullFowFlags != FOWTileFlagsEnum.None)
                {
                    retList.Add(
                        new SpriteInst()
                        {
                            Index = (int)fullFowFlags,
                            DisplayCoords = this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(quadTile.MapCoords, new RCIntVector(1, 1))).Location,
                            Section = RCIntRectangle.Undefined
                        });
                }
            }
            return retList;
        }

        #endregion IFogOfWarView methods

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
