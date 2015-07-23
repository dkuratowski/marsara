using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Calculates a MapDirection out of a heading vector.
    /// </summary>
    class HeadingToMapDirConverter : IValueRead<MapDirection>
    {
        /// <summary>
        /// Constructs a HeadingToMapDirConverter instance.
        /// </summary>
        /// <param name="headingVector">The heading vector.</param>
        public HeadingToMapDirConverter(IValueRead<RCNumVector> headingVector)
        {
            if (headingVector == null) { throw new ArgumentNullException("headingVector"); }

            this.headingVector = headingVector;
            this.lastKnownHeadingVector = RCNumVector.Undefined;
            this.cachedMapDirection = MapDirection.Undefined;
        }

        #region IValueRead<MapDirection> methods

        /// <see cref="IValueRead&lt;MapDirection&gt;.Read"/>
        public MapDirection Read()
        {
            RCNumVector currentHeadingVector = this.headingVector.Read();
            if (currentHeadingVector == RCNumVector.Undefined) { throw new InvalidOperationException("Heading vector cannot be RCNumVector.Undefined!"); }

            if (currentHeadingVector != this.lastKnownHeadingVector)
            {
                this.CalculateMapDirection(currentHeadingVector);
                this.lastKnownHeadingVector = currentHeadingVector;
            }

            return this.cachedMapDirection;
        }

        /// <see cref="IValueRead&lt;MapDirection&gt;.ValueChanged"/>
        public event EventHandler ValueChanged
        {
            add { throw new NotSupportedException("ValueChanged event is not supported in HeadingToMapDirectionConverter!"); }
            remove { throw new NotSupportedException("ValueChanged event is not supported in HeadingToMapDirectionConverter!"); }
        }

        #endregion IValueRead<MapDirection> methods

        /// <summary>
        /// Calculates the map direction out of the given heading vector.
        /// </summary>
        /// <param name="headingVector">The heading vector.</param>
        private void CalculateMapDirection(RCNumVector headingVector)
        {
            // New heading -> recalculate the map direction.
            if (headingVector.X == 0 && headingVector.Y == 0) { this.cachedMapDirection = MapDirection.Undefined; } // In case of 0 heading vector -> map direction is undefined.
            else if (headingVector.X == 0) { this.cachedMapDirection = headingVector.Y > 0 ? MapDirection.South : MapDirection.North; } // In case of vertical heading vector -> map direction is South or North based on the Y-coordinate.
            else
            {
                // In any other cases -> map direction is calculated from the tangent of the heading vector.
                RCNumber tangentOfVelocity = headingVector.Y / headingVector.X;
                if (SE_NW_TANGENT_LOWERBOUND < tangentOfVelocity && tangentOfVelocity <= SE_NW_TANGENT_UPPERBOUND)
                {
                    // SouthEast or NorthWest
                    this.cachedMapDirection = headingVector.X > 0 && headingVector.Y > 0
                                            ? MapDirection.SouthEast
                                            : MapDirection.NorthWest;
                }
                else if (SW_NE_TANGENT_LOWERBOUND < tangentOfVelocity && tangentOfVelocity <= SW_NE_TANGENT_UPPERBOUND)
                {
                    // SouthWest or NorthEast
                    this.cachedMapDirection = headingVector.X < 0 && headingVector.Y > 0
                                            ? MapDirection.SouthWest
                                            : MapDirection.NorthEast;
                }
                else if (W_E_TANGENT_LOWERBOUND < tangentOfVelocity && tangentOfVelocity <= W_E_TANGENT_UPPERBOUND)
                {
                    // West or East
                    this.cachedMapDirection = headingVector.X < 0 ? MapDirection.West : MapDirection.East;
                }
                else
                {
                    // North or South
                    this.cachedMapDirection = headingVector.Y < 0 ? MapDirection.North : MapDirection.South;
                }
            }
        }

        /// <summary>
        /// The heading vector.
        /// </summary>
        private readonly IValueRead<RCNumVector> headingVector;

        /// <summary>
        /// The last known heading vector.
        /// </summary>
        private RCNumVector lastKnownHeadingVector;

        /// <summary>
        /// The cached value of the calculated map direction.
        /// </summary>
        private MapDirection cachedMapDirection;

        /// <summary>
        /// Lower and upper bounds of tangent for heading vectors in direction SouthEast and NorthWest.
        /// </summary>
        private static readonly RCNumber SE_NW_TANGENT_LOWERBOUND = (RCNumber)414 / (RCNumber)1000; // tan(pi/8)
        private static readonly RCNumber SE_NW_TANGENT_UPPERBOUND = (RCNumber)2414 / (RCNumber)1000; // tan(3*pi/8)

        /// <summary>
        /// Lower and upper bounds of tangent for heading vectors in direction SouthWest and NorthEast.
        /// </summary>
        private static readonly RCNumber SW_NE_TANGENT_LOWERBOUND = -(RCNumber)2414 / (RCNumber)1000; // tan(5*pi/8)
        private static readonly RCNumber SW_NE_TANGENT_UPPERBOUND = -(RCNumber)414 / (RCNumber)1000; // tan(7*pi/8)

        /// <summary>
        /// Lower and upper bounds of tangent for heading vectors in direction West and East.
        /// </summary>
        private static readonly RCNumber W_E_TANGENT_LOWERBOUND = SW_NE_TANGENT_UPPERBOUND; // tan(7*pi/8)
        private static readonly RCNumber W_E_TANGENT_UPPERBOUND = SE_NW_TANGENT_LOWERBOUND; // tan(pi/8)

        /// <summary>
        /// Lower and upper bounds of tangent for heading vectors in direction South and North.
        /// </summary>
        private static readonly RCNumber S_N_TANGENT_LOWERBOUND = SE_NW_TANGENT_UPPERBOUND; // tan(3*pi/8)
        private static readonly RCNumber S_N_TANGENT_UPPERBOUND = SW_NE_TANGENT_LOWERBOUND; // tan(5*pi/8)
    }

    /// <summary>
    /// This map direction source wrapper directly return the value of the wrapped value source if it is not MapDirection.Undefined
    /// and generates a random map direction if it is MapDirection.Undefined.
    /// </summary>
    class MapDirValueSrcWrapper : IValueRead<MapDirection>
    {
        /// <summary>
        /// Constructs a MapDirValueSrcWrapper instance.
        /// </summary>
        /// <param name="directionValueSrc">The wrapped direction value source.</param>
        public MapDirValueSrcWrapper(IValueRead<MapDirection> directionValueSrc)
        {
            if (directionValueSrc == null) { throw new ArgumentNullException("directionValueSrc"); }
            this.wrappedDirectionValueSrc = directionValueSrc;
            this.lastKnownWrappedMapDirection = MapDirection.Undefined;
            this.cachedMapDirection = (MapDirection)RandomService.DefaultGenerator.Next(8);
        }

        #region IValueRead<MapDirection> methods

        /// <see cref="IValueRead&lt;MapDirection&gt;.Read"/>
        public MapDirection Read()
        {
            MapDirection currentWrappedMapDirection = this.wrappedDirectionValueSrc.Read();

            if (currentWrappedMapDirection != this.lastKnownWrappedMapDirection)
            {
                // New wrapped map direction -> recalculate the map direction.
                this.cachedMapDirection = currentWrappedMapDirection != MapDirection.Undefined
                                        ? currentWrappedMapDirection
                                        : (MapDirection) RandomService.DefaultGenerator.Next(8);
                this.lastKnownWrappedMapDirection = currentWrappedMapDirection;
            }

            return this.cachedMapDirection;
        }

        /// <see cref="IValueRead&lt;MapDirection&gt;.ValueChanged"/>
        public event EventHandler ValueChanged
        {
            add { throw new NotSupportedException("ValueChanged event is not supported in MapDirValueSrcWrapper!"); }
            remove { throw new NotSupportedException("ValueChanged event is not supported in MapDirValueSrcWrapper!"); }
        }

        #endregion IValueRead<MapDirection> methods

        /// <summary>
        /// The wrapped direction value source.
        /// </summary>
        private readonly IValueRead<MapDirection> wrappedDirectionValueSrc;

        /// <summary>
        /// The last known value of the wrapped map direction.
        /// </summary>
        private MapDirection lastKnownWrappedMapDirection;

        /// <summary>
        /// The cached value of the calculated map direction.
        /// </summary>
        private MapDirection cachedMapDirection;
    }
}
