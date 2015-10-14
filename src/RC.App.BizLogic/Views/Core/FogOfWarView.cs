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

        /// <see cref="IFogOfWarView.GetFOWTiles"/>
        public List<SpriteRenderInfo> GetFOWTiles()
        {
            List<SpriteRenderInfo> retList = new List<SpriteRenderInfo>();
            this.CollectPartialFOWTiles(ref retList);
            this.CollectFullFOWTiles(ref retList);
            return retList;
        }

        #endregion IFogOfWarView methods

        /// <summary>
        /// Collects the partial Fog Of War tiles to update into the given list.
        /// </summary>
        /// <param name="targetList">The target list.</param>
        private void CollectPartialFOWTiles(ref List<SpriteRenderInfo> targetList)
        {
            foreach (IQuadTile quadTile in this.fogOfWarBC.GetQuadTilesToUpdate())
            {
                FOWTileFlagsEnum partialFowFlags = this.fogOfWarBC.GetPartialFowTileFlags(quadTile.MapCoords);
                if (partialFowFlags != FOWTileFlagsEnum.None)
                {
                    targetList.Add(
                        new SpriteRenderInfo()
                        {
                            SpriteGroup = SpriteGroupEnum.PartialFogOfWarSpriteGroup,
                            Index = (int)partialFowFlags,
                            DisplayCoords = this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(quadTile.MapCoords, new RCIntVector(1, 1))).Location,
                            Section = RCIntRectangle.Undefined
                        });
                }
            }
        }

        /// <summary>
        /// Collects the full Fog Of War tiles to update into the given list.
        /// </summary>
        /// <param name="targetList">The target list.</param>
        private void CollectFullFOWTiles(ref List<SpriteRenderInfo> targetList)
        {
            foreach (IQuadTile quadTile in this.fogOfWarBC.GetQuadTilesToUpdate())
            {
                FOWTileFlagsEnum fullFowFlags = this.fogOfWarBC.GetFullFowTileFlags(quadTile.MapCoords);
                if (fullFowFlags != FOWTileFlagsEnum.None)
                {
                    targetList.Add(
                        new SpriteRenderInfo()
                        {
                            SpriteGroup = SpriteGroupEnum.FullFogOfWarSpriteGroup,
                            Index = (int)fullFowFlags,
                            DisplayCoords = this.MapWindowBC.AttachedWindow.QuadToWindowRect(new RCIntRectangle(quadTile.MapCoords, new RCIntVector(1, 1))).Location,
                            Section = RCIntRectangle.Undefined
                        });
                }
            }
        }

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
