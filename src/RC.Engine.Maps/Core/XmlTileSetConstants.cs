using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Constants defined for reading and writing tileset XML descriptors.
    /// </summary>
    public static class XmlTileSetConstants
    {
        public const string TILESET_ELEM = "tileset";
        public const string TILESET_NAME_ATTR = "name";
        public const string TERRAINTYPE_ELEM = "terrainType";
        public const string TERRAINTYPE_NAME_ATTR = "name";
        public const string TERRAINTYPE_TRANSLENGTH_ATTR = "transitionLength";
        public const string DECLARETILES_ELEM = "declareTiles";
        public const string SIMPLETILE_ELEM = "simpleTile";
        public const string SIMPLETILE_TERRAIN_ATTR = "terrain";
        public const string MIXEDTILE_ELEM = "mixedTile";
        public const string MIXEDTILE_TERRAINA_ATTR = "terrainA";
        public const string MIXEDTILE_TERRAINB_ATTR = "terrainB";
        public const string MIXEDTILE_COMBINATION_ATTR = "combination";
        public const string VARIANT_ELEM = "variant";
        public const string VARIANT_IMAGE_ATTR = "image";
        public const string VARIANT_TRANSPCOLOR_ATTR = "transparentColor";
        public const string CELLDATACHANGESET_ALL_ELEM = "dataChangesetAll";
        public const string CELLDATACHANGESET_FIELD_ATTR = "field";
        public const string CELLDATACHANGESET_ROW_ELEM = "dataChangesetRow";
        public const string CELLDATACHANGESET_ROW_INDEX_ATTR = "index";
        public const string CELLDATACHANGESET_COL_ELEM = "dataChangesetCol";
        public const string CELLDATACHANGESET_COL_INDEX_ATTR = "index";
        public const string CELLDATACHANGESET_QUARTER_ELEM = "dataChangesetQuarter";
        public const string CELLDATACHANGESET_QUARTER_WHICH_ATTR = "which";
        public const string CELLDATACHANGESET_RECT_ELEM = "dataChangesetRect";
        public const string CELLDATACHANGESET_RECT_RECT_ATTR = "rect";
        public const string CELLDATACHANGESET_CELL_ELEM = "dataChangesetCell";
        public const string CELLDATACHANGESET_CELL_CELL_ATTR = "cell";
        public const string CELLDATA_ISWALKABLE_NAME = "IsWalkable";
        public const string CELLDATA_ISBUILDABLE_NAME = "IsBuildable";
        public const string CELLDATA_GROUNDLEVEL_NAME = "GroundLevel";
        public const string IF_ELEM = "if";
        public const string ELSEIF_ELEM = "elseIf";
        public const string ELSE_ELEM = "else";
        public const string THEN_ELEM = "then";
        public const string NEIGHBOURCOND_ELEM = "neighbourCondition";
        public const string NEIGHBOURCOND_WHICH_ATTR = "which";
        public const string NEIGHBOURCOND_COMBINATION_ATTR = "combination";
        public const string COMPLEXCOND_AND_ELEM = "and";
        public const string COMPLEXCOND_OR_ELEM = "or";
        public const string COMPLEXCOND_NOT_ELEM = "not";
        public const string DECLARETERRAINOBJECTS_ELEM = "declareTerrainObjects";
        public const string TERRAINOBJECT_ELEM = "terrainObject";
        public const string TERRAINOBJ_NAME_ATTR = "name";
        public const string TERRAINOBJ_IMAGE_ATTR = "image";
        public const string TERRAINOBJ_QUADSIZE_ATTR = "quadSize";
        public const string TERRAINOBJ_OFFSET_ATTR = "offset";  // NOT USED
        public const string TERRAINOBJ_TRANSPCOLOR_ATTR = "transparentColor";
        public const string TERRAINOBJ_EXCLUDEAREA_ELEM = "excludeArea";
        public const string TERRAINOBJ_EXCLUDEAREA_RECT_ATTR = "rect";
        public const string TERRAINOBJ_TILECONSTRAINT_ELEM = "tileConstraint";
        public const string TERRAINOBJ_TILECONSTRAINT_QUADCOORDS_ATTR = "quadCoords";
        public const string TERRAINOBJ_TILECONSTRAINT_TERRAIN_ATTR = "terrain";
        public const string TERRAINOBJ_TILECONSTRAINT_TERRAINA_ATTR = "terrainA";
        public const string TERRAINOBJ_TILECONSTRAINT_TERRAINB_ATTR = "terrainB";
        public const string TERRAINOBJ_TILECONSTRAINT_COMBINATIONS_ATTR = "combinations";
    }
}
