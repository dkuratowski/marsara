using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// View for the details panel.
    /// </summary>
    class DetailsView : IDetailsView
    {
        /// <summary>
        /// Constructs a DetailsView instance.
        /// </summary>
        public DetailsView()
        {
            this.selectionManager = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
        }

        #region IDetailsView methods

        /// <see cref="IDetailsView.SelectionCount"/>
        public int SelectionCount { get { return this.selectionManager.CurrentSelection.Count; } }

        /// <see cref="IDetailsView.GetHPCondition"/>
        public MapObjectConditionEnum GetHPCondition(int selectionOrdinal)
        {
            int[] currentSelectionArray = this.selectionManager.CurrentSelection.ToArray();
            int entityID = currentSelectionArray[selectionOrdinal];
            Entity entity = this.scenarioManager.ActiveScenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' doesn't exist!", entityID)); }
            if (entity.Biometrics.HP == -1) { return MapObjectConditionEnum.Undefined; }
            
            RCNumber hpNorm = entity.Biometrics.HP / entity.ElementType.MaxHP.Read();
            if (hpNorm <= (RCNumber)1 / (RCNumber)3) { return MapObjectConditionEnum.Critical; }
            else if (hpNorm <= (RCNumber)2 / (RCNumber)3) { return MapObjectConditionEnum.Moderate; }
            else { return MapObjectConditionEnum.Excellent; }
        }

        /// <see cref="IDetailsView.GetHPIcon"/>
        public SpriteInst GetHPIcon(int selectionOrdinal)
        {
            int[] currentSelectionArray = this.selectionManager.CurrentSelection.ToArray();
            int entityID = currentSelectionArray[selectionOrdinal];
            Entity entity = this.scenarioManager.ActiveScenario.GetElementOnMap<Entity>(entityID);
            if (entity == null) { throw new InvalidOperationException(string.Format("Entity with ID '{0}' doesn't exist!", entityID)); }

            if (entity.ElementType.HPIconPalette == null) { throw new InvalidOperationException(string.Format("ElementType '{0}' has no HPIconPalette defined!", entity.ElementType.Name)); }
            int smallIconSpriteIdx = entity.ElementType.HPIconPalette.GetSpriteIndex(SMALL_ICON_SPRITE_NAME); // TODO: cache this index!
            return new SpriteInst()
            {
                Index = entity.ElementType.HPIconPalette.Index,
                DisplayCoords = new RCIntVector(0, 0),
                Section = entity.ElementType.HPIconPalette.GetSection(smallIconSpriteIdx)
            };
        }

        #endregion IDetailsView methods

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private readonly ISelectionManagerBC selectionManager;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private readonly IScenarioManagerBC scenarioManager;

        /// <summary>
        /// The name of the small icon in the HPIconPalette.
        /// </summary>
        private const string SMALL_ICON_SPRITE_NAME = "SmallIcon";
    }
}
