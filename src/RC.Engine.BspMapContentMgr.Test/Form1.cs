using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RC.Common;
using System.Diagnostics;
using RC.Engine.Maps.Core;

namespace RC.Engine.BspMapContentMgr.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.contentManager = new BspMapContentManager<TestContent>(new RCNumRectangle(0, 0, this.Size.Width, this.Size.Height), 5, 10);
            this.nonSelectedContents = new HashSet<TestContent>();
            this.selectedContents = new HashSet<TestContent>();
            this.currentMode = Mode.None;
            this.beginPos = RCNumVector.Undefined;
            this.currentPos = RCNumVector.Undefined;

            Random rnd = new Random();
            for (int i = 0; i < OBJ_NUM; ++i)
            {
                RCNumVector rndPos = new RCNumVector(rnd.Next(this.Width), rnd.Next(this.Height));
                RCNumVector rndSize = new RCNumVector(rnd.Next(this.MIN_SIZE.X, this.MAX_SIZE.X), rnd.Next(this.MIN_SIZE.Y, this.MAX_SIZE.Y));
                TestContent newContent = new TestContent(new RCNumRectangle(rndPos, rndSize));
                this.contentManager.AttachContent(newContent);
                this.nonSelectedContents.Add(newContent);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (TestContent content in this.nonSelectedContents)
            {
                e.Graphics.DrawRectangle(Pens.Green, (int)content.Position.X, (int)content.Position.Y, (int)content.Position.Width, (int)content.Position.Height);
            }

            foreach (TestContent content in this.selectedContents)
            {
                e.Graphics.DrawRectangle(Pens.Red, (int)content.Position.X, (int)content.Position.Y, (int)content.Position.Width, (int)content.Position.Height);
            }

            foreach (RCNumRectangle nodeRect in this.contentManager.GetTreeNodeBoundaries())
            {
                e.Graphics.DrawRectangle(Pens.OrangeRed, (int)nodeRect.X, (int)nodeRect.Y, (int)nodeRect.Width, (int)nodeRect.Height);
            }
        }

        private BspMapContentManager<TestContent> contentManager;

        private HashSet<TestContent> nonSelectedContents;
        private HashSet<TestContent> selectedContents;

        private enum Mode
        {
            None = 0,
            Creating = 1,
            Selecting = 2,
            Dragging = 3
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.currentMode == Mode.None)
            {
                this.beginPos = new RCNumVector(e.X, e.Y);
                this.currentPos = new RCNumVector(e.X, e.Y);
                this.currentMode = Mode.Creating;
            }
            else if (e.Button == MouseButtons.Left && this.currentMode == Mode.None)
            {
                this.beginPos = new RCNumVector(e.X, e.Y);
                this.currentPos = new RCNumVector(e.X, e.Y);

                this.stopwatch.Reset(); this.stopwatch.Start();
                HashSet<TestContent> draggedContents = this.contentManager.GetContents(this.beginPos);
                this.stopwatch.Stop(); this.avgGetContentAtPos.NewItem((int)this.stopwatch.ElapsedMilliseconds);

                bool isDragging = false;
                foreach (TestContent item in draggedContents)
                {
                    if (this.selectedContents.Contains(item)) { isDragging = true; break; }
                }
                this.currentMode = isDragging ? Mode.Dragging : Mode.Selecting;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.currentMode == Mode.Creating || this.currentMode == Mode.Selecting)
            {
                this.currentPos = new RCNumVector(this.beginPos.X > e.X ? this.beginPos.X : e.X, this.beginPos.Y > e.Y ? this.beginPos.Y : e.Y);
            }
            else if (this.currentMode == Mode.Dragging)
            {
                this.currentPos = new RCNumVector(e.X, e.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.currentMode == Mode.Creating)
            {
                RCNumVector sizeVect = this.currentPos - this.beginPos;
                if (sizeVect.X > 0 && sizeVect.Y > 0)
                {
                    TestContent newContent = new TestContent(new RCNumRectangle(this.beginPos, sizeVect));

                    this.stopwatch.Reset(); this.stopwatch.Start();
                    this.contentManager.AttachContent(newContent);
                    this.stopwatch.Stop(); this.avgAttachContent.NewItem((int)this.stopwatch.ElapsedMilliseconds);

                    this.nonSelectedContents.Add(newContent);
                    this.Invalidate();
                }
                this.currentMode = Mode.None;
            }
            else if (e.Button == MouseButtons.Left && this.currentMode == Mode.Selecting)
            {
                RCNumVector sizeVect = this.currentPos - this.beginPos;
                if (sizeVect.X > 0 && sizeVect.Y > 0)
                {
                    foreach (TestContent content in this.selectedContents)
                    {
                        this.nonSelectedContents.Add(content);
                    }
                    this.selectedContents.Clear();

                    RCNumRectangle selBox = new RCNumRectangle(this.beginPos, sizeVect);

                    this.stopwatch.Reset(); this.stopwatch.Start();
                    HashSet<TestContent> contents = this.contentManager.GetContents(selBox);
                    this.stopwatch.Stop(); this.avgGetContentInBox.NewItem((int)this.stopwatch.ElapsedMilliseconds);

                    foreach (TestContent content in contents)
                    {
                        this.nonSelectedContents.Remove(content);
                        this.selectedContents.Add(content);
                    }
                    this.Invalidate();
                }
                this.currentMode = Mode.None;
            }
            else if (e.Button == MouseButtons.Left && this.currentMode == Mode.Dragging)
            {
                foreach (TestContent draggedContent in this.selectedContents)
                {
                    this.stopwatch.Reset(); this.stopwatch.Start();
                    draggedContent.Position += this.currentPos - this.beginPos;
                    this.stopwatch.Stop(); this.avgPositionChange.NewItem((int)this.stopwatch.ElapsedMilliseconds);
                }
                this.Invalidate();
                this.currentMode = Mode.None;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (TestContent content in this.selectedContents)
                {
                    this.stopwatch.Reset(); this.stopwatch.Start();
                    this.contentManager.DetachContent(content);
                    this.stopwatch.Stop(); this.avgDetachContent.NewItem((int)this.stopwatch.ElapsedMilliseconds);
                }
                this.selectedContents.Clear();
                this.Invalidate();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                string performanceMeasurement = string.Format("Average attach time: {0} ms\nAverage detach time: {1} ms\n Average search-in-box time: {2} ms\nAverage search-at-position time: {3} ms\nAverage position change time: {4} ms",
                                                              this.avgAttachContent.Average, this.avgDetachContent.Average, this.avgGetContentInBox.Average, this.avgGetContentAtPos.Average, this.avgPositionChange.Average);
                MessageBox.Show(performanceMeasurement, "Performance measurement");
            }
        }

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    return;
        //}

        private Mode currentMode;
        private RCNumVector beginPos;
        private RCNumVector currentPos;

        private AverageCalculator avgAttachContent = new AverageCalculator(5, 0);
        private AverageCalculator avgDetachContent = new AverageCalculator(5, 0);
        private AverageCalculator avgGetContentAtPos = new AverageCalculator(5, 0);
        private AverageCalculator avgGetContentInBox = new AverageCalculator(5, 0);
        private AverageCalculator avgPositionChange = new AverageCalculator(5, 0);
        private Stopwatch stopwatch = new Stopwatch();

        private readonly RCIntVector MAX_SIZE = new RCIntVector(30, 30);
        private readonly RCIntVector MIN_SIZE = new RCIntVector(5, 5);
        private int OBJ_NUM = 500;
    }
}
