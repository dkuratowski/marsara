using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI.Test
{
    static class TestScene
    {
        public static void Test()
        {
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();

            obj0 = new TestUIObject(new RCIntVector(10, 10), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 70, 70));
            obj1 = new TestUIObject(new RCIntVector(1, 1), new RCIntVector(0, 0), new RCIntRectangle(10, 10, 50, 50));
            //obj2 = new TestUIObject(new RCIntVector(1, 2), new RCIntVector(6, 7), new RCIntRectangle(0, 0, 20, 20));

            obj0.Attach(obj1);
            //obj1.Attach(obj2);

            obj0.LoadSprite("./test_scene_sprites/obj0.png");
            obj1.LoadSprite("./test_scene_sprites/background.png");
            //obj2.LoadSprite("./test_scene_sprites/obj2.png");

            root.KeyboardAccess.StateChanged += OnKeyboardEvent;
            root.MouseAccess.StateChanged += OnMouseEvent;

            UISprite mouseIcon = root.GraphicsPlatform.SpriteManager.LoadSprite("./test_scene_sprites/pointer.png");
            mouseIcon.TransparentColor = new RCColor(255, 0, 255);
            //UIMouseManager mouseMgr = new UIMouseManager(obj1);
            //mouseMgr.Pointer = new UIBasicPointer(mouseIcon, new RCIntVector(4, 4));

            root.GraphicsPlatform.RenderManager.Attach(obj0);
            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(800, 600));

            root.Dispose();
        }

        private static void OnKeyboardEvent()
        {
            string s = string.Empty;
            foreach (UIKey key in UIRoot.Instance.KeyboardAccess.PressedKeys)
            {
                s += " " + key;
            }
            TraceManager.WriteAllTrace(s, TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private static void OnMouseEvent()
        {
            string s = string.Empty;
            foreach (UIMouseButton btn in UIRoot.Instance.MouseAccess.PressedButtons)
            {
                s += " " + btn;
            }
            s += "; wheel(" + UIRoot.Instance.MouseAccess.ScrollWheelPos + ")";
            TraceManager.WriteAllTrace(s, TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private static TestUIObject obj0;
        private static TestUIObject obj1;
        //private static TestUIObject obj2;
    }

    class TestUIObject : UIObject
    {
        public TestUIObject(RCIntVector pixelScaling, RCIntVector position, RCIntRectangle range)
            : base(pixelScaling, position, range)
        {
        }

        public void LoadSprite(string spritePath)
        {
            this.sprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite(spritePath, this.AbsolutePixelScaling);
            this.sprite.Upload();
        }

        public void ActivateStringRender()
        {
            this.str = new UIString("Hours: {0} Minutes: {1} Seconds: {2}", UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), this.AbsolutePixelScaling, new RCColor(255, 255, 255));
            //this.str = new UIString("::", UIFont.TestFont, this.AbsolutePixelScaling, new RCColor(255, 255, 255));
        }

        public UISprite Sprite { get { return this.sprite; } }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.sprite != null)
            {
                renderContext.RenderSprite(this.sprite, new RCIntVector(0, 0));
            }
            if (this.str != null)
            {
                DateTime currTime = DateTime.Now;
                str[0] = currTime.Hour;
                str[1] = currTime.Minute;
                str[2] = currTime.Second;
                renderContext.RenderString(this.str, new RCIntVector(20, 60));
            }
        }

        private UISprite sprite;
        private UIString str;
    }
}
