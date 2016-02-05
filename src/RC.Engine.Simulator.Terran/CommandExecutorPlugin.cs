using System;
using System.Collections.Generic;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Commands;
using RC.Engine.Simulator.ComponentInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Engine.Simulator.Terran.Buildings;
using RC.Engine.Simulator.Terran.Commands;
using RC.Engine.Simulator.Terran.Units;
using RC.Engine.Simulator.Terran.Addons;

namespace RC.Engine.Simulator.Terran
{
    /// <summary>
    /// This class represents the command executor plugin for the Terran race.
    /// </summary>
    [Plugin(typeof(ICommandExecutor))]
    class CommandExecutorPlugin : IPlugin<ICommandExecutorPluginInstall>
    {
        /// <summary>
        /// Constructs a CommandExecutorPlugin instance.
        /// </summary>
        public CommandExecutorPlugin()
        {
        }

        /// <see cref="IPlugin<T>.Install"/>
        public void Install(ICommandExecutorPluginInstall extendedComponent)
        {
            /// Terran SCV
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Move));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Stop));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Attack));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Patrol));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Hold));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Repair));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Gather));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Return));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Build));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.StopBuild));
            extendedComponent.RegisterCommandExecutionFactory(new SCVCmdExecutionFactory(SCVCommandEnum.Undefined));

            /// Terran Marine
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new SpecialAbilityExecutionFactory<Marine>(TerranAbilities.STIMPACKS, Marine.MARINE_TYPE_NAME,
                (recipientMarines, targetPosition, targetEntityID, parameter) =>
                    TerranAbilities.MarineStimPacksFactoryMethod(recipientMarines)));

            /// Terran Goliath
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Attack, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Patrol, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Hold, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Goliath.GOLIATH_TYPE_NAME));

            /// Terran Command Center
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME, SCV.SCV_TYPE_NAME, ComsatStation.COMSATSTATION_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(CommandCenter.COMMANDCENTER_TYPE_NAME));

            /// Terran Comsat Station
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME));

            /// Terran Control Tower
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(ControlTower.CONTROLTOWER_TYPE_NAME));

            /// Terran Barracks
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Barracks.BARRACKS_TYPE_NAME, Marine.MARINE_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(Barracks.BARRACKS_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Barracks.BARRACKS_TYPE_NAME));

            /// Terran Factory
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Factory.FACTORY_TYPE_NAME, Goliath.GOLIATH_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(Factory.FACTORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Factory.FACTORY_TYPE_NAME));

            /// Terran Starport
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, Starport.STARPORT_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, Starport.STARPORT_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, Starport.STARPORT_TYPE_NAME));
            // TODO
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Starport.STARPORT_TYPE_NAME, "Wraith", "Dropship", ControlTower.CONTROLTOWER_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Starport.STARPORT_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(Starport.STARPORT_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(Starport.STARPORT_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Starport.STARPORT_TYPE_NAME));

            /// Terran Academy
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Academy.ACADEMY_TYPE_NAME, TerranUpgrades.U238_SHELLS, TerranAbilities.STIMPACKS));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Academy.ACADEMY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Academy.ACADEMY_TYPE_NAME));

            /// Terran Armory
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(Armory.ARMORY_TYPE_NAME,
                TerranUpgrades.VEHICLE_WEAPONS_1, TerranUpgrades.VEHICLE_WEAPONS_2, TerranUpgrades.VEHICLE_WEAPONS_3,
                TerranUpgrades.VEHICLE_PLATING_1, TerranUpgrades.VEHICLE_PLATING_2, TerranUpgrades.VEHICLE_PLATING_3,
                TerranUpgrades.SHIP_WEAPONS_1, TerranUpgrades.SHIP_WEAPONS_2, TerranUpgrades.SHIP_WEAPONS_3,
                TerranUpgrades.SHIP_PLATING_1, TerranUpgrades.SHIP_PLATING_2, TerranUpgrades.SHIP_PLATING_3));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(Armory.ARMORY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(Armory.ARMORY_TYPE_NAME));

            /// Terran Engineering Bay
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(EngineeringBay.ENGINEERINGBAY_TYPE_NAME,
                TerranUpgrades.INFANTRY_WEAPONS_1,
                TerranUpgrades.INFANTRY_WEAPONS_2,
                TerranUpgrades.INFANTRY_WEAPONS_3,
                TerranUpgrades.INFANTRY_ARMOR_1,
                TerranUpgrades.INFANTRY_ARMOR_2,
                TerranUpgrades.INFANTRY_ARMOR_3));
            extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(EngineeringBay.ENGINEERINGBAY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(EngineeringBay.ENGINEERINGBAY_TYPE_NAME));

            /// Terran Science Facility
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Move, ScienceFacility.SCIENCEFACILITY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Stop, ScienceFacility.SCIENCEFACILITY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new BasicCmdExecutionFactory(BasicCommandEnum.Undefined, ScienceFacility.SCIENCEFACILITY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LiftOffExecutionFactory(ScienceFacility.SCIENCEFACILITY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new LandExecutionFactory(ScienceFacility.SCIENCEFACILITY_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(ScienceFacility.SCIENCEFACILITY_TYPE_NAME));

            /// Terran Missile Turret
            extendedComponent.RegisterCommandExecutionFactory(new DefenseCmdExecutionFactory(DefenseCommandEnum.Stop, MissileTurret.MISSILETURRET_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new DefenseCmdExecutionFactory(DefenseCommandEnum.Attack, MissileTurret.MISSILETURRET_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new DefenseCmdExecutionFactory(DefenseCommandEnum.Undefined, MissileTurret.MISSILETURRET_TYPE_NAME));
            extendedComponent.RegisterCommandExecutionFactory(new ConstructionCancelExecutionFactory(MissileTurret.MISSILETURRET_TYPE_NAME));

            /// TEST:
            //extendedComponent.RegisterCommandExecutionFactory(new ProductionExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME,
            //    TerranUpgrades.INFANTRY_WEAPONS_1,
            //    TerranUpgrades.INFANTRY_WEAPONS_2,
            //    TerranUpgrades.INFANTRY_WEAPONS_3,
            //    TerranUpgrades.INFANTRY_ARMOR_1,
            //    TerranUpgrades.INFANTRY_ARMOR_2,
            //    TerranUpgrades.INFANTRY_ARMOR_3));
            //extendedComponent.RegisterCommandExecutionFactory(new ProductionCancelExecutionFactory(ComsatStation.COMSATSTATION_TYPE_NAME));
        }

        /// <see cref="IPlugin<T>.Uninstall"/>
        public void Uninstall(ICommandExecutorPluginInstall extendedComponent)
        {
            /// TODO: Write uninstallation code here!
        }
    }
}
