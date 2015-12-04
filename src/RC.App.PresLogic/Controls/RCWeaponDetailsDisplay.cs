using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Displays detailed informations about the weapons and armor of the selected map object on the details panel.
    /// </summary>
    public class RCWeaponDetailsDisplay : RCTextInformationDisplay
    {
        /// <summary>
        /// Constructs a RCWeaponDetailsDisplay control at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        public RCWeaponDetailsDisplay(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.mapObjectDetailsView = viewService.CreateView<IMapObjectDetailsView>();
            this.selectionDetailsView = viewService.CreateView<ISelectionDetailsView>();
            this.metadataView = viewService.CreateView<IMetadataView>();

            UIFont textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.displayedTexts = new List<UIString>();

            /// Load the UIStrings for displaying the weapon names.
            this.weaponTexts = new Dictionary<string, UIString>();
            foreach (KeyValuePair<string, string> weaponName in this.metadataView.GetWeaponDisplayedNames())
            {
                this.weaponTexts.Add(weaponName.Key,
                    new UIString(string.Format("{0}: {{0}}", weaponName.Value), textFont, UIWorkspace.Instance.PixelScaling, RCColor.White));
            }
            this.armorText = new UIString("Armor: {0}", textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
        }

        /// <see cref="RCTextInformationDisplay.GetDisplayedTexts"/>
        protected override List<UIString> GetDisplayedTexts()
        {
            /// Check if there is 1 object is selected.
            if (this.selectionDetailsView.SelectionCount != 1) { return new List<UIString>(); }
            int mapObjectID = this.selectionDetailsView.GetObjectID(0);

            this.displayedTexts.Clear();

            /// Fill the list of displayed text with the armor informations of the selected object.
            Tuple<int, int> armorInfo = this.mapObjectDetailsView.GetArmorInfo(mapObjectID);
            if (armorInfo != null)
            {
                int originalArmor = armorInfo.Item1;
                int armorUpgrade = armorInfo.Item2;

                string armorInfoStr = armorUpgrade != 0
                    ? string.Format("{0}+{1}", originalArmor, armorUpgrade)
                    : string.Format("{0}", originalArmor);
                this.armorText[0] = armorInfoStr;
                this.displayedTexts.Add(this.armorText);
            }

            /// Fill the list of displayed text with the weapon informations of the selected object.
            List<Tuple<string, int, int>> weaponInfoList = this.mapObjectDetailsView.GetWeaponInfo(mapObjectID);
            foreach (Tuple<string, int, int> weaponInfo in weaponInfoList)
            {
                string weaponName = weaponInfo.Item1;
                int originalDamage = weaponInfo.Item2;
                int damageUpgrade = weaponInfo.Item3;

                string weaponInfoStr = damageUpgrade != 0
                    ? string.Format("{0}+{1}", originalDamage, damageUpgrade)
                    : string.Format("{0}", originalDamage);

                UIString weaponText = this.weaponTexts[weaponName];
                weaponText[0] = weaponInfoStr;
                this.displayedTexts.Add(weaponText);
            }

            return this.displayedTexts;
        }

        /// <summary>
        /// The texts to be displayed.
        /// </summary>
        private readonly List<UIString> displayedTexts;
        private readonly UIString armorText;
        private readonly Dictionary<string, UIString> weaponTexts;

        /// <summary>
        /// Reference to the necessary views.
        /// </summary>
        private readonly IMapObjectDetailsView mapObjectDetailsView;
        private readonly ISelectionDetailsView selectionDetailsView;
        private readonly IMetadataView metadataView;
    }
}
