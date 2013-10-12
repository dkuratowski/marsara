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

            /// Load the datatype definitions.
            foreach (XElement dataTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.DATATYPE_ELEM))
            {
                LoadDataType(dataTypeElem, metadata);
            }

            /// Load the element type definitions.
            foreach (XElement elemTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.ELEMENTTYPE_ELEM))
            {
                LoadElementType(elemTypeElem, metadata);
            }

            /// Load the behavior type definitions.
            foreach (XElement behaviorTypeElem in xmlDoc.Root.Elements(XmlMetadataConstants.BEHAVIORTYPE_ELEM))
            {
                LoadBehaviorType(behaviorTypeElem, metadata);
            }
        }

        /// <summary>
        /// Load a data type definition from the given XML node.
        /// </summary>
        /// <param name="dataTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadDataType(XElement dataTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = dataTypeElem.Attribute(XmlMetadataConstants.DATATYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("DataType name not defined!"); }

            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            foreach (XElement fieldElem in dataTypeElem.Elements(XmlMetadataConstants.FIELD_ELEM))
            {
                fields.Add(LoadField(fieldElem));
                // TODO: Load bit definitions!
            }
            metadata.AddCompositeHeapType(new SimHeapType(nameAttr.Value, fields));
        }

        /// <summary>
        /// Load an element type definition from the given XML node.
        /// </summary>
        /// <param name="elemTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadElementType(XElement elemTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = elemTypeElem.Attribute(XmlMetadataConstants.ELEMENTTYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("ElementType name not defined!"); }

            /// Load the data type definition of the element type.
            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            foreach (XElement fieldElem in elemTypeElem.Elements(XmlMetadataConstants.FIELD_ELEM))
            {
                fields.Add(LoadField(fieldElem));
                // TODO: Load bit definitions!
            }
            SimHeapType elemHeapType = new SimHeapType(nameAttr.Value, fields);
            metadata.AddCompositeHeapType(elemHeapType);

            /// Load the indicator definition of the element type if exists.
            XElement indicatorElem = elemTypeElem.Element(XmlMetadataConstants.INDICATOR_ELEM);
            if (indicatorElem != null)
            {
                SimElemIndicatorDef indicator = LoadIndicatorDef(nameAttr.Value, indicatorElem);
                metadata.AddIndicatorDef(indicator);
            }

            /// Load behavior tree definition.
            List<SimElemBehaviorTreeNode> rootNodes = new List<SimElemBehaviorTreeNode>();
            foreach (XElement behaviorElem in elemTypeElem.Elements(XmlMetadataConstants.BEHAVIOR_ELEM))
            {
                rootNodes.Add(LoadBehaviorTreeNode(behaviorElem));
            }
            metadata.AddBehaviorTreeDef(nameAttr.Value, rootNodes);
        }

        /// <summary>
        /// Loads a behavior type definition from the given XML node.
        /// </summary>
        /// <param name="behaviorTypeElem">The XML node to load from.</param>
        /// <param name="metadata">The metadata object being constructed.</param>
        private static void LoadBehaviorType(XElement behaviorTypeElem, SimulationMetadata metadata)
        {
            XAttribute nameAttr = behaviorTypeElem.Attribute(XmlMetadataConstants.BEHAVIORTYPE_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("DataType name not defined!"); }

            XElement factoryElem = behaviorTypeElem.Element(XmlMetadataConstants.FACTORY_ELEM);
            if (factoryElem == null) { throw new SimulatorException(string.Format("<{0}> element not found!", XmlMetadataConstants.FACTORY_ELEM)); }

            XElement assemblyElem = factoryElem.Element(XmlMetadataConstants.ASSEMBLY_ELEM);
            if (assemblyElem == null) { throw new SimulatorException(string.Format("<{0}> element not found!", XmlMetadataConstants.ASSEMBLY_ELEM)); }

            XElement classElem = factoryElem.Element(XmlMetadataConstants.CLASS_ELEM);
            if (classElem == null) { throw new SimulatorException(string.Format("<{0}> element not found!", XmlMetadataConstants.CLASS_ELEM)); }

            metadata.AddBehaviorFactory(nameAttr.Value, assemblyElem.Value, classElem.Value);
        }

        /// <summary>
        /// Loads a field definition of a data or element type from the given XML node.
        /// </summary>
        /// <param name="fieldElem">The XML node to load from.</param>
        /// <returns>A pair that contains the name and the type of the field.</returns>
        private static KeyValuePair<string, string> LoadField(XElement fieldElem)
        {
            XAttribute nameAttr = fieldElem.Attribute(XmlMetadataConstants.FIELD_NAME_ATTR);
            XAttribute typeAttr = fieldElem.Attribute(XmlMetadataConstants.FIELD_TYPE_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Field name not defined!"); }
            if (typeAttr == null) { throw new SimulatorException("Field type not defined!"); }

            return new KeyValuePair<string, string>(nameAttr.Value, typeAttr.Value);
        }

        /// <summary>
        /// Loads an indicator definition from the given XML node.
        /// </summary>
        /// <param name="elementType">The name of the corresponding element type.</param>
        /// <param name="indicatorElem">The XML node to load from.</param>
        /// <returns>The constructed indicator definition.</returns>
        private static SimElemIndicatorDef LoadIndicatorDef(string elementType, XElement indicatorElem)
        {
            XAttribute imageAttr = indicatorElem.Attribute(XmlMetadataConstants.INDICATOR_IMAGE_ATTR);
            XAttribute transpColorAttr = indicatorElem.Attribute(XmlMetadataConstants.INDICATOR_TRANSPCOLOR_ATTR);
            XAttribute ownerMaskColorAttr = indicatorElem.Attribute(XmlMetadataConstants.INDICATOR_OWNERMASKCOLOR_ATTR);
            if (imageAttr == null) { throw new SimulatorException("Image not defined for indicator definition!"); }

            /// Read the image data.
            string imagePath = System.IO.Path.Combine(tmpImageDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the indicator definition object.
            SimElemIndicatorDef indicatorDef = new SimElemIndicatorDef(elementType,
                                                                       imageData,
                                                                       transpColorAttr != null ? transpColorAttr.Value : null,
                                                                       ownerMaskColorAttr != null ? ownerMaskColorAttr.Value : null);

            /// Load the animations.
            foreach (XElement animElem in indicatorElem.Elements(XmlMetadataConstants.ANIMATION_ELEM))
            {
                LoadAnimation(animElem, indicatorDef);
            }
            return indicatorDef;
        }

        /// <summary>
        /// Loads an animation of the given indicator definition from the given XML node.
        /// </summary>
        /// <param name="animElem">The XML node to load from.</param>
        /// <param name="indicatorDef">The indicator definition.</param>
        private static void LoadAnimation(XElement animElem, SimElemIndicatorDef indicatorDef)
        {
            XAttribute nameAttr = animElem.Attribute(XmlMetadataConstants.ANIMATION_NAME_ATTR);
            if (nameAttr == null) { throw new SimulatorException("Animation name not defined!"); }

            List<SimElemAnimFrame> frames = new List<SimElemAnimFrame>();
            foreach (XElement frameElem in animElem.Elements(XmlMetadataConstants.FRAME_ELEM))
            {
                XAttribute sourceRegionAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_SOURCEREGION_ATTR);
                XAttribute offsetAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_OFFSET_ATTR);
                XAttribute repeatAttr = frameElem.Attribute(XmlMetadataConstants.FRAME_REPEAT_ATTR);
                if (sourceRegionAttr == null) { throw new SimulatorException("Source region not defined for animation frame!"); }

                RCIntRectangle sourceRegion = XmlHelper.LoadRectangle(sourceRegionAttr.Value);
                RCIntVector offset = offsetAttr != null ? XmlHelper.LoadVector(offsetAttr.Value) : new RCIntVector(0, 0);
                int repeatCount = repeatAttr != null ? XmlHelper.LoadInt(repeatAttr.Value) : 1;
                if (repeatCount <= 0) { throw new SimulatorException("Frame repeat count must be greater than 0!"); }

                for (int i = 0; i < repeatCount; i++)
                {
                    frames.Add(new SimElemAnimFrame(sourceRegion, offset));
                }
            }

            indicatorDef.AddAnimation(nameAttr.Value, frames);
        }

        /// <summary>
        /// Loads a behavior tree node from the given XML node.
        /// </summary>
        /// <param name="behaviorElem">The XML node to load from.</param>
        /// <returns>The created behavior tree node.</returns>
        private static SimElemBehaviorTreeNode LoadBehaviorTreeNode(XElement behaviorElem)
        {
            XAttribute typeAttr = behaviorElem.Attribute(XmlMetadataConstants.BEHAVIOR_TYPE_ATTR);
            if (typeAttr == null) { throw new SimulatorException("Behavior type attribute not defined!"); }

            List<SimElemBehaviorTreeNode> children = new List<SimElemBehaviorTreeNode>();
            foreach (XElement childBehaviorElem in behaviorElem.Elements(XmlMetadataConstants.BEHAVIOR_ELEM))
            {
                children.Add(LoadBehaviorTreeNode(childBehaviorElem));
            }
            return new SimElemBehaviorTreeNode(typeAttr.Value, children);
        }

        /// <summary>
        /// Temporary string that contains the directory of the referenced images (TODO: this is a hack).
        /// </summary>
        private static string tmpImageDir;
    }
}
