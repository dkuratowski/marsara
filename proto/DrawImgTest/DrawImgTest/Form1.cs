using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DrawImgTest
{
    /// <summary> 
    /// Summary description for Form1. 
    /// </summary> 
    public partial class Form1 : System.Windows.Forms.Form
    {
        float _angle;
        bool _doBuffer = true;
        Bitmap theImage = null;
        int translate = 0;

        public Form1()
        {
            // 
            // Required for Windows Form Designer support 
            // 
            InitializeComponent();
            this.timer2.Enabled = true;
            this.WindowState = FormWindowState.Maximized;
            //this.FormBorderStyle = FormBorderStyle.None;
            //this.TopMost = true;

            Bitmap img = (Bitmap)Bitmap.FromFile("..\\..\\..\\screen_layout_big.bmp");
            this.theImage = ResizeImage(img, 3);
            //this.theImage = img;
        }

        private Bitmap ResizeImage(Bitmap img, int scale)
        {
            Bitmap retImg = new Bitmap(img.Width * scale, img.Height * scale);
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    for (int k = 0; k < scale; k++)
                    {
                        for (int l = 0; l < scale; l++)
                        {
                            retImg.SetPixel(j * scale + l, i * scale + k, img.GetPixel(j, i));
                        }
                    }
                }
            }
            return retImg;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            this.translate += 1;
            if (this.translate > 100)
            {
                this.translate = 0;
            }
            Invalidate();
        }

        private Bitmap _backBuffer;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_backBuffer == null)
            {
                _backBuffer = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            }

            Graphics g = null;
            if (_doBuffer)
            {
                g = Graphics.FromImage(_backBuffer);
            }
            else
            {
                g = e.Graphics;
            }

            g.Clear(Color.Black);            
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            g.SmoothingMode = SmoothingMode.None;

            /*
            Matrix mx = new Matrix();
            mx.Rotate(_angle, MatrixOrder.Append);
            mx.Translate(100, 100, MatrixOrder.Append);
            g.Transform = mx;
            g.FillRectangle(Brushes.Red, -100, -100, 200, 200);

            mx = new Matrix();
            mx.Rotate(-_angle, MatrixOrder.Append);
            //mx.Translate(this.ClientSize.Width / 2, this.ClientSize.Height / 2, MatrixOrder.Append);
            mx.Translate(100, 100, MatrixOrder.Append);
            g.Transform = mx;
            g.FillRectangle(Brushes.Green, -75, -75, 149, 149);
            
            mx = new Matrix();
            mx.Rotate(_angle * 2, MatrixOrder.Append);
            //mx.Translate(this.ClientSize.Width / 2, this.ClientSize.Height / 2, MatrixOrder.Append);
            mx.Translate(100, 100, MatrixOrder.Append);
            g.Transform = mx;            
            g.FillRectangle(Brushes.Blue, -50, -50, 100, 100);
            */

            g.DrawImageUnscaled(this.theImage, this.translate * 3, this.translate * 3);
            //g.DrawImage(this.theImage, this.translate * 3, this.translate * 3, this.theImage.Width * 3, this.theImage.Height * 3);
            if (_doBuffer)
            {
                g.Dispose();
                //Copy the back buffer to the screen 
                e.Graphics.DrawImageUnscaled(_backBuffer, 0, 0);
//                e.Graphics.DrawImage(_backBuffer,
//                                     this.ClientRectangle.Left,
//                                     this.ClientRectangle.Top,
//                                     this.ClientRectangle.Width * 4,
//                                     this.ClientRectangle.Height * 4);
            }
            //base.OnPaint (e); //optional but not recommended 
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //Don't allow the background to paint 
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (_backBuffer != null)
            {
                _backBuffer.Dispose();
                _backBuffer = null;
            }

            base.OnSizeChanged(e);
        }
    }
}

