using System;
using System.Collections.Generic;
using System.Drawing;
using RC.RenderSystem;
using RC.Common.Configuration;

namespace RC.DssServices.Test
{
    /// <summary>
    /// Enumerates the possible colors of the players.
    /// </summary>
    public enum PlayerColor
    {
        White = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Cyan = 5,
        Orange = 6,
        Magenta = 7
    }

    /// <summary>
    /// Enumerates the directions that the players can move.
    /// </summary>
    public enum PlayerDirection
    {
        NoMove = 0,     /// The player is not moving
        Up = 1,         /// The player is moving upwards
        Down = 2,       /// The player is moving downwards
        Left = 3,       /// The player is moving leftwards
        Right = 4       /// The player is moving rightwards
    }

    /// <summary>
    /// This class represents a player in the test simulator.
    /// </summary>
    class Player
    {
        /// <summary>
        /// Constructs a Player object.
        /// </summary>
        public Player(PlayerColor initialColor, Rectangle initialPos, TestSimulator simulator)
        {
            this.initialColor = initialColor;
            this.initialPosition = initialPos;

            this.currentColor = initialColor;
            this.currentPosition = initialPos;
            this.currentDirection = PlayerDirection.NoMove;

            this.previousPosition = initialPos;

            this.simulator = simulator;
            this.isActive = false;
            this.isDirty = false;
        }

        /// <summary>
        /// Gets whether it is an activated or deactivated player.
        /// </summary>
        public bool IsActive
        {
            get { return this.isActive; }
        }

        /// <summary>
        /// The diameter of a player.
        /// </summary>
        public static readonly int DIAMETER = ConstantsTable.Get<int>("RC.DssServices.Test.PlayerDiameter");

        /// <summary>
        /// The speed of a player (px/frm).
        /// </summary>
        public static readonly int SPEED = ConstantsTable.Get<int>("RC.DssServices.Test.PlayerSpeed");

        /// <summary>
        /// Activates this player with the generated position and color.
        /// </summary>
        public void Activate()
        {
            if (this.isActive) { throw new Exception("Player already activated!"); }

            this.currentDirection = PlayerDirection.NoMove;
            this.isActive = true;
            this.isDirty = true;
        }

        /// <summary>
        /// Deactivates this player.
        /// </summary>
        public void Deactivate()
        {
            if (!this.isActive) { throw new Exception("Player already deactivated!"); }

            this.currentDirection = PlayerDirection.NoMove;
            this.isActive = false;
            this.isDirty = true;
        }

        /// <summary>
        /// Resets the datas of the player to the generated initial values.
        /// </summary>
        public void Reset()
        {
            if (this.isActive) { throw new Exception("Cannot reset player's position while it is active!"); }

            this.previousPosition = this.currentPosition;
            this.currentPosition = this.initialPosition;
            this.currentColor = this.initialColor;
        }

        /// <summary>
        /// Gets the rectangles on the Display that have to be redrawn.
        /// </summary>
        /// <param name="dirtyRects">The rectangles that has to be redrawn.</param>
        public void GetDirtyRects(ref List<Rectangle> dirtyRects)
        {
            if (this.isDirty)
            {
                if (this.currentPosition == this.previousPosition)
                {
                    dirtyRects.Add(this.currentPosition);
                }
                else
                {
                    if (!Rectangle.Intersect(this.currentPosition, this.previousPosition).IsEmpty)
                    {
                        dirtyRects.Add(Rectangle.Union(this.currentPosition, this.previousPosition));
                    }
                    else
                    {
                        dirtyRects.Add(this.currentPosition);
                        dirtyRects.Add(this.previousPosition);
                    }
                }
            }
        }

        /// <summary>
        /// Draws this player to it's current position.
        /// </summary>
        /// <param name="drawTarget">The draw target in which to draw.</param>
        /// <param name="drawRect">The bounding rectangle in the draw target.</param>
        public void Draw(IDrawTarget drawTarget, Rectangle drawRect)
        {
            if (this.isActive)
            {
                /// Draw only if this is an active player.
                drawTarget.DrawBitmap(this.simulator.GetPlayerBitmap(this.currentColor),
                                      this.currentPosition.X,
                                      this.currentPosition.Y);
            }
            this.previousPosition = this.currentPosition;
            this.isDirty = false;
        }

