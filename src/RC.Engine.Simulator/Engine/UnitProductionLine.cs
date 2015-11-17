using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Core;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents a production line that produces units.
    /// </summary>
    public class UnitProductionLine : ProductionLine
    {
        /// <summary>
        /// Constructs a new UnitProductionLine instance for the given building with the given list of unit types it can produce.
        /// </summary>
        /// <param name="ownerBuilding">The owner of this production line.</param>
        /// <param name="unitProducts">The type of unit products that can be produced by this production line.</param>
        public UnitProductionLine(Building ownerBuilding, List<IUnitType> unitProducts)
            : base(ownerBuilding, UNIT_PRODUCTION_LINE_CAPACITY, new List<IScenarioElementType>(unitProducts))
        {
            this.unitProducts = new Dictionary<string, IUnitType>();
            foreach (IUnitType unitProduct in unitProducts) { this.unitProducts.Add(unitProduct.Name, unitProduct); }

            this.ownerBuilding = this.ConstructField<Building>("ownerBuilding");
            this.ownerBuilding.Write(ownerBuilding);
        }

        /// <see cref="ProductionLine.CreateJob"/>
        protected override ProductionJob CreateJob(string productName, int jobID)
        {
            return new UnitProductionJob(this.ownerBuilding.Read(), this.unitProducts[productName], jobID);
        }

        /// <see cref="ProductionLine.IsProductAvailableImpl"/>
        protected override bool IsProductAvailableImpl(string productName)
        {
            return !this.ownerBuilding.Read().MotionControl.IsFlying;
        }

        /// <see cref="ProductionLine.IsProductEnabledImpl"/>
        protected override bool IsProductEnabledImpl(string productName)
        {
            IUnitType product = this.unitProducts[productName];
            return product.NecessaryAddon == null ||
                   (this.ownerBuilding.Read().CurrentAddon != null &&
                   !this.ownerBuilding.Read().CurrentAddon.Biometrics.IsUnderConstruction &&
                    this.ownerBuilding.Read().CurrentAddon.ElementType == product.NecessaryAddon);
        }

        /// <summary>
        /// Reference to the owner building of this production line.
        /// </summary>
        private readonly HeapedValue<Building> ownerBuilding;

        /// <summary>
        /// The list of the unit types that can be produced by this production line mapped by their names.
        /// </summary>
        private readonly Dictionary<string, IUnitType> unitProducts;

        private const int UNIT_PRODUCTION_LINE_CAPACITY = 5;
    }
}
