using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// This provider gives suggestions for placing the corresponding building type next to addons to which it can connect to.
    /// </summary>
    public class CorrespondingAddonSuggestion : BuildingPlacementSuggestionProvider
    {
        /// <see cref="BuildingPlacementSuggestionProvider.GetSuggestionsImpl"/>
        protected override RCSet<Tuple<RCIntRectangle, RCIntVector>> GetSuggestionsImpl(Scenario scenario, RCIntRectangle area)
        {
            RCSet<Tuple<RCIntRectangle, RCIntVector>> retList = new RCSet<Tuple<RCIntRectangle, RCIntVector>>();
            RCSet<Addon> processedAddons = new RCSet<Addon>();
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    RCIntVector quadCoords = new RCIntVector(x, y);
                    Addon addon = scenario.GetFixedEntity<Addon>(quadCoords);
                    if (addon == null || processedAddons.Contains(addon) ||
                        addon.CurrentMainBuilding != null || !this.BuildingType.HasAddonType(addon.AddonType.Name)) { continue; }

                    retList.Add(Tuple.Create(
                        addon.MapObject.QuadraticPosition,
                        (-1) * this.BuildingType.GetRelativeAddonPosition(scenario.Map, addon.AddonType)));
                    processedAddons.Add(addon);
                }
            }
            return retList;
        }
    }
}
