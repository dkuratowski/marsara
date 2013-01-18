using System;
using System.Collections.Generic;
using System.Reflection;

namespace RC.Common
{
    /// <summary>
    /// This attribute can be used on enum members to map them to an arbitrary type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnumMappingAttribute : Attribute
    {
        /// <summary>
        /// Constructs an EnumMappingAttribute object.
        /// </summary>
        /// <param name="mapTarget">
        /// The target object you want to map the enum member on which this attribute is applied.
        /// </param>
        public EnumMappingAttribute(object mapTarget)
        {
            if (mapTarget == null) { throw new ArgumentNullException("mapTarget"); }
            this.mapTarget = mapTarget;
        }

        /// <summary>
        /// Gets the target object you want to map the enum member on which this attribute is applied.
        /// </summary>
        public object MapTarget { get { return this.mapTarget; } }

        /// <summary>
        /// The target object you want to map the enum member on which this attribute is applied.
        /// </summary>
        private object mapTarget;
    }

    /// <summary>
    /// Use this static class to map an enum type to an arbitrary type and vice versa.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <typeparam name="TMapTarget">The type of the map target.</typeparam>
    public static class EnumMap<TEnum, TMapTarget> where TEnum : struct
    {
        /// <summary>
        /// Constructs the mapping table at first use.
        /// </summary>
        static EnumMap()
        {
            mappingTable = null;
            demappingTable = null;
        }

        /// <summary>
        /// Maps the given enum value to it's target value declared by it's EnumMappingAttribute.
        /// </summary>
        /// <param name="enumVal">The enum value you want to map.</param>
        /// <param name="tgtVal">This will contain the mapped value in case of success or default(TMapTarget) otherwise.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public static bool Map(TEnum enumVal, out TMapTarget tgtVal)
        {
            tgtVal = default(TMapTarget);
            if (typeof(TEnum).IsEnum)
            {
                if (mappingTable == null || demappingTable == null)
                {
                    BuildMappingTables();
                }

                if (mappingTable.ContainsKey(enumVal))
                {
                    tgtVal = mappingTable[enumVal];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("TEnum must be an enum!");
            }
        }

        /// <summary>
        /// Maps the given value to it's corresponding enum value declared by EnumMappingAttribute of the enum value.
        /// </summary>
        /// <param name="srcVal">The value you want to map.</param>
        /// <param name="enumVal">This will contain the corresponding enum value in case of success or default(TEnum) otherwise.</param>
        /// <returns>True in case of success, false otherwise.</returns>
        public static bool Demap(TMapTarget srcVal, out TEnum enumVal)
        {
            if (srcVal == null) { throw new ArgumentNullException("srcVal"); }

            enumVal = default(TEnum);
            if (typeof(TEnum).IsEnum)
            {
                if (mappingTable == null || demappingTable == null)
                {
                    BuildMappingTables();
                }

                if (demappingTable.ContainsKey(srcVal))
                {
                    enumVal = demappingTable[srcVal];
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new ArgumentException("TEnum must be an enum!");
            }
        }

        /// <summary>
        /// Builds up the mapping tables for TEnum and TMapTarget.
        /// </summary>
        private static void BuildMappingTables()
        {
            mappingTable = new Dictionary<TEnum, TMapTarget>();
            demappingTable = new Dictionary<TMapTarget, TEnum>();

            Type enumType = typeof(TEnum);
            FieldInfo[] enumFields = enumType.GetFields();

            /// Parse the fields of the attributes of TEnum.
            for (int i = 0; i < enumFields.Length; i++)
            {
                EnumMappingAttribute[] enumAttributes =
                    enumFields[i].GetCustomAttributes(typeof(EnumMappingAttribute), false) as EnumMappingAttribute[];

                /// Parse the EnumMappingAttributes of the current enum member. 
                if (enumAttributes != null)
                {
                    for (int j = 0; j < enumAttributes.Length; j++)
                    {
                        /// Check the type of the current attribute.
                        Type tgtType = enumAttributes[j].MapTarget.GetType();
                        if (tgtType.Equals(typeof(TMapTarget)))
                        {
                            /// Insert the found map into the mapping tables.
                            TEnum enumVal = (TEnum)enumFields[i].GetValue(default(TEnum));
                            TMapTarget tgtVal = (TMapTarget)enumAttributes[j].MapTarget;
                            if (!mappingTable.ContainsKey(enumVal) && !demappingTable.ContainsKey(tgtVal))
                            {
                                mappingTable[enumVal] = tgtVal;
                                demappingTable[tgtVal] = enumVal;
                            }
                        }
                    }
                }
            }

            return;
        }

        /// <summary>
        /// The cached mapping table from TEnum to TMapTarget.
        /// </summary>
        private static Dictionary<TEnum, TMapTarget> mappingTable;

        /// <summary>
        /// The cached mapping table from TMapTarget to TEnum.
        /// </summary>
        private static Dictionary<TMapTarget, TEnum> demappingTable;
    }
}
