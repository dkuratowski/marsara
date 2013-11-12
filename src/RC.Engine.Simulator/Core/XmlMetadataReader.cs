using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Engine.Simulator.PublicInterfaces;
using System.IO;
using RC.Common;
using RC.Common.Configuration;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// This reader can read simulator descriptor XML files.
    /// </summary>
    static class XmlMetadataReader
    {
        /// <summary>
        /// Reads metadata from the given XML document and loads it to the given metadata object.
        /// </summary>
        /// <param name="xmlStr">The string that contains the XML document to read.</param>
        /// <param name="imageDir">The directory where the referenced images can be found. (TODO: this is a hack!)</param>
        /// <param name="metadata">Reference to the metadata object being constructed.</param>
        public static void Read(string xmlStr, string imageDir, SimMetadata metadata)
        {
            if (xmlStr == null) { throw new ArgumentNullException("xmlStr"); }
            if (imageDir == null) { throw new ArgumentNullException("imageDir"); }

            tmpImageDir = imageDir;

            /// Load the XML document.
            XDocument xmlDoc = XDocument.Parse(xmlStr);

            /// Load the building type definitions.
            foreach (XElement buildingTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.BUILDINGTYPE_ELEM))
            {
                LoadBuildingType(buildingTypeElem, metadata);
            }

            /// Load the unit type definitions.
            foreach (XElement unitTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.UNITTYPE_ELEM))
            {
                LoadUnitType(unitTypeElem, metadata);
            }

            /// Load the addon type definitions.
            foreach (XElement addonTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.ADDONTYPE_ELEM))
            {
                LoadAddonType(addonTypeElem, metadata);
            }

            /// Load the upgrade type definitions.
            foreach (XElement upgradeTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.UPGRADETYPE_ELEM))
            {
                LoadUpgradeType(upgradeTypeElem, metadata);
            }
        }

        /// <summary>
        /// Loads a building type definition from the given XML node.
        /// </summary>
        /// <param name="buildingTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadBuildingType(XElement buildingTypeElem, SimMetadata metadata)
        {
            XAttribute nameAttr = buildingTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Building type name not defined!"); }

            BuildingType buildingType = new BuildingType(nameAttr.Value, metadata);
            LoadSimObjectTypeData(buildingTypeElem, buildingType, metadata);
            LoadEntityTypeData(buildingTypeElem, buildingType, metadata);

            metadata.AddBuildingType(buildingType);
        }

        /// <summary>
        /// Loads a unit type definition from the given XML node.
        /// </summary>
        /// <param name="unitTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUnitType(XElement unitTypeElem, SimMetadata metadata)
        {
            XAttribute nameAttr = unitTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Unit type name not defined!"); }

            UnitType unitType = new UnitType(nameAttr.Value, metadata);
            LoadSimObjectTypeData(unitTypeElem, unitType, metadata);
            LoadEntityTypeData(unitTypeElem, unitType, metadata);

            XElement createdInElem = unitTypeElem.Element(XmlMetadataConstants.CREATEDIN_ELEM);
            if (createdInElem != null)
            {
                Tuple<string, string> createdIn = ParseBuildingAddonStr(createdInElem.Value);
                unitType.SetCreatedIn(createdIn.Item1);
                if (createdIn.Item2 != null) { unitType.SetNecessaryAddonName(createdIn.Item2); }
            }

            metadata.AddUnitType(unitType);
        }

        /// <summary>
        /// Loads an addon type definition from the given XML node.
        /// </summary>
        /// <param name="addonTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadAddonType(XElement addonTypeElem, SimMetadata metadata)
        {
            XAttribute nameAttr = addonTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Addon type name not defined!"); }

            AddonType addonType = new AddonType(nameAttr.Value, metadata);
            LoadSimObjectTypeData(addonTypeElem, addonType, metadata);
            LoadEntityTypeData(addonTypeElem, addonType, metadata);

            XElement mainBuildingElem = addonTypeElem.Element(XmlMetadataConstants.MAINBUILDING_ELEM);
            if (mainBuildingElem == null) { throw new SimulatorException("Main building not found for addon type!"); }
            addonType.SetMainBuilding(mainBuildingElem.Value);

            metadata.AddAddonType(addonType);
        }

        /// <summary>
        /// Loads an upgrade type definition from the given XML node.
        /// </summary>
        /// <param name="upgradeTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUpgradeType(XElement upgradeTypeElem, SimMetadata metadata)
        {
            XAttribute nameAttr = upgradeTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Upgrade type name not defined!"); }

            UpgradeType upgradeType = new UpgradeType(nameAttr.Value, metadata);
            LoadSimObjectTypeData(upgradeTypeElem, upgradeType, metadata);

            XElement researchedInElem = upgradeTypeElem.Element(XmlMetadataConstants.RESEARCHEDIN_ELEM);
            if (researchedInElem != null) { upgradeType.SetResearchedIn(researchedInElem.Value); }

            XElement previousLevelElem = upgradeTypeElem.Element(XmlMetadataConstants.PREVIOUSLEVEL_ELEM);
            if (previousLevelElem != null) { upgradeType.SetPreviousLevelName(previousLevelElem.Value); }

            metadata.AddUpgradeType(upgradeType);
        }

        /// <summary>
        /// Loads the necessary data of a simulated object type.
        /// </summary>
        /// <param name="simObjTypeElem">The XML node to load from.</param>
        /// <param name="simObjType">The simulated object type being constructed.</param>
        /// <param name="metadata">The metadata object.</param>
        private static void LoadSimObjectTypeData(XElement simObjTypeElem, SimObjectType simObjType, SimMetadata metadata)
        {
            /// Load the sprite palette of the object type.
            XElement spritePaletteElem = simObjTypeElem.Element(XmlMetadataConstants.SPRITE_ELEM);
            if (spritePaletteElem != null)
            {
                simObjType.SetSpritePalette(LoadSpritePalette(spritePaletteElem, metadata));
            }

            /// Load the cost data of the object type.
            XElement costsDataElem = simObjTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem != null)
            {
                simObjType.SetCostsData(LoadCostsData(costsDataElem, metadata));
            }

            /// Load the requirements of the object type.
            XElement requiresElem = simObjTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                string reqListStr = requiresElem.Value.Trim();
                string[] requirementStrings = reqListStr.Split(',');
                foreach (string reqStr in requirementStrings)
                {
                    Tuple<string, string> buildingAddonPair = ParseBuildingAddonStr(reqStr);
                    simObjType.AddRequirement(new Requirement(buildingAddonPair.Item1, buildingAddonPair.Item2, metadata));
                }
            }
        }

        /// <summary>
        /// Loads the necessary data of an entity type.
        /// </summary>
        /// <param name="entityTypeElem">The XML node to load from.</param>
        /// <param name="entityType">The entity type being constructed.</param>
        /// <param name="metadata">The metadata object.</param>
        private static void LoadEntityTypeData(XElement entityTypeElem, EntityType entityType, SimMetadata metadata)
        {
            XElement genDataElem = entityTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem != null) { entityType.SetGeneralData(LoadGeneralData(genDataElem, metadata)); }

            XElement gndWeaponElem = entityTypeElem.Element(XmlMetadataConstants.GROUNDWEAPON_ELEM);
            if (gndWeaponElem != null) { entityType.SetGroundWeapon(LoadWeaponData(gndWeaponElem, metadata)); }

            XElement airWeaponElem = entityTypeElem.Element(XmlMetadataConstants.AIRWEAPON_ELEM);
            if (airWeaponElem != null) { entityType.SetAirWeapon(LoadWeaponData(airWeaponElem, metadata)); }
        }

        /// <summary>
        /// Loads a sprite palette definition from the given XML node.
        /// </summary>
        /// <param name="spritePaletteElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        /// <returns>The constructed sprite palette definition.</returns>
        private static SpritePalette LoadSpritePalette(XElement spritePaletteElem, SimMetadata metadata)
        {
            XAttribute imageAttr = spritePaletteElem.Attribute(XmlMetadataConstants.SPRITE_IMAGE_ATTR);
            XAttribute transpColorAttr = spritePaletteElem.Attribute(XmlMetadataConstants.SPRITE_TRANSPCOLOR_ATTR);
            XAttribute ownerMaskColorAttr = spritePaletteElem.Attribute(XmlMetadataConstants.SPRITE_OWNERMASKCOLOR_ATTR);
            if (imageAttr == null) { throw new SimulatorException("Image not defined for sprite palette!"); }

            /// Read the image data.
            string imagePath = System.IO.Path.Combine(tmpImageDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the sprite palette object.
            SpritePalette spritePalette = new SpritePalette(imageData,
                                                            transpColorAttr != null ? transpColorAttr.Value : null,
                                                            ownerMaskColorAttr != null ? ownerMaskColorAttr.Value : null,
                                                            metadata);

            /// Load the frames.
            foreach (XElement frameElem in spritePaletteElem.Elements(XmlMetadataConstants.FRAME_ELEM))
            {
                XAttribute frameNameAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_NAME_ATTR);
                XAttribute sourceRegionAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_SOURCEREGION_ATTR);
                XAttribute offsetAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_OFFSET_ATTR);
                if (frameNameAttr == null) { throw new SimulatorException("Frame name not defined in sprite palette!"); }
                if (sourceRegionAttr == null) { throw new SimulatorException("Source region not defined in sprite palette!"); }
                if (offsetAttr == null) { throw new SimulatorException("Offset not defined in sprite palette!"); }
                spritePalette.AddFrame(frameNameAttr.Value, XmlHelper.LoadIntRectangle(sourceRegionAttr.Value), XmlHelper.LoadIntVector(offsetAttr.Value));
            }
            return spritePalette;
        }

        /// <summary>
        /// Loads the general data of a building/unit/addon type from the given element.
        /// </summary>
        /// <param name="genDataElem">The XML element to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        private static GeneralData LoadGeneralData(XElement genDataElem, SimMetadata metadata)
        {
            XElement areaElem = genDataElem.Element(XmlMetadataConstants.GENDATA_AREA_ELEM);
            XElement armorElem = genDataElem.Element(XmlMetadataConstants.GENDATA_ARMOR_ELEM);
            XElement maxEnergyElem = genDataElem.Element(XmlMetadataConstants.GENDATA_MAXENERGY_ELEM);
            XElement maxHPElem = genDataElem.Element(XmlMetadataConstants.GENDATA_MAXHP_ELEM);
            XElement sightRangeElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SIGHTRANGE_ELEM);
            XElement sizeElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SIZE_ELEM);
            XElement speedElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SPEED_ELEM);

            if (areaElem == null) { throw new SimulatorException("Area not defined!"); }
            if (maxHPElem == null) { throw new SimulatorException("MaxHP not defined!"); }
            if (sightRangeElem == null) { throw new SimulatorException("SightRange not defined!"); }
            if (sizeElem == null) { throw new SimulatorException("Size not defined!"); }

            GeneralData genData = new GeneralData(metadata);
            genData.SetArea(XmlHelper.LoadNumVector(areaElem.Value));
            genData.SetMaxHP(XmlHelper.LoadInt(maxHPElem.Value));
            genData.SetSightRange(XmlHelper.LoadInt(sightRangeElem.Value));

            SizeEnum size;
            if (!EnumMap<SizeEnum, string>.Demap(sizeElem.Value, out size))
            {
                throw new SimulatorException(string.Format("Unexpected size '{0}' defined in general data!", sizeElem.Value));
            }
            genData.SetSize(size);

            if (armorElem != null) { genData.SetArmor(XmlHelper.LoadInt(armorElem.Value)); }
            if (maxEnergyElem != null) { genData.SetMaxEnergy(XmlHelper.LoadInt(maxEnergyElem.Value)); }
            if (speedElem != null) { genData.SetSpeed(XmlHelper.LoadNum(speedElem.Value)); }

            return genData;
        }

        /// <summary>
        /// Loads the costs data of a building/unit/addon/upgrade type from the given element.
        /// </summary>
        /// <param name="costsDataElem">The XML element to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        private static CostsData LoadCostsData(XElement costsDataElem, SimMetadata metadata)
        {
            XElement buildTimeElem = costsDataElem.Element(XmlMetadataConstants.COSTS_BUILDTIME_ELEM);
            XElement foodCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_FOODCOST_ELEM);
            XElement mineralCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_MINERALCOST_ELEM);
            XElement gasCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_GASCOST_ELEM);

            if (buildTimeElem == null) { throw new SimulatorException("BuildTime not defined!"); }

            CostsData costsData = new CostsData(metadata);
            costsData.SetBuildTime(XmlHelper.LoadInt(buildTimeElem.Value));

            if (foodCostElem != null) { costsData.SetFoodCost(XmlHelper.LoadInt(foodCostElem.Value)); }
            if (mineralCostElem != null) { costsData.SetMineralCost(XmlHelper.LoadInt(mineralCostElem.Value)); }
            if (gasCostElem != null) { costsData.SetGasCost(XmlHelper.LoadInt(gasCostElem.Value)); }

            return costsData;
        }

        /// <summary>
        /// Loads the weapon data of a building/unit type from the given element.
        /// </summary>
        /// <param name="weaponDataElem">The XML element to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        private static WeaponData LoadWeaponData(XElement weaponDataElem, SimMetadata metadata)
        {
            XElement cooldownElem = weaponDataElem.Element(XmlMetadataConstants.WPN_COOLDOWN_ELEM);
            XElement damageElem = weaponDataElem.Element(XmlMetadataConstants.WPN_DAMAGE_ELEM);
            XElement damageTypeElem = weaponDataElem.Element(XmlMetadataConstants.WPN_DAMAGETYPE_ELEM);
            XElement incrementElem = weaponDataElem.Element(XmlMetadataConstants.WPN_INCREMENT_ELEM);
            XElement rangeMaxElem = weaponDataElem.Element(XmlMetadataConstants.WPN_RANGEMAX_ELEM);
            XElement rangeMinElem = weaponDataElem.Element(XmlMetadataConstants.WPN_RANGEMIN_ELEM);
            XElement splashTypeElem = weaponDataElem.Element(XmlMetadataConstants.WPN_SPLASHTYPE_ELEM);

            if (cooldownElem == null) { throw new SimulatorException("Cooldown not defined!"); }
            if (damageElem == null) { throw new SimulatorException("Damage not defined!"); }
            if (damageTypeElem == null) { throw new SimulatorException("DamageType not defined!"); }
            if (rangeMaxElem == null) { throw new SimulatorException("RangeMax not defined!"); }

            WeaponData weaponData = new WeaponData(metadata);
            weaponData.SetCooldown(XmlHelper.LoadInt(cooldownElem.Value));
            weaponData.SetDamage(XmlHelper.LoadInt(damageElem.Value));
            weaponData.SetRangeMax(XmlHelper.LoadInt(rangeMaxElem.Value));

            DamageTypeEnum damageType;
            if (!EnumMap<DamageTypeEnum, string>.Demap(damageTypeElem.Value, out damageType))
            {
                throw new SimulatorException(string.Format("Unexpected damage type '{0}' defined in weapon data!", damageTypeElem.Value));
            }
            weaponData.SetDamageType(damageType);

            if (incrementElem != null) { weaponData.SetIncrement(XmlHelper.LoadInt(incrementElem.Value)); }
            if (rangeMinElem != null) { weaponData.SetRangeMin(XmlHelper.LoadInt(rangeMinElem.Value)); }
            if (splashTypeElem != null)
            {
                SplashTypeEnum splashType;
                if (!EnumMap<SplashTypeEnum, string>.Demap(splashTypeElem.Value, out splashType))
                {
                    throw new SimulatorException(string.Format("Unexpected splash type '{0}' defined in weapon data!", splashTypeElem.Value));
                }
                weaponData.SetSplashType(splashType);
            }

            return weaponData;
        }

        /// <summary>
        /// Parses the given string that contains a building type name with an optional addon type name.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>
        /// A pair whose 1st element is the name of the building type and the 2nd element is the name of the
        /// addon type or null.
        /// </returns>
        private static Tuple<string, string> ParseBuildingAddonStr(string str)
        {
            str = str.Trim();
            if (str == null || str.Length == 0) { throw new ArgumentNullException("str"); }
            string[] splittedStr = str.Split('-');
            Tuple<string, string> buildingAddonPair = null;
            if (splittedStr.Length == 1)
            {
                buildingAddonPair = new Tuple<string, string>(splittedStr[0].Trim(), null);
            }
            else if (splittedStr.Length == 2)
            {
                buildingAddonPair = new Tuple<string, string>(splittedStr[0].Trim(), splittedStr[1].Trim());
            }
            else
            {
                throw new ArgumentException("Syntax error!", "str");
            }

            return buildingAddonPair;
        }

        /// <summary>
        /// Temporary string that contains the directory of the referenced images (TODO: this is a hack).
        /// </summary>
        private static string tmpImageDir;
    }
}
