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
        }

        #region IMapObjectView methods

        /// <see cref="IMapObjectView.GetVisibleMapObjects"/>
        public List<ObjectInst> GetVisibleMapObjects()
        {
            /// Display the currently visible map objects inside the currently visible window of quadratic tiles.
            List<ObjectInst> retList = new List<ObjectInst>();
            foreach (MapObject mapObject in this.fogOfWarBC.GetMapObjectsToUpdate())
            {
                RCIntRectangle displayRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(mapObject.BoundingBox);
                List<SpriteInst> entitySprites = new List<SpriteInst>();
                foreach (AnimationPlayer animation in mapObject.CurrentAnimations)
                {
                    foreach (int spriteIdx in animation.CurrentFrame)
                    {
                        entitySprites.Add(new SpriteInst()
                        {
                            Index = mapObject.Owner.ElementType.SpritePalette.Index,
                            DisplayCoords = displayRect.Location + mapObject.Owner.ElementType.SpritePalette.GetOffset(spriteIdx),
                            Section = mapObject.Owner.ElementType.SpritePalette.GetSection(spriteIdx)
                        });
                    }
                }

                retList.Add(new ObjectInst()
                {
                    Owner = BizLogicHelpers.GetMapObjectOwner(mapObject),
                    Sprites = entitySprites
                });
            }

            /// Display the currently visible entity snapshots.
            foreach (EntitySnapshot entitySnapshot in this.fogOfWarBC.GetEntitySnapshotsToUpdate())
            {
                RCIntRectangle displayRect = this.MapWindowBC.AttachedWindow.MapToWindowRect(entitySnapshot.Position);
                List<SpriteInst> snapshotSprites = new List<SpriteInst>();
                foreach (int spriteIdx in entitySnapshot.AnimationFrame)
                {
                    snapshotSprites.Add(new SpriteInst()
                    {
                        Index = entitySnapshot.EntityType.SpritePalette.Index,
                        DisplayCoords = displayRect.Location + entitySnapshot.EntityType.SpritePalette.GetOffset(spriteIdx),
                        Section = entitySnapshot.EntityType.SpritePalette.GetSection(spriteIdx)
                    });
                }
                retList.Add(new ObjectInst()
                {
                    Owner = entitySnapshot.Owner,
                    Sprites = snapshotSprites
                });
            }

            return retList;
        }

        /// <see cref="IMapObjectView.GetMapObjectID"/>
        public int GetMapObjectID(RCIntVector position)
        {
            foreach (Entity entity in this.Scenario.GetElementsOnMap<Entity>(this.MapWindowBC.AttachedWindow.WindowToMapCoords(position)))
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
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
