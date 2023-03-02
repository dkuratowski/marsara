using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Common.Configuration;
using RC.Common;

namespace RC.UI.Test
{
    class WorkspaceTest
    {
        public static void Test()
        {
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            UIRoot root = new UIRoot();
            Assembly monoGamePlugin = Assembly.Load("RC.UI.MonoGamePlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(monoGamePlugin);
            root.InstallPlugins();

            UIWorkspace workspace = new UIWorkspace(new RCIntVector(740, 500), new RCIntVector(320, 200));

            pageA = new UIPage();
            pageB = new UIPage();

            panelAA = new MyPanel(new RCIntRectangle(5, 5, 100, 90), new RCIntRectangle(5, 5, 90, 80), UIPanel.ShowMode.DriftFromTop, UIPanel.HideMode.DriftToLeft);
            panelAB = new MyPanel(new RCIntRectangle(5, 100, 100, 90), new RCIntRectangle(5, 5, 90, 80), UIPanel.ShowMode.DriftFromBottom, UIPanel.HideMode.DriftToLeft);
            panelAC = new MyPanel(new RCIntRectangle(110, 5, 200, 90), new RCIntRectangle(5, 5, 190, 80), UIPanel.ShowMode.DriftFromTop, UIPanel.HideMode.DriftToRight);
            panelAD = new MyPanel(new RCIntRectangle(110, 100, 200, 90), new RCIntRectangle(5, 5, 190, 80), UIPanel.ShowMode.DriftFromBottom, UIPanel.HideMode.DriftToRight);

            panelBA = new MyPanel(new RCIntRectangle(5, 5, 100, 90), new RCIntRectangle(5, 5, 90, 80), UIPanel.ShowMode.DriftFromLeft, UIPanel.HideMode.DriftToTop);
            panelBB = new MyPanel(new RCIntRectangle(5, 100, 100, 90), new RCIntRectangle(5, 5, 90, 80), UIPanel.ShowMode.DriftFromLeft, UIPanel.HideMode.DriftToBottom);
            panelBC = new MyPanel(new RCIntRectangle(110, 5, 200, 90), new RCIntRectangle(5, 5, 190, 80), UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToTop);
            panelBD = new MyPanel(new RCIntRectangle(110, 100, 200, 90), new RCIntRectangle(5, 5, 190, 80), UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToBottom);

            workspace.RegisterPage(pageA);
            workspace.RegisterPage(pageB);
            pageA.RegisterPanel(panelAA);
            pageA.RegisterPanel(panelAB);
            pageA.RegisterPanel(panelAC);
            pageA.RegisterPanel(panelAD);
            pageB.RegisterPanel(panelBA);
            pageB.RegisterPanel(panelBB);
            pageB.RegisterPanel(panelBC);
            pageB.RegisterPanel(panelBD);

            pageB.Activate();
            panelBA.Show();
            panelBB.Show();
            panelBC.Show();
            panelBD.Show();

            pageA.StatusChanged += OnPageStatusChanged;
            pageB.StatusChanged += OnPageStatusChanged;
            pageB.Deactivate();

            root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
            root.Dispose();
        }

        static UIPage pageA;
        static MyPanel panelAA;
        static MyPanel panelAB;
        static MyPanel panelAC;
        static MyPanel panelAD;
        static UIPage pageB;
        static MyPanel panelBA;
        static MyPanel panelBB;
        static MyPanel panelBC;
        static MyPanel panelBD;

        static void OnPageStatusChanged(UIPage sender, UIPage.Status newState)
        {
            if (newState == UIPage.Status.Inactive)
            {
                if (sender == pageA)
                {
                    pageB.Activate();
                    panelBA.Show();
                    panelBB.Show();
                    panelBC.Show();
                    panelBD.Show();
                    pageB.Deactivate();
                }
                else if (sender == pageB)
                {
                    pageA.Activate();
                    panelAA.Show();
                    panelAB.Show();
                    panelAC.Show();
                    panelAD.Show();
                    pageA.Deactivate();
                }
            }
        }
    }

    class MyPanel : UIPanel
    {
        public MyPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect, ShowMode showMode, HideMode hideMode)
            : base(backgroundRect, contentRect, showMode, hideMode, 300, 300)
        {
            this.backgroundSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Green, this.Range.Size, UIWorkspace.Instance.PixelScaling);
            this.backgroundSprite.Upload();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.backgroundSprite, this.Range.Location);
        }

        private UISprite backgroundSprite;
    }
}
