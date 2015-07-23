using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Engine
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
        /// <param name="initialAmount">The initial amount of resources in this resource object.</param>
        protected ResourceObject(string elementTypeName, int initialAmount)
            : base(elementTypeName)
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

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position)
        {
            bool attachToMapSuccess = base.AttachToMap(position);
            if (attachToMapSuccess) { this.SetAnimationsImpl(); }
            return attachToMapSuccess;
        }

        /// <summary>
        /// This method is called when the animation of this ResourceObject shall be changed.
        /// </summary>
        protected virtual void SetAnimationsImpl() { }

        /// <summary>
        /// This method is called when the amount of resources in this resource object has been changed.
        /// </summary>
        private void OnResourceAmountChanged(object sender, EventArgs args)
        {
            if (this.resourceAmount.Read() < 0) { throw new InvalidOperationException("The amount of resources cannot be negative!"); }

            if (this.MapObject != null) { this.SetAnimationsImpl(); }
        }

        /// <summary>
        /// The amount of resources in this resource object.
        /// </summary>
        private readonly HeapedValue<int> resourceAmount;
    }

    /// <summary>
    /// Represents a mineral field.
    /// </summary>
    public class MineralField : ResourceObject
    {
        /// <summary>
        /// Constructs a mineral field instance.
        /// </summary>
        public MineralField()
            : base(MINERALFIELD_TYPE_NAME, INITIAL_RESOURCE_AMOUNT)
        {
        }

        /// <see cref="ResourceObject.SetAnimationsImpl"/>
        protected override void SetAnimationsImpl()
        {
            if (this.ResourceAmount.Read() == 0) { throw new InvalidOperationException("The amount of minerals in a mineral field cannot be 0!"); }

            if (this.ResourceAmount.Read() <= 200) { this.MapObject.SetCurrentAnimation("Amount_0_200", this.MotionControl.VelocityVector); }
            else if (this.ResourceAmount.Read() <= 400) { this.MapObject.SetCurrentAnimation("Amount_200_400", this.MotionControl.VelocityVector); }
            else if (this.ResourceAmount.Read() <= 700) { this.MapObject.SetCurrentAnimation("Amount_400_700", this.MotionControl.VelocityVector); }
            else { this.MapObject.SetCurrentAnimation("Amount_Full", this.MotionControl.VelocityVector); }
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
        public VespeneGeyser()
            : base(VESPENEGEYSER_TYPE_NAME, INITIAL_RESOURCE_AMOUNT)
        {
        }

        /// <see cref="ResourceObject.SetAnimationsImpl"/>
        protected override void SetAnimationsImpl()
        {
            if (this.ResourceAmount.Read() == 0) { this.MapObject.SetCurrentAnimation("Depleted", this.MotionControl.VelocityVector); }
            else { this.MapObject.SetCurrentAnimation("Normal", this.MotionControl.VelocityVector); }
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
