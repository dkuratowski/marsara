using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Units;

namespace RC.Engine.Simulator.Terran.CustomWeapons
{
    /// <summary>
    /// Represents the repair tool of an SCV.
    /// </summary>
    class SCVRepairTool : CustomWeapon
    {
        /// <summary>
        /// Constructs an SCVRepairTool instance.
        /// </summary>
        public SCVRepairTool()
        {
            this.frameIndexOfLastLaunch = this.ConstructField<int>("frameIndexOfLastLaunch");
            this.frameIndexOfLastLaunch.Write(-1);
        }

        /// <see cref="Weapon.CanTargetEntity"/>
        public override bool CanTargetEntity(Entity entityToCheck)
        {
            /// Check if the target is a damaged Terran building or addon that is not under construction or is a damaged Terran mechanical unit.
            // TODO: Check for additional unit types as well!
            return (entityToCheck is Addon ||
                    entityToCheck is TerranBuilding ||
                    entityToCheck is SCV ||
                    entityToCheck is Goliath) &&
                   !entityToCheck.Biometrics.IsUnderConstruction &&
                   entityToCheck.Biometrics.HP != -1 &&
                   entityToCheck.Biometrics.HP < entityToCheck.ElementType.MaxHP.Read();
        }

        /// <see cref="Weapon.CanLaunchMissiles"/>
        public override bool CanLaunchMissiles()
        {
            int currentFrameIndex = this.Owner.Scenario.CurrentFrameIndex;
            if (this.frameIndexOfLastLaunch.Read() == -1 ||
                currentFrameIndex - this.frameIndexOfLastLaunch.Read() >= this.WeaponData.Cooldown.Read())
            {
                this.frameIndexOfLastLaunch.Write(currentFrameIndex);
                return true;
            }
            return false;
        }

        /// <see cref="Weapon.IsInRange"/>
        public override bool IsInRange(RCNumber distance)
        {
            return distance <= Weapon.NEARBY_DISTANCE;
        }

        /// <summary>
        /// The index of the frame when this build tool launched a missile group last time.
        /// </summary>
        private readonly HeapedValue<int> frameIndexOfLastLaunch;
    }
}
