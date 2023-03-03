using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    sealed class MonoGamePlugin : IUIPlugin
    {
        public MonoGamePlugin()
        {
        }

        #region IUIPlugin Members

        public string Name
        {
            get { return "RC.UI.MonoGamePlugin"; }
        }

        public void Install()
        {
            this.graphicsPlatform = new MonoGameGraphicsPlatform();
            UIRoot.Instance.RegisterGraphicsPlatform(this.graphicsPlatform);
        }

        public void Uninstall()
        {
            UIRoot.Instance.UnregisterGraphicsPlatform();
            this.graphicsPlatform.Dispose();
        }

        #endregion

        private MonoGameGraphicsPlatform graphicsPlatform;
    }
}
