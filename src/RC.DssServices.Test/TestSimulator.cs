using System;
using System.Collections.Generic;
using RC.RenderSystem;
using System.Drawing;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This class contains the data model of the test simulation and implements a ViewPort for drawing.
    /// </summary>
    class TestSimulator : ViewPort
    {
        /// <summary>
        /// Constructs a TestSimulator object.
        /// </summary>
        /// <param name="width">The width of the simulation field.</param>
        /// <param name="height">The height of the simulation field.</param>
        public TestSimulator(int width, int height, int opCount) : base(0, 0, width, height)
        {
            /// Create the Display and the ViewPort objects that we will use for drawing.
            this.theDisplay = Display.Create(width, height, 1, 1, CGAColor.Black);
            this.theDisplay.RegisterViewPort(this);

            /// Create the player bitmaps for each possible colors.
            Bitmap originalBmp = (Bitmap)Bitmap.FromFile("player.png");
            this.playerBitmaps = new Dictionary<PlayerColor, ScaledBitmap>();
            this.playerBitmaps.Add(PlayerColor.White, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.White));
            this.playerBitmaps[PlayerColor.White].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Red, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Red));
            this.playerBitmaps[PlayerColor.Red].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Blue, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Blue));
            this.playerBitmaps[PlayerColor.Blue].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Green, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Green));
            this.playerBitmaps[PlayerColor.Green].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Yellow, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Yellow));
            this.playerBitmaps[PlayerColor.Yellow].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Cyan, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Cyan));
            this.playerBitmaps[PlayerColor.Cyan].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Orange, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Brown));
            this.playerBitmaps[PlayerColor.Orange].MakeTransparent(CGAColor.LightMagenta);
            this.playerBitmaps.Add(PlayerColor.Magenta, ScaledBitmap.FromBitmap(originalBmp, CGAColor.LightCyan, CGAColor.Magenta));
            this.playerBitmaps[PlayerColor.Magenta].MakeTransparent(CGAColor.LightMagenta);
            originalBmp.Dispose();

            /// Generate disjunct random initial positions.
            List<Rectangle> initPosList = new List<Rectangle>();
            for (int i = 0; i < opCount; i++)
            {
                bool rdy = false;
                while (!rdy)
                {
                    int x = RandomService.DefaultGenerator.Next(width - Player.DIAMETER);
                    int y = RandomService.DefaultGenerator.Next(height - Player.DIAMETER);
                    Rectangle generatedRect = new Rectangle(x, y, Player.DIAMETER, Player.DIAMETER);
                    bool intersectionFound = false;
                    foreach (Rectangle r in initPosList)
                    {
                        if (!Rectangle.Intersect(r, generatedRect).IsEmpty)
                        {
                            /// The generated rectangle intersects another rectangle.
                            intersectionFound = true;
                            break;
                        }
                    }
                    if (!intersectionFound)
                    {
                        /// Intersection not found with the other rectangles --> put it into the list.
                        initPosList.Add(generatedRect);
                        rdy = true;
                    }
                }
            }

            /// Create the players.
            this.players = new Player[opCount];
            for (int i = 0; i < opCount; i++)
            {
                this.players[i] = new Player((PlayerColor)i, initPosList[i], this);
            }
        }

        /// <summary>
        /// Gets the bitmap of the player with the given color.
        /// </summary>
        /// <param name="color">The color of the player whose bitmap you want to get.</param>
        /// <returns>The bitmap of the player or null if there is no bitmap for that player.</returns>
        public ScaledBitmap GetPlayerBitmap(PlayerColor color)
        {
            if (this.playerBitmaps.ContainsKey(color))
            {
                return this.playerBitmaps[color];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Test function used to save the player bitmaps.
        /// </summary>
        public void SaveBitmaps()
        {
            this.playerBitmaps[PlayerColor.White].Save("white_player.png");
            this.playerBitmaps[PlayerColor.Red].Save("red_player.png");
            this.playerBitmaps[PlayerColor.Blue].Save("blue_player.png");
            this.playerBitmaps[PlayerColor.Green].Save("green_player.png");
            this.playerBitmaps[PlayerColor.Yellow].Save("yellow_player.png");
            this.playerBitmaps[PlayerColor.Cyan].Save("cyan_player.png");
            this.playerBitmaps[PlayerColor.Orange].Save("orange_player.png");
            this.playerBitmaps[PlayerColor.Magenta].Save("magenta_player.png");
        }

        #region ViewPort members

        /// <see cref="ViewPort.Draw"/>
        public override void Draw(IDrawTarget drawTarget, Rectangle drawRect)
        {
            drawTarget.Clear(CGAColor.Black);
            for (int i = 0; i < this.players.Length; i++)
            {
                this.players[i].Draw(drawTarget, drawRect);
            }
        }

        /// <see cref="ViewPort.GetDirtyRects"/>
        public override List<Rectangle> GetDirtyRects()
        {
            List<Rectangle> collectedRects = new List<Rectangle>();
            for (int i = 0; i < this.players.Length; i++)
            {
                this.players[i].GetDirtyRects(ref collectedRects);
            }
            return collectedRects;
        }

        #endregion

        /// <summary>
        /// Checks whether the given rectangle intersects the rectangle of any player except caller.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <param name="caller">The player who called this function.</param>
        /// <returns>True in case of collision, false otherwise.</returns>
        public bool CheckCollision(Rectangle rect, Player caller)
        {
            if (rect.X + rect.Width >= this.Width ||
                rect.Y + rect.Height >= this.Height ||
                rect.X < 0 || rect.Y < 0)
            {
                return true;
            }

            bool collision = false;
            for (int i = 0; i < this.players.Length; i++)
            {
                if (this.players[i] != caller && this.players[i].IsActive &&
                    !Rectangle.Intersect(this.players[i].Position, rect).IsEmpty)
                {
                    collision = true;
                    break;
                }
            }
            return collision;
        }

        /// <summary>
        /// Executes one simulation step on the players.
        /// </summary>
        public void MakeStep()
        {
            for (int i = 0; i < this.players.Length; i++)
            {
                this.players[i].MakeStep();
            }
        }

        /// <summary>
        /// Gets the player with the given index.
        /// </summary>
        public Player GetPlayer(int idx)
        {
            if (idx < 0 || idx >= this.players.Length) { throw new ArgumentOutOfRangeException("idx"); }

            return this.players[idx];
        }

        /// <summary>
        /// Gets the maximum number of players.
        /// </summary>
        public int MaxNumOfPlayers { get { return this.players.Length; } }

        /// <summary>
        /// List of the players in the test simulation.
        /// </summary>
        private Player[] players;

        /// <summary>
        /// The bitmaps that are used to draw the players.
        /// </summary>
        private Dictionary<PlayerColor, ScaledBitmap> playerBitmaps;

        /// <summary>
        /// The display that contains the viewport.
        /// </summary>
        private Display theDisplay;
    }
}
