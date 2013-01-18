using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
        public static RCIntVector LoadVector(string fromStr)
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
        /// Loads an RCIntRectangle defined in the given string in format: "X;Y;Width;Height".
        /// </summary>
        /// <param name="fromStr">The string to load from.</param>
        /// <returns>The loaded RCIntRectangle.</returns>
        public static RCIntRectangle LoadRectangle(string fromStr)
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
    }
}
