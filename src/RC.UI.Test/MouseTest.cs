using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.Reflection;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI.Test
{
    class MouseTest
    {
        public static void Test()
        {
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();

            UISprite mouseIcon = root.GraphicsPlatform.SpriteManager.LoadSprite("..\\..\\..\\..\\sprites\\pointers\\normal_pointer.png", new RCIntVector(2, 2));
            mouseIcon.TransparentColor = new RCColor(255, 0, 255);
            mouseIcon.Upload();
            UIPointer pointer = new UIPointer(mouseIcon, new RCIntVector(0, 0));
            
            DynamicString.RegisterResolver("RC.UI.UIWorkspace.PixelScaling",
                delegate()
                {
                    return "2;2";
                });
            UIResourceManager.LoadResourceGroup("RC.App.SplashScreen");
            UIResourceManager.LoadResourceGroup("RC.App.CommonResources");

            display = new TestUIObject(new RCIntVector(2, 2), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 400, 300));
            workspace = new MySensitiveObject(new RCIntVector(40, 50), new RCIntRectangle(0, 0, 320, 200),
                                              "Workspace", RCColor.Gray, RCColor.Gray);
            display.Attach(workspace);

            MySensitiveObject objA = new MySensitiveObject(new RCIntVector(5, 12), new RCIntRectangle(0, 0, 130, 90), "A", RCColor.Blue, RCColor.LightBlue);
            MySensitiveObject objB = new MySensitiveObject(new RCIntVector(5, 107), new RCIntRectangle(0, 0, 130, 90), "B", RCColor.Blue, RCColor.LightBlue);
            MyDraggableObject objC = new MyDraggableObject(new RCIntVector(140, 12), new RCIntRectangle(0, 0, 130, 180), "C", RCColor.Cyan, RCColor.LightCyan);
            workspace.Attach(objA);
            workspace.Attach(objB);
            workspace.Attach(objC);
            workspace.AttachSensitive(objA);
            workspace.AttachSensitive(objB);
            workspace.AttachSensitive(objC);

            MySensitiveObject objAA = new MySensitiveObject(new RCIntVector(5, 12), new RCIntRectangle(0, 0, 120, 30), "AA", RCColor.Green, RCColor.LightGreen);
            MySensitiveObject objAB = new MySensitiveObject(new RCIntVector(5, 47), new RCIntRectangle(0, 0, 120, 30), "AB", RCColor.Green, RCColor.LightGreen);
            MySensitiveObject objBA = new MySensitiveObject(new RCIntVector(5, 12), new RCIntRectangle(0, 0, 120, 30), "BA", RCColor.Green, RCColor.LightGreen);
            MySensitiveObject objBB = new MySensitiveObject(new RCIntVector(5, 47), new RCIntRectangle(0, 0, 120, 30), "BB", RCColor.Green, RCColor.LightGreen);
            MyDraggableObject objCA = new MyDraggableObject(new RCIntVector(5, 12), new RCIntRectangle(0, 0, 120, 30), "CA", RCColor.Magenta, RCColor.LightMagenta);
            MyDraggableObject objCB = new MyDraggableObject(new RCIntVector(5, 47), new RCIntRectangle(0, 0, 120, 30), "CB", RCColor.Gray, RCColor.White);
            objA.Attach(objAA);
            objA.Attach(objAB);
            objB.Attach(objBA);
            objB.Attach(objBB);
            objC.Attach(objCA);
            objC.Attach(objCB);
            objA.AttachSensitive(objAA);
            objA.AttachSensitive(objAB);
            objB.AttachSensitive(objBA);
            objB.AttachSensitive(objBB);
            objC.AttachSensitive(objCA);
            objC.AttachSensitive(objCB);

            //root.SystemEventQueue.Subscribe<UIKeyboardEventArgs>(OnKeyboardEvent);
            //root.SystemEventQueue.Subscribe<UIMouseEventArgs>(OnMouseEvent);

            UIMouseManager mouseMgr = new UIMouseManager(workspace);
            mouseMgr.SetDefaultMousePointer(pointer);

            root.GraphicsPlatform.RenderManager.Attach(display);
            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(800, 600));

            root.Dispose();
        }

        private static TestUIObject display;
        private static MySensitiveObject workspace;
    }

    class MySensitiveObject : UISensitiveObject
    {
        public MySensitiveObject(RCIntVector position,
                                 RCIntRectangle range,
                                 string name,
                                 RCColor basicColor,
                                 RCColor highColor)
            : base(position, range)
        {
            this.nameStrBasic = new UIString(name, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), RCColor.WhiteHigh);
            this.nameStrHigh = new UIString(name, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), RCColor.LightRed);
            this.backgroundBasic = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(basicColor, this.Range.Size, new RCIntVector(2, 2));
            this.backgroundHigh = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(highColor, this.Range.Size, new RCIntVector(2, 2));

            this.backgroundBasic.Upload();
            this.backgroundHigh.Upload();

            this.name = name;
            this.isHighlighted = false;
            this.activatorBtn = UIMouseButton.Undefined;

            this.MouseSensor.Enter += this.OnEnter;
            this.MouseSensor.Leave += this.OnLeave;
            this.MouseSensor.ButtonDown += this.OnButtonDown;
            this.MouseSensor.ButtonUp += this.OnButtonUp;
            this.MouseSensor.Wheel += this.OnWheel;
            this.MouseSensor.Move += this.OnMove;
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.isHighlighted ? this.backgroundHigh : this.backgroundBasic,
                                       new RCIntVector(0, 0));
            renderContext.RenderString(this.activatorBtn != UIMouseButton.Undefined ? this.nameStrHigh : this.nameStrBasic,
                                       new RCIntVector(1, this.nameStrHigh.Font.CharTopMaximum + 1));
        }

        protected event ActivatedHdl Activated;
        protected event InactivatedHdl Inactivated;

        protected delegate void ActivatedHdl(MySensitiveObject sender, UIMouseButton btn, RCIntVector pos);
        protected delegate void InactivatedHdl(MySensitiveObject sender);

        private void OnEnter(UISensitiveObject sender)
        {
            this.isHighlighted = true;
        }

        private void OnLeave(UISensitiveObject sender)
        {
            this.isHighlighted = false;
        }

        private void OnButtonDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == UIMouseButton.Undefined)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.Activated != null) { this.Activated(this, evtArgs.Button, evtArgs.Position); }
            }
            TraceManager.WriteAllTrace(string.Format("{0}: {1} down", this.name, evtArgs.Button), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private void OnButtonUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.activatorBtn == evtArgs.Button)
            {
                this.activatorBtn = UIMouseButton.Undefined;
                if (this.Inactivated != null) { this.Inactivated(this); }
            }
            TraceManager.WriteAllTrace(string.Format("{0}: {1} up", this.name, evtArgs.Button), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private void OnWheel(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            TraceManager.WriteAllTrace(string.Format("{0}: wheel({1})", this.name, evtArgs.WheelDelta), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private void OnMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            TraceManager.WriteAllTrace(string.Format("{0}: move({1})", this.name, evtArgs.Position), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
        }

        private UIMouseButton activatorBtn;
        private string name;
        private UIString nameStrBasic;
        private UIString nameStrHigh;
        private UISprite backgroundBasic;
        private UISprite backgroundHigh;
        private bool isHighlighted;
    }

    class MySensitiveAnimObject : MySensitiveObject
    {
        public MySensitiveAnimObject(RCIntVector position,
                                 RCIntRectangle range,
                                 string name,
                                 RCColor basicColor,
                                 RCColor highColor)
            : base(position, range, name, basicColor, highColor)
        {
            this.anim = UIResourceManager.GetResource<UIAnimation>("RC.App.Animations.MainMenuTitleAnim");
            this.anim.Reset(true);
            this.anim.Start();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);
            renderContext.RenderSprite(this.anim.CurrentSprite, new RCIntVector(25, 140));
        }

        private UIAnimation anim;
    }

    class MyDraggableObject : MySensitiveObject
    {
        public MyDraggableObject(RCIntVector position,
                                 RCIntRectangle range,
                                 string name,
                                 RCColor basicColor,
                                 RCColor highColor)
            : base(position, range, name, basicColor, highColor)
        {
            this.hookPosition = RCIntVector.Undefined;

            this.Activated += this.OnHooked;
            this.MouseSensor.Move += this.OnDrag;
            this.Inactivated += this.OnUnhooked;
        }

        private void OnHooked(MySensitiveObject sender, UIMouseButton btn, RCIntVector hookPos)
        {
            if (btn == UIMouseButton.Left)
            {
                this.hookPosition = hookPos;
            }
        }

        private void OnDrag(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.hookPosition != RCIntVector.Undefined)
            {
                this.Position += evtArgs.Position - this.hookPosition;
            }
        }

        private void OnUnhooked(MySensitiveObject sender)
        {
            this.hookPosition = RCIntVector.Undefined;
        }

        private RCIntVector hookPosition;
    }
}
