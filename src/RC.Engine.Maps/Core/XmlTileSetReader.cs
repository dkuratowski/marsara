using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using RC.Common.Diagnostics;
using RC.Common.Configuration;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This reader can read a tileset descriptor XML file and construct a TileSet object from it.
    /// </summary>
    static class XmlTileSetReader
    {
        /// <summary>
        /// Reads the given XML document, and constructs a TileSet object from it.
        /// </summary>
        /// <param name="xmlStr">The string that contains the XML document to read.</param>
        /// <param name="imageDir">The directory where the referenced images can be found. (TODO: this is a hack!)</param>
        /// <returns>The constructed TileSet object.</returns>
        public static TileSet Read(string xmlStr, string imageDir)
        {
            if (xmlStr == null) { throw new ArgumentNullException("xmlStr"); }
            if (imageDir == null) { throw new ArgumentNullException("imageDir"); }

            tmpImageDir = imageDir;

            /// Load the XML document.
            XDocument xmlDoc = XDocument.Parse(xmlStr);
            XAttribute tilesetNameAttr = xmlDoc.Root.Attribute(XmlTileSetConstants.TILESET_NAME_ATTR);
            XElement terrainTreeElem = xmlDoc.Root.Element(XmlTileSetConstants.TERRAINTYPE_ELEM);
            XElement declareTilesElem = xmlDoc.Root.Element(XmlTileSetConstants.DECLARETILES_ELEM);
            XElement declareTerrainObjectsElem = xmlDoc.Root.Element(XmlTileSetConstants.DECLARETERRAINOBJECTS_ELEM);
            if (tilesetNameAttr == null) { throw new TileSetException("Tileset name not defined!"); }
            if (terrainTreeElem == null) { throw new TileSetException("Terrain-tree not defined!"); }
            if (declareTilesElem == null) { throw new TileSetException("Tile declarations not found"); }

            /// Create the TileSet object.
            TileSet tileset = new TileSet(tilesetNameAttr.Value);

            /// Load the terrain tree.
            LoadTerrainTree(terrainTreeElem, null, tileset);

            /// Load the simple tiles.
            foreach (XElement simpleTileElem in declareTilesElem.Elements(XmlTileSetConstants.SIMPLETILE_ELEM))
            {
                LoadSimpleTile(simpleTileElem, tileset);
            }

            /// Load the mixed tiles.
            foreach (XElement mixedTileElem in declareTilesElem.Elements(XmlTileSetConstants.MIXEDTILE_ELEM))
            {
                LoadMixedTile(mixedTileElem, tileset);
            }

            /// Load the terrain objects.
            if (declareTerrainObjectsElem != null)
            {
                foreach (XElement terrainObjElem in declareTerrainObjectsElem.Elements(XmlTileSetConstants.TERRAINOBJECT_ELEM))
                {
                    LoadTerrainObject(terrainObjElem, tileset);
                }
            }

            tileset.CheckAndFinalize();
            return tileset;
        }

        /// <summary>
        /// Loads the terrain tree from the given XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="parent">
        /// The parent of the currently loaded TerrainType of null if the root is being loaded.
        /// </param>
        /// <param name="tileset">The TileSet to load to.</param>
        private static void LoadTerrainTree(XElement fromElem, TerrainType parent, TileSet tileset)
        {
            XAttribute nameAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINTYPE_NAME_ATTR);
            XAttribute transLengthAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINTYPE_TRANSLENGTH_ATTR);
            if (nameAttr == null) { throw new TileSetException("TerrainType name not defined!"); }
            if (parent == null && transLengthAttr != null) { throw new TileSetException("Transition length cannot be defined for the root TerrainType!"); }

            /// Create the terrain type and add it to it's parent as a child.
            tileset.CreateTerrainType(nameAttr.Value);
            if (parent != null)
            {
                if (transLengthAttr != null)
                {
                    int transLength = XmlHelper.LoadInt(transLengthAttr.Value);
                    if (transLength < 0) { throw new TileSetException("Transition length must be non-negative!"); }
                    parent.AddChild(nameAttr.Value, transLength);
                }
                else
                {
                    parent.AddChild(nameAttr.Value);
                }
            }

            /// Process the child XML elements.
            TerrainType currentTerrain = tileset.GetTerrainTypeImpl(nameAttr.Value);
            foreach (XElement childTerrainElem in fromElem.Elements(XmlTileSetConstants.TERRAINTYPE_ELEM))
            {
                LoadTerrainTree(childTerrainElem, currentTerrain, tileset);
            }
        }

        /// <summary>
        /// Loads a simple tile type from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The TileSet to load to.</param>
        private static void LoadSimpleTile(XElement fromElem, TileSet tileset)
        {
            XAttribute terrainAttr = fromElem.Attribute(XmlTileSetConstants.SIMPLETILE_TERRAIN_ATTR);
            if (terrainAttr == null) { throw new TileSetException("Terrain type not defined for simple tile!"); }

            tileset.CreateSimpleTileType(terrainAttr.Value);
            IsoTileType tile = tileset.GetIsoTileTypeImpl(terrainAttr.Value);

            LoadVariants(fromElem, tile, tileset);
        }

        /// <summary>
        /// Loads a mixed tile type from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The TileSet to load to.</param>
        private static void LoadMixedTile(XElement fromElem, TileSet tileset)
        {
            XAttribute terrainAAttr = fromElem.Attribute(XmlTileSetConstants.MIXEDTILE_TERRAINA_ATTR);
            XAttribute terrainBAttr = fromElem.Attribute(XmlTileSetConstants.MIXEDTILE_TERRAINB_ATTR);
            XAttribute combinationAttr = fromElem.Attribute(XmlTileSetConstants.MIXEDTILE_COMBINATION_ATTR);
            if (terrainAAttr == null) { throw new TileSetException("Terrain type A not defined for mixed tile!"); }
            if (terrainBAttr == null) { throw new TileSetException("Terrain type B not defined for mixed tile!"); }
            if (combinationAttr == null) { throw new TileSetException("Terrain combination not defined for mixed tile!"); }

            TerrainCombination combination;
            if (!EnumMap<TerrainCombination, string>.TryDemap(combinationAttr.Value, out combination))
            {
                throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for mixed tile.", combinationAttr.Value));
            }

            tileset.CreateMixedTileType(terrainAAttr.Value, terrainBAttr.Value, combination);
            IsoTileType tile = tileset.GetIsoTileTypeImpl(terrainAAttr.Value, terrainBAttr.Value, combination);

            LoadVariants(fromElem, tile, tileset);
        }

        /// <summary>
        /// Loads a terrain object definition from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The TileSet to load to.</param>
        private static void LoadTerrainObject(XElement fromElem, TileSet tileset)
        {
            XAttribute nameAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_NAME_ATTR);
            XAttribute imageAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_IMAGE_ATTR);
            XAttribute quadSizeAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_QUADSIZE_ATTR);
            XAttribute transpColorAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TRANSPCOLOR_ATTR);
            if (nameAttr == null) { throw new TileSetException("Name not defined for terrain object!"); }
            if (imageAttr == null) { throw new TileSetException("Image not defined for terrain object!"); }
            if (quadSizeAttr == null) { throw new TileSetException("Quadratic size not defined for terrain object!"); }
            if (transpColorAttr == null) { throw new TileSetException("Transparent color not defined for terrain object!"); }

            /// Read the image data.
            string imagePath = Path.Combine(tmpImageDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            tileset.CreateTerrainObjectType(nameAttr.Value,
                                            imageData,
                                            XmlHelper.LoadIntVector(quadSizeAttr.Value),
                                            XmlHelper.LoadColor(transpColorAttr.Value));
            TerrainObjectType terrainObj = tileset.GetTerrainObjectTypeImpl(nameAttr.Value);

            /// Apply the defined area exclusions.
            foreach (XElement excludeAreaElem in fromElem.Elements(XmlTileSetConstants.TERRAINOBJ_EXCLUDEAREA_ELEM))
            {
                XAttribute rectAttr = excludeAreaElem.Attribute(XmlTileSetConstants.TERRAINOBJ_EXCLUDEAREA_RECT_ATTR);
                if (rectAttr == null) { throw new TileSetException("The rectangle of the excluded area not defined!"); }
                terrainObj.ExcludeArea(XmlHelper.LoadIntRectangle(rectAttr.Value));
            }

            /// Load the constraints and the cell data changesets.
            foreach (XElement childElem in fromElem.Elements())
            {
                if (childElem.Name.LocalName == XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_ELEM)
                {
                    ITerrainObjectConstraint constraint = LoadTileConstraint(childElem, terrainObj, tileset);
                    terrainObj.AddConstraint(constraint);
                }
                else if (childElem.Name.LocalName != XmlTileSetConstants.TERRAINOBJ_EXCLUDEAREA_ELEM)
                {
                    ICellDataChangeSet changeset = LoadCellDataChangeSet(childElem, tileset);
                    terrainObj.AddCellDataChangeset(changeset);
                }
                /// TODO: loading other constraint types can take place here!
            }
        }

        /// <summary>
        /// Load a tile-constraint for a terrain object from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="terrainObj">The terrain object.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        /// <returns>The loaded tile-constraint.</returns>
        private static ITerrainObjectConstraint LoadTileConstraint(XElement fromElem, TerrainObjectType terrainObj, TileSet tileset)
        {
            /// Load the attributes of the constraint.
            XAttribute quadCoordsAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_QUADCOORDS_ATTR);
            XAttribute terrainAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_TERRAIN_ATTR);
            XAttribute terrainAAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_TERRAINA_ATTR);
            XAttribute terrainBAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_TERRAINB_ATTR);
            XAttribute combinationsAttr = fromElem.Attribute(XmlTileSetConstants.TERRAINOBJ_TILECONSTRAINT_COMBINATIONS_ATTR);
            if (quadCoordsAttr == null) { throw new TileSetException("Quadratic coordinates not defined for tile constraint element!"); }
            if (terrainAttr != null && (terrainAAttr != null || terrainBAttr != null || combinationsAttr != null)) { throw new TileSetException("Invalid attributes defined for tile constraint on a simple tile!"); }
            if (terrainAttr == null && (terrainAAttr == null || terrainBAttr == null || combinationsAttr == null)) { throw new TileSetException("Invalid attributes defined for tile constraint on a mixed tile!"); }

            RCIntVector quadCoords = XmlHelper.LoadIntVector(quadCoordsAttr.Value);
            if (terrainObj.IsExcluded(quadCoords)) { throw new TileSetException(string.Format("TileConstraint at excluded coordinates {0} cannot be defined!", quadCoords)); }
            if (terrainAttr != null)
            {
                TerrainType terrain = tileset.GetTerrainTypeImpl(terrainAttr.Value);
                return new IsoTileConstraint(quadCoords, terrain, tileset);
            }
            else
            {
                TerrainType terrainA = tileset.GetTerrainTypeImpl(terrainAAttr.Value);
                TerrainType terrainB = tileset.GetTerrainTypeImpl(terrainBAttr.Value);
                List<TerrainCombination> combinations = new List<TerrainCombination>();
                string[] combinationStrings = combinationsAttr.Value.Split(';');
                if (combinationStrings.Length == 0) { throw new TileSetException("Terrain combination not defined for tile constraint on a mixed tile!"); }
                foreach (string combStr in combinationStrings)
                {
                    TerrainCombination combination;
                    if (!EnumMap<TerrainCombination, string>.TryDemap(combStr, out combination) || combination == TerrainCombination.Simple)
                    {
                        throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for tile constraint!", combStr));
                    }
                    combinations.Add(combination);
                }

                return new IsoTileConstraint(quadCoords, terrainA, terrainB, combinations, tileset);
            }
        }

        /// <summary>
        /// Loads the variants of a tile type from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tile">The tile type to load to.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        private static void LoadVariants(XElement fromElem, IsoTileType tile, TileSet tileset)
        {
            TileReadStatus status = TileReadStatus.None;
            foreach (XElement childElem in fromElem.Elements())
            {
                if (status == TileReadStatus.None)
                {
                    /// We are in the initial read status.
                    if (childElem.Name == XmlTileSetConstants.VARIANT_ELEM)
                    {
                        /// No conditional expression at the current tile type.
                        status = TileReadStatus.NoCondition;
                        LoadVariant(childElem, tile, tileset);
                    }
                    else if (childElem.Name == XmlTileSetConstants.IF_ELEM)
                    {
                        /// Beginning of the conditional branch.
                        status = TileReadStatus.ConditionalBranch;
                        LoadBranch(childElem, tile, tileset);
                    }
                    else
                    {
                        /// Other XML elements not allowed at this read status.
                        throw new TileSetException(string.Format("Unexpected node '{0}'!", childElem.Name));
                    }
                }
                else if (status == TileReadStatus.NoCondition)
                {
                    /// We are in the read status where only variant elements are allowed.
                    if (childElem.Name == XmlTileSetConstants.VARIANT_ELEM)
                    {
                        LoadVariant(childElem, tile, tileset);
                    }
                    else
                    {
                        /// Other XML elements not allowed at this read status.
                        throw new TileSetException(string.Format("Unexpected node '{0}'!", childElem.Name));
                    }
                }
                else if (status == TileReadStatus.ConditionalBranch)
                {
                    /// We are in the read status where only conditional branch and default branch elements are allowed.
                    if (childElem.Name == XmlTileSetConstants.ELSEIF_ELEM)
                    {
                        /// Load the conditional branch.
                        LoadBranch(childElem, tile, tileset);
                    }
                    else if (childElem.Name == XmlTileSetConstants.ELSE_ELEM)
                    {
                        /// Load the default branch.
                        status = TileReadStatus.DefaultBranch;
                        foreach (XElement varElem in childElem.Elements(XmlTileSetConstants.VARIANT_ELEM))
                        {
                            LoadVariant(varElem, tile, tileset);
                        }
                    }
                    else
                    {
                        /// Other XML elements not allowed at this read status.
                        throw new TileSetException(string.Format("Unexpected node '{0}'!", childElem.Name));
                    }
                }
                else
                {
                    /// Other XML elements not allowed.
                    throw new TileSetException(string.Format("Unexpected node '{0}' after default branch!", childElem.Name));
                }
            }
        }

        /// <summary>
        /// Loads a tile variant from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tile">The tile type being loaded.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        private static void LoadVariant(XElement fromElem, IsoTileType tile, TileSet tileset)
        {
            XAttribute imageAttr = fromElem.Attribute(XmlTileSetConstants.VARIANT_IMAGE_ATTR);
            XAttribute transpColorAttr = fromElem.Attribute(XmlTileSetConstants.VARIANT_TRANSPCOLOR_ATTR);
            if (imageAttr == null) { throw new TileSetException("Image not defined for tile variant!"); }
            if (transpColorAttr == null) { throw new TileSetException("Transparent color not defined for tile variant!"); }

            /// Read the image data.
            string imagePath = Path.Combine(tmpImageDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the new TileVariant object and add it to the tile type.
            IsoTileVariant newVariant = new IsoTileVariant(imageData, XmlHelper.LoadColor(transpColorAttr.Value), tileset);
            tile.AddVariant(newVariant);

            /// Load the cell data changesets.
            foreach (XElement childElem in fromElem.Elements())
            {
                ICellDataChangeSet changeset = LoadCellDataChangeSet(childElem, tileset);
                newVariant.AddCellDataChangeset(changeset);
            }

            /// Register the variant to the tileset.
            tileset.RegisterVariant(newVariant);
        }

        /// <summary>
        /// Load a cell data changeset from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        /// <returns>The loaded cell data changeset.</returns>
        private static ICellDataChangeSet LoadCellDataChangeSet(XElement fromElem, TileSet tileset)
        {
            ICellDataChangeSet retObj = null;
            ICellDataModifier modifier = null;

            /// Load the name of the target field.
            XAttribute fieldAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_FIELD_ATTR);
            if (fieldAttr == null) { throw new TileSetException("Field name not defined for a data changeset element!"); }

            if (fieldAttr.Value == XmlTileSetConstants.CELLDATA_ISWALKABLE_NAME)
            {
                modifier = new WalkabilityFlagModifier(XmlHelper.LoadBool(fromElem.Value));
            }
            else if (fieldAttr.Value == XmlTileSetConstants.CELLDATA_ISBUILDABLE_NAME)
            {
                modifier = new BuildabilityFlagModifier(XmlHelper.LoadBool(fromElem.Value));
            }
            else if (fieldAttr.Value == XmlTileSetConstants.CELLDATA_GROUNDLEVEL_NAME)
            {
                modifier = new GroundLevelModifier(XmlHelper.LoadInt(fromElem.Value));
            }
            if (modifier == null) { throw new TileSetException("Unexpected field name defined for a data changeset element!"); }

            switch (fromElem.Name.LocalName)
            {
                case XmlTileSetConstants.CELLDATACHANGESET_ALL_ELEM:
                    retObj = new CellDataChangeSetBase(modifier, tileset);
                    break;
                case XmlTileSetConstants.CELLDATACHANGESET_CELL_ELEM:
                    XAttribute cellAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_CELL_CELL_ATTR);
                    if (cellAttr == null) { throw new TileSetException("Cell not defined for a cell data changeset element!"); }
                    retObj = new CellChangeSet(XmlHelper.LoadIntVector(cellAttr.Value), modifier, tileset);
                    break;
                case XmlTileSetConstants.CELLDATACHANGESET_COL_ELEM:
                    XAttribute colIndexAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_COL_INDEX_ATTR);
                    if (colIndexAttr == null) { throw new TileSetException("Column not defined for a column data changeset element!"); }
                    retObj = new ColumnChangeSet(XmlHelper.LoadInt(colIndexAttr.Value), modifier, tileset);
                    break;
                case XmlTileSetConstants.CELLDATACHANGESET_QUARTER_ELEM:
                    XAttribute quarterAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_QUARTER_WHICH_ATTR);
                    if (quarterAttr == null) { throw new TileSetException("Quarter not defined for a quarter data changeset element!"); }
                    MapDirection quarter;
                    if (!EnumMap<MapDirection, string>.TryDemap(quarterAttr.Value, out quarter))
                    {
                        throw new TileSetException(string.Format("Unexpected quarter '{0}' defined for quarter data changeset!", quarterAttr.Value));
                    }
                    retObj = new IsoQuarterChangeSet(quarter, modifier, tileset);
                    break;
                case XmlTileSetConstants.CELLDATACHANGESET_RECT_ELEM:
                    XAttribute rectAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_RECT_RECT_ATTR);
                    if (rectAttr == null) { throw new TileSetException("Rectangle not defined for a rectangle data changeset element!"); }
                    retObj = new RectangleChangeSet(XmlHelper.LoadIntRectangle(rectAttr.Value), modifier, tileset);
                    break;
                case XmlTileSetConstants.CELLDATACHANGESET_ROW_ELEM:
                    XAttribute rowIndexAttr = fromElem.Attribute(XmlTileSetConstants.CELLDATACHANGESET_ROW_INDEX_ATTR);
                    if (rowIndexAttr == null) { throw new TileSetException("Row not defined for a row data changeset element!"); }
                    retObj = new RowChangeSet(XmlHelper.LoadInt(rowIndexAttr.Value), modifier, tileset);
                    break;
                default:
                    throw new TileSetException(string.Format("Unexpected data changeset element '{0}'!", fromElem.Name));
            }

            if (retObj == null) { throw new TileSetException("Unable to load cell data changeset!"); }
            return retObj;
        }

        /// <summary>
        /// Loads a conditional branch of a tile type from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tile">The tile type being loaded.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        private static void LoadBranch(XElement fromElem, IsoTileType tile, TileSet tileset)
        {
            /// Check whether the branch element has exactly 2 child elements.
            XElement conditionElem = null;
            XElement actionElem = null;
            int i = 0;
            foreach (XElement child in fromElem.Elements())
            {
                if (i == 0) { conditionElem = child; }
                else if (i == 1) { actionElem = child; }
                else { throw new TileSetException("Unexpected nodes in conditional branch!"); }
                i++;
            }
            if (i != 2) { throw new TileSetException("Missing nodes in conditional branch!"); }

            /// Check the action element.
            if (actionElem.Name != XmlTileSetConstants.THEN_ELEM) { throw new TileSetException(string.Format("Unexpected node '{0}' at conditional branch!", actionElem.Name)); }

            /// Load the condition and start defining the conditional branch.
            IIsoTileCondition condition = LoadCondition(conditionElem, tileset);
            tile.BeginConditionalBranch(condition);

            /// Load the variants of the conditional branch.
            foreach (XElement variantElem in actionElem.Elements(XmlTileSetConstants.VARIANT_ELEM))
            {
                LoadVariant(variantElem, tile, tileset);
            }

            /// Close the conditional branch.
            tile.EndConditionalBranch();
        }

        /// <summary>
        /// Loads the condition of a conditional branch from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        /// <returns>The loaded condition.</returns>
        private static IIsoTileCondition LoadCondition(XElement fromElem, TileSet tileset)
        {
            if (fromElem.Name == XmlTileSetConstants.NEIGHBOURCOND_ELEM)
            {
                /// Load neighbour condition.
                XAttribute whichNeighbourAttr = fromElem.Attribute(XmlTileSetConstants.NEIGHBOURCOND_WHICH_ATTR);
                XAttribute whatCombinationAttr = fromElem.Attribute(XmlTileSetConstants.NEIGHBOURCOND_COMBINATION_ATTR);
                if (whichNeighbourAttr == null) { throw new TileSetException("Neighbour not defined for a neighbour condition!"); }
                if (whatCombinationAttr == null) { throw new TileSetException("Terrain combination not defined for a neighbour condition!"); }

                TerrainCombination combination;
                if (!EnumMap<TerrainCombination, string>.TryDemap(whatCombinationAttr.Value, out combination))
                {
                    throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for neighbour condition!", whatCombinationAttr.Value));
                }

                MapDirection neighbour;
                if (!EnumMap<MapDirection, string>.TryDemap(whichNeighbourAttr.Value, out neighbour))
                {
                    throw new TileSetException(string.Format("Unexpected neighbour direction '{0}' defined for neighbour condition!", whichNeighbourAttr.Value));
                }

                return new NeighbourCondition(combination, neighbour, tileset);
            }
            else if (fromElem.Name == XmlTileSetConstants.COMPLEXCOND_AND_ELEM ||
                     fromElem.Name == XmlTileSetConstants.COMPLEXCOND_OR_ELEM ||
                     fromElem.Name == XmlTileSetConstants.COMPLEXCOND_NOT_ELEM)
            {
                /// Load complex condition.
                LogicalOp logicalOp;
                if (!EnumMap<LogicalOp, string>.TryDemap(fromElem.Name.LocalName, out logicalOp))
                {
                    throw new TileSetException(string.Format("Unexpected logical operator '{0}' defined for complex condition!", fromElem.Name.LocalName));
                }

                List<IIsoTileCondition> subconditions = new List<IIsoTileCondition>();
                foreach (XElement subconditionElem in fromElem.Elements())
                {
                    subconditions.Add(LoadCondition(subconditionElem, tileset));
                }

                return new ComplexCondition(subconditions, logicalOp, tileset);
            }
            else
            {
                throw new TileSetException(string.Format("Unexpected condition element: {0}!", fromElem.Name.LocalName));
            }
        }

        /// <summary>
        /// Enumerates the possible read statuses when reading the variants of a tile type.
        /// </summary>
        private enum TileReadStatus
        {
            None = 0,
            NoCondition = 1,
            ConditionalBranch = 2,
            DefaultBranch = 3
        }

        /// <summary>
        /// Temporary string that contains the directory of the referenced images (TODO: this is a hack).
        /// </summary>
        private static string tmpImageDir;
    }
}
