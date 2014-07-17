using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents the definition of a command workflow.
    /// </summary>
    class CommandWorkflowDefinition
    {
        /// <summary>
        /// Constructs a new CommandWorkflowDefinition instance.
        /// </summary>
        /// <param name="name">The name of the workflow definition.</param>
        /// <param name="spritePalette">The sprite palette of the workflow definition.</param>
        /// <param name="activator">The activator of the workflow definition.</param>
        public CommandWorkflowDefinition(string name, ISpritePalette spritePalette, CommandWorkflowActivator activator)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (spritePalette == null) { throw new ArgumentNullException("spritePalette"); }
            if (activator == null) { throw new ArgumentNullException("activator"); }

            this.name = name;
            this.spritePalette = spritePalette;
            this.activator = activator;
        }

        /// <summary>
        /// Gets the name of this command workflow definition.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the sprite palette of this workflow definition.
        /// </summary>
        public ISpritePalette SpritePalette { get { return this.spritePalette; } }

        /// <summary>
        /// The activator of this workflow definition.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The sprite palette of this workflow definition.
        /// </summary>
        private readonly ISpritePalette spritePalette;

        /// <summary>
        /// The activator of this workflow definition.
        /// </summary>
        private readonly CommandWorkflowActivator activator;
    }
}
