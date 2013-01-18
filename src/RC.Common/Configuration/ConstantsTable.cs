using System;
using System.Collections.Generic;
using System.Globalization;

namespace RC.Common.Configuration
{
    /// <summary>
    /// This static class is used to store constants defined in the configuration files.
    /// </summary>
    public static class ConstantsTable
    {
        /// <summary>
        /// Adds the given constant name, value and type to this map.
        /// </summary>
        /// <param name="constName">Name of the constant you want to add.</param>
        /// <param name="constValue">Value of the constant you want to add.</param>
        /// <param name="constType">Type of the constant you want to add.</param>
        /// <remarks>
        /// If a constant with the same name has already been registered then it will be overwritten.
        /// </remarks>
        public static void Add(string constName, string constValue, string constType)
        {
            if (constName == null || constName.Length == 0) { throw new ArgumentNullException("constName"); }
            if (constValue == null) { throw new ArgumentNullException("constValue"); }
            if (constType == null) { throw new ArgumentNullException("constType"); }

            /// Try to parse the constant type string.
            ConstantType constTypeEnum;
            if (!EnumMap<ConstantType, string>.Demap(constType, out constTypeEnum))
            {
                throw new ConfigurationException(string.Format("Unexpected constant type {0} defined for constant {1}.", constType, constName));
            }

            object constObj = ParseConstantValue(constValue, constTypeEnum);

            if (constantTable.ContainsKey(constName))
            {
                /// Overwrite the existing constant in the table.
                constantTable[constName] = constObj;
            }
            else
            {
                /// Insert the new constant into the table.
                constantTable.Add(constName, constObj);
            }
        }

        /// <summary>
        /// Checks whether this table already contains the given constant or not.
        /// </summary>
        /// <param name="constName">The name of the constant you want to check.</param>
        /// <returns>True if this map already contains the given constant, false otherwise.</returns>
        public static bool Contains(string constName)
        {
            if (constName == null || constName.Length == 0) { throw new ArgumentNullException("constName"); }

            return constantTable.ContainsKey(constName);
        }

        /// <summary>
        /// Gets the value of the given constant.
        /// </summary>
        /// <param name="constName">Name of the constant you want to get.</param>
        /// <returns>The value of the given constant.</returns>
        public static T Get<T>(string constName)
        {
            if (constName == null || constName.Length == 0) { throw new ArgumentNullException("constName"); }
            if (!constantTable.ContainsKey(constName)) { throw new ConfigurationException(string.Format("Constant {0} not found!", constName)); }

            object constObj = constantTable[constName];
            Type actualType = constObj.GetType();
            Type requestedType = typeof(T);

            if (actualType.Equals(requestedType))
            {
                return (T)constObj;
            }
            else
            {
                throw new ConfigurationException(string.Format("Constant type mismatch! Actual: {0}. Requested: {1}.", actualType.ToString(), requestedType.ToString()));
            }
        }

        /// <summary>
        /// Clears the constant table.
        /// </summary>
        public static void Clear()
        {
            constantTable.Clear();
        }

        /// <summary>
        /// Enumerates the possible constant types.
        /// </summary>
        private enum ConstantType
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
        /// Converts the constant value string to the given type. 
        /// </summary>
        /// <param name="constValue">The value string of the constant.</param>
        /// <param name="constType">The type of the constant.</param>
        /// <returns>The value of the constant in the given type.</returns>
        private static object ParseConstantValue(string constValue, ConstantType constType)
        {
            switch (constType)
            {
                case ConstantType.INT:
                    return int.Parse(constValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case ConstantType.FLOAT:
                    return float.Parse(constValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                case ConstantType.BOOL:
                    return bool.Parse(constValue);
                case ConstantType.STRING:
                    return constValue;
                default:
                    throw new ConfigurationException(string.Format("Unable to convert constant value {0} to type {1}!", constValue, constType.ToString()));
            }
        }

        /// <summary>
        /// List of the registered constants mapped by their names.
        /// </summary>
        private static Dictionary<string, object> constantTable = new Dictionary<string, object>();
    }
}
