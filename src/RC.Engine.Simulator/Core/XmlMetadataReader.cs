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
        public static void Read(string xmlStr, string imageDir, SimulationMetadata metadata)
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
        private static void LoadBuildingType(XElement buildingTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = buildingTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Building type name not defined!"); }

            /// Load the sprite palette of the building type.
            XElement spritePaletteElem = buildingTypeElem.Element(XmlMetadataConstants.SPRITE_ELEM);
            if (spritePaletteElem == null) { throw new SimulatorException("Sprite palette not defined for building type!"); }

            BuildingType buildingType = new BuildingType(nameAttr.Value, LoadSpritePalette(spritePaletteElem));
            XElement genDataElem = buildingTypeElem.Element(XmlMetadataConstants.GENERALDATA_ELEM);
            if (genDataElem == null) { throw new SimulatorException("General data not found for building type!"); }
            LoadGeneralData(genDataElem);

            metadata.AddBuildingType(buildingType);
        }

        /// <summary>
        /// Loads a unit type definition from the given XML node.
        /// </summary>
        /// <param name="unitTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUnitType(XElement unitTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = unitTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Unit type name not defined!"); }
        }

        /// <summary>
        /// Loads an addon type definition from the given XML node.
        /// </summary>
        /// <param name="addonTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadAddonType(XElement addonTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = addonTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Addon type name not defined!"); }
        }

        /// <summary>
        /// Loads an upgrade type definition from the given XML node.
        /// </summary>
        /// <param name="upgradeTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadUpgradeType(XElement upgradeTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = upgradeTypeElem.Attribute(XmlMetadataConstants.TYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Upgrade type name not defined!"); }
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
                spritePalette.AddFrame(frameNameAttr.Value, XmlHelper.LoadRectangle(sourceRegionAttr.Value), XmlHelper.LoadVector(offsetAttr.Value));
            }
            return spritePalette;
        }

        /// <summary>
        /// Loads the general data of a building/unit/addon type from the given element.
        /// </summary>
        /// <param name="genDataElem">The XML element to load from.</param>
        private static void LoadGeneralData(XElement genDataElem)
        {
            /// TODO: load general data
        }

        /// <summary>
        /// Temporary string that contains the directory of the referenced images (TODO: this is a hack).
        /// </summary>
        private static string tmpImageDir;
    }
}
