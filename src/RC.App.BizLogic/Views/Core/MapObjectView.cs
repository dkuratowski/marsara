using System;
using System.Collections.Generic;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.App.BizLogic.BusinessComponents;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on the objects of the currently opened map.
    /// </summary>
    class MapObjectView : MapViewBase, IMapObjectView
    {
        /// <summary>
        /// Constructs a MapObjectView instance.
        /// </summary>
        public MapObjectView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectView.GetVisibleMapObjectSprites"/>
        public List<Tuple<SpriteRenderInfo, PlayerEnum>> GetVisibleMapObjectSprites()
        {
            /// Display the currently visible entity snapshots.
            List<Tuple<SpriteRenderInfo, PlayerEnum>> retList = new List<Tuple<SpriteRenderInfo, PlayerEnum>>();
            foreach (EntitySnapshot entitySnapshot in this.fogOfWarBC.GetEntitySnapshotsToUpdate())
            {
                RCIntRectangle displayRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(entitySnapshot.Position);
                foreach (int spriteIdx in entitySnapshot.AnimationFrame)
                {
                    retList.Add(Tuple.Create(new SpriteRenderInfo()
                    {
                        SpriteGroup = SpriteGroupEnum.MapObjectSpriteGroup,
                        Index = entitySnapshot.EntityType.SpritePalette.Index,
                        DisplayCoords = displayRect.Location + entitySnapshot.EntityType.SpritePalette.GetOffset(spriteIdx),
                        Section = entitySnapshot.EntityType.SpritePalette.GetSection(spriteIdx)
                    }, entitySnapshot.Owner));
                }
            }

            /// Display the currently visible map objects on the ground inside the currently visible window of quadratic tiles.
            foreach (MapObject groundMapObject in this.fogOfWarBC.GetGroundMapObjectsToUpdate())
            {
                this.CollectMapObjectSprites(groundMapObject, ref retList);
            }

            /// Display the shadows of the currently visible map objects in the air inside the currently visible window of quadratic tiles.
            foreach (MapObject airMapObject in this.fogOfWarBC.GetAirMapObjectsToUpdate())
            {
                this.CollectMapObjectShadowSprites(airMapObject, ref retList);
            }

            /// Display the currently visible map objects in the air inside the currently visible window of quadratic tiles.
            foreach (MapObject airMapObject in this.fogOfWarBC.GetAirMapObjectsToUpdate())
            {
                this.CollectMapObjectSprites(airMapObject, ref retList);
            }

            return retList;
        }

        /// <see cref="IMapObjectView.GetMapObjectID"/>
        public int GetMapObjectID(RCIntVector position)
        {
            foreach (Entity entity in this.Scenario.GetElementsOnMap<Entity>(this.MapWindowBC.AttachedWindow.WindowToMapCoords(position), MapObjectLayerEnum.AirObjects, MapObjectLayerEnum.GroundObjects))
            {
                /// Get the ID of the entity only if it's not hidden by FOW.
                if (this.fogOfWarBC.IsMapObjectVisible(entity.MapObject))
                {
                    return entity.ID.Read();
                }
            }
            return -1;
        }

        #endregion IMapObjectView methods

        /// <summary>
        /// Collect the sprites of the given map object into the given list.
        /// </summary>
        /// <param name="mapObject">The given map object.</param>
        /// <param name="targetList">The given target list.</param>
        private void CollectMapObjectSprites(MapObject mapObject, ref List<Tuple<SpriteRenderInfo, PlayerEnum>> targetList)
        {
            RCIntRectangle displayRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(mapObject.BoundingBox);
            foreach (AnimationPlayer animation in mapObject.CurrentAnimations)
            {
                foreach (int spriteIdx in animation.CurrentFrame)
                {
                    targetList.Add(Tuple.Create(new SpriteRenderInfo()
                    {
                        SpriteGroup = SpriteGroupEnum.MapObjectSpriteGroup,
                        Index = mapObject.Owner.ElementType.SpritePalette.Index,
                        DisplayCoords = displayRect.Location + mapObject.Owner.ElementType.SpritePalette.GetOffset(spriteIdx),
                        Section = mapObject.Owner.ElementType.SpritePalette.GetSection(spriteIdx)
                    }, BizLogicHelpers.GetMapObjectOwner(mapObject)));
                }
            }
        }

        /// <summary>
        /// Collect the shadow sprites of the given map object into the given list.
        /// </summary>
        /// <param name="mapObject">The given map object.</param>
        /// <param name="targetList">The given target list.</param>
        private void CollectMapObjectShadowSprites(MapObject mapObject, ref List<Tuple<SpriteRenderInfo, PlayerEnum>> targetList)
        {
            if (mapObject.ShadowCenter != RCNumVector.Undefined)
            {
                /// This map object has a shadow to be rendered -> calculate the display coordinates of the shadow
                RCIntVector shadowCenterOnDisplay = this.MapWindowBC.AttachedWindow.MapToWindowCoords(mapObject.ShadowCenter);
                RCIntRectangle shadowSection = this.scenarioManagerBC.Metadata.ShadowPalette.GetSection(mapObject.Owner.ElementType.ShadowSpriteIndex);
                RCIntVector shadowDisplayCoords = shadowCenterOnDisplay - (shadowSection.Size / 2);

                /// Ensure that overlapping shadows won't overlie the terrain.
                if ((shadowDisplayCoords.X + shadowDisplayCoords.Y) % 2 != 0) { shadowDisplayCoords += new RCIntVector(1, 0); }

                /// Create the sprite render info from the calculated display coordinates.
                targetList.Add(Tuple.Create(new SpriteRenderInfo()
                {
                    SpriteGroup = SpriteGroupEnum.MapObjectShadowSpriteGroup,
                    Index = this.scenarioManagerBC.Metadata.ShadowPalette.Index,
                    DisplayCoords = shadowDisplayCoords,
                    Section = shadowSection
                }, BizLogicHelpers.GetMapObjectOwner(mapObject)));
            }
        }

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private readonly IScenarioManagerBC scenarioManagerBC;
    }
}
