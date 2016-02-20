using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// The base class of custom weapon implementations.
    /// </summary>
    public abstract class CustomWeapon : HeapedObject
    {
        #region Overridable methods

        /// <summary>
        /// Checks whether the given entity can be targeted by this weapon.
        /// </summary>
        /// <param name="entityToCheck">The entity to be checked.</param>
        /// <returns>True if the given entity can be targeted by this weapon; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        public abstract bool CanTargetEntity(Entity entityToCheck);

        /// <summary>
        /// Checks whether this weapon is currently be able to launch the next missiles.
        /// </summary>
        /// <returns>True if this weapon is currently be able to launch the next missiles; otherwise false.</returns>
        /// <remarks>Must be overriden in the derived classes.</remarks>
        public abstract bool CanLaunchMissiles();

        /// <summary>
        /// Check whether the given distance is in the range of this weapon.
        /// </summary>
        /// <param name="distance">The distance to be checked in cells.</param>
        /// <returns>True if the given distance is in the range of this weapon.</returns>
        public abstract bool IsInRange(RCNumber distance);

        /// <summary>
        /// This method is called when at least 1 missile of a missile group has been successfully launched.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        public virtual void OnLaunch() { }

        /// <summary>
        /// This method is called when at least 1 missile of a missile group has been successfully impacted.
        /// </summary>
        /// <param name="impactedMissile">Reference to the impacted missile.</param>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        public virtual void OnImpact(Missile impactedMissile) { }

        #endregion Overridable methods

        #region Protected methods for the derived classes

        /// <summary>
        /// Constructs a custom weapon instance.
        /// </summary>
        protected CustomWeapon()
        {
            this.stub = this.ConstructField<CustomWeaponStub>("stub");
            this.stub.Write(null);
        }
        
        /// <summary>
        /// Gets the owner of this weapon.
        /// </summary>
        protected Entity Owner { get { return this.stub.Read().StubOwner; } }

        /// <summary>
        /// Gets the weapon data of this weapon from the metadata.
        /// </summary>
        protected IWeaponData WeaponData { get { return this.stub.Read().WeaponData; } }

        #endregion Protected methods for the derived classes

        #region Internal methods

        /// <summary>
        /// This method is called when this custom weapon is being attached to a stub.
        /// </summary>
        /// <param name="stub">The stub to which this custom weapon is being attached.</param>
        internal void OnAttachingToStub(CustomWeaponStub stub)
        {
            if (stub == null) { throw new ArgumentNullException("stub"); }
            if (this.stub.Read() != null) { throw new InvalidOperationException("This custom weapon has already been attached to another stub!"); }

            this.stub.Write(stub);
        }

        #endregion Internal methods

        /// <summary>
        /// Reference to the stub to which this custom weapon is attached to.
        /// </summary>
        private readonly HeapedValue<CustomWeaponStub> stub;
    }
}
