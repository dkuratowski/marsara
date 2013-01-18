using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using RC.Common.Diagnostics;
using RC.Common.Configuration;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// This reader can read a tileset descriptor XML file and construct a TileSet object from it.
    /// </summary>
    static class XmlTileSetReader
    {
        /// <summary>
        /// Reads the given XML file, and constructs a TileSet object from it.
        /// </summary>
        /// <param name="xmlFile">The XML file to read.</param>
        /// <returns>The constructed TileSet object.</returns>
        public static TileSet Read(string xmlFile)
        {
            if (xmlFile == null) { throw new ArgumentNullException("xmlFile"); }

            /// Load the XML document.
            XDocument xmlDoc = XDocument.Load(xmlFile);
            XAttribute tilesetNameAttr = xmlDoc.Root.Attribute(XmlTileSetConstants.TILESET_NAME_ATTR);
            XElement terrainTreeElem = xmlDoc.Root.Element(XmlTileSetConstants.TERRAINTYPE_ELEM);
            XElement declareFieldsElem = xmlDoc.Root.Element(XmlTileSetConstants.DECLAREFIELDS_ELEM);
            XElement declareTilesElem = xmlDoc.Root.Element(XmlTileSetConstants.DECLARETILES_ELEM);
            if (tilesetNameAttr == null) { throw new TileSetException("Tileset name not defined!"); }
            if (terrainTreeElem == null) { throw new TileSetException("Terrain-tree not defined!"); }
            if (declareFieldsElem == null) { throw new TileSetException("Field declarations not found!"); }
            if (declareTilesElem == null) { throw new TileSetException("Tile declarations not found"); }

            tmpTilesetFile = new FileInfo(xmlFile);

            /// Create the TileSet object.
            TileSet tileset = new TileSet(tilesetNameAttr.Value);

            /// Load the terrain tree.
            LoadTerrainTree(terrainTreeElem, null, tileset);

            /// Load the cell data field declarations.
            foreach (XElement fieldDeclarationElem in declareFieldsElem.Elements(XmlTileSetConstants.DECLAREFIELD_ELEM))
            {
                LoadFieldDeclaration(fieldDeclarationElem, tileset);
            }

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
            TerrainType currentTerrain = tileset.GetTerrainType(nameAttr.Value);
            foreach (XElement childTerrainElem in fromElem.Elements(XmlTileSetConstants.TERRAINTYPE_ELEM))
            {
                LoadTerrainTree(childTerrainElem, currentTerrain, tileset);
            }
        }

        /// <summary>
        /// Loads the data field declarations from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The TileSet to load to.</param>
        private static void LoadFieldDeclaration(XElement fromElem, TileSet tileset)
        {
            XAttribute nameAttr = fromElem.Attribute(XmlTileSetConstants.DECLAREFIELD_NAME_ATTR);
            XAttribute typeAttr = fromElem.Attribute(XmlTileSetConstants.DECLAREFIELD_TYPE_ATTR);
            if (nameAttr == null) { throw new TileSetException("Data field name not defined!"); }
            if (typeAttr == null) { throw new TileSetException("Data field type not defined!"); }

            /// Try to parse the type string.
            CellDataType type;
            if (!EnumMap<CellDataType, string>.Demap(typeAttr.Value, out type))
            {
                throw new TileSetException(string.Format("Unexpected data type {0} defined for field {1}.", typeAttr.Value, nameAttr.Value));
            }

            /// Declare the field...
            tileset.DeclareField(nameAttr.Value, type);

            /// ... and load it's default value.
            int fieldIdx = tileset.GetFieldIndex(nameAttr.Value);
            switch (type)
            {
                case CellDataType.BOOL:
                    tileset.DefaultValues.WriteBool(fieldIdx, XmlHelper.LoadBool(fromElem.Value));
                    break;
                case CellDataType.INT:
                    tileset.DefaultValues.WriteInt(fieldIdx, XmlHelper.LoadInt(fromElem.Value));
                    break;
                default:
                    throw new TileSetException("Unexpected data field type!");
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
            TileType tile = tileset.GetSimpleTileType(terrainAttr.Value);

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
            if (!EnumMap<TerrainCombination, string>.Demap(combinationAttr.Value, out combination))
            {
                throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for mixed tile.", combinationAttr.Value));
            }

            tileset.CreateMixedTileType(terrainAAttr.Value, terrainBAttr.Value, combination);
            TileType tile = tileset.GetMixedTileType(terrainAAttr.Value, terrainBAttr.Value, combination);

            LoadVariants(fromElem, tile, tileset);
        }

        /// <summary>
        /// Loads the variants of a tile type from the XML element into the given tileset.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tile">The tile type to load to.</param>
        /// <param name="tileset">The tileset of the tile type.</param>
        private static void LoadVariants(XElement fromElem, TileType tile, TileSet tileset)
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
        private static void LoadVariant(XElement fromElem, TileType tile, TileSet tileset)
        {
            XAttribute imageAttr = fromElem.Attribute(XmlTileSetConstants.VARIANT_IMAGE_ATTR);
            if (imageAttr == null) { throw new TileSetException("Image not defined for tile variant!"); }

            /// Read the image data.
            string imagePath = Path.Combine(tmpTilesetFile.DirectoryName, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the new TileVariant object and add it to the tile type.
            TileVariant newVariant = new TileVariant(imageData, tileset);
            tile.AddVariant(newVariant);

            /// Load the navigation cell data overwritings and the properties.
            foreach (XElement childElem in fromElem.Elements())
            {
                if (childElem.Name.LocalName == XmlTileSetConstants.VARIANTPROP_ELEM)
                {
                    XAttribute propNameAttr = childElem.Attribute(XmlTileSetConstants.VARIANTPROP_NAME_ATTR);
                    if (propNameAttr == null) { throw new TileSetException("Variant property name not defined!"); }
                    newVariant.AddProperty(propNameAttr.Value, childElem.Value);
                }
                else
                {
                    ITileDataOverwriting overwriting = LoadOverwriting(childElem, tileset);
                    newVariant.AddOverwriting(overwriting);
                }
            }

            /// Register the variant to the tileset.
            tileset.RegisterVariant(newVariant);
        }

        /// <summary>
        /// Load a cell data overwriting from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        /// <returns>The loaded cell data overwriting.</returns>
        private static ITileDataOverwriting LoadOverwriting(XElement fromElem, TileSet tileset)
        {
            ITileDataOverwriting retObj = null;

            /// Load the name of the target field.
            XAttribute fieldAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRT_FIELD_ATTR);
            if (fieldAttr == null) { throw new TileSetException("Field name not defined for data overwriting element!"); }
            CellDataType fieldType = tileset.GetFieldType(tileset.GetFieldIndex(fieldAttr.Value));

            switch (fromElem.Name.LocalName)
            {
                case XmlTileSetConstants.DATAOVERWRTALL_ELEM:
                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new TileDataOverwriting(fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new TileDataOverwriting(fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                case XmlTileSetConstants.DATAOVERWRTCELL_ELEM:
                    /// Load the target cell.
                    XAttribute cellAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRTCELL_CELL_ATTR);
                    if (cellAttr == null) { throw new TileSetException("Cell not defined for a cell data overwriting element!"); }

                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new CellOverwriting(XmlHelper.LoadVector(cellAttr.Value), fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new CellOverwriting(XmlHelper.LoadVector(cellAttr.Value), fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                case XmlTileSetConstants.DATAOVERWRTCOL_ELEM:
                    /// Load the target column.
                    XAttribute colIndexAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRTCOL_INDEX_ATTR);
                    if (colIndexAttr == null) { throw new TileSetException("Column not defined for a column data overwriting element!"); }

                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new ColOverwriting(XmlHelper.LoadInt(colIndexAttr.Value), fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new ColOverwriting(XmlHelper.LoadInt(colIndexAttr.Value), fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                case XmlTileSetConstants.DATAOVERWRTQUARTER_ELEM:
                    /// Load the target quarter.
                    XAttribute quarterAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRTQUARTER_WHICH_ATTR);
                    if (quarterAttr == null) { throw new TileSetException("Quarter not defined for a quarter data overwriting element!"); }
                    MapDirection quarter;
                    if (!EnumMap<MapDirection, string>.Demap(quarterAttr.Value, out quarter))
                    {
                        throw new TileSetException(string.Format("Unexpected quarter '{0}' defined for quarter data overwriting!", quarterAttr.Value));
                    }

                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new QuarterOverwriting(quarter, fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new QuarterOverwriting(quarter, fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                case XmlTileSetConstants.DATAOVERWRTRECT_ELEM:
                    /// Load the target rectangle.
                    XAttribute rectAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRTRECT_RECT_ATTR);
                    if (rectAttr == null) { throw new TileSetException("Rectangle not defined for a rectangle data overwriting element!"); }

                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new RectOverwriting(XmlHelper.LoadRectangle(rectAttr.Value), fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new RectOverwriting(XmlHelper.LoadRectangle(rectAttr.Value), fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                case XmlTileSetConstants.DATAOVERWRTROW_ELEM:
                    /// Load the target row.
                    XAttribute rowIndexAttr = fromElem.Attribute(XmlTileSetConstants.DATAOVERWRTROW_INDEX_ATTR);
                    if (rowIndexAttr == null) { throw new TileSetException("Row not defined for a row data overwriting element!"); }

                    /// Create the overwriting object.
                    if (fieldType == CellDataType.BOOL)
                    {
                        retObj = new RowOverwriting(XmlHelper.LoadInt(rowIndexAttr.Value), fieldAttr.Value, XmlHelper.LoadBool(fromElem.Value), tileset);
                    }
                    else if (fieldType == CellDataType.INT)
                    {
                        retObj = new RowOverwriting(XmlHelper.LoadInt(rowIndexAttr.Value), fieldAttr.Value, XmlHelper.LoadInt(fromElem.Value), tileset);
                    }
                    break;

                default:
                    throw new TileSetException(string.Format("Unexpected data overwriting element '{0}'!", fromElem.Name));
            }

            if (retObj == null) { throw new TileSetException("Unable to load data overwriting!"); }
            return retObj;
        }

        /// <summary>
        /// Loads a conditional branch of a tile type from the given XML element.
        /// </summary>
        /// <param name="fromElem">The XML element to load from.</param>
        /// <param name="tile">The tile type being loaded.</param>
        /// <param name="tileset">The tileset being loaded.</param>
        private static void LoadBranch(XElement fromElem, TileType tile, TileSet tileset)
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
            ITileCondition condition = LoadCondition(conditionElem, tileset);
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
        private static ITileCondition LoadCondition(XElement fromElem, TileSet tileset)
        {
            if (fromElem.Name == XmlTileSetConstants.NEIGHBOURCOND_ELEM)
            {
                /// Load neighbour condition.
                XAttribute whichNeighbourAttr = fromElem.Attribute(XmlTileSetConstants.NEIGHBOURCOND_WHICH_ATTR);
                XAttribute whatCombinationAttr = fromElem.Attribute(XmlTileSetConstants.NEIGHBOURCOND_COMBINATION_ATTR);
                if (whichNeighbourAttr == null) { throw new TileSetException("Neighbour not defined for a neighbour condition!"); }
                if (whatCombinationAttr == null) { throw new TileSetException("Terrain combination not defined for a neighbour condition!"); }

                TerrainCombination combination;
                if (!EnumMap<TerrainCombination, string>.Demap(whatCombinationAttr.Value, out combination))
                {
                    throw new TileSetException(string.Format("Unexpected terrain combination '{0}' defined for neighbour condition!", whatCombinationAttr.Value));
                }

                MapDirection neighbour;
                if (!EnumMap<MapDirection, string>.Demap(whichNeighbourAttr.Value, out neighbour))
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
                if (!EnumMap<LogicalOp, string>.Demap(fromElem.Name.LocalName, out logicalOp))
                {
                    throw new TileSetException(string.Format("Unexpected logical operator '{0}' defined for complex condition!", fromElem.Name.LocalName));
                }

                List<ITileCondition> subconditions = new List<ITileCondition>();
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
        /// Temporary reference to the tileset XML file.
        /// </summary>
        private static FileInfo tmpTilesetFile;
    }
}
