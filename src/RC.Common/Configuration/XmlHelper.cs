using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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
        /// Regular expression for checking the syntax of the RCNumbers.
        /// </summary>
        private static readonly Regex RCNUMBER_SYNTAX = new Regex("^[+-]?[0-9]{1,5}" + Regex.Escape(".") + "[0-9]{1,3}$");
    }
}
