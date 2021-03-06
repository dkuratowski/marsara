﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Contains helper methods for loading different data types from XML-attributes or XML-elements.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Loads a character from the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded character.</returns>
        public static char LoadChar(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }
            if (fromStr.Length != 1) { throw new ConfigurationException("Character format error!"); }
            return fromStr[0];
        }

        /// <summary>
        /// Loads an integer from the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded integer.</returns>
        public static int LoadInt(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }
            int retVal = 0;
            if (!int.TryParse(fromStr.Trim(), out retVal)) { throw new ConfigurationException("Integer format error!"); }
            return retVal;
        }

        /// <summary>
        /// Loads an RCNumber from the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCNumber.</returns>
        public static RCNumber LoadNum(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }
            fromStr = fromStr.Trim();
            if (!RCNUMBER_SYNTAX.IsMatch(fromStr)) { throw new ArgumentException("fromStr", "RCNumber syntax error!"); }

            string[] numParts = fromStr.Split('.');
            int integerPart = int.Parse(numParts[0]);
            int fractionPart = int.Parse(numParts[1]);

            return numParts[0].StartsWith("-") ?
                (RCNumber)integerPart - (RCNumber)fractionPart / (RCNumber)Math.Pow(10, numParts[1].Length) :
                (RCNumber)integerPart + (RCNumber)fractionPart / (RCNumber)Math.Pow(10, numParts[1].Length);
        }

        /// <summary>
        /// Loads a boolean value ('true' or 'false') from the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded boolean value.</returns>
        public static bool LoadBool(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }
            if (fromStr == "false")
            {
                return false;
            }
            else if (fromStr == "true")
            {
                return true;
            }
            else
            {
                throw new ConfigurationException("Boolean format error!");
            }
        }

        /// <summary>
        /// Loads an RCIntVector defined in the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCIntVector.</returns>
        public static RCIntVector LoadIntVector(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }

            string[] componentStrings = fromStr.Split(';');
            if (componentStrings.Length != 2) { throw new ConfigurationException("Vector format error!"); }

            int vectorX = 0;
            int vectorY = 0;
            if (int.TryParse(componentStrings[0].Trim(), out vectorX) &&
                int.TryParse(componentStrings[1].Trim(), out vectorY))
            {
                return new RCIntVector(vectorX, vectorY);
            }
            else
            {
                throw new ConfigurationException("Vector format error!");
            }
        }

        /// <summary>
        /// Loads an RCNumVector defined in the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCNumVector.</returns>
        public static RCNumVector LoadNumVector(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }

            string[] componentStrings = fromStr.Split(';');
            if (componentStrings.Length != 2) { throw new ConfigurationException("Vector format error!"); }

            RCNumber vectorX = LoadNum(componentStrings[0]);
            RCNumber vectorY = LoadNum(componentStrings[1]);
            return new RCNumVector(vectorX, vectorY);
        }

        /// <summary>
        /// Loads an RCIntRectangle defined in the given string in format: "X;Y;Width;Height".
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCIntRectangle.</returns>
        public static RCIntRectangle LoadIntRectangle(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }

            string[] componentStrings = fromStr.Split(';');
            if (componentStrings.Length != 4) { throw new ConfigurationException("Rectangle format error!"); }

            int rectX = 0;
            int rectY = 0;
            int rectWidth = 0;
            int rectHeight = 0;
            if (int.TryParse(componentStrings[0].Trim(), out rectX) &&
                int.TryParse(componentStrings[1].Trim(), out rectY) &&
                int.TryParse(componentStrings[2].Trim(), out rectWidth) &&
                int.TryParse(componentStrings[3].Trim(), out rectHeight))
            {
                return new RCIntRectangle(rectX, rectY, rectWidth, rectHeight);
            }
            else
            {
                throw new ConfigurationException("Rectangle format error!");
            }
        }

        /// <summary>
        /// Loads an RCNumRectangle defined in the given string in format: "X;Y;Width;Height".
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCNumRectangle.</returns>
        public static RCNumRectangle LoadNumRectangle(string fromStr)
        {
            if (fromStr == null) { throw new ArgumentNullException("fromStr"); }

            string[] componentStrings = fromStr.Split(';');
            if (componentStrings.Length != 4) { throw new ConfigurationException("Rectangle format error!"); }

            RCNumber rectX = LoadNum(componentStrings[0]);
            RCNumber rectY = LoadNum(componentStrings[1]);
            RCNumber rectWidth = LoadNum(componentStrings[2]);
            RCNumber rectHeight = LoadNum(componentStrings[3]);
            return new RCNumRectangle(rectX, rectY, rectWidth, rectHeight);
        }

        /// <summary>
        /// Loads a RCColor from the given string.
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCColor.</returns>
        public static RCColor LoadColor(string fromStr)
        {
            string[] colorStrings = fromStr.Trim().Split(';');
            if (colorStrings.Length != 3) { throw new ArgumentException(string.Format("Invalid RCColor format: '{0}'!", fromStr)); }

            int r = XmlHelper.LoadInt(colorStrings[0]);
            int g = XmlHelper.LoadInt(colorStrings[1]);
            int b = XmlHelper.LoadInt(colorStrings[2]);
            return new RCColor(r, g, b);
        }
        
        /// <summary>
        /// Creates an instance of the given type defined in the given XML-load.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be created.</typeparam>
        /// <param name="sourceElem">The XML-node that contains informations about the instantiation.</param>
        /// <returns>The created instance of type T.</returns>
        public static T CreateInstance<T>(XElement sourceElem)
        {
            /// Get the name of the assembly that contains the class to be instantiated.
            XAttribute assemblyAttr = sourceElem.Attribute(INSTANTIATION_ASSEMBLY_ATTR);
            if (assemblyAttr == null) { throw new ConfigurationException(string.Format("'{0}' attribute not found!", INSTANTIATION_ASSEMBLY_ATTR)); }

            /// Get the name of the class to be instantiated.
            XAttribute classElem = sourceElem.Attribute(INSTANTIATION_CLASS_ATTR);
            if (classElem == null) { throw new ConfigurationException(string.Format("'{0}' attribute not found!", INSTANTIATION_CLASS_ATTR)); }

            /// Collect the parameters for the constructor.
            List<object> ctorParams = new List<object>();
            foreach (XElement paramElem in sourceElem.Elements(INSTANTIATION_CTORPARAM_ELEM))
            {
                XAttribute paramTypeAttr = paramElem.Attribute(INSTANTIATION_CTORPARAM_TYPE_ATTR);
                if (paramTypeAttr == null) { throw new ConfigurationException("No type defined for trace constructor parameter!"); }

                /// Try to parse the type attribute.
                CtorParamType paramType;
                if (!EnumMap<CtorParamType, string>.TryDemap(paramTypeAttr.Value, out paramType))
                {
                    throw new ConfigurationException(string.Format("Unexpected constructor parameter type '{0}' defined.", paramTypeAttr.Value));
                }

                object parameter = ParseParameterValue(paramElem.Value, paramType);
                ctorParams.Add(parameter);
            }

            /// Load the assembly that contains the class and create an instance of that class.
            Assembly asm = Assembly.Load(assemblyAttr.Value);
            if (asm == null) { throw new ConfigurationException(string.Format("Unable to load assembly '{0}'!", assemblyAttr.Value)); }
            Type objType = asm.GetType(classElem.Value);
            if (objType == null) { throw new ConfigurationException(string.Format("Unable to load type '{0}' from assembly '{1}'!", classElem.Value, assemblyAttr.Value)); }
            return (T)Activator.CreateInstance(objType, ctorParams.ToArray());
        }

        /// <summary>
        /// Loads a single-variant sprite palette definition from the given XML node.
        /// </summary>
        /// <param name="spritePaletteElem">The XML node to load from.</param>
        /// <param name="imagesDir">The directory of the referenced images.</param>
        /// <returns>The constructed sprite palette.</returns>
        public static ISpritePalette LoadSpritePalette(XElement spritePaletteElem, string imagesDir)
        {
            XAttribute imageAttr = spritePaletteElem.Attribute(SPRITEPALETTE_IMAGE_ATTR);
            XAttribute transpColorAttr = spritePaletteElem.Attribute(SPRITEPALETTE_TRANSPCOLOR_ATTR);
            XAttribute ownerMaskColorAttr = spritePaletteElem.Attribute(SPRITEPALETTE_MASKCOLOR_ATTR);
            if (imageAttr == null) { throw new InvalidOperationException("Image not defined for sprite palette!"); }

            /// Read the image data.
            string imagePath = Path.Combine(imagesDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the sprite palette object.
            SpritePalette spritePalette =
                new SpritePalette(imageData,
                                  transpColorAttr != null ? XmlHelper.LoadColor(transpColorAttr.Value) : RCColor.Undefined,
                                  ownerMaskColorAttr != null ? XmlHelper.LoadColor(ownerMaskColorAttr.Value) : RCColor.Undefined);

            /// Load the sprites of the sprite palette.
            LoadSprites(spritePaletteElem, spritePalette, SpritePalette.DummyEnum.DummyEnumItem);
            return spritePalette;
        }

        /// <summary>
        /// Loads a multi-variant sprite palette definition from the given XML node.
        /// </summary>
        /// <param name="spritePaletteElem">The XML node to load from.</param>
        /// <param name="defaultVariant">The default variant that should be used when no variant was found for a sprite definition.</param>
        /// <param name="imagesDir">The directory of the referenced images.</param>
        /// <returns>The constructed sprite palette.</returns>
        public static ISpritePalette<TVariant> LoadSpritePalette<TVariant>(XElement spritePaletteElem, TVariant defaultVariant, string imagesDir) where TVariant : struct
        {
            XAttribute imageAttr = spritePaletteElem.Attribute(SPRITEPALETTE_IMAGE_ATTR);
            XAttribute transpColorAttr = spritePaletteElem.Attribute(SPRITEPALETTE_TRANSPCOLOR_ATTR);
            XAttribute ownerMaskColorAttr = spritePaletteElem.Attribute(SPRITEPALETTE_MASKCOLOR_ATTR);
            if (imageAttr == null) { throw new InvalidOperationException("Image not defined for sprite palette!"); }

            /// Read the image data.
            string imagePath = Path.Combine(imagesDir, imageAttr.Value);
            byte[] imageData = File.ReadAllBytes(imagePath);

            /// Create the sprite palette object.
            SpritePalette<TVariant> spritePalette =
                new SpritePalette<TVariant>(imageData,
                                            transpColorAttr != null ? XmlHelper.LoadColor(transpColorAttr.Value) : RCColor.Undefined,
                                            ownerMaskColorAttr != null ? XmlHelper.LoadColor(ownerMaskColorAttr.Value) : RCColor.Undefined);

            /// Load the sprites.
            LoadSprites(spritePaletteElem, spritePalette, defaultVariant);
            return spritePalette;
        }

        /// <summary>
        /// Loads the sprites of the given sprite palette from the given XML-node.
        /// </summary>
        /// <typeparam name="TVariant">The type of the enumeration that determines the variants of a sprite.</typeparam>
        /// <param name="spritePaletteElem">The XML-node to load from.</param>
        /// <param name="palette">The target sprite palette.</param>
        /// <param name="defaultVariant">The default variant that should be used when no variant was found for a sprite definition.</param>
        private static void LoadSprites<TVariant>(XElement spritePaletteElem, SpritePalette<TVariant> palette, TVariant defaultVariant) where TVariant : struct
        {
            foreach (XElement spriteElem in spritePaletteElem.Elements(SPRITE_ELEM))
            {
                XAttribute spriteNameAttr = spriteElem.Attribute(SPRITE_NAME_ATTR);
                XAttribute spriteVariantAttr = spriteElem.Attribute(SPRITE_VARIANT_ATTR);
                XAttribute sourceRegionAttr = spriteElem.Attribute(SPRITE_SOURCEREGION_ATTR);
                XAttribute offsetAttr = spriteElem.Attribute(SPRITE_OFFSET_ATTR);
                if (spriteNameAttr == null) { throw new InvalidOperationException("Sprite name not defined for a sprite in sprite palette!"); }
                if (sourceRegionAttr == null) { throw new InvalidOperationException("Source region not defined in sprite palette!"); }

                TVariant variant = defaultVariant;
                if (spriteVariantAttr != null)
                {
                    if (!EnumMap<TVariant, string>.TryDemap(spriteVariantAttr.Value, out variant))
                    {
                        throw new InvalidOperationException(string.Format("Unexpected sprite variant '{0}'!", spriteVariantAttr.Value));
                    }
                }
                palette.AddSprite(spriteNameAttr.Value,
                                  variant,
                                  XmlHelper.LoadIntRectangle(sourceRegionAttr.Value),
                                  offsetAttr != null ? XmlHelper.LoadIntVector(offsetAttr.Value) : new RCIntVector(0, 0));
            }
        }

        /// <summary>
        /// Converts the parameter value string to the given type. 
        /// </summary>
        /// <param name="paramValue">The value string of the parameter.</param>
        /// <param name="paramType">The type of the parameter.</param>
        /// <returns>The value of the parameter in the given type.</returns>
        private static object ParseParameterValue(string paramValue, CtorParamType paramType)
        {
            // TODO: extend this switch if necessary!
            switch (paramType)
            {
                case CtorParamType.INT:
                    return XmlHelper.LoadInt(paramValue);
                case CtorParamType.FLOAT:
                    return float.Parse(paramValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                case CtorParamType.BOOL:
                    return XmlHelper.LoadBool(paramValue);
                case CtorParamType.STRING:
                    return paramValue;
                default:
                    throw new ConfigurationException(string.Format("Unable to convert parameter value '{0}' to type '{1}'!", paramValue, paramType.ToString()));
            }
        }

        /// <summary>
        /// Enumerates the possible types of constructor parameters.
        /// TODO: extend this enumeration if necessary!
        /// </summary>
        private enum CtorParamType
        {
            [EnumMapping("INT")]
            INT = 0,

            [EnumMapping("FLOAT")]
            FLOAT = 1,

            [EnumMapping("BOOL")]
            BOOL = 2,

            [EnumMapping("STRING")]
            STRING = 3
        }

        /// <summary>
        /// Regular expression for checking the syntax of the RCNumbers.
        /// </summary>
        private static readonly Regex RCNUMBER_SYNTAX = new Regex("^[+-]?[0-9]{1,5}" + Regex.Escape(".") + "[0-9]{1,3}$");

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        public const string SPRITEPALETTE_IMAGE_ATTR = "image";
        public const string SPRITEPALETTE_TRANSPCOLOR_ATTR = "transparentColor";
        public const string SPRITEPALETTE_MASKCOLOR_ATTR = "maskColor";
        public const string SPRITE_ELEM = "sprite";
        public const string SPRITE_NAME_ATTR = "name";
        public const string SPRITE_VARIANT_ATTR = "variant";
        public const string SPRITE_SOURCEREGION_ATTR = "sourceRegion";
        public const string SPRITE_OFFSET_ATTR = "offset";

        public const string INSTANTIATION_ASSEMBLY_ATTR = "assembly";
        public const string INSTANTIATION_CLASS_ATTR = "class";
        public const string INSTANTIATION_CTORPARAM_ELEM = "ctorParam";
        public const string INSTANTIATION_CTORPARAM_TYPE_ATTR = "type";
    }
}
