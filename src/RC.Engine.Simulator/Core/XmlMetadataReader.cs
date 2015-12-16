using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.Metadata.Core;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;

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
        public static void Read(string xmlStr, string imageDir, ScenarioMetadata metadata)
        {
            if (xmlStr == null) { throw new ArgumentNullException("xmlStr"); }
            if (imageDir == null) { throw new ArgumentNullException("imageDir"); }
            if (metadata == null) { throw new ArgumentNullException("metadata"); }

            tmpImageDir = imageDir;

            /// Load the XML document.
            XDocument xmlDoc = XDocument.Parse(xmlStr);

            /// Load the scenario element type definitions.
            foreach (XElement metadataElement in xmlDoc.Root.Elements())
            {
                if (metadataElement.Name == XmlMetadataConstants.BUILDINGTYPE_ELEM)
                {
                    LoadBuildingType(metadataElement, metadata);
                }
                else if (metadataElement.Name == XmlMetadataConstants.UNITTYPE_ELEM)
                {
                    LoadUnitType(metadataElement, metadata);
                }
                else if (metadataElement.Name == XmlMetadataConstants.ADDONTYPE_ELEM)
                {
                    LoadAddonType(metadataElement, metadata);
                }
                else if (metadataElement.Name == XmlMetadataConstants.UPGRADETYPE_ELEM)
                {
                    LoadUpgradeType(metadataElement, metadata);
                }
                else if (metadataElement.Name == XmlMetadataConstants.MISSILETYPE_ELEM)
                {
                    LoadMissileType(metadataElement, metadata);
                }
                else if (metadataElement.Name == XmlMetadataConstants.CUSTOMTYPE_ELEM)
                {
                    XAttribute nameAttr = metadataElement.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
                    if (nameAttr == null) { throw new SimulatorException("Custom type name not defined!"); }

                    ScenarioElementType elementType = new ScenarioElementType(nameAttr.Value, metadata);
                    LoadScenarioElementType(metadataElement, elementType, metadata);
                    metadata.AddCustomType(elementType);
                }
                else if (metadataElement.Name == XmlMetadataConstants.SHADOWPALETTE_ELEM)
                {
                    metadata.SetShadowPalette(XmlHelper.LoadSpritePalette(metadataElement, imageDir));
                }
            }
        }

        /// <summary>
        /// Loads a building type definition from the given XML node.
        /// </summary>
        /// <param name="buildingTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadBuildingType(XElement buildingTypeElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = buildingTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Building type name not defined!"); }

            BuildingType buildingType = new BuildingType(nameAttr.Value, metadata);
            LoadScenarioElementType(buildingTypeElem, buildingType, metadata);

            metadata.AddBuildingType(buildingType);
        }

        /// <summary>
        /// Loads a unit type definition from the given XML node.
        /// </summary>
        /// <param name="unitTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUnitType(XElement unitTypeElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = unitTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Unit type name not defined!"); }

            UnitType unitType = new UnitType(nameAttr.Value, metadata);
            LoadScenarioElementType(unitTypeElem, unitType, metadata);

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
        private static void LoadAddonType(XElement addonTypeElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = addonTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Addon type name not defined!"); }

            AddonType addonType = new AddonType(nameAttr.Value, metadata);
            LoadScenarioElementType(addonTypeElem, addonType, metadata);

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
        private static void LoadUpgradeType(XElement upgradeTypeElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = upgradeTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Upgrade type name not defined!"); }

            UpgradeType upgradeType = new UpgradeType(nameAttr.Value, metadata);
            LoadScenarioElementType(upgradeTypeElem, upgradeType, metadata);

            XElement researchedInElem = upgradeTypeElem.Element(XmlMetadataConstants.RESEARCHEDIN_ELEM);
            if (researchedInElem != null) { upgradeType.SetResearchedIn(researchedInElem.Value); }

            XElement previousLevelElem = upgradeTypeElem.Element(XmlMetadataConstants.PREVIOUSLEVEL_ELEM);
            if (previousLevelElem != null) { upgradeType.SetPreviousLevelName(previousLevelElem.Value); }

            XElement effectsElem = upgradeTypeElem.Element(XmlMetadataConstants.EFFECTS_ELEM);
            if (effectsElem != null)
            {
                foreach (XElement effectElem in effectsElem.Elements())
                {
                    upgradeType.AddEffect(LoadUpgradeEffect(effectElem, metadata));
                }
            }

            metadata.AddUpgradeType(upgradeType);
        }

        /// <summary>
        /// Loads a missile type definition from the given XML node.
        /// </summary>
        /// <param name="missileTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadMissileType(XElement missileTypeElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = missileTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Missile type name not defined!"); }

            MissileType missileType = new MissileType(nameAttr.Value, metadata);
            LoadScenarioElementType(missileTypeElem, missileType, metadata);

            XElement launchAnimElem = missileTypeElem.Element(XmlMetadataConstants.LAUNCHANIMATION_ELEM);
            if (launchAnimElem != null)
            {
                XAttribute launchDelayAttr = launchAnimElem.Attribute(XmlMetadataConstants.LAUNCH_DELAY_ATTR);
                if (launchDelayAttr == null) { throw new SimulatorException("Launch delay not defined for missile type!"); }
                int launchDelay = XmlHelper.LoadInt(launchDelayAttr.Value);
                missileType.SetLaunchAnimation(launchAnimElem.Value, launchDelay);
            }

            XElement flyingAnimElem = missileTypeElem.Element(XmlMetadataConstants.FLYINGANIMATION_ELEM);
            if (flyingAnimElem != null) { missileType.SetFlyingAnimation(flyingAnimElem.Value); }

            XElement trailAnimElem = missileTypeElem.Element(XmlMetadataConstants.TRAILANIMATION_ELEM);
            if (trailAnimElem != null)
            {
                XAttribute trailAnimFreqAttr = trailAnimElem.Attribute(XmlMetadataConstants.TRAILANIMATION_FREQUENCY_ATTR);
                if (trailAnimFreqAttr == null) { throw new SimulatorException("Trail animation frequency not defined for missile type!"); }
                int trailAnimFreq = XmlHelper.LoadInt(trailAnimFreqAttr.Value);
                missileType.SetTrailAnimation(trailAnimElem.Value, trailAnimFreq);
            }

            XElement impactAnimElem = missileTypeElem.Element(XmlMetadataConstants.IMPACTANIMATION_ELEM);
            if (impactAnimElem != null) { missileType.SetImpactAnimation(impactAnimElem.Value); }

            metadata.AddMissileType(missileType);
        }

        /// <summary>
        /// Loads the necessary data of a scenario element type.
        /// </summary>
        /// <param name="elementTypeElem">The XML node to load from.</param>
        /// <param name="elementType">The scenario element type being constructed.</param>
        /// <param name="metadata">The metadata object.</param>
        private static void LoadScenarioElementType(XElement elementTypeElem, ScenarioElementType elementType, ScenarioMetadata metadata)
        {
            /// Load the displayed name of the element type.
            XAttribute displayedNameAttr = elementTypeElem.Attribute(XmlMetadataConstants.TYPE_DISPLAYEDNAME_ATTR);
            if (displayedNameAttr != null) { elementType.SetDisplayedName(displayedNameAttr.Value); }

            /// Load the has owner flag of the element type.
            XAttribute hasOwnerAttr = elementTypeElem.Attribute(XmlMetadataConstants.TYPE_HASOWNER_ATTR);
            elementType.SetHasOwner(hasOwnerAttr != null && XmlHelper.LoadBool(hasOwnerAttr.Value));

            /// Load the sprite palette of the element type.
            XElement spritePaletteElem = elementTypeElem.Element(XmlMetadataConstants.SPRITEPALETTE_ELEM);
            ISpritePalette<MapDirection> spritePalette = null;
            if (spritePaletteElem != null)
            {
                spritePalette = XmlHelper.LoadSpritePalette(spritePaletteElem, MapDirection.Undefined, tmpImageDir);
                elementType.SetSpritePalette(spritePalette);
            }

            /// Load the HP indicator icon palette of this element type.
            XElement hpIconPaletteElem = elementTypeElem.Element(XmlMetadataConstants.HPICONPALETTE_ELEM);
            if (hpIconPaletteElem != null)
            {
                ISpritePalette hpIconPalette = XmlHelper.LoadSpritePalette(hpIconPaletteElem, tmpImageDir);
                elementType.SetHPIconPalette(hpIconPalette);
            }

            /// Load the animation palette of the element type.
            XElement animPaletteElem = elementTypeElem.Element(XmlMetadataConstants.ANIMPALETTE_ELEM);
            if (animPaletteElem != null)
            {
                if (spritePalette == null) { throw new SimulatorException("Animation palette definition requires a sprite palette definition!"); }
                elementType.SetAnimationPalette(LoadAnimationPalette(animPaletteElem, spritePalette, metadata));
            }

            /// Load the cost data of the element type.
            XElement costsDataElem = elementTypeElem.Element(XmlMetadataConstants.COSTS_ELEM);
            if (costsDataElem != null) { LoadCostsData(costsDataElem, elementType); }

            /// Load the general data of the element type.
            XElement genDataElem = elementTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem != null) { LoadGeneralData(genDataElem, elementType); }

            XElement shadowDataElem = elementTypeElem.Element(XmlMetadataConstants.SHADOWDATA_ELEM);
            if (shadowDataElem != null) { LoadShadowData(shadowDataElem, elementType); }

            /// Load the ground weapon definition of the element type.
            XElement gndWeaponElem = elementTypeElem.Element(XmlMetadataConstants.GROUNDWEAPON_ELEM);
            if (gndWeaponElem != null) { elementType.AddStandardWeapon(LoadWeaponData(gndWeaponElem, metadata)); }

            /// Load the air weapon definition of the element type.
            XElement airWeaponElem = elementTypeElem.Element(XmlMetadataConstants.AIRWEAPON_ELEM);
            if (airWeaponElem != null) { elementType.AddStandardWeapon(LoadWeaponData(airWeaponElem, metadata)); }

            /// Load the air-ground weapon definition of the element type.
            XElement airGroundWeaponElem = elementTypeElem.Element(XmlMetadataConstants.AIRGROUNDWEAPON_ELEM);
            if (airGroundWeaponElem != null) { elementType.AddStandardWeapon(LoadWeaponData(airGroundWeaponElem, metadata)); }

            /// Load the requirements of the element type.
            XElement requiresElem = elementTypeElem.Element(XmlMetadataConstants.REQUIRES_ELEM);
            if (requiresElem != null)
            {
                string reqListStr = requiresElem.Value.Trim();
                string[] requirementStrings = reqListStr.Split(',');
                foreach (string reqStr in requirementStrings)
                {
                    Tuple<string, string> buildingAddonPair = ParseBuildingAddonStr(reqStr);
                    elementType.AddRequirement(new Requirement(buildingAddonPair.Item1, buildingAddonPair.Item2, metadata));
                }
            }
        }

        /// <summary>
        /// Loads an animation palette definition from the given XML node.
        /// </summary>
        /// <param name="animPaletteElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        /// <param name="spritePalette">The sprite palette that the animation palette is based on.</param>
        /// <returns>The constructed animation palette definition.</returns>
        private static AnimationPalette LoadAnimationPalette(XElement animPaletteElem, ISpritePalette<MapDirection> spritePalette, ScenarioMetadata metadata)
        {
            /// Create the animation palette object.
            AnimationPalette animPalette = new AnimationPalette(metadata);

            /// Load the animations.
            foreach (XElement animElem in animPaletteElem.Elements(XmlMetadataConstants.ANIMATION_ELEM))
            {
                Animation animation = LoadAnimation(animElem, animPalette.Count, spritePalette);
                animPalette.AddAnimation(animation);
            }
            return animPalette;
        }

        /// <summary>
        /// Loads an animation definition from the given XML node.
        /// </summary>
        /// <param name="animElem">The XML node to load from.</param>
        /// <param name="layerIndex">The index of the render layer of this animation.</param>
        /// <param name="spritePalette">The sprite palette that the animation is based on.</param>
        /// <param name="animName">The name of the animation to be loaded.</param>
        /// <returns>The constructed animation definition.</returns>
        private static Animation LoadAnimation(XElement animElem, int layerIndex, ISpritePalette<MapDirection> spritePalette)
        {
            XAttribute animNameAttr = animElem.Attribute(XmlMetadataConstants.ANIMATION_NAME_ATTR);
            if (animNameAttr == null) { throw new SimulatorException("Animation name not defined in animation palette!"); }

            /// Collect the labels.
            Dictionary<string, int> labels = new Dictionary<string, int>();
            int i = 0;
            foreach (XElement instructionElem in animElem.Elements())
            {
                if (instructionElem.Name == XmlMetadataConstants.LABEL_ELEM)
                {
                    XAttribute labelNameAttr = instructionElem.Attribute(XmlMetadataConstants.LABEL_NAME_ATTR);
                    if (labelNameAttr == null) { throw new SimulatorException("Label name not defined in animation!"); }
                    labels.Add(labelNameAttr.Value, i);
                }
                else
                {
                    i++;
                }
            }

            /// Collect the instructions
            i = 0;
            List<Animation.IInstruction> instructions = new List<Animation.IInstruction>();
            foreach (XElement instructionElem in animElem.Elements())
            {
                if (instructionElem.Name == XmlMetadataConstants.FRAME_ELEM)
                {
                    instructions.Add(LoadNewFrameInstruction(instructionElem, spritePalette));
                }
                else if (instructionElem.Name == XmlMetadataConstants.GOTO_ELEM)
                {
                    instructions.Add(LoadGotoInstruction(instructionElem, labels));
                }
                else if (instructionElem.Name == XmlMetadataConstants.WAIT_ELEM)
                {
                    instructions.Add(LoadWaitInstruction(instructionElem));
                }
                else if (instructionElem.Name == XmlMetadataConstants.REPEAT_ELEM)
                {
                    instructions.Add(new RepeatInstruction());
                }
            }

            XAttribute isPreviewAttr = animElem.Attribute(XmlMetadataConstants.ANIMATION_ISPREVIEW_ATTR);
            
            /// Create the animation object.
            return new Animation(animNameAttr.Value, layerIndex, isPreviewAttr != null && XmlHelper.LoadBool(isPreviewAttr.Value), instructions);
        }

        /// <summary>
        /// Loads a new frame instruction from the given XML node.
        /// </summary>
        /// <param name="instructionElem">The XML node to load from.</param>
        /// <param name="spritePalette">The sprite palette that the animation instruction is based on.</param>
        /// <returns>The constructed instruction.</returns>
        private static Animation.IInstruction LoadNewFrameInstruction(XElement instructionElem, ISpritePalette<MapDirection> spritePalette)
        {
            XAttribute spritesAttr = instructionElem.Attribute(XmlMetadataConstants.FRAME_SPRITES_ATTR);
            XAttribute durationAttr = instructionElem.Attribute(XmlMetadataConstants.FRAME_DURATION_ATTR);
            if (spritesAttr == null) { throw new SimulatorException("Sprites not defined for new frame instruction!"); }

            string[] spriteNames = spritesAttr.Value.Split(',');
            if (spriteNames.Length == 0) { throw new SimulatorException("Syntax error!"); }

            Dictionary<MapDirection, int[]> spriteIndices = new Dictionary<MapDirection, int[]>
            {
                { MapDirection.North, new int[spriteNames.Length] },
                { MapDirection.NorthEast, new int[spriteNames.Length] },
                { MapDirection.East, new int[spriteNames.Length] },
                { MapDirection.SouthEast, new int[spriteNames.Length] },
                { MapDirection.South, new int[spriteNames.Length] },
                { MapDirection.SouthWest, new int[spriteNames.Length] },
                { MapDirection.West, new int[spriteNames.Length] },
                { MapDirection.NorthWest, new int[spriteNames.Length] },
            };

            /// Search the appropriate sprite indices for each directions from the sprite palette.
            foreach (KeyValuePair<MapDirection, int[]> item in spriteIndices)
            {
                MapDirection direction = item.Key;
                for (int i = 0; i < spriteNames.Length; i++)
                {
                    /// Get the sprite index for the current direction or for MapDirection.Undefined if not found.
                    int spriteIndex = spritePalette.GetSpriteIndex(spriteNames[i], direction);
                    if (spriteIndex == -1) { spriteIndex = spritePalette.GetSpriteIndex(spriteNames[i], MapDirection.Undefined); }

                    if (spriteIndex == -1) { throw new SimulatorException(string.Format("Sprite '{0}' not defined for neither {1} nor {2}!", spriteNames[i], direction, MapDirection.Undefined)); }
                    spriteIndices[direction][i] = spriteIndex;
                }
            }

            return new NewFrameInstruction(spriteIndices, durationAttr != null ? XmlHelper.LoadInt(durationAttr.Value) : 1);
        }

        /// <summary>
        /// Loads a goto instruction from the given XML node.
        /// </summary>
        /// <param name="instructionElem">The XML node to load from.</param>
        /// <param name="labels">List of the labels mapped by their names.</param>
        /// <returns>The constructed instruction.</returns>
        private static Animation.IInstruction LoadGotoInstruction(XElement instructionElem, Dictionary<string, int> labels)
        {
            XAttribute labelAttr = instructionElem.Attribute(XmlMetadataConstants.GOTO_LABEL_ATTR);
            if (labelAttr == null) { throw new SimulatorException("Target label not defined for goto instruction!"); }
            if (!labels.ContainsKey(labelAttr.Value)) { throw new SimulatorException(string.Format("Label '{0}' doesn't exist!", labelAttr.Value)); }

            return new GotoInstruction(labels[labelAttr.Value]);
        }

        /// <summary>
        /// Loads a wait instruction from the given XML node.
        /// </summary>
        /// <param name="instructionElem">The XML node to load from.</param>
        /// <returns>The constructed instruction.</returns>
        private static Animation.IInstruction LoadWaitInstruction(XElement instructionElem)
        {
            XAttribute durationAttr = instructionElem.Attribute(XmlMetadataConstants.WAIT_DURATION_ATTR);
            return new WaitInstruction(durationAttr != null ? XmlHelper.LoadInt(durationAttr.Value) : 1);
        }

        /// <summary>
        /// Loads the general data of a scenario element type from the given element.
        /// </summary>
        /// <param name="genDataElem">The XML element to load from.</param>
        /// <param name="elementType">The scenario element type being constructed.</param>
        private static void LoadGeneralData(XElement genDataElem, ScenarioElementType elementType)
        {
            XElement areaElem = genDataElem.Element(XmlMetadataConstants.GENDATA_AREA_ELEM);
            XElement armorElem = genDataElem.Element(XmlMetadataConstants.GENDATA_ARMOR_ELEM);
            XElement maxEnergyElem = genDataElem.Element(XmlMetadataConstants.GENDATA_MAXENERGY_ELEM);
            XElement maxHPElem = genDataElem.Element(XmlMetadataConstants.GENDATA_MAXHP_ELEM);
            XElement sightRangeElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SIGHTRANGE_ELEM);
            XElement sizeElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SIZE_ELEM);
            XElement speedElem = genDataElem.Element(XmlMetadataConstants.GENDATA_SPEED_ELEM);

            if (areaElem != null) { elementType.SetArea(XmlHelper.LoadNumVector(areaElem.Value)); }
            if (armorElem != null) { elementType.SetArmor(XmlHelper.LoadInt(armorElem.Value)); }
            if (maxEnergyElem != null) { elementType.SetMaxEnergy(XmlHelper.LoadInt(maxEnergyElem.Value)); }
            if (maxHPElem != null) { elementType.SetMaxHP(XmlHelper.LoadInt(maxHPElem.Value)); }
            if (sightRangeElem != null) { elementType.SetSightRange(XmlHelper.LoadInt(sightRangeElem.Value)); }
            if (speedElem != null) { elementType.SetSpeed(XmlHelper.LoadNum(speedElem.Value)); }

            if (sizeElem != null)
            {
                SizeEnum size;
                if (!EnumMap<SizeEnum, string>.TryDemap(sizeElem.Value, out size))
                {
                    throw new SimulatorException(string.Format("Unexpected size '{0}' defined in general data!", sizeElem.Value));
                }
                elementType.SetSize(size);
            }
        }

        /// <summary>
        /// Loads the costs data of a scenario element type from the given element.
        /// </summary>
        /// <param name="costsDataElem">The XML element to load from.</param>
        /// <param name="elementType">The scenario element type being constructed.</param>
        private static void LoadCostsData(XElement costsDataElem, ScenarioElementType elementType)
        {
            XElement buildTimeElem = costsDataElem.Element(XmlMetadataConstants.COSTS_BUILDTIME_ELEM);
            XElement supplyUsedElem = costsDataElem.Element(XmlMetadataConstants.COSTS_SUPPLYUSED_ELEM);
            XElement supplyProvidedElem = costsDataElem.Element(XmlMetadataConstants.COSTS_SUPPLYPROVIDED_ELEM);
            XElement mineralCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_MINERALCOST_ELEM);
            XElement gasCostElem = costsDataElem.Element(XmlMetadataConstants.COSTS_GASCOST_ELEM);

            if (buildTimeElem != null) { elementType.SetBuildTime(XmlHelper.LoadInt(buildTimeElem.Value)); }
            if (supplyUsedElem != null) { elementType.SetSupplyUsed(XmlHelper.LoadInt(supplyUsedElem.Value)); }
            if (supplyProvidedElem != null) { elementType.SetSupplyProvided(XmlHelper.LoadInt(supplyProvidedElem.Value)); }
            if (mineralCostElem != null) { elementType.SetMineralCost(XmlHelper.LoadInt(mineralCostElem.Value)); }
            if (gasCostElem != null) { elementType.SetGasCost(XmlHelper.LoadInt(gasCostElem.Value)); }
        }

        /// <summary>
        /// Loads the shadow data of a scenario element type from the given element.
        /// </summary>
        /// <param name="shadowDataElem">The XML element to load from.</param>
        /// <param name="elementType">The scenario element type being constructed.</param>
        private static void LoadShadowData(XElement shadowDataElem, ScenarioElementType elementType)
        {
            XElement spriteNameElem = shadowDataElem.Element(XmlMetadataConstants.SHADOWDATA_SPRITENAME_ELEM);
            XElement offsetElem = shadowDataElem.Element(XmlMetadataConstants.SHADOWDATA_OFFSET_ELEM);

            if (spriteNameElem == null) { throw new SimulatorException("Shadow sprite not defined!"); }
            if (offsetElem == null) { throw new SimulatorException("Shadow offset not defined!"); }

            elementType.SetShadowData(spriteNameElem.Value, XmlHelper.LoadNumVector(offsetElem.Value));
        }

        /// <summary>
        /// Loads the weapon data of a building/unit type from the given element.
        /// </summary>
        /// <param name="weaponDataElem">The XML element to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        private static WeaponData LoadWeaponData(XElement weaponDataElem, ScenarioMetadata metadata)
        {
            XAttribute nameAttr = weaponDataElem.Attribute(XmlMetadataConstants.WPN_NAME_ATTR);
            XAttribute displayedNameAttr = weaponDataElem.Attribute(XmlMetadataConstants.WPN_DISPLAYEDNAME_ATTR);
            XElement cooldownElem = weaponDataElem.Element(XmlMetadataConstants.WPN_COOLDOWN_ELEM);
            XElement damageElem = weaponDataElem.Element(XmlMetadataConstants.WPN_DAMAGE_ELEM);
            XElement damageTypeElem = weaponDataElem.Element(XmlMetadataConstants.WPN_DAMAGETYPE_ELEM);
            XElement incrementElem = weaponDataElem.Element(XmlMetadataConstants.WPN_INCREMENT_ELEM);
            XElement rangeMaxElem = weaponDataElem.Element(XmlMetadataConstants.WPN_RANGEMAX_ELEM);
            XElement rangeMinElem = weaponDataElem.Element(XmlMetadataConstants.WPN_RANGEMIN_ELEM);
            XElement splashTypeElem = weaponDataElem.Element(XmlMetadataConstants.WPN_SPLASHTYPE_ELEM);

            if (nameAttr == null) { throw new SimulatorException("Weapon name not defined!"); }
            if (cooldownElem == null) { throw new SimulatorException("Cooldown not defined!"); }
            if (damageElem == null) { throw new SimulatorException("Damage not defined!"); }
            if (damageTypeElem == null) { throw new SimulatorException("DamageType not defined!"); }
            if (rangeMaxElem == null) { throw new SimulatorException("RangeMax not defined!"); }

            WeaponTypeEnum weaponType;
            if (!EnumMap<WeaponTypeEnum, string>.TryDemap(weaponDataElem.Name.LocalName, out weaponType))
            {
                throw new SimulatorException(string.Format("Unexpected weapon type '{0}' defined in weapon data!", weaponDataElem.Name.LocalName));
            }

            WeaponData weaponData = new WeaponData(nameAttr.Value, metadata, weaponType);
            weaponData.SetCooldown(XmlHelper.LoadInt(cooldownElem.Value));
            weaponData.SetDamage(XmlHelper.LoadInt(damageElem.Value));
            weaponData.SetRangeMax(XmlHelper.LoadInt(rangeMaxElem.Value));

            DamageTypeEnum damageType;
            if (!EnumMap<DamageTypeEnum, string>.TryDemap(damageTypeElem.Value, out damageType))
            {
                throw new SimulatorException(string.Format("Unexpected damage type '{0}' defined in weapon data!", damageTypeElem.Value));
            }
            weaponData.SetDamageType(damageType);

            if (displayedNameAttr != null) { weaponData.SetDisplayedName(displayedNameAttr.Value); }
            if (incrementElem != null) { weaponData.SetIncrement(XmlHelper.LoadInt(incrementElem.Value)); }
            if (rangeMinElem != null) { weaponData.SetRangeMin(XmlHelper.LoadInt(rangeMinElem.Value)); }
            if (splashTypeElem != null)
            {
                SplashTypeEnum splashType;
                if (!EnumMap<SplashTypeEnum, string>.TryDemap(splashTypeElem.Value, out splashType))
                {
                    throw new SimulatorException(string.Format("Unexpected splash type '{0}' defined in weapon data!", splashTypeElem.Value));
                }
                weaponData.SetSplashType(splashType);
            }

            foreach (XElement missileElem in weaponDataElem.Elements(XmlMetadataConstants.WPN_MISSILE_ELEM))
            {
                weaponData.AddMissile(LoadMissileData(missileElem, metadata));
            }

            return weaponData;
        }

        /// <summary>
        /// Loads a missile definition from the given XML node.
        /// </summary>
        /// <param name="missileElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        /// <returns>The constructed missile definition.</returns>
        private static MissileData LoadMissileData(XElement missileElem, ScenarioMetadata metadata)
        {
            XAttribute missileTypeAttr = missileElem.Attribute(XmlMetadataConstants.WPN_MISSILE_TYPE_ATTR);
            if (missileTypeAttr == null) { throw new SimulatorException("Missile type not defined!"); }

            MissileData missile = new MissileData(missileTypeAttr.Value, metadata);
            foreach (XElement launchElem in missileElem.Elements(XmlMetadataConstants.WPN_MISSILE_LAUNCH_ELEM))
            {
                XAttribute directionAttr = launchElem.Attribute(XmlMetadataConstants.WPN_MISSILE_LAUNCH_DIR_ATTR);
                XAttribute positionAttr = launchElem.Attribute(XmlMetadataConstants.WPN_MISSILE_LAUNCH_POS_ATTR);
                if (directionAttr == null) { throw new SimulatorException("Direction not defined for missile data launch position!"); }
                if (positionAttr == null) { throw new SimulatorException("Relative position not defined for missile data launch position!"); }

                MapDirection direction;
                if (!EnumMap<MapDirection, string>.TryDemap(directionAttr.Value, out direction))
                {
                    throw new SimulatorException(string.Format("Unexpected MapDirection '{0}' defined in missile data!", directionAttr.Value));
                }

                RCNumVector relativePosition = XmlHelper.LoadNumVector(positionAttr.Value);
                missile.AddRelativeLaunchPosition(direction, relativePosition);
            }

            return missile;
        }

        /// <summary>
        /// Loads an upgrade effect definition from the given XML node.
        /// </summary>
        /// <param name="effectElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object.</param>
        /// <returns>The constructed upgrade effect definition.</returns>
        private static UpgradeEffectBase LoadUpgradeEffect(XElement effectElem, ScenarioMetadata metadata)
        {
            string actionName = effectElem.Name.ToString();
            XAttribute targetTypeAttr = effectElem.Attribute(XmlMetadataConstants.EFFECT_TARGETTYPE_ATTR);
            if (targetTypeAttr == null) { throw new SimulatorException("Target type not defined for an upgrade effect!"); }

            return UpgradeEffectFactory.CreateUpgradeEffect(actionName, effectElem.Value, targetTypeAttr.Value, metadata);
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
