using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.Configuration;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a button listener that selects a building type when the appropriate button is pressed on the command panel.
    /// </summary>
    class BuildingButtonListener : ButtonListener
    {
        /// <see cref="CommandInputListener.CheckCompletionStatus"/>
        /// TODO: implement this method!
        public override bool CheckCompletionStatus() { return true; }

        /// <see cref="CommandInputListener.TryComplete"/>
        /// TODO: implement this method!
        public override bool TryComplete()
        {
            this.CommandBuilder.Parameter = this.selectedBuildingType;
            return true;
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute buildingAttr = listenerElem.Attribute(BUILDING_ATTR);
            if (buildingAttr == null) { throw new InvalidOperationException("Building type not defined for a building button listener!"); }
            this.selectedBuildingType = buildingAttr.Value;
        }

        /// <summary>
        /// The type of the building selected by this listener.
        /// </summary>
        private string selectedBuildingType;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string BUILDING_ATTR = "building";
    }
}
