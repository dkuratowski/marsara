using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.PlacementConstraints
{
    /// <summary>
    /// This provider gives suggestions for placing the corresponding building type onto VespeneGeysers.
    /// </summary>
    public class VespeneGeyserSuggestion : BuildingPlacementSuggestionProvider
    {
        /// <see cref="BuildingPlacementSuggestionProvider.GetSuggestionsImpl"/>
        protected override RCSet<Tuple<RCIntRectangle, RCIntVector>> GetSuggestionsImpl(Scenario scenario, RCIntRectangle area)
        {
            RCSet<Tuple<RCIntRectangle, RCIntVector>> retList = new RCSet<Tuple<RCIntRectangle, RCIntVector>>();
            RCSet<VespeneGeyser> processedVespeneGeysers = new RCSet<VespeneGeyser>();
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    RCIntVector quadCoords = new RCIntVector(x, y);
                    VespeneGeyser vespeneGeyser = scenario.GetFixedEntity<VespeneGeyser>(quadCoords);
                    if (vespeneGeyser == null || processedVespeneGeysers.Contains(vespeneGeyser)) { continue; }

                    retList.Add(Tuple.Create(vespeneGeyser.MapObject.QuadraticPosition, new RCIntVector(0, 0)));
                    processedVespeneGeysers.Add(vespeneGeyser);
                }
            }
            return retList;
        }
    }
}
