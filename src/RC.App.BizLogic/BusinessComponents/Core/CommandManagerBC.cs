using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// The implementation of the command manager business component.
    /// </summary>
    [Component("RC.App.BizLogic.CommandManagerBC")]
    class CommandManagerBC : ICommandManagerBC, IComponent
    {
        #region IComponent methods

        /// <see cref="IComponent.Start"/>
        public void Start()
        {
            /// Load the command workflow definitions from the configured directory
            this.workflowDefinitions = new Dictionary<string, CommandWorkflowDefinition>();
            DirectoryInfo rootDir = new DirectoryInfo(BizLogicConstants.COMMAND_WORKFLOW_DIR);
            IEnumerator<int> indexSource = this.IndexSourceMethod();
            if (rootDir.Exists)
            {
                FileInfo[] cmdWorkflowFiles = rootDir.GetFiles("*.xml", SearchOption.AllDirectories);
                foreach (FileInfo cmdWorkflowFile in cmdWorkflowFiles)
                {
                    string xmlStr = File.ReadAllText(cmdWorkflowFile.FullName);
                    string imageDir = cmdWorkflowFile.DirectoryName;
                    foreach (CommandWorkflowDefinition workflowDef in XmlCommandWorkflowReader.Read(xmlStr, imageDir, indexSource))
                    {
                        if (this.workflowDefinitions.ContainsKey(workflowDef.Name)) { throw new InvalidOperationException(string.Format("Workflow definition '{0}' already exists!!", workflowDef.Name)); }
                        this.workflowDefinitions.Add(workflowDef.Name, workflowDef);
                    }
                }
            }
        }

        /// <see cref="IComponent.Stop"/>
        public void Stop()
        {
            /// Do nothing
        }
        
        #endregion IComponent methods

        #region ICommandManagerBC methods

        /// <see cref="ICommandManagerBC.CommandWorkflowSpritePalettes"/>
        public IEnumerable<ISpritePalette> CommandWorkflowSpritePalettes
        {
            get
            {
                HashSet<ISpritePalette> returnedPalettes = new HashSet<ISpritePalette>();
                foreach (CommandWorkflowDefinition workflowDef in this.workflowDefinitions.Values)
                {
                    if (!returnedPalettes.Contains(workflowDef.SpritePalette))
                    {
                        returnedPalettes.Add(workflowDef.SpritePalette);
                        yield return workflowDef.SpritePalette;
                    }
                }
            }
        }

        #endregion ICommandManagerBC methods

        /// <summary>
        /// Method for providing indices for the sprite palettes of the command workflow definitions.
        /// </summary>
        /// <returns>An enumerator that provides the indices for the sprite palettes of the command workflow definitions.</returns>
        private IEnumerator<int> IndexSourceMethod()
        {
            int i = 0;
            while (true) { yield return i++; }
        }

        /// <summary>
        /// List of the loaded command workflow definitions mapped by their names.
        /// </summary>
        private Dictionary<string, CommandWorkflowDefinition> workflowDefinitions;
    }
}
