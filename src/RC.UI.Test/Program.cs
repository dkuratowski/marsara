using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using RC.Common;
using System.Diagnostics;
using System.Reflection;
using RC.Common.Configuration;
using System.Drawing.Imaging;
using RC.UI.XnaPlugin;

namespace RC.UI.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            new UIRoot();
            UIRoot.Instance.LoadPlugins(xnaPlugin);
            UIRoot.Instance.InstallPlugins();
            UIFont testFont = UIFont.TestFont;

            //UIString str = new UIString("This change affects the LobbyServer class. The network event handling and calling the interface of LobbyServer are performed in separate threads. Suppose that the caller thread has called LobbyServer.SendPackage with a package that has been inserted into a FIFO queue and then it returns. At the same time the lobby thread notices that the current connection on a line was lost, but it accepts another connection immediately on that line. The problem is that the package is still waiting in the FIFO, however it was targeted to the previous connection. We have to implement a mechanism in the lobby thread that removes those packages from the FIFO that were sent to lost connections. This mechanism should be executed just before accepting a new connection.", UIFont.TestFont, new RCIntVector(3, 6), new RCColor(0, 0, 255));
            UIString str = new UIString("{0}{1}ABCDEFG{2} {0} {1}HIJKM {3} NOPQRSTUVWXYZ{0}{1}{2}abcdefghijklmnopq{0}rstuvwxyz 0123456789 !?;:.,\"'&()[]{{}}+-/\\_<>=@*%#$", UIFont.TestFont, new RCIntVector(1, 1), new RCColor(0, 255, 0));
            str[0] = "X";
            UISprite testImg =
                UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(testFont.TransparentColor,
                                                                            new RCIntVector(1500, testFont.MinimumLineHeight),
                                                                            new RCIntVector(1, 1));

            IUIRenderContext ctx = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(testImg);
            int cursorX = 0;
            int cursorY = testFont.CharTopMaximum;
            foreach (UIStringFragment fragment in str.Fragments)
            {
                if (fragment.Source != null)
                {
                    ctx.RenderSprite(fragment.Source,
                                     new RCIntVector(cursorX, cursorY + fragment.Offset),
                                     fragment.Section);
                }
                cursorX += fragment.CursorStep;
            }
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(testImg);
            testImg.Save("testribbon.png");

            str[1] = "Y";
            str[2] = "Z";

            testImg =
                UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(testFont.TransparentColor,
                                                                            new RCIntVector(1500, testFont.MinimumLineHeight),
                                                                            new RCIntVector(1, 1));

            ctx = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(testImg);
            cursorX = 0;
            cursorY = testFont.CharTopMaximum;
            foreach (UIStringFragment fragment in str.Fragments)
            {
                if (fragment.Source != null)
                {
                    ctx.RenderSprite(fragment.Source,
                                     new RCIntVector(cursorX, cursorY + fragment.Offset),
                                     fragment.Section);
                }
                cursorX += fragment.CursorStep;
            }
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(testImg);
            testImg.Save("testribbon2.png");*/
            //IEnumerable<UIStringFragment> strFragments = str.Fragments;
            //Test4();
            //Test5();
            //Test6();
            //TestScene2.Test();
            //MouseTest.Test();
            ControlTest.Test();
            //WorkspaceTest.Test();
        }

        static void Test0()
        {
            mgr.Attach(a);

            b.Attach(c); b.Attach(g);
            c.Attach(d); c.Attach(e); c.Attach(f);
            g.Attach(h); g.Attach(i);
            i.Attach(j); i.Attach(k);

            l.Attach(m); l.Attach(n); l.Attach(t);
            n.Attach(o); n.Attach(r); n.Attach(s);
            o.Attach(p); o.Attach(q);

            a.Attach(b); a.Attach(l);
            Console.WriteLine("DFS: " + a.PrintSubtree());
            Console.WriteLine("Render queue: " + mgr.ToString());

            Stopwatch watch = new Stopwatch();
            watch.Restart();
            for (int idx = 0; idx < 1000000; idx++)
            {
                //mgr.Render();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            Console.ReadLine();
        }

        static void Test1()
        {
            mgr.Attach(a);
            for (int idx = 0; idx < 1000; idx++)
            {
                MyUIObject newObj = new MyUIObject(idx.ToString());
                a.Attach(newObj);
                objList[idx] = newObj;
            }

            Stopwatch watch = new Stopwatch();
            watch.Restart();
            for (int idx = 0; idx < 10000; idx++)
            {
                //mgr.Render();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            Console.ReadLine();
        }

        static void Test2()
        {
            UIRoot root = new UIRoot();
            Assembly fromAsm = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(fromAsm);
        }

        static void Test4()
        {
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();
            MyUIObject2 obj = new MyUIObject2();
            root.GraphicsPlatform.RenderManager.Attach(obj);
            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(320, 200));
            root.Dispose();
        }

        static void Test5()
        {
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();

            UISprite source = root.GraphicsPlatform.SpriteManager.LoadSprite("./sprite_tests/orig_source.png");
            source.TransparentColor = new RCColor(255, 0, 0);
            UISprite med = root.GraphicsPlatform.SpriteManager.LoadSprite("./sprite_tests/orig_med.png", new RCIntVector(1, 2));
            med.TransparentColor = new RCColor(0, 255, 255);
            UISprite final = root.GraphicsPlatform.SpriteManager.LoadSprite("./sprite_tests/orig_final.png", new RCIntVector(2, 1));

            IUIRenderContext ctx = root.GraphicsPlatform.SpriteManager.CreateRenderContext(med);
            ctx.RenderSprite(source, new RCIntVector(0, 0), new RCIntRectangle(3, 2, 5, 6));
            root.GraphicsPlatform.SpriteManager.CloseRenderContext(med);

            ctx = root.GraphicsPlatform.SpriteManager.CreateRenderContext(final);
            ctx.RenderSprite(med, new RCIntVector(1, 2));
            root.GraphicsPlatform.SpriteManager.CloseRenderContext(final);

            med.TransparentColor = RCColor.Undefined;
            med.Save("./sprite_tests/med.png");
            final.Save("./sprite_tests/final.png");

            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(320, 200));

            root.Dispose();
        }

        static void Test6()
        {
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            new UIRoot();
            UIRoot.Instance.LoadPlugins(xnaPlugin);
            UIRoot.Instance.InstallPlugins();
            UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6").GetFontSprite(new RCIntVector(6, 3), new RCColor(255, 0, 0)).Save("c255_0_0_ps6_3.png");
            UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6").GetFontSprite(new RCIntVector(2, 3), new RCColor(0, 255, 0)).Save("c0_255_0_ps2_3.png");
            UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6").GetFontSprite(new RCIntVector(4, 4), new RCColor(0, 0, 255)).Save("c0_0_255_ps4_4.png");
        }

        static MyUIObject[] objList = new MyUIObject[1000];

        static DefaultRenderManager mgr = new DefaultRenderManager();
        static MyUIObject a = new MyUIObject("A");
        static MyUIObject b = new MyUIObject("B");
        static MyUIObject c = new MyUIObject("C");
        static MyUIObject d = new MyUIObject("D");
        static MyUIObject e = new MyUIObject("E");
        static MyUIObject f = new MyUIObject("F");
        static MyUIObject g = new MyUIObject("G");
        static MyUIObject h = new MyUIObject("H");
        static MyUIObject i = new MyUIObject("I");
        static MyUIObject j = new MyUIObject("J");
        static MyUIObject k = new MyUIObject("K");
        static MyUIObject l = new MyUIObject("L");
        static MyUIObject m = new MyUIObject("M");
        static MyUIObject n = new MyUIObject("N");
        static MyUIObject o = new MyUIObject("O");
        static MyUIObject p = new MyUIObject("P");
        static MyUIObject q = new MyUIObject("Q");
        static MyUIObject r = new MyUIObject("R");
        static MyUIObject s = new MyUIObject("S");
        static MyUIObject t = new MyUIObject("T");
    }

    class MyUIObject2 : UIObject
    {
        public MyUIObject2(): base(new RCIntVector(1, 1), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 100, 100))
        {
            mySprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.LoadSprite("source.png", new RCIntVector(10, 5));
            mySprite.TransparentColor = new RCColor(255, 0, 0);
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.mySprite, new RCIntVector(0, 0));//, new RCIntRectangle(3, 9, 3, 3));
        }

        private UISprite mySprite;
    }

    class MyUIObject : UIObject
    {
        public MyUIObject(string name) : base(new RCIntVector(1, 1), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 100, 100))
        {
            this.name = name;
        }

        public string PrintSubtree()
        {
            List<UIObject> dfsWalk = new List<UIObject>();
            this.WalkTreeDFS(ref dfsWalk);

            string retString = string.Empty;
            int i = 0;
            foreach (UIObject item in dfsWalk)
            {
                retString += item.ToString();
                if (i < dfsWalk.Count - 1)
                {
                    retString += "-";
                }
                i++;
            }
            return retString;
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            //Console.Write(this.name);
        }

        public override string ToString()
        {
            return this.name;
        }

        private string name;
    }
}
