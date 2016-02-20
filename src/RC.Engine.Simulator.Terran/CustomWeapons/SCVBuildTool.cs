using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Engine;
using RC.Common;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;

namespace RC.Engine.Simulator.Terran.CustomWeapons
{
    /// <summary>
    /// Represents the build tool of an SCV.
    /// </summary>
    class SCVBuildTool : CustomWeapon
    {
        /// <summary>
        /// Constructs an SCVBuildTool instance.
        /// </summary>
        public SCVBuildTool()
        {
            this.frameIndexOfLastLaunch = this.ConstructField<int>("frameIndexOfLastLaunch");
            this.frameIndexOfLastLaunch.Write(-1);
        }

        /// <see cref="Weapon.CanTargetEntity"/>
        public override bool CanTargetEntity(Entity entityToCheck)
        {
            TerranBuilding terranBuilding = entityToCheck as TerranBuilding;
            return terranBuilding != null &&
                   terranBuilding.ConstructionJob != null &&
                   !terranBuilding.ConstructionJob.IsFinished &&
                   terranBuilding.ConstructionJob.AttachedSCV == this.Owner;
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
