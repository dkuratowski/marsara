using RC.Common;
using RC.Engine.Simulator.Behaviors;
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
    public abstract class ResourceObject : Entity
    {
        /// <summary>
        /// Constructs a resource object.
        /// </summary>
        /// <param name="elementTypeName">The name of the element type of the resource object.</param>
        /// <param name="initialAmount">The initial amount of resources in this resource object.</param>
        protected ResourceObject(string elementTypeName, int initialAmount)
            : base(elementTypeName, false, new ResourceObjectBehavior())
        {
            if (initialAmount < 0) { throw new ArgumentOutOfRangeException("initialAmount"); }

            this.resourceAmount = this.ConstructField<int>("resourceAmount");
            this.resourceAmount.Write(initialAmount);
        }

        /// <summary>
        /// Gets the value of the amount of resources in this resource object.
        /// </summary>
        public IValue<int> ResourceAmount { get { return this.resourceAmount; } }

        /// <see cref="ScenarioElement.AttachToMap"/>
        public override bool AttachToMap(RCNumVector position, params ScenarioElement[] elementsToIgnore)
        {
            bool attachToMapSuccess = base.AttachToMap(position, elementsToIgnore);
            if (attachToMapSuccess)
            {
                this.MotionControl.Fix();
                this.SetAnimationsImpl();
            }
            return attachToMapSuccess;
        }

        /// <summary>
        /// This method is called when the animation of this ResourceObject shall be changed.
        /// </summary>
        protected virtual void SetAnimationsImpl() { }

        /// <summary>
        /// This method is called when the amount of resources in this resource object has been changed.
        /// </summary>
        private void OnResourceAmountChanged()
        {
            if (this.resourceAmount.Read() < 0) { throw new InvalidOperationException("The amount of resources cannot be negative!"); }
            if (this.MapObject != null) { this.SetAnimationsImpl(); }
        }

        /// <summary>
        /// The base class of the behaviors of the resource objects.
        /// </summary>
        private class ResourceObjectBehavior : EntityBehavior
        {
            /// <summary>
            /// Constructs a ResourceObjectBehavior instance.
            /// </summary>
            public ResourceObjectBehavior()
            {
                this.lastKnownResourceAmount = this.ConstructField<int>("lastKnownResourceAmount");
                this.lastKnownResourceAmount.Write(-1);
            }

            /// <see cref="EntityBehavior.UpdateMapObject"/>
            public override void UpdateMapObject(Entity entity)
            {
                ResourceObject resourceObj = (ResourceObject)entity;
                if (this.lastKnownResourceAmount.Read() == -1 ||
                    this.lastKnownResourceAmount.Read() != resourceObj.ResourceAmount.Read())
                {
                    resourceObj.OnResourceAmountChanged();
                }
            }

            /// <summary>
            /// Stores the last known value of resource amount or -1 if no such value is known yet.
            /// </summary>
            private readonly HeapedValue<int> lastKnownResourceAmount;
        }

        /// <summary>
        /// The amount of resources in this resource object.
        /// </summary>
        private readonly HeapedValue<int> resourceAmount;
    }

    /// <summary>
    /// Represents a mineral field.
    /// </summary>
    public class MineralField : ResourceObject, IResourceProvider
    {
        /// <summary>
        /// Constructs a mineral field instance.
        /// </summary>
        public MineralField()
            : base(MINERALFIELD_TYPE_NAME, INITIAL_RESOURCE_AMOUNT)
        {
        }

        #region IResourceProvider methods

        /// <see cref="IResourceProvider.MineralsAmount"/>
        int IResourceProvider.MineralsAmount { get { return this.ResourceAmount.Read(); } }

        /// <see cref="IResourceProvider.VespeneGasAmount"/>
        int IResourceProvider.VespeneGasAmount { get { return -1; } }

        #endregion IResourceProvider methods

        /// <see cref="ResourceObject.SetAnimationsImpl"/>
        protected override void SetAnimationsImpl()
        {
            if (this.ResourceAmount.Read() == 0) { throw new InvalidOperationException("The amount of minerals in a mineral field cannot be 0!"); }

            if (this.ResourceAmount.Read() <= 200)
            {
                this.MapObject.StartAnimation("Amount_0_200", this.MotionControl.VelocityVector);
                this.MapObject.StopAnimation("Amount_200_400");
                this.MapObject.StopAnimation("Amount_400_700");
                this.MapObject.StopAnimation("Amount_Full");
            }
            else if (this.ResourceAmount.Read() <= 400)
            {
                this.MapObject.StopAnimation("Amount_0_200");
                this.MapObject.StartAnimation("Amount_200_400", this.MotionControl.VelocityVector);
                this.MapObject.StopAnimation("Amount_400_700");
                this.MapObject.StopAnimation("Amount_Full");
            }
            else if (this.ResourceAmount.Read() <= 700)
            {
                this.MapObject.StopAnimation("Amount_0_200");
                this.MapObject.StopAnimation("Amount_200_400");
                this.MapObject.StartAnimation("Amount_400_700", this.MotionControl.VelocityVector);
                this.MapObject.StopAnimation("Amount_Full");
            }
            else
            {
                this.MapObject.StopAnimation("Amount_0_200");
                this.MapObject.StopAnimation("Amount_200_400");
                this.MapObject.StopAnimation("Amount_400_700");
                this.MapObject.StartAnimation("Amount_Full", this.MotionControl.VelocityVector);
            }
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
    public class VespeneGeyser : ResourceObject, IResourceProvider
    {
        /// <summary>
        /// Constructs a vespene geyser instance.
        /// </summary>
        public VespeneGeyser()
            : base(VESPENEGEYSER_TYPE_NAME, INITIAL_RESOURCE_AMOUNT)
        {
        }

        #region IResourceProvider methods

        /// <see cref="IResourceProvider.MineralsAmount"/>
        int IResourceProvider.MineralsAmount { get { return -1; } }

        /// <see cref="IResourceProvider.VespeneGasAmount"/>
        int IResourceProvider.VespeneGasAmount { get { return this.ResourceAmount.Read(); } }

        #endregion IResourceProvider methods

        /// <see cref="ResourceObject.SetAnimationsImpl"/>
        protected override void SetAnimationsImpl()
        {
            if (this.ResourceAmount.Read() == 0)
            {
                this.MapObject.StopAnimation("Normal");
                this.MapObject.StartAnimation("Depleted", this.MotionControl.VelocityVector);
            }
            else
            {
                this.MapObject.StartAnimation("Normal", this.MotionControl.VelocityVector);
                this.MapObject.StopAnimation("Depleted");
            }
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