        /// <summary>
        /// Moves this player if this is possible.
        /// </summary>
        public void MakeStep()
        {
            if (this.isActive)
            {
                bool movement = false;
                Rectangle newPos = new Rectangle();
                if (this.currentDirection == PlayerDirection.Up)
                {
                    newPos = new Rectangle(this.currentPosition.X, this.currentPosition.Y - SPEED, DIAMETER, DIAMETER);
                    movement = true;
                }
                else if (this.currentDirection == PlayerDirection.Down)
                {
                    newPos = new Rectangle(this.currentPosition.X, this.currentPosition.Y + SPEED, DIAMETER, DIAMETER);
                    movement = true;
                }
                else if (this.currentDirection == PlayerDirection.Left)
                {
                    newPos = new Rectangle(this.currentPosition.X - SPEED, this.currentPosition.Y, DIAMETER, DIAMETER);
                    movement = true;
                }
                else if (this.currentDirection == PlayerDirection.Right)
                {
                    newPos = new Rectangle(this.currentPosition.X + SPEED, this.currentPosition.Y, DIAMETER, DIAMETER);
                    movement = true;
                }

                if (movement)
                {
                    if (!this.simulator.CheckCollision(newPos, this))
                    {
                        this.currentPosition = newPos;
                        this.isDirty = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the direction of this player.
        /// </summary>
        public PlayerDirection Direction
        {
            set
            {
                if (this.isActive)
                {
                    if (value == PlayerDirection.NoMove)
                    {
                        this.currentDirection = value;
                    }
                    else if (value == PlayerDirection.Up)
                    {
                        if (this.currentDirection == PlayerDirection.Down)
                        {
                            this.currentDirection = PlayerDirection.NoMove;
                        }
                        else
                        {
                            this.currentDirection = value;
                        }
                    }
                    else if (value == PlayerDirection.Down)
                    {
                        if (this.currentDirection == PlayerDirection.Up)
                        {
                            this.currentDirection = PlayerDirection.NoMove;
                        }
                        else
                        {
                            this.currentDirection = value;
                        }
                    }
                    else if (value == PlayerDirection.Left)
                    {
                        if (this.currentDirection == PlayerDirection.Right)
                        {
                            this.currentDirection = PlayerDirection.NoMove;
                        }
                        else
                        {
                            this.currentDirection = value;
                        }
                    }
                    else if (value == PlayerDirection.Right)
                    {
                        if (this.currentDirection == PlayerDirection.Left)
                        {
                            this.currentDirection = PlayerDirection.NoMove;
                        }
                        else
                        {
                            this.currentDirection = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of this player.
        /// </summary>
        public PlayerColor Color
        {
            get { return this.currentColor; }

            set
            {
                if (this.currentColor != value)
                {
                    this.currentColor = value;
                    if (this.isActive) { this.isDirty = true; }
                }
            }
        }

        /// <summary>
        /// Gets or sets the current position of this player.
        /// </summary>
        public Rectangle Position
        {
            get { return this.currentPosition; }

            set
            {
                if (this.currentPosition != value)
                {
                    this.previousPosition = this.currentPosition;
                    this.currentPosition = value;
                    if (this.isActive) { this.isDirty = true; }
                }
            }
        }

        /// <summary>
        /// Reference to the simulator.
        /// </summary>
        private TestSimulator simulator;

        /// <summary>
        /// The initial color of this Player.
        /// </summary>
        private readonly PlayerColor initialColor;

        /// <summary>
        /// The initial position of this player.
        /// </summary>
        private readonly Rectangle initialPosition;

        /// <summary>
        /// The current color of this player.
        /// </summary>
        private PlayerColor currentColor;

        /// <summary>
        /// The current position of this player.
        /// </summary>
        private Rectangle currentPosition;

        /// <summary>
        /// The current direction where the player is moving.
        /// </summary>
        private PlayerDirection currentDirection;

        /// <summary>
        /// The previous position of this player.
        /// </summary>
        private Rectangle previousPosition;

        /// <summary>
        /// True if this is an active player, false otherwise.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// This flag indicates whether the player should be redrawn or not.
        /// </summary>
        private bool isDirty;
    }
}
