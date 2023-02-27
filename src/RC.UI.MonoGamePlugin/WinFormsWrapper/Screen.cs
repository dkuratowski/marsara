using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    class Screen
    {
        public static Screen[] AllScreens
        {
            get
            {
                return new Screen[] { new Screen() };
            }
        }

        public Rectangle WorkingArea
        {
            get
            {
                return new Rectangle();
            }
        }
    }
}