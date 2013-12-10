using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Scenarios
{
    /// <summary>
    /// Represents a resource object.
    /// </summary>
    public abstract class ResourceObject : QuadEntity
    {
        /// <summary>
        /// Constructs a resource object.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of the resource object.</param>
        /// <param name="quadCoords">The quadratic coordinates of the resource object.</param>
        /// <param name="initialAmount">The initial amount of resources in this resource object.</param>
        public ResourceObject(string elementTypeName, RCIntVector quadCoords, int initialAmount)
            : base(elementTypeName, quadCoords)
        {
            if (initialAmount < 0) { throw new ArgumentOutOfRangeException("initialAmount"); }

            this.resourceAmount = this.ConstructField<int>("resourceAmount");
            this.resourceAmount.ValueChanged += this.OnResourceAmountChanged;
            this.resourceAmount.Write(initialAmount);
        }

        /// <summary>
        /// Gets the value of the amount of resources in this resource object.
        /// </summary>
        public IValue<int> ResourceAmount { get { return this.resourceAmount; } }

        /// <summary>
        /// This method is called when the amount of resources in this resource object has been changed.
        /// Can be overriden in the derived classes.
        /// </summary>
        protected virtual void OnResourceAmountChangedImpl() { }

        /// <summary>
        /// This method is called when the amount of resources in this resource object has been changed.
        /// </summary>
        private void OnResourceAmountChanged(object sender, EventArgs args)
        {
            if (this.resourceAmount.Read() < 0) { throw new InvalidOperationException("The amount of resources cannot be negative!"); }
            this.OnResourceAmountChangedImpl();
        }

        /// <summary>
        /// The amount of resources in this resource object.
        /// </summary>
        private HeapedValue<int> resourceAmount;
    }

    /// <summary>
    /// Represents a mineral field.
    /// </summary>
    public class MineralField : ResourceObject
    {
        /// <summary>
        /// Constructs a mineral field instance.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the mineral field.</param>
        public MineralField(RCIntVector quadCoords)
            : base(MINERALFIELD_TYPE_NAME, quadCoords, INITIAL_RESOURCE_AMOUNT)
        {
        }

        /// <see cref="ResourceObject.OnResourceAmountChangedImpl"/>
        protected override void OnResourceAmountChangedImpl()
        {
            if (this.ResourceAmount.Read() == 0) { throw new InvalidOperationException("The amount of minerals in a mineral field cannot be 0!"); }

            if (this.ResourceAmount.Read() <= 200) { this.SetCurrentAnimation("Amount_0_200"); }
            else if (this.ResourceAmount.Read() <= 400) { this.SetCurrentAnimation("Amount_200_400"); }
            else if (this.ResourceAmount.Read() <= 700) { this.SetCurrentAnimation("Amount_400_700"); }
            else { this.SetCurrentAnimation("Amount_Full"); }
        }

        /// <summary>
        /// The name of the mineral field element type.
        /// </summary>
        public const string MINERALFIELD_TYPE_NAME = "MineralField";

        /// <summary>
        /// The initial amount of minerals in a mineral field.
        /// </summary>
        public const int INITIAL_RESOURCE_AMOUNT = 1500;

        /// <summary>
        /// The initial amount of minerals in a mineral field.
        /// </summary>
        public const int MINIMUM_RESOURCE_AMOUNT = 10;
    }

    /// <summary>
    /// Represents a vespene geyser.
    /// </summary>
    public class VespeneGeyser : ResourceObject
    {
        /// <summary>
        /// Constructs a vespene geyser instance.
        /// </summary>
        /// <param name="quadCoords">The quadratic coordinates of the vespene geyser.</param>
        public VespeneGeyser(RCIntVector quadCoords)
            : base(VESPENEGEYSER_TYPE_NAME, quadCoords, INITIAL_RESOURCE_AMOUNT)
        {
        }

        /// <see cref="ResourceObject.OnResourceAmountChangedImpl"/>
        protected override void OnResourceAmountChangedImpl()
        {
            if (this.ResourceAmount.Read() == 0) { this.SetCurrentAnimation("Depleted"); }
            else { this.SetCurrentAnimation("Normal"); }
        }

        /// <summary>
        /// The name of the vespene geyser element type.
        /// </summary>
        public const string VESPENEGEYSER_TYPE_NAME = "VespeneGeyser";

        /// <summary>
        /// The initial amount of vespene gas in a vespene geyser.
        /// </summary>
        public const int INITIAL_RESOURCE_AMOUNT = 5000;

        /// <summary>
        /// The minimum amount of vespene gas in a vespene geyser.
        /// </summary>
        public const int MINIMUM_RESOURCE_AMOUNT = 0;
    }
}
