using System;
using System.Collections.Generic;
using System.Drawing;

namespace RC.RenderSystem.Test
{
    class TestViewPort : ViewPort
    {
        public enum Directions
        {
            Left = 0,
            Right = 1
        }

        public TestViewPort(int x, int y, int width, int height, Color background,
                            ScaledBitmap testSprite, int spriteCount, Random rnd)
            : base(x, y, width, height)
        {
            this.backgroundColor = background;
            this.rnd = rnd;
            this.testSprite = testSprite;
            this.testSprite.MakeTransparent(CGAColor.WhiteHigh);
            this.spritePositions = new Rectangle[spriteCount];
            this.spriteDirections = new Directions[spriteCount];
            this.spriteOldPositions = new Rectangle[spriteCount];
            for (int i = 0; i < this.spritePositions.Length; i++)
            {
                Rectangle spriteRect = new Rectangle(this.rnd.Next(this.Width - this.testSprite.Width),
                                                     this.rnd.Next(this.Height - this.testSprite.Height),
                                                     this.testSprite.Width, this.testSprite.Height);
                this.spritePositions[i] = spriteRect;
                this.spriteOldPositions[i] = spriteRect;
                this.spriteDirections[i] = (Directions)this.rnd.Next(2);
            }
        }

        public override List<Rectangle> GetDirtyRects()
        {
            List<Rectangle> dirties = new List<Rectangle>();
            for (int i = 0; i < this.spritePositions.Length; i++)
            {
                dirties.Add(Rectangle.Union(this.spriteOldPositions[i], this.spritePositions[i]));
            }
            return dirties;
        }

        public override void Draw(IDrawTarget drawTarget, Rectangle drawRect)
        {
            drawTarget.Clear(this.backgroundColor);
            for (int i = 0; i < this.spritePositions.Length; i++)
            {
                drawTarget.DrawBitmap(this.testSprite, this.spritePositions[i].X, this.spritePositions[i].Y);
            }
        }

        public void Step()
        {
            for (int i = 0; i < this.spritePositions.Length; i++)
            {
                this.spriteOldPositions[i] = this.spritePositions[i];
                if (this.spriteDirections[i] == Directions.Left)
                {
                    this.spritePositions[i].X -= 1;
                    if (this.spritePositions[i].X < 0)
                    {
                        this.spritePositions[i].X = 0;
                        this.spriteDirections[i] = Directions.Right;
                    }
                }
                else if (this.spriteDirections[i] == Directions.Right)
                {
                    this.spritePositions[i].X += 1;
                    if (this.spritePositions[i].X > this.Width - this.testSprite.Width - 1)
                    {
                        this.spritePositions[i].X = this.Width - this.testSprite.Width - 1;
                        this.spriteDirections[i] = Directions.Left;
                    }
                }
            }
        }

        private Color backgroundColor;

        private ScaledBitmap testSprite;

        private Rectangle[] spritePositions;

        private Directions[] spriteDirections;

        private Rectangle[] spriteOldPositions;

        private Random rnd;
    }
}
