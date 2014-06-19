using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.MotionControl.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Initialize the UI-core and install the XNA-plugin (TODO: make it configurable)
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();

            /// Activate the event sources
            root.GetEventSource("RC.UI.XnaPlugin.XnaMouseEventSource").Activate();
            root.GetEventSource("RC.UI.XnaPlugin.XnaKeyboardEventSource").Activate();

            /// Create the UIWorkspace and set the mouse pointer
            UIWorkspace workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(1024, 768));
            UISprite mouseIcon = root.GraphicsPlatform.SpriteManager.LoadSprite("..\\..\\..\\..\\sprites\\pointers\\menu_pointer.png");
            mouseIcon.TransparentColor = new RCColor(255, 0, 255);
            mouseIcon.Upload();
            UIBasicPointer basicPtr = new UIBasicPointer(mouseIcon, new RCIntVector(0, 0));
            UIWorkspace.Instance.SetMousePointer(basicPtr);

            /// Create and activate the test page
            RCMotionControlTestPage motionControlTestPage = new RCMotionControlTestPage();
            UIWorkspace.Instance.RegisterPage(motionControlTestPage);
            motionControlTestPage.Activate();

            /// Start and run the render loop
            root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);

            /// After the render loop has been stopped, release all resources of the UIRoot.
            root.GraphicsPlatform.SpriteManager.DestroySprite(mouseIcon);
            root.Dispose();
        }
    }
}
