using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
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
        public const string DECLAREFIELDS_ELEM = "declareFields";
        public const string DECLAREFIELD_ELEM = "declareField";
        public const string DECLAREFIELD_NAME_ATTR = "name";
        public const string DECLAREFIELD_TYPE_ATTR = "type";
        public const string DECLARETILES_ELEM = "declareTiles";
        public const string SIMPLETILE_ELEM = "simpleTile";
        public const string SIMPLETILE_TERRAIN_ATTR = "terrain";
        public const string MIXEDTILE_ELEM = "mixedTile";
        public const string MIXEDTILE_TERRAINA_ATTR = "terrainA";
        public const string MIXEDTILE_TERRAINB_ATTR = "terrainB";
        public const string MIXEDTILE_COMBINATION_ATTR = "combination";
        public const string VARIANT_ELEM = "variant";
        public const string VARIANT_IMAGE_ATTR = "image";
        public const string VARIANTPROP_ELEM = "property";
        public const string VARIANTPROP_NAME_ATTR = "name";
        public const string DATAOVERWRTALL_ELEM = "dataOverwriteAll";
        public const string DATAOVERWRT_FIELD_ATTR = "field";
        public const string DATAOVERWRTROW_ELEM = "dataOverwriteRow";
        public const string DATAOVERWRTROW_INDEX_ATTR = "index";
        public const string DATAOVERWRTCOL_ELEM = "dataOverwriteCol";
        public const string DATAOVERWRTCOL_INDEX_ATTR = "index";
        public const string DATAOVERWRTQUARTER_ELEM = "dataOverwriteQuarter";
        public const string DATAOVERWRTQUARTER_WHICH_ATTR = "which";
        public const string DATAOVERWRTRECT_ELEM = "dataOverwriteRect";
        public const string DATAOVERWRTRECT_RECT_ATTR = "rect";
        public const string DATAOVERWRTCELL_ELEM = "dataOverwriteCell";
        public const string DATAOVERWRTCELL_CELL_ATTR = "cell";
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
    }
}
