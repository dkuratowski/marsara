using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI.XnaPlugin
{
    sealed class XnaPlugin : IUIPlugin
    {
        public XnaPlugin()
        {
        }

        #region IUIPlugin Members

        public string Name
        {
            get { return "RC.UI.XnaPlugin"; }
        }

        public void Install()
        {
            this.graphicsPlatform = new XnaGraphicsPlatform();
            UIRoot.Instance.RegisterGraphicsPlatform(this.graphicsPlatform);
        }

        public void Uninstall()
        {
            UIRoot.Instance.UnregisterGraphicsPlatform();
            this.graphicsPlatform.Dispose();
        }

        #endregion

        private XnaGraphicsPlatform graphicsPlatform;
    }
}
