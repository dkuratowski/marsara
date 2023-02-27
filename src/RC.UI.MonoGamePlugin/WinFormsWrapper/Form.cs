using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Forms
{
    class Form : Control
    {
        public static Control? FromHandle(IntPtr handle)
        {
            throw new NotImplementedException();
        }


        public FormBorderStyle FormBorderStyle
        {
            get { return this.formBorderStyle; }
            set { this.formBorderStyle = value; }
        }

        public int Top
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Left
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        private FormBorderStyle formBorderStyle;
    }
}