using System;
using System.Collections.Generic;

namespace RC.Common
{
    /// <summary>
    /// Enumerates the possible types of a field inside an RCPackage.
    /// </summary>
    public enum RCPackageFieldType
    {
        [EnumMapping("BYTE")]
        BYTE = 0,           /// 8-bit unsigned integer

        [EnumMapping("SHORT")]
        SHORT = 1,          /// 16-bit signed integer

        [EnumMapping("INT")]
        INT = 2,            /// 32-bit signed integer

        [EnumMapping("LONG")]
        LONG = 3,           /// 64-bit signed integer

        [EnumMapping("STRING")]
        STRING = 4,         /// UTF-8 encoded string

        [EnumMapping("BYTE_ARRAY")]
        BYTE_ARRAY = 5,     /// An array of 8-bit unsigned integers

        [EnumMapping("SHORT_ARRAY")]
        SHORT_ARRAY = 6,    /// An array of 16-bit signed integers

        [EnumMapping("INT_ARRAY")]
        INT_ARRAY = 7,      /// An array of 32-bit signed integers

        [EnumMapping("LONG_ARRAY")]
        LONG_ARRAY = 8,     /// An array of 64-bit signed integers

        [EnumMapping("STRING_ARRAY")]
        STRING_ARRAY = 9,   /// An array of UTF-8 encoded strings

        UNKNOWN = -1        /// Used to indicate error cases (for internal use only)
    }

    /// <summary>
    /// Defines an RCPackage format that defines the fields of all package in such format. Every format has a 0-based
    /// index that is used to identify the format inside an RCPackage.
    /// </summary>
    public class RCPackageFormat
    {
        /// <summary>
        /// Registers the given package format.
        /// </summary>
        /// <param name="format">The package format you want to register.</param>
        /// <returns>The ID of the registered format.</returns>
        /// <remarks>
        /// This ID is used in packages to define the format of every package.
        /// Warning! If peers want to communicate over a network environment, then all of them should define exactly
        /// the same package formats with the same IDs otherwise the communication is not possible.
        /// </remarks>
        /// <exception cref="RCPackageException">If you want to register a package format with no fields.</exception>
        public static int RegisterFormat(RCPackageFormat format)
        {
            if (format.fieldDefinitions.Count != 0)
            {
                registeredFormats.Add(format);
                format.formatID = registeredFormats.Count - 1;
                format.definitionFinished = true;
                return registeredFormats.Count - 1;
            }
            else
            {
                throw new RCPackageException("Registering RCPackageFormat with no fields is not possible!");
            }
        }

        /// <summary>
        /// Returns the package format with the given ID or null if no such package format exists.
        /// </summary>
        /// <param name="formatID">The ID of the package format you want to get.</param>
        /// <returns>The package format with the given ID or null if no such package format exists.</returns>
        public static RCPackageFormat GetPackageFormat(int formatID)
        {
            if (0 <= formatID && registeredFormats.Count - 1 >= formatID)
            {
                return registeredFormats[formatID];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Class-level initializer function.
        /// </summary>
        static RCPackageFormat()
        {
            registeredFormats = new List<RCPackageFormat>();
        }

        /// <summary>
        /// List of the registered package formats.
        /// </summary>
        private static List<RCPackageFormat> registeredFormats;

        /// <summary>
        /// Constructs a new package format object.
        /// </summary>
        public RCPackageFormat()
        {
            this.fieldDefinitions = new List<RCPackageFieldType>();
            this.formatID = -1;                 /// will be initialized when format is being registered.
            this.name = null;
            this.definitionFinished = false;    /// will be set when format is being registered.
        }

        /// <summary>
        /// Constructs a new package format object with the given name.
        /// </summary>
        public RCPackageFormat(string name)
        {
            if (name == null || name.Length == 0) { throw new ArgumentNullException("name"); }

            this.fieldDefinitions = new List<RCPackageFieldType>();
            this.formatID = -1;                 /// will be initialized when format is being registered.
            this.name = name;
            this.definitionFinished = false;    /// will be set when format is being registered.
        }

        /// <summary>
        /// Defines a new field in this package format.
        /// </summary>
        /// <param name="typeOfField">The type of the defined field.</param>
        /// <returns>The ID of the defined field.</returns>
        /// <exception cref="RCPackageException">
        /// If you give RCPackageFieldType.UNKNOWN in the parameter.
        /// If this RCPackageFormat has been already registered.
        /// </exception>
        /// <remarks>
        /// This ID is used to reference the field in a package with this format.
        /// </remarks>
        public int DefineField(RCPackageFieldType typeOfField)
        {
            if (typeOfField != RCPackageFieldType.UNKNOWN)
            {
                if (!this.definitionFinished)
                {
                    this.fieldDefinitions.Add(typeOfField);
                    return this.fieldDefinitions.Count - 1;
                }
                else
                {
                    throw new RCPackageException("It is not possible to define new fields to a registered RCPackageFormat!");
                }
            }
            else
            {
                throw new RCPackageException("RCPackageFieldType.UNKNOWN cannot be used as a field type!");
            }
        }

        /// <summary>
        /// Gets the type of the given field.
        /// </summary>
        /// <param name="fieldID">The ID of the field whose type you want to get.</param>
        /// <returns>
        /// The type of the field or RCPackageFieldType.UNKNOWN if the given field doesn't exist.
        /// </returns>
        public RCPackageFieldType GetFieldType(int fieldID)
        {
            if (0 <= fieldID && this.fieldDefinitions.Count - 1 >= fieldID)
            {
                return this.fieldDefinitions[fieldID];
            }
            else
            {
                return RCPackageFieldType.UNKNOWN;
            }
        }

        /// <summary>
        /// Gets the number of fields defined by this package format.
        /// </summary>
        public int NumOfFields { get { return this.fieldDefinitions.Count; } }

        /// <summary>
        /// Gets the ID of this package format.
        /// </summary>
        public int ID { get { return this.formatID; } }

        /// <summary>
        /// Gets the name of this package format. Can be used for debugging.
        /// </summary>
        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                else
                {
                    return string.Format("PACKAGE_FORMAT_{0}", this.formatID);
                }
            }
        }

        /// <summary>
        /// List of the types of the fields.
        /// </summary>
        private List<RCPackageFieldType> fieldDefinitions;

        /// <summary>
        /// The ID of this package format.
        /// </summary>
        private int formatID;

        /// <summary>
        /// The name of this package format. Can be used for debugging.
        /// </summary>
        private string name;

        /// <summary>
        /// This flag becomes true when the format is registered. After that it is not possible to define more
        /// fields to this format.
        /// </summary>
        private bool definitionFinished;
    }
}
