using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Constants defined for reading and writing metadata XML descriptors.
    /// </summary>
    static class XmlMetadataConstants
    {
        public const string METADATA_ELEM = "metadata";
        public const string BUILDINGTYPE_ELEM = "buildingType";
        public const string ADDONTYPE_ELEM = "addonType";
        public const string UNITTYPE_ELEM = "unitType";
        public const string UPGRADETYPE_ELEM = "upgradeType";
        public const string MISSILETYPE_ELEM = "missileType";
        public const string CUSTOMTYPE_ELEM = "customType";
        public const string SHADOWPALETTE_ELEM = "shadowPalette";
        public const string TYPE_NAME_ATTR = "name";
        public const string TYPE_DISPLAYEDNAME_ATTR = "displayedName";
        public const string TYPE_HASOWNER_ATTR = "hasOwner";
        public const string GENERALDATA_ELEM = "generalData";
        public const string COSTS_ELEM = "costs";
        public const string SHADOWDATA_ELEM = "shadowData";
        public const string GROUNDWEAPON_ELEM = "groundWeapon";
        public const string AIRWEAPON_ELEM = "airWeapon";
        public const string CREATEDIN_ELEM = "createdIn";
        public const string MAINBUILDING_ELEM = "mainBuilding";
        public const string EFFECTS_ELEM = "effects";
        public const string EFFECT_TARGETTYPE_ATTR = "targetType";
        public const string RESEARCHEDIN_ELEM = "researchedIn";
        public const string PREVIOUSLEVEL_ELEM = "previousLevel";
        public const string REQUIRES_ELEM = "requires";
        public const string LAUNCHANIMATION_ELEM = "launchAnimation";
        public const string LAUNCH_DELAY_ATTR = "launchDelay";
        public const string FLYINGANIMATION_ELEM = "flyingAnimation";
        public const string TRAILANIMATION_ELEM = "trailAnimation";
        public const string TRAILANIMATION_FREQUENCY_ATTR = "frequency";
        public const string IMPACTANIMATION_ELEM = "impactAnimation";
        public const string SPRITEPALETTE_ELEM = "spritePalette";
        public const string HPICONPALETTE_ELEM = "hpIconPalette";
        public const string ANIMPALETTE_ELEM = "animationPalette";
        public const string ANIMATION_ELEM = "animation";
        public const string ANIMATION_NAME_ATTR = "name";
        public const string ANIMATION_ISPREVIEW_ATTR = "isPreview";
        public const string FRAME_ELEM = "frame";
        public const string FRAME_SPRITES_ATTR = "sprites";
        public const string FRAME_DURATION_ATTR = "duration";
        public const string LABEL_ELEM = "label";
        public const string LABEL_NAME_ATTR = "name";
        public const string GOTO_ELEM = "goto";
        public const string GOTO_LABEL_ATTR = "label";
        public const string REPEAT_ELEM = "repeat";
        public const string GENDATA_AREA_ELEM = "area";
        public const string GENDATA_ARMOR_ELEM = "armor";
        public const string GENDATA_MAXENERGY_ELEM = "maxEnergy";
        public const string GENDATA_MAXHP_ELEM = "maxHP";
        public const string GENDATA_SIGHTRANGE_ELEM = "sightRange";
        public const string GENDATA_SIZE_ELEM = "size";
        public const string GENDATA_SPEED_ELEM = "speed";
        public const string COSTS_BUILDTIME_ELEM = "buildTime";
        public const string COSTS_SUPPLYUSED_ELEM = "supplyUsed";
        public const string COSTS_SUPPLYPROVIDED_ELEM = "supplyProvided";
        public const string COSTS_GASCOST_ELEM = "gas";
        public const string COSTS_MINERALCOST_ELEM = "mineral";
        public const string SHADOWDATA_SPRITENAME_ELEM = "spriteName";
        public const string SHADOWDATA_OFFSET_ELEM = "centerOffset";
        public const string WPN_NAME_ATTR = "name";
        public const string WPN_DISPLAYEDNAME_ATTR = "displayedName";
        public const string WPN_COOLDOWN_ELEM = "cooldown";
        public const string WPN_DAMAGE_ELEM = "damage";
        public const string WPN_DAMAGETYPE_ELEM = "damageType";
        public const string WPN_INCREMENT_ELEM = "increment";
        public const string WPN_RANGEMAX_ELEM = "rangeMax";
        public const string WPN_RANGEMIN_ELEM = "rangeMin";
        public const string WPN_SPLASHTYPE_ELEM = "splashType";
        public const string WPN_MISSILE_ELEM = "missile";
        public const string WPN_MISSILE_TYPE_ATTR = "type";
        public const string WPN_MISSILE_LAUNCH_ELEM = "launch";
        public const string WPN_MISSILE_LAUNCH_DIR_ATTR = "direction";
        public const string WPN_MISSILE_LAUNCH_POS_ATTR = "relativePosition";
    }
}
