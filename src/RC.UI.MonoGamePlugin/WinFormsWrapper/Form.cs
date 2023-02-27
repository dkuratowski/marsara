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
            return new Form(handle);
        }

        public Form(IntPtr handle)
        {
            this.handle = handle;
        }


        public FormBorderStyle FormBorderStyle
        {
            get { return this.formBorderStyle; }
            set { this.formBorderStyle = value; }
        }

        public int Top
        {
            get { return this.top; }
            set { this.top = value; }
        }

        public int Left
        {
            get { return this.left; }
            set { this.left = value; }
        }

        private FormBorderStyle formBorderStyle;
        private int top;
        private int left;
        private IntPtr handle;
    }
}