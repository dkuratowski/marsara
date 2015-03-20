using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RC.Common.Diagnostics;

namespace RC.Common.Configuration
{
    /// <summary>
    /// Loads the contents of a trace configuration file.
    /// </summary>
    class TraceConfigLoader : ConfigObjectContents
    {
        #region ConfigObjectContents members

        /// <see cref="ConfigObjectContents.Load_i"/>
        protected override void Load_i(XElement rootElem)
        {
            IEnumerable<XElement> childElems = rootElem.Elements();
            foreach (XElement elem in childElems)
            {
                string elemName = elem.Name.ToString();
                if (elemName.CompareTo(REGISTER_TRACE_FILTER_ELEM) == 0)
                {
                    RegisterTraceFilter(elem);
                }
                else if (elemName.CompareTo(ACTIVATE_TRACE_FILTERS_ELEM) == 0)
                {
                    SwitchTraceFilters(elem, true);
                }
                else if (elemName.CompareTo(DEACTIVATE_TRACE_FILTERS_ELEM) == 0)
                {
                    SwitchTraceFilters(elem, false);
                }
                else if (elemName.CompareTo(REGISTER_TRACE_ELEM) == 0)
                {
                    RegisterTrace(elem);
                }
            }
        }

        #endregion ConfigObjectContents members

        /// <summary>
        /// Enumerates the possible types of trace object constructor parameters.
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
        /// Registers the trace filter defined by the given XML element.
        /// </summary>
        /// <param name="regElem">The XML element that contains the registration instruction.</param>
        private void RegisterTraceFilter(XElement regElem)
        {
            XAttribute namespaceAttr = regElem.Attribute(NAMESPACE_ATTR);
            XAttribute nameAttr = regElem.Attribute(NAME_ATTR);

            if (namespaceAttr != null && nameAttr != null)
            {
                string newTraceFilterName = string.Format("{0}.{1}", namespaceAttr.Value, nameAttr.Value);
                TraceManager.RegisterTraceFilter(newTraceFilterName);
            }
            else
            {
                /// Error: no namespace or name defined
                throw new ConfigurationException("No namespace or name defined for trace filter!");
            }
        }

        /// <summary>
        /// Activates/deactivates the trace filters defined by the given XML element.
        /// </summary>
        /// <param name="actElem">The XML element that contains the activation/deactivation instruction.</param>
        /// <param name="activate">True in case of activation, false in case of deactivation.</param>
        private void SwitchTraceFilters(XElement actElem, bool activate)
        {
            XAttribute patternAttr = actElem.Attribute(PATTERN_ATTR);

            if (patternAttr != null && patternAttr.Value != null && patternAttr.Value.Length != 0)
            {
                TraceManager.SwitchTraceFilters(patternAttr.Value, activate);
            }
            else
            {
                /// Error: no namespace or name defined
                throw new ConfigurationException("No pattern defined for trace target switch!");
            }
        }

        /// <summary>
        /// Creates and registers the trace object defined by the given XML element.
        /// </summary>
        /// <param name="regElem">The XML element that contains the registration instruction.</param>
        private void RegisterTrace(XElement regElem)
        {
            /// Get the name of the assembly that contains the trace class.
            XElement assemblyElem = regElem.Element(ASSEMBLY_ELEM);
            if (assemblyElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", ASSEMBLY_ELEM)); }

            /// Get the name of the trace class.
            XElement classElem = regElem.Element(CLASS_ELEM);
            if (classElem == null) { throw new ConfigurationException(string.Format("<{0}> element not found!", CLASS_ELEM)); }

            /// Collect the parameters for the constructor of the trace object.
            List<object> ctorParams = new List<object>();
            IEnumerable<XElement> ctorParamElems = regElem.Elements(CTOR_PARAM_ELEM);
            foreach (XElement paramElem in ctorParamElems)
            {
                XAttribute paramTypeAttr = paramElem.Attribute(TYPE_ATTR);
                if (paramTypeAttr != null)
                {
                    /// Try to parse the constant type string.
                    CtorParamType paramType;
                    if (!EnumMap<CtorParamType, string>.TryDemap(paramTypeAttr.Value, out paramType))
                    {
                        throw new ConfigurationException(string.Format("Unexpected constructor parameter type {0} defined.", paramTypeAttr.Value));
                    }

                    object parameter = ParseParameterValue(paramElem.Value, paramType);
                    ctorParams.Add(parameter);
                }
                else
                {
                    throw new ConfigurationException("No type defined for trace constructor parameter!");
                }
            }

            /// Load the assembly that contains the trace class and create an instance of that class.
            Assembly asm = Assembly.Load(assemblyElem.Value);
            if (asm != null)
            {
                Type trcType = asm.GetType(classElem.Value);
                if (trcType != null)
                {
                    ITrace newTrc = Activator.CreateInstance(trcType, ctorParams.ToArray()) as ITrace;
                    if (newTrc != null)
                    {
                        /// Register the created trace object to the trace manager.
                        TraceManager.RegisterTrace(newTrc);
                    }
                    else
                    {
                        throw new ConfigurationException(string.Format("Type {0} doesn't implement interface RC.Common.Diagnostics.ITrace!", classElem.Value));
                    }
                }
                else
                {
                    throw new ConfigurationException(string.Format("Unable to load type {0} from assembly {1}!", classElem.Value, assemblyElem.Value));
                }
            }
            else
            {
                throw new ConfigurationException(string.Format("Unable to load assembly {0}!", assemblyElem.Value));
            }
        }

        /// <summary>
        /// Converts the parameter value string to the given type. 
        /// </summary>
        /// <param name="paramValue">The value string of the parameter.</param>
        /// <param name="paramType">The type of the parameter.</param>
        /// <returns>The value of the parameter in the given type.</returns>
        private object ParseParameterValue(string paramValue, CtorParamType paramType)
        {
            switch (paramType)
            {
                case CtorParamType.INT:
                    return int.Parse(paramValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                case CtorParamType.FLOAT:
                    return float.Parse(paramValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                case CtorParamType.BOOL:
                    return bool.Parse(paramValue);
                case CtorParamType.STRING:
                    return paramValue;
                default:
                    throw new ConfigurationException(string.Format("Unable to convert parameter value {0} to type {1}!", paramValue, paramType.ToString()));
            }
        }

        /// <summary>
        /// Supported XML elements and attributes in trace configuration files.
        /// </summary>
        private const string REGISTER_TRACE_FILTER_ELEM = "registerTraceFilter";
        private const string ACTIVATE_TRACE_FILTERS_ELEM = "activateTraceFilters";
        private const string DEACTIVATE_TRACE_FILTERS_ELEM = "deactivateTraceFilters";
        private const string REGISTER_TRACE_ELEM = "registerTrace";
        private const string ASSEMBLY_ELEM = "assembly";
        private const string CLASS_ELEM = "class";
        private const string CTOR_PARAM_ELEM = "ctorParam";
        private const string NAMESPACE_ATTR = "namespace";
        private const string NAME_ATTR = "name";
        private const string PATTERN_ATTR = "pattern";
        private const string TYPE_ATTR = "type";
    }
}
