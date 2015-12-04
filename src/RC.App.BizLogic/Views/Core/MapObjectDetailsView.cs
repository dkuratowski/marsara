using System;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;
using System.Collections.Generic;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views for providing detailed informations about map objects.
    /// </summary>
    class MapObjectDetailsView : MapViewBase, IMapObjectDetailsView
    {
        /// <summary>
        /// Constructs a map object details view.
        /// </summary>
        public MapObjectDetailsView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
        }

        #region IMapObjectDetailsView members

        /// <see cref="IMapObjectDetailsView.GetObjectTypeID"/>
        public int GetObjectTypeID(int objectID)
        {
            Entity entity = this.GetEntity(objectID);
            return entity.ElementType.ID;
        }

        /// <see cref="IMapObjectDetailsView.GetVespeneGasAmount"/>
        public int GetVespeneGasAmount(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            VespeneGeyser entityAsVespeneGeyser = this.Scenario.GetElement<VespeneGeyser>(objectID);
            return entityAsVespeneGeyser != null ? entityAsVespeneGeyser.ResourceAmount.Read() : -1;
        }

        /// <see cref="IMapObjectDetailsView.GetMineralsAmount"/>
        public int GetMineralsAmount(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            MineralField entityAsMineralField = this.Scenario.GetElement<MineralField>(objectID);
            return entityAsMineralField != null ? entityAsMineralField.ResourceAmount.Read() : -1;
        }

        /// <see cref="IMapObjectDetailsView.GetHPCondition"/>
        public MapObjectConditionEnum GetHPCondition(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Biometrics.HP == -1) { return MapObjectConditionEnum.Undefined; }

            RCNumber hpNorm = entity.Biometrics.HP / entity.ElementType.MaxHP.Read();
            if (hpNorm <= (RCNumber)1 / (RCNumber)3) { return MapObjectConditionEnum.Critical; }
            else if (hpNorm <= (RCNumber)2 / (RCNumber)3) { return MapObjectConditionEnum.Moderate; }
            else { return MapObjectConditionEnum.Excellent; }
        }

        /// <see cref="IMapObjectDetailsView.GetBigHPIcon"/>
        public SpriteRenderInfo GetBigHPIcon(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.ElementType.HPIconPalette == null) { throw new InvalidOperationException(String.Format("ElementType '{0}' has no HPIconPalette defined!", entity.ElementType.Name)); }
            int bigIconSpriteIdx = entity.ElementType.HPIconPalette.GetSpriteIndex(BIG_ICON_SPRITE_NAME); // TODO: cache this index!
            return new SpriteRenderInfo()
            {
                SpriteGroup = SpriteGroupEnum.HPIconSpriteGroup,
                Index = entity.ElementType.HPIconPalette.Index,
                DisplayCoords = new RCIntVector(0, 0),
                Section = entity.ElementType.HPIconPalette.GetSection(bigIconSpriteIdx)
            };
        }

        /// <see cref="IMapObjectDetailsView.GetSmallHPIcon"/>
        public SpriteRenderInfo GetSmallHPIcon(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.ElementType.HPIconPalette == null) { throw new InvalidOperationException(String.Format("ElementType '{0}' has no HPIconPalette defined!", entity.ElementType.Name)); }
            int smallIconSpriteIdx = entity.ElementType.HPIconPalette.GetSpriteIndex(SMALL_ICON_SPRITE_NAME); // TODO: cache this index!
            return new SpriteRenderInfo()
            {
                SpriteGroup = SpriteGroupEnum.HPIconSpriteGroup,
                Index = entity.ElementType.HPIconPalette.Index,
                DisplayCoords = new RCIntVector(0, 0),
                Section = entity.ElementType.HPIconPalette.GetSection(smallIconSpriteIdx)
            };
        }

        /// <see cref="IMapObjectDetailsView.GetCurrentHP"/>
        public int GetCurrentHP(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            return (int)entity.Biometrics.HP;
        }

        /// <see cref="IMapObjectDetailsView.GetMaxHP"/>
        public int GetMaxHP(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            return entity.ElementType.MaxHP != null ? entity.ElementType.MaxHP.Read() : -1;
        }

        /// <see cref="IMapObjectDetailsView.GetCurrentEnergy"/>
        public int GetCurrentEnergy(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Owner != null && entity.Owner.PlayerIndex == (int) this.selectionManager.LocalPlayer)
            {
                return (int)entity.Biometrics.Energy;
            }
            else
            {
                return -1;
            }
        }

        /// <see cref="IMapObjectDetailsView.GetMaxEnergy"/>
        public int GetMaxEnergy(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Owner != null && entity.Owner.PlayerIndex == (int) this.selectionManager.LocalPlayer)
            {
                return entity.ElementType.MaxEnergy != null ? entity.ElementType.MaxEnergy.Read() : -1;
            }
            else
            {
                return -1;
            }
        }

        /// <see cref="IMapObjectDetailsView.GetSuppliesProvided"/>
        public int GetSuppliesProvided(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Owner != null && entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer)
            {
                return entity.ElementType.SupplyProvided != null ? entity.ElementType.SupplyProvided.Read() : -1;
            }
            else
            {
                return -1;
            }
        }

        /// <see cref="IMapObjectDetailsView.GetArmorInfo"/>
        public Tuple<int, int> GetArmorInfo(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Owner != null && entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer && entity.ElementType.Armor != null)
            {
                int currentArmor = entity.ElementType.Armor.Read();
                int armorUpgrade = entity.ElementTypeUpgrade.CumulatedArmorUpgrade;
                return Tuple.Create(currentArmor - armorUpgrade, armorUpgrade);
            }
            else
            {
                return null;
            }
        }

        /// <see cref="IMapObjectDetailsView.GetWeaponInfo"/>
        public List<Tuple<string, int, int>> GetWeaponInfo(int objectID)
        {
            if (objectID < 0) { throw new ArgumentOutOfRangeException("objectID"); }

            Entity entity = this.GetEntity(objectID);
            if (entity.Owner != null && entity.Owner.PlayerIndex == (int)this.selectionManager.LocalPlayer)
            {
                List<Tuple<string, int, int>> retList = new List<Tuple<string, int, int>>();
                List<IWeaponData> weaponDataList = new List<IWeaponData>(entity.ElementType.StandardWeapons);
                List<IWeaponDataUpgrade> weaponUpgradeList = new List<IWeaponDataUpgrade>(entity.ElementTypeUpgrade.WeaponUpgrades);
                for (int i = 0; i < weaponDataList.Count; i++)
                {
                    int currentDamage = weaponDataList[i].Damage.Read();
                    int damageUpgrade = weaponUpgradeList[i].CumulatedDamageUpgrade;
                    retList.Add(Tuple.Create(weaponDataList[i].Name, currentDamage - damageUpgrade, damageUpgrade));
                }
                return retList;
            }
            else
            {
                return new List<Tuple<string,int,int>>();
            }
        }

        #endregion IMapObjectDetailsView members

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private readonly ISelectionManagerBC selectionManager;

        /// <summary>
        /// The name of the big icon in the HPIconPalette.
        /// </summary>
        private const string BIG_ICON_SPRITE_NAME = "BigIcon";

        /// <summary>
        /// The name of the small icon in the HPIconPalette.
        /// </summary>
        private const string SMALL_ICON_SPRITE_NAME = "SmallIcon";
    }
}
