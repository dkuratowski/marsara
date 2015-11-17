using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a production line that produces addons.
    /// </summary>
    public class AddonProductionLine : ProductionLine
    {
        /// <summary>
        /// Constructs a new AddonProductionLine instance for the given building with the given list of addon types it can produce.
        /// </summary>
        /// <param name="ownerBuilding">The owner of this production line.</param>
        /// <param name="addonProducts">The type of addons products that can be produced by this production line.</param>
        public AddonProductionLine(Building ownerBuilding, List<IAddonType> addonProducts)
            : base(ownerBuilding, ADDON_PRODUCTION_LINE_CAPACITY, new List<IScenarioElementType>(addonProducts))
        {
            this.addonProducts = new Dictionary<string, IAddonType>();
            foreach (IAddonType addonProduct in addonProducts) { this.addonProducts.Add(addonProduct.Name, addonProduct); }

            this.ownerBuilding = this.ConstructField<Building>("ownerBuilding");
            this.ownerBuilding.Write(ownerBuilding);
        }

        /// <see cref="ProductionLine.CreateJob"/>
        protected override ProductionJob CreateJob(string productName, int jobID)
        {
            return new AddonProductionJob(this.ownerBuilding.Read(), this.addonProducts[productName], jobID);
        }

        /// <see cref="ProductionLine.IsProductAvailableImpl"/>
        protected override bool IsProductAvailableImpl(string productName)
        {
            return this.ownerBuilding.Read().CurrentAddon == null;
        }

        /// <summary>
        /// Reference to the owner building of this production line.
        /// </summary>
        private readonly HeapedValue<Building> ownerBuilding;

        /// <summary>
        /// The list of the addon types that can be produced by this production line mapped by their names.
        /// </summary>
        private readonly Dictionary<string, IAddonType> addonProducts;

        private const int ADDON_PRODUCTION_LINE_CAPACITY = 1;
    }
}
