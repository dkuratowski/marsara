using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using RC.Common;

namespace RC.RenderSystem.Test
{
    public partial class Form1 : Form, IRenderLoopListener
    {
        public Form1()
        {
            InitializeComponent();

            this.display = Display.Create(320, 200, 2, 2, CGAColor.Black);
            this.display.RegisterListener(this);

            ScaledBitmap testSprite = ScaledBitmap.FromBitmap((Bitmap)Bitmap.FromFile("test_sprite.png"));
            Random rnd = new Random();
            this.viewPorts = new List<TestViewPort>();
            this.viewPorts.Add(new TestViewPort(0, 0, 200, 150, CGAColor.Cyan, testSprite, 50, rnd));
            this.viewPorts.Add(new TestViewPort(50, 70, 100, 50, CGAColor.Green, testSprite, 50, rnd));
            this.viewPorts.Add(new TestViewPort(70, 80, 100, 50, CGAColor.Red, testSprite, 50, rnd));
            foreach (ViewPort vp in this.viewPorts)
            {
                this.display.RegisterViewPort(vp);                
            }
            this.selectedVP = this.viewPorts[2];

            this.formClosing = new AutoResetEvent(false);
        }

        public bool CreateFrameBegin(long timeSinceLastFrameFinish)
        {
            foreach (TestViewPort vp in this.viewPorts)
            {
                vp.Step();
            }
            return true;
        }

        public void DrawBegin(long timeSinceFrameBegin)
        {
            
        }

        public void DrawFinish(long timeSinceDrawBegin, Rectangle refreshedArea)
        {
            if (!refreshedArea.IsEmpty)
            {
                Invalidate(new Rectangle(refreshedArea.X * Display.HorizontalScale,
                                         refreshedArea.Y * Display.VerticalScale,
                                         refreshedArea.Width * Display.HorizontalScale,
                                         refreshedArea.Height * Display.VerticalScale));
            }
        }

        public bool CreateFrameFinish(long timeSinceLastFrameFinish, long timeSinceThisFrameBegin, long timeSinceDrawFinish)
        {
            if (!this.formClosing.WaitOne(0))
            {
                if (timeSinceLastFrameFinish < 30)
                {
                    int waitTime = (int)(30 - timeSinceLastFrameFinish);
                    RCThread.Sleep(waitTime);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.display.AccessCurrentFrame(e.Graphics, e.ClipRectangle);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            return;
        }

        private void RenderProc()
        {
            this.display.StartRenderLoop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.renderThread = new RCThread(this.RenderProc, "Render");
            this.renderThread.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.formClosing.Set();
            this.renderThread.Join();
            this.formClosing.Close();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (null != this.selectedVP)
                    {
                        this.selectedVP.MoveViewPort(this.selectedVP.Position.X - 1, this.selectedVP.Position.Y);
                    }
                    break;
                case Keys.Right:
                    if (null != this.selectedVP)
                    {
                        this.selectedVP.MoveViewPort(this.selectedVP.Position.X + 1, this.selectedVP.Position.Y);
                    }
                    break;
                case Keys.Down:
                    if (null != this.selectedVP)
                    {
                        this.selectedVP.MoveViewPort(this.selectedVP.Position.X, this.selectedVP.Position.Y + 1);
                    }
                    break;
                case Keys.Up:
                    if (null != this.selectedVP)
                    {
                        this.selectedVP.MoveViewPort(this.selectedVP.Position.X, this.selectedVP.Position.Y - 1);
                    }
                    break;
                case Keys.Space:
                    if (null != this.selectedVP)
                    {
                        int selectedIdx = this.viewPorts.IndexOf(this.selectedVP);
                        if (selectedIdx >= 0 && selectedIdx < this.viewPorts.Count)
                        {
                            selectedIdx++;
                            if (selectedIdx == this.viewPorts.Count) { selectedIdx = 0; }
                            this.selectedVP = this.viewPorts[selectedIdx];
                        }
                    }
                    else if (0 != this.viewPorts.Count)
                    {
                        this.selectedVP = this.viewPorts[0];
                    }
                    break;
                case Keys.PageDown:
                    if (null != this.selectedVP)
                    {
                        this.display.SendViewPortBackward(this.selectedVP);
                    }
                    break;
                case Keys.PageUp:
                    if (null != this.selectedVP)
                    {
                        this.display.BringViewPortForward(this.selectedVP);
                    }
                    break;
                case Keys.End:
                    if (null != this.selectedVP)
                    {
                        this.display.SendViewPortBottom(this.selectedVP);
                    }
                    break;
                case Keys.Home:
                    if (null != this.selectedVP)
                    {
                        this.display.BringViewPortTop(this.selectedVP);
                    }
                    break;
                case Keys.Delete:
                    if (null != this.selectedVP)
                    {
                        this.display.UnregisterViewPort(this.selectedVP);
                    }
                    break;
                case Keys.Insert:
                    if (null != this.selectedVP)
                    {
                        this.display.RegisterViewPort(this.selectedVP);
                    }
                    break;
                default:
                    break;
            }
        }

        private Display display;

        private List<TestViewPort> viewPorts;

        private TestViewPort selectedVP;

        private RCThread renderThread;

        private AutoResetEvent formClosing;

    }
}
