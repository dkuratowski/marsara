using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.ComponentInterfaces;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Implementation of the map editor component.
    /// </summary>
    [Component("RC.Engine.Maps.MapEditor")]
    class MapEditor : IMapEditor
    {
        #region IMapEditor methods

        /// <see cref="IMapEditor.DrawTerrain"/>
        public IEnumerable<IIsoTile> DrawTerrain(IMapAccess targetMap, IIsoTile targetTile, ITerrainType terrainType)
        {
            if (targetMap == null) { throw new ArgumentNullException("targetMap"); }
            if (targetTile == null) { throw new ArgumentNullException("targetTile"); }
            if (terrainType == null) { throw new ArgumentNullException("terrainType"); }
            if (targetMap.Tileset != terrainType.Tileset) { throw new InvalidOperationException("The tileset of the new terrain type must be the same as the tileset of the map!"); }

            /// Notify the map that the tile exchanging procedure is started.
            targetMap.BeginExchangingTiles();

            /// First we have to search the basis layer of the draw operation.
            ITerrainType baseLayer = terrainType;
            FloodArea floodArea = new FloodArea();
            while (!this.CheckLayer(targetMap, targetTile, baseLayer, floodArea))
            {
                floodArea.Enlarge(baseLayer.TransitionLength + 1);
                baseLayer = baseLayer.Parent;
                if (baseLayer == null) { throw new MapException("Basis-layer not found for draw terrain operation!"); }
            }

            /// Clear the appropriate areas of the map around the target tile of the draw operation.
            foreach (ITerrainType topmostLayer in targetMap.Tileset.TerrainTypes)
            {
                if (topmostLayer.IsDescendantOf(baseLayer) && topmostLayer != terrainType && !topmostLayer.HasChildren)
                {
                    ITerrainType[] layersToClear = terrainType.FindRoute(topmostLayer);
                    this.ClearLayers(targetMap, targetTile, terrainType, baseLayer, layersToClear);
                }
            }

            /// Fill the appropriate areas of the map around the target tile of the draw operation.
            this.FillLayers(targetMap, targetTile, terrainType, baseLayer);

            /// Force regenerating the variant of the draw operation center and its neighbours.
            targetTile.ExchangeType(targetTile.Type);
                        
            /// Remove the terrain objects that are violating the new map terrain.
            IEnumerable<IIsoTile> affectedIsoTiles = targetMap.EndExchangingTiles();
            foreach (IIsoTile affectedIsoTile in affectedIsoTiles)
            {
                foreach (IQuadTile cuttingQuadTile in affectedIsoTile.CuttingQuadTiles)
                {
                    ITerrainObject affectedTerrainObj = cuttingQuadTile.TerrainObject;
                    if (affectedTerrainObj != null && affectedTerrainObj.Type.CheckConstraints(targetMap, affectedTerrainObj.MapCoords).Count != 0)
                    {
                        this.RemoveTerrainObject(targetMap, affectedTerrainObj);
                    }
                }
            }
            return affectedIsoTiles;
        }

        /// <see cref="IMapEditor.PlaceTerrainObject"/>
        public ITerrainObject PlaceTerrainObject(IMapAccess targetMap, IQuadTile targetTile, ITerrainObjectType type)
        {
            if (targetMap == null) { throw new ArgumentNullException("targetMap"); }
            if (targetTile == null) { throw new ArgumentNullException("targetTile"); }
            if (type == null) { throw new ArgumentNullException("type"); }
            if (targetMap.Tileset != type.Tileset) { throw new InvalidOperationException("The tileset of the terrain object type must be the same as the tileset of the map!"); }

            /// TODO: Avoid this downcast!
            MapAccess targetMapObj = targetMap as MapAccess;
            if (targetMapObj == null) { throw new ArgumentException("The given map cannot be handled by the MapEditor!", "targetMap"); }

            if (type.CheckConstraints(targetMap, targetTile.MapCoords).Count != 0) { return null; }
            if (type.CheckTerrainObjectIntersections(targetMap, targetTile.MapCoords).Count != 0) { return null; }

            /// TODO: Might be better to create the TerrainObject with a factory?
            ITerrainObject newObj = new TerrainObject(targetMap, type, targetTile.MapCoords);
            foreach (ICellDataChangeSet changeset in newObj.Type.CellDataChangesets)
            {
                changeset.Apply(newObj);
            }
            targetMapObj.AttachTerrainObject(newObj);
            return newObj;
        }

        /// <see cref="IMapEditor.RemoveTerrainObject"/>
        public void RemoveTerrainObject(IMapAccess targetMap, ITerrainObject terrainObject)
        {
            if (targetMap == null) { throw new ArgumentNullException("targetMap"); }
            if (terrainObject == null) { throw new ArgumentNullException("terrainObject"); }
            if (targetMap != terrainObject.ParentMap) { throw new InvalidOperationException("The map of the terrain object must equal with the target map!"); }

            /// TODO: Avoid this downcast!
            MapAccess targetMapObj = targetMap as MapAccess;
            if (targetMapObj == null) { throw new ArgumentException("The given map cannot be handled by the MapEditor!", "targetMap"); }

            /// Undo the cell data changesets of the removed terrain object.
            foreach (ICellDataChangeSet changeset in terrainObject.Type.CellDataChangesets)
            {
                changeset.Undo(terrainObject);
            }
            targetMapObj.DetachTerrainObject(terrainObject);
        }

        #endregion IMapEditor methods

        #region Internal helper methods

        /// <summary>
        /// Checks whether the given area with the given center tile is a subset of the given layer.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <param name="center">The center of the area to check.</param>
        /// <param name="layer">The terrain type of the layer to check.</param>
        /// <param name="area">The area to check.</param>
        /// <returns>True if the area is a subset of the given layer, false otherwise.</returns>
        private bool CheckLayer(IMapAccess map, IIsoTile center, ITerrainType layer, FloodArea area)
        {
            foreach (FloodItem floodItem in area)
            {
                IIsoTile checkedTile = map.GetIsoTile(center.MapCoords + floodItem.Coordinates);
                if (checkedTile != null)
                {
                    if (checkedTile.Type.TerrainA.IsDescendantOf(layer) || checkedTile.Type.TerrainA == layer) { continue; }

                    if (checkedTile.Type.Combination != TerrainCombination.Simple)
                    {
                        if (checkedTile.Type.TerrainB == layer)
                        {
                            /// We have to check the combinations
                            if (((int)floodItem.Combination & (int)checkedTile.Type.Combination) != (int)floodItem.Combination)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Clears the given layers for a draw operation.
        /// </summary>
        /// <param name="map">Reference to the map.</param>
        /// <param name="center">The center of the draw operation.</param>
        /// <param name="targetTerrain">The target terrain of the draw operation.</param>
        /// <param name="baseTerrain">The base layer of the draw operation.</param>
        /// <param name="layersToClear">The route from the target terrain up to a topmost layer in the terrain tree.</param>
        private void ClearLayers(IMapAccess map, IIsoTile center, ITerrainType targetTerrain, ITerrainType baseTerrain, ITerrainType[] layersToClear)
        {
            /// Find the biggest flood area to be cleared.
            FloodArea areaToClear = new FloodArea();
            ITerrainType lastUninjuredLayer = null;
            for (int routeIdx = 0; routeIdx < layersToClear.Length; routeIdx++)
            {
                ITerrainType currTerrain = layersToClear[routeIdx];
                if (lastUninjuredLayer == null)
                {
                    /// We are going downstairs.
                    ITerrainType nextTerrain = layersToClear[routeIdx + 1];
                    if (nextTerrain.Parent == currTerrain)
                    {
                        /// Last uninjured layer found, from now we go upstairs.
                        lastUninjuredLayer = currTerrain;

                        /// Enlarge the clear area by 1 if there was a previous layer along the way downstairs.
                        ITerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToClear[routeIdx - 1] : null;
                        if (prevTerrain != null) { areaToClear.Enlarge(1); }
                    }
                    else
                    {
                        /// Enlarge the clear area by the transition length of the previous layer if there
                        /// was a previous layer along the way downstairs.
                        ITerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToClear[routeIdx - 1] : null;
                        if (prevTerrain != null) { areaToClear.Enlarge(prevTerrain.TransitionLength + 1); }
                    }
                }
                else
                {
                    /// We are going upstairs.
                    ITerrainType prevTerrain = layersToClear[routeIdx - 1];
                    if (prevTerrain != lastUninjuredLayer) { areaToClear.Enlarge(currTerrain.TransitionLength + 1); }
                }
            }

            /// Clear the appropriate layers.
            if (lastUninjuredLayer == null) { throw new MapException("Last uninjured layer not found for draw terrain operation!"); }
            for (int routeIdx = layersToClear.Length - 1; routeIdx >= 0; routeIdx--)
            {
                ITerrainType currLayer = layersToClear[routeIdx];
                if (currLayer == lastUninjuredLayer) { break; }

                /// Clear the current layer at the appropriate area.
                foreach (FloodItem floodItem in areaToClear)
                {
                    IIsoTile clearedTile = map.GetIsoTile(center.MapCoords + floodItem.Coordinates);
                    if (clearedTile != null)
                    {
                        if (clearedTile.Type.Combination != TerrainCombination.Simple)
                        {
                            /// Mixed tile.
                            if (clearedTile.Type.TerrainB.IsDescendantOf(currLayer))
                            {
                                /// Check whether TerrainB will be cleared by another branch or this is an error.
                                if (!layersToClear.Contains(clearedTile.Type.TerrainB)) { continue; }
                                else { throw new MapException("Clearing non-topmost layer is not possible!"); }
                            }
                            if (clearedTile.Type.TerrainB == currLayer)
                            {
                                TerrainCombination newComb = (TerrainCombination)((int)clearedTile.Type.Combination & ~(floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF));
                                if (newComb != clearedTile.Type.Combination)
                                {
                                    clearedTile.ExchangeType(
                                        newComb == TerrainCombination.Simple ?
                                        map.Tileset.GetIsoTileType(clearedTile.Type.TerrainA.Name) :
                                        map.Tileset.GetIsoTileType(clearedTile.Type.TerrainA.Name, clearedTile.Type.TerrainB.Name, newComb));
                                }
                            }
                        }
                        else
                        {
                            /// Simple tile.
                            if (clearedTile.Type.TerrainA.IsDescendantOf(currLayer))
                            {
                                /// Check whether TerrainA will be cleared by another branch or this is an error.
                                if (!layersToClear.Contains(clearedTile.Type.TerrainA)) { continue; }
                                else { throw new MapException("Clearing non-topmost layer is not possible!"); }
                            }
                            if (clearedTile.Type.TerrainA == currLayer)
                            {
                                TerrainCombination newComb = (TerrainCombination)(0xF & ~(floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF));
                                clearedTile.ExchangeType(
                                    newComb == TerrainCombination.Simple ?
                                    map.Tileset.GetIsoTileType(clearedTile.Type.TerrainA.Parent.Name) :
                                    map.Tileset.GetIsoTileType(clearedTile.Type.TerrainA.Parent.Name, clearedTile.Type.TerrainA.Name, newComb));
                            }
                        }
                    }
                }

                if (routeIdx > 1) { areaToClear.Reduce(); }
            }
        }

        /// <summary>
        /// Fills up the layers from the base layer up to the target layer.
        /// </summary>
        /// <param name="center">The center of the draw operation.</param>
        /// <param name="targetTerrain">The target terrain of the draw operation.</param>
        /// <param name="baseTerrain">The base layer of the draw operation.</param>
        public void FillLayers(IMapAccess map, IIsoTile center, ITerrainType targetTerrain, ITerrainType baseTerrain)
        {
            /// Find the biggest flood area to be filled.
            FloodArea areaToFill = new FloodArea();
            ITerrainType[] layersToFill = targetTerrain.FindRoute(baseTerrain);
            for (int routeIdx = 0; routeIdx < layersToFill.Length; routeIdx++)
            {
                ITerrainType prevTerrain = routeIdx - 1 >= 0 ? layersToFill[routeIdx - 1] : null;
                if (prevTerrain != null) { areaToFill.Enlarge(prevTerrain.TransitionLength + 1); }
            }

            /// Fill the appropriate layers
            for (int routeIdx = layersToFill.Length - 1; routeIdx >= 0; routeIdx--)
            {
                /// Fill the current layer at the appropriate area.
                ITerrainType currLayer = layersToFill[routeIdx];
                foreach (FloodItem floodItem in areaToFill)
                {
                    IIsoTile filledTile = map.GetIsoTile(center.MapCoords + floodItem.Coordinates);
                    if (filledTile != null)
                    {
                        if (filledTile.Type.Combination != TerrainCombination.Simple)
                        {
                            /// Mixed tile.
                            if (filledTile.Type.TerrainB == currLayer)
                            {
                                int newCombInt = (int)filledTile.Type.Combination | (floodItem.Combination != TerrainCombination.Simple ? (int)floodItem.Combination : 0xF);
                                TerrainCombination newComb = newCombInt != 0xF ? (TerrainCombination)newCombInt : TerrainCombination.Simple;
                                if (newComb != filledTile.Type.Combination)
                                {
                                    filledTile.ExchangeType(
                                        newComb == TerrainCombination.Simple ?
                                        map.Tileset.GetIsoTileType(filledTile.Type.TerrainB.Name) :
                                        map.Tileset.GetIsoTileType(filledTile.Type.TerrainA.Name, filledTile.Type.TerrainB.Name, newComb));
                                }
                            }
                            else if (currLayer.IsDescendantOf(filledTile.Type.TerrainB))
                            {
                                throw new MapException("Filling over the topmost layer is not possible!");
                            }
                        }
                        else
                        {
                            /// Simple tile.
                            if (filledTile.Type.TerrainA == currLayer.Parent)
                            {
                                filledTile.ExchangeType(
                                    floodItem.Combination == TerrainCombination.Simple ?
                                    map.Tileset.GetIsoTileType(currLayer.Name) :
                                    map.Tileset.GetIsoTileType(filledTile.Type.TerrainA.Name, currLayer.Name, floodItem.Combination));
                            }
                            else if (currLayer.IsDescendantOf(filledTile.Type.TerrainA))
                            {
                                throw new MapException("Filling over the topmost layer is not possible!");
                            }
                        }
                    }
                }

                if (routeIdx > 0) { areaToFill.Reduce(); }
            }
        }

        #endregion Internal helper methods
    }
}
