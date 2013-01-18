using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Represents a string with sections resolved only at runtime.
    /// </summary>
    public class DynamicString
    {
        /// <summary>
        /// Represents a method that resolves a runtime section of a DynamicString.
        /// </summary>
        /// <returns></returns>
        public delegate string Resolver();

        #region Static methods

        /// <summary>
        /// Creates a DynamicString instance from the given expression.
        /// </summary>
        /// <param name="expression">
        /// A string that have runtime resolved sections marked with the syntax "$(resolver-name)".
        /// For example: "The current time is $(get-current-time)".
        /// To specify a single literal '$' character in the expression, specify two '$' characters; that is,
        /// "$$".
        /// </param>
        /// <returns>The created DynamicString instance.</returns>
        public static DynamicString FromString(string expression)
        {
            if (expression == null) { throw new ArgumentNullException("expression"); }

            DynamicString newCfgString = new DynamicString();
            int searchIdx = 0;
            int searchOffset = 0;
            while (searchIdx + searchOffset < expression.Length)
            {
                int dollarIdx = expression.IndexOf('$', searchIdx + searchOffset);
                if (dollarIdx == -1)
                {
                    /// No more runtime sections, add the remaining part of the expression to the
                    /// section list.
                    newCfgString.sections.Add(expression.Substring(searchIdx).Replace("$$", "$"));
                    newCfgString.runtimeFlags.Add(false);
                    break;
                }
                else
                {
                    /// Potentially runtime section found, we have to check it.
                    if (expression.Length > dollarIdx + 1 && expression[dollarIdx + 1] == '$')
                    {
                        /// No, it's not a runtime section, just one simple '$' character escaped.
                        searchOffset = dollarIdx + 2 - searchIdx;
                        continue;
                    }
                    else if (expression.Length > dollarIdx + 1 && expression[dollarIdx + 1] == '(')
                    {
                        /// We have found the beginning of a runtime section, so we have to store the
                        /// previous non-runtime section.
                        if (dollarIdx - searchIdx > 0)
                        {
                            newCfgString.sections.Add(
                                expression.Substring(searchIdx, dollarIdx - searchIdx).Replace("$$", "$"));
                            newCfgString.runtimeFlags.Add(false);
                        }

                        /// Now parse the runtime section.
                        int sectionEndIdx = expression.IndexOf(')', dollarIdx);
                        if (sectionEndIdx != -1 && sectionEndIdx - dollarIdx > 2)
                        {
                            /// End of the runtime section has been found
                            string runtimeSection = expression.Substring(dollarIdx + 2, sectionEndIdx - (dollarIdx + 2));
                            newCfgString.sections.Add(runtimeSection);
                            newCfgString.runtimeFlags.Add(true);
                            searchIdx = sectionEndIdx + 1;
                            searchOffset = 0;
                        }
                        else
                        {
                            /// Syntax error
                            throw new ConfigurationException(string.Format("Syntax error in DynamicString expression: {0}", expression));
                        }
                    }
                    else
                    {
                        /// Syntax error
                        throw new ConfigurationException(string.Format("Syntax error in DynamicString expression: {0}", expression));
                    }
                }
            }
            
            return newCfgString;
        }

        /// <summary>
        /// Registers a resolver method to the DynamicString system.
        /// </summary>
        /// <param name="name">The name of the resolver method to register.</param>
        /// <param name="method">A reference to the resolver method to register.</param>
        public static void RegisterResolver(string name, Resolver method)
        {
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException("name"); }
            if (method == null) { throw new ArgumentNullException("method"); }            
            if (resolverMethods.ContainsKey(name)) { throw new ConfigurationException(string.Format("Resolver method with name '{0}' already exists!", name)); }
            resolverMethods.Add(name, method);
        }

        /// <summary>
        /// Unregisters a resolver method from the DynamicString system.
        /// </summary>
        /// <param name="name">The name of the resolver method to unregister.</param>
        public static void UnregisterResolver(string name)
        {
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException("name"); }
            if (!resolverMethods.ContainsKey(name)) { throw new ConfigurationException(string.Format("Resolver method with name '{0}' doesn't exist!", name)); }
            resolverMethods.Remove(name);
        }

        #endregion Static methods

        #region Public methods

        /// <summary>
        /// Gets the current value of this DynamicString.
        /// </summary>
        public string Value
        {
            get
            {
                StringBuilder strBuilder = new StringBuilder();
                for (int i = 0; i < this.sections.Count; i++)
                {
                    if (runtimeFlags[i])
                    {
                        if (!resolverMethods.ContainsKey(this.sections[i])) { throw new ConfigurationException(string.Format("Resolver method with name '{0}' doesn't exist!", this.sections[i])); }
                        strBuilder.Append(resolverMethods[this.sections[i]]());
                    }
                    else
                    {
                        strBuilder.Append(this.sections[i]);
                    }
                }
                return strBuilder.ToString();
            }
        }

        /// <summary>
        /// Use the DynamicString.Value property instead!
        /// </summary>
        public override string ToString()
        {
            throw new InvalidOperationException("Use the DynamicString.Value property instead!");
        }

        /// <summary>
        /// Checks whether the specified object is a DynamicString and has the same value as this DynamicString.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if obj is a DynamicString and has the same value as this DynamicString.</returns>
        public override bool Equals(object obj)
        {
            return (obj is DynamicString) && Equals((DynamicString)obj);
        }

        /// <summary>
        /// Checks whether this DynamicString has the same value as the specified DynamicString.
        /// </summary>
        /// <param name="other">The DynamicString to test.</param>
        /// <returns>True if other DynamicString has the same value as this DynamicString.</returns>
        public bool Equals(DynamicString other)
        {
            return other.Value == this.Value;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        #endregion Public methods

        #region Private fields

        /// <summary>
        /// Private constructor.
        /// </summary>
        private DynamicString()
        {
            this.sections = new List<string>();
            this.runtimeFlags = new List<bool>();
        }

        /// <summary>
        /// List of the sections of this DynamicString.
        /// </summary>
        private List<string> sections;

        /// <summary>
        /// List of flags indicating the runtime and non-runtime sections.
        /// </summary>
        private List<bool> runtimeFlags;

        /// <summary>
        /// List of the registered resolver methods mapped by their name.
        /// </summary>
        private static Dictionary<string, Resolver> resolverMethods = new Dictionary<string, Resolver>();

        #endregion Private fields
    }
}
