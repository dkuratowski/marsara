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
        public static void Read(string xmlStr, string imageDir, Metadata metadata)
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
        private static void LoadBuildingType(XElement buildingTypeElem, Metadata metadata)
        {
            XAttribute nameAttr = buildingTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Building type name not defined!"); }

            /// Load the sprite palette of the building type.
            XElement spritePaletteElem = buildingTypeElem.Element(XmlMetadataConstants.SPRITE_ELEM);
            if (spritePaletteElem == null) { throw new SimulatorException("Sprite palette not defined for building type!"); }

            BuildingType buildingType = new BuildingType(nameAttr.Value, LoadSpritePalette(spritePaletteElem));

            XElement genDataElem = buildingTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem == null) { throw new SimulatorException("General data not found for building type!"); }
            buildingType.GeneralData = LoadGeneralData(genDataElem);

            XElement costsDataElem = buildingTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem == null) { throw new SimulatorException("Costs data not found for building type!"); }
            buildingType.Costs = LoadCostsData(costsDataElem);

            XElement gndWeaponElem = buildingTypeElem.Element(XmlMetadataConstants.GROUNDWEAPON_ELEM);
            if (gndWeaponElem != null) { buildingType.GroundWeapon = LoadWeaponData(gndWeaponElem); }

            XElement airWeaponElem = buildingTypeElem.Element(XmlMetadataConstants.AIRWEAPON_ELEM);
            if (airWeaponElem != null) { buildingType.AirWeapon = LoadWeaponData(airWeaponElem); }

            XElement requiresElem = buildingTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                foreach (Requirement requirement in LoadRequirements(requiresElem.Value))
                {
                    buildingType.AddRequirement(requirement);
                }
            }

            metadata.AddBuildingType(buildingType);
        }

        /// <summary>
        /// Loads a unit type definition from the given XML node.
        /// </summary>
        /// <param name="unitTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUnitType(XElement unitTypeElem, Metadata metadata)
        {
            XAttribute nameAttr = unitTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Unit type name not defined!"); }

            /// Load the sprite palette of the unit type.
            XElement spritePaletteElem = unitTypeElem.Element(XmlMetadataConstants.SPRITE_ELEM);
            if (spritePaletteElem == null) { throw new SimulatorException("Sprite palette not defined for unit type!"); }

            UnitType unitType = new UnitType(nameAttr.Value, LoadSpritePalette(spritePaletteElem));

            XElement genDataElem = unitTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem == null) { throw new SimulatorException("General data not found for unit type!"); }
            unitType.GeneralData = LoadGeneralData(genDataElem);

            XElement costsDataElem = unitTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem == null) { throw new SimulatorException("Costs data not found for unit type!"); }
            unitType.Costs = LoadCostsData(costsDataElem);

            XElement gndWeaponElem = unitTypeElem.Element(XmlMetadataConstants.GROUNDWEAPON_ELEM);
            if (gndWeaponElem != null) { unitType.GroundWeapon = LoadWeaponData(gndWeaponElem); }

            XElement airWeaponElem = unitTypeElem.Element(XmlMetadataConstants.AIRWEAPON_ELEM);
            if (airWeaponElem != null) { unitType.AirWeapon = LoadWeaponData(airWeaponElem); }

            XElement createdInElem = unitTypeElem.Element(XmlMetadataConstants.CREATEDIN_ELEM);
            if (createdInElem != null)
            {
                Tuple<string, string> createdIn = ParseBuildingAddonStr(createdInElem.Value);
                unitType.CreatedIn = createdIn.Item1;
                unitType.NecessaryAddonName = createdIn.Item2;
            }

            XElement requiresElem = unitTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                foreach (Requirement requirement in LoadRequirements(requiresElem.Value))
                {
                    unitType.AddRequirement(requirement);
                }
            }

            metadata.AddUnitType(unitType);
        }

        /// <summary>
        /// Loads an addon type definition from the given XML node.
        /// </summary>
        /// <param name="addonTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadAddonType(XElement addonTypeElem, Metadata metadata)
        {
            XAttribute nameAttr = addonTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Addon type name not defined!"); }

            /// Load the sprite palette of the addon type.
            XElement spritePaletteElem = addonTypeElem.Element(XmlMetadataConstants.SPRITE_ELEM);
            if (spritePaletteElem == null) { throw new SimulatorException("Sprite palette not defined for addon type!"); }

            AddonType addonType = new AddonType(nameAttr.Value, LoadSpritePalette(spritePaletteElem));

            XElement genDataElem = addonTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem == null) { throw new SimulatorException("General data not found for addon type!"); }
            addonType.GeneralData = LoadGeneralData(genDataElem);

            XElement costsDataElem = addonTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem == null) { throw new SimulatorException("Costs data not found for addon type!"); }
            addonType.Costs = LoadCostsData(costsDataElem);

            XElement mainBuildingElem = addonTypeElem.Element(XmlMetadataConstants.MAINBUILDING_ELEM);
            if (mainBuildingElem == null) { throw new SimulatorException("Main building not found for addon type!"); }
            addonType.MainBuilding = mainBuildingElem.Value;

            XElement requiresElem = addonTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                foreach (Requirement requirement in LoadRequirements(requiresElem.Value))
                {
                    addonType.AddRequirement(requirement);
                }
            }

            metadata.AddAddonType(addonType);
        }

        /// <summary>
        /// Loads an upgrade type definition from the given XML node.
        /// </summary>
        /// <param name="upgradeTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUpgradeType(XElement upgradeTypeElem, Metadata metadata)
        {
            XAttribute nameAttr = upgradeTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Upgrade type name not defined!"); }

            UpgradeType upgradeType = new UpgradeType(nameAttr.Value);

            XElement costsDataElem = upgradeTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem == null) { throw new SimulatorException("Costs data not found for addon type!"); }
            upgradeType.Costs = LoadCostsData(costsDataElem);

            XElement researchedInElem = upgradeTypeElem.Element(XmlMetadataConstants.RESEARCHEDIN_ELEM);
            if (researchedInElem != null) { upgradeType.ResearchedIn = researchedInElem.Value; }

            XElement previousLevelElem = upgradeTypeElem.Element(XmlMetadataConstants.PREVIOUSLEVEL_ELEM);
            if (previousLevelElem != null) { upgradeType.PreviousLevelName = previousLevelElem.Value; }

            XElement requiresElem = upgradeTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                foreach (Requirement requirement in LoadRequirements(requiresElem.Value))
                {
                    upgradeType.AddRequirement(requirement);
                }
            }

            metadata.AddUpgradeType(upgradeType);
        }

        /// <summary>
        /// Loads a sprite palette definition from the given XML node.
        /// </summary>
        /// <param name="spritePaletteElem">The XML node to load from.</param>
        /// <returns>The constructed sprite palette definition.</returns>
        private static SpritePalette LoadSpritePalette(XElement spritePaletteElem)
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
                                                            ownerMaskColorAttr != null ? ownerMaskColorAttr.Value : null);

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
        private static GeneralData LoadGeneralData(XElement genDataElem)
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

            GeneralData genData = new GeneralData()
            {
                Area = new ConstValue<RCNumVector>(XmlHelper.LoadNumVector(areaElem.Value)),
                MaxHP = new ConstValue<int>(XmlHelper.LoadInt(maxHPElem.Value)),
                SightRange = new ConstValue<int>(XmlHelper.LoadInt(sightRangeElem.Value))
            };
            SizeEnum size;
            if (!EnumMap<SizeEnum, string>.Demap(sizeElem.Value, out size))
            {
                throw new SimulatorException(string.Format("Unexpected size '{0}' defined in general data!", sizeElem.Value));
            }
            genData.Size = new ConstValue<SizeEnum>(size);

            if (armorElem != null) { genData.Armor = new ConstValue<int>(XmlHelper.LoadInt(armorElem.Value)); }
            if (maxEnergyElem != null) { genData.MaxEnergy = new ConstValue<int>(XmlHelper.LoadInt(maxEnergyElem.Value)); }
            if (speedElem != null) { genData.Speed = new ConstValue<RCNumber>(XmlHelper.LoadNum(speedElem.Value)); }

            return genData;
        }

        /// <summary>
        /// Loads the costs data of a building/unit/addon/upgrade type from the given element.
        /// </summary>
        /// <param name="costsDataElem">The XML element to load from.</param>
        private static CostsData LoadCostsData(XElement costsDataElem)
        {
            XElement buildTimeElem = costsDataElem.Element(XmlMetadataConstants.COSTS_BUILDTIME_ELEM);
            XElement foodCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_FOODCOST_ELEM);
            XElement mineralCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_MINERALCOST_ELEM);
            XElement gasCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_GASCOST_ELEM);

            if (buildTimeElem == null) { throw new SimulatorException("BuildTime not defined!"); }

            CostsData costsData = new CostsData()
            {
                BuildTime = new ConstValue<int>(XmlHelper.LoadInt(buildTimeElem.Value)),
            };

            if (foodCostElem != null) { costsData.FoodCost = new ConstValue<int>(XmlHelper.LoadInt(foodCostElem.Value)); }
            if (mineralCostElem != null) { costsData.MineralCost = new ConstValue<int>(XmlHelper.LoadInt(mineralCostElem.Value)); }
            if (gasCostElem != null) { costsData.GasCost = new ConstValue<int>(XmlHelper.LoadInt(gasCostElem.Value)); }

            return costsData;
        }

        /// <summary>
        /// Loads the weapon data of a building/unit type from the given element.
        /// </summary>
        /// <param name="weaponDataElem">The XML element to load from.</param>
        private static WeaponData LoadWeaponData(XElement weaponDataElem)
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

            WeaponData weaponData = new WeaponData()
            {
                Cooldown = new ConstValue<int>(XmlHelper.LoadInt(cooldownElem.Value)),
                Damage = new ConstValue<int>(XmlHelper.LoadInt(damageElem.Value)),
                RangeMax = new ConstValue<int>(XmlHelper.LoadInt(rangeMaxElem.Value))
            };
            DamageTypeEnum damageType;
            if (!EnumMap<DamageTypeEnum, string>.Demap(damageTypeElem.Value, out damageType))
            {
                throw new SimulatorException(string.Format("Unexpected damage type '{0}' defined in weapon data!", damageTypeElem.Value));
            }
            weaponData.DamageType = new ConstValue<DamageTypeEnum>(damageType);

            if (incrementElem != null) { weaponData.Increment = new ConstValue<int>(XmlHelper.LoadInt(incrementElem.Value)); }
            if (rangeMinElem != null) { weaponData.RangeMin = new ConstValue<int>(XmlHelper.LoadInt(rangeMinElem.Value)); }
            if (splashTypeElem != null)
            {
                SplashTypeEnum splashType;
                if (!EnumMap<SplashTypeEnum, string>.Demap(splashTypeElem.Value, out splashType))
                {
                    throw new SimulatorException(string.Format("Unexpected splash type '{0}' defined in weapon data!", splashTypeElem.Value));
                }
                weaponData.SplashType = new ConstValue<SplashTypeEnum>(splashType);
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
        /// Load the list of requirements defined in the given string.
        /// </summary>
        /// <param name="fromStr">The string that contains the requirements.</param>
        /// <returns>The list of the defined requirements.</returns>
        private static List<Requirement> LoadRequirements(string fromStr)
        {
            List<Requirement> retList = new List<Requirement>();
            fromStr = fromStr.Trim();
            string[] requirementStrings = fromStr.Split(',');
            foreach (string reqStr in requirementStrings)
            {
                Tuple<string, string> buildingAddonPair = ParseBuildingAddonStr(reqStr);
                retList.Add(new Requirement(buildingAddonPair.Item1, buildingAddonPair.Item2));
            }
            return retList;
        }

        /// <summary>
        /// Temporary string that contains the directory of the referenced images (TODO: this is a hack).
        /// </summary>
        private static string tmpImageDir;
    }
}
