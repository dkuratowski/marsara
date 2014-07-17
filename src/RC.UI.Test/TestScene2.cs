using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Common;
using RC.Common.Configuration;

namespace RC.UI.Test
{
    class TestScene2
    {
        public static void Test()
        {
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();

            display = new TestUIObject(new RCIntVector(1, 1), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 800, 600));
            workspace = new TestUIObject(new RCIntVector(2, 2), new RCIntVector(80, 100), new RCIntRectangle(0, 0, 320, 200));
            display.Attach(workspace);
            workspace.LoadSprite(".\\testui_sprites\\workspace.png");
            workspace.Sprite.TransparentColor = new RCColor(255, 0, 255);
            workspace.ActivateStringRender();

            //root.SystemEventQueue.Subscribe<UIKeyboardEventArgs>(OnKeyboardEvent);
            //root.SystemEventQueue.Subscribe<UIMouseEventArgs>(OnMouseEvent);

            UISprite mouseIcon = root.GraphicsPlatform.SpriteManager.LoadSprite(".\\testui_sprites\\pointer.png");
            mouseIcon.TransparentColor = new RCColor(255, 0, 255);
            mouseIcon.Upload();
            //UIMouseManager mouseMgr = new UIMouseManager(workspace);
            //mouseMgr.Pointer = new UIBasicPointer(mouseIcon, new RCIntVector(4, 4));

            root.GraphicsPlatform.RenderManager.Attach(display);
            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(800, 600));

            root.Dispose();
        }

        private static TestUIObject display;
        private static TestUIObject workspace;
    }
}
