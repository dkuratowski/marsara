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
    /// The abstract base class of building placement suggestion providers.
    /// </summary>
    public abstract class BuildingPlacementSuggestionProvider
    {
        /// <summary>
        /// Sets the building type that this provider belongs to.
        /// </summary>
        /// <param name="buildingType">The building type that this provider belongs to.</param>
        /// <exception cref="SimulatorException">If a corresponding building type has already been set for this provider.</exception>
        public void SetBuildingType(IBuildingType buildingType)
        {
            if (buildingType == null) { throw new ArgumentNullException("buildingType"); }
            if (this.buildingType != null) { throw new SimulatorException("Building type has already been set for this provider!"); }
            this.buildingType = buildingType;
        }

        /// <summary>
        /// Gets the suggestions provided by this suggestion provider for the corresponding building type inside the given area on the map of
        /// the given scenario.
        /// </summary>
        /// <param name="scenario">The given scenario.</param>
        /// <param name="area">The area on the map of the given scenario in quadratic coordinates.</param>
        /// <returns>
        /// A list that contains pairs of an RCIntRectangle and an RCIntVector. Each of these pair gives informations about
        /// a suggestion box to the caller. The RCIntRectangle component represents the area whose visibility needs to be
        /// checked by the caller. If that area is visible then the RCIntVector component contains the coordinates of the
        /// top-left corner of the suggestion box relative to the RCIntRectangle component.
        /// </returns>
        public RCSet<Tuple<RCIntRectangle, RCIntVector>> GetSuggestions(Scenario scenario, RCIntRectangle area)
        {
            if (this.buildingType == null) { throw new SimulatorException("Building type has not yet been set for this provider!"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }

            return this.GetSuggestionsImpl(scenario, area);
        }

        /// <summary>
        /// Constructs an BuildingPlacementSuggestionProvider instance.
        /// </summary>
        protected BuildingPlacementSuggestionProvider()
        {
            this.buildingType = null;
        }

        /// <see cref="BuildingPlacementSuggestionProvider.GetSuggestions"/>
        protected abstract RCSet<Tuple<RCIntRectangle, RCIntVector>> GetSuggestionsImpl(Scenario scenario, RCIntRectangle area);

        /// <summary>
        /// Gets the building type that this suggestion provider belongs to.
        /// </summary>
        protected IBuildingType BuildingType { get { return this.buildingType; } }

        /// <summary>
        /// Reference to the building type that this suggestion provider belongs to.
        /// </summary>
        private IBuildingType buildingType;
    }
}
