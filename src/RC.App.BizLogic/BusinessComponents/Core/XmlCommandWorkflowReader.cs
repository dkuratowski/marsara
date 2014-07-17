using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Helper class for reading command workflow definition files.
    /// </summary>
    static class XmlCommandWorkflowReader
    {
        /// <summary>
        /// Reads the command workflow definitions from the given XML document.
        /// </summary>
        /// <param name="xmlStr">The string that contains the XML document to read.</param>
        /// <param name="imageDir">The directory where the referenced images can be found. (TODO: this is a hack!)</param>
        /// <param name="spritePaletteIdxSrc">The enumerator that provides the indices for the sprite palettes.</param>
        public static IEnumerable<CommandWorkflowDefinition> Read(string xmlStr, string imageDir, IEnumerator<int> spritePaletteIdxSrc)
        {
            if (xmlStr == null) { throw new ArgumentNullException("xmlStr"); }
            if (imageDir == null) { throw new ArgumentNullException("imageDir"); }
            if (spritePaletteIdxSrc == null) { throw new ArgumentNullException("spritePaletteIdxSrc"); }
            if (!spritePaletteIdxSrc.MoveNext()) { throw new InvalidOperationException("No index for the next sprite palette could be retrieved!"); }

            /// Load the XML document.
            XDocument xmlDoc = XDocument.Parse(xmlStr);

            /// Load the sprite palette.
            XElement spritePaletteElem = xmlDoc.Root.Element(XmlCommandWorkflowConstants.SPRITEPALETTE_ELEM);
            if (spritePaletteElem == null) { throw new InvalidOperationException("SpritePalette definition not found!"); }
            ISpritePalette spritePalette = XmlHelper.LoadSpritePalette(spritePaletteElem, imageDir);
            spritePalette.SetIndex(spritePaletteIdxSrc.Current);

            /// Load the command workflow definitions.
            foreach (XElement commandWorkflowElem in xmlDoc.Root.Elements(XmlCommandWorkflowConstants.COMMANDWORKFLOW_ELEM))
            {
                yield return LoadCommandWorkflow(commandWorkflowElem, spritePalette);
            }
        }

        /// <summary>
        /// Loads a command workflow definition from the given XML-node.
        /// </summary>
        /// <param name="commandWorkflowElem">The XML-node to load from.</param>
        /// <param name="spritePalette">The sprite palette for the command workflow.</param>
        /// <returns>The loaded command workflow definition.</returns>
        private static CommandWorkflowDefinition LoadCommandWorkflow(XElement commandWorkflowElem, ISpritePalette spritePalette)
        {
            XAttribute workflowNameAttr = commandWorkflowElem.Attribute(XmlCommandWorkflowConstants.COMMANDWORKFLOW_NAME_ATTR);
            if (workflowNameAttr == null) { throw new InvalidOperationException("Name not defined for command workflow definition!"); }

            /// Load the activator of the workflow.
            XElement workflowActivatorElem = commandWorkflowElem.Element(XmlCommandWorkflowConstants.COMMANDWORKFLOW_ACTIVATOR_ELEM);
            if (workflowActivatorElem == null) { throw new InvalidOperationException("Activator not defined for command workflow definition!"); }
            CommandWorkflowActivator activator = XmlHelper.CreateInstance<CommandWorkflowActivator>(workflowActivatorElem);

            /// TODO: finish the implementation!
            return new CommandWorkflowDefinition(workflowNameAttr.Value, spritePalette, activator);
        }
    }
}
