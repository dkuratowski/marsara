using System;
using System.Collections.Generic;

namespace RC.Common.Configuration
{
    /// <summary>
    /// This static class is used to map the names of registered RCPackageFormats to their IDs.
    /// </summary>
    public static class RCPackageFormatMap
    {
        /// <summary>
        /// Adds the given package format name and ID to this map.
        /// </summary>
        /// <param name="formatName">Name of the package format you want to add.</param>
        /// <param name="formatId">ID of the package format you want to add.</param>
        /// <exception cref="ConfigurationException">
        /// If a package format ID has already been registered with the same name.
        /// </exception>
        public static void Add(string formatName, int formatId)
        {
            if (formatName == null || formatName.Length == 0) { throw new ArgumentNullException("formatName"); }
            if (formatId < 0) { throw new ArgumentOutOfRangeException("formatId"); }
            if (packageFormatIDs.ContainsKey(formatName)) { throw new ConfigurationException(string.Format("RCPackageFormat {0} has already been registered!", formatName)); }

            packageFormatIDs.Add(formatName, formatId);
        }

        /// <summary>
        /// Checks whether this map already contains the given format or not.
        /// </summary>
        /// <param name="formatName">The name of the format you want to check.</param>
        /// <returns>True if this map already contains the given format, false otherwise.</returns>
        public static bool Contains(string formatName)
        {
            if (formatName == null || formatName.Length == 0) { throw new ArgumentNullException("formatName"); }

            return packageFormatIDs.ContainsKey(formatName);
        }

        /// <summary>
        /// Gets the ID of the given format.
        /// </summary>
        /// <param name="formatName">Name of the format whose ID you want to get.</param>
        /// <returns>The ID of the given format.</returns>
        public static int Get(string formatName)
        {
            if (formatName == null || formatName.Length == 0) { throw new ArgumentNullException("formatName"); }
            if (!packageFormatIDs.ContainsKey(formatName)) { throw new ConfigurationException(string.Format("RCPackageFormat {0} not registered!", formatName)); }

            return packageFormatIDs[formatName];
        }

        /// <summary>
        /// Clears the package format map.
        /// </summary>
        public static void Clear()
        {
            packageFormatIDs.Clear();
        }

        /// <summary>
        /// List of the registered package format IDs mapped by their names.
        /// </summary>
        private static Dictionary<string, int> packageFormatIDs = new Dictionary<string, int>();
    }
}
