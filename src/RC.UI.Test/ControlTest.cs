using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.Reflection;
using RC.Common;
using RC.UI;
using RC.Common.Diagnostics;

namespace RC.UI.Test
{
    class ControlTest
    {
        public static void Test()
        {
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            UIRoot root = new UIRoot();
            Assembly monoGamePlugin = Assembly.Load("RC.UI.MonoGamePlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(monoGamePlugin);
            root.InstallPlugins();

            DynamicString.RegisterResolver("RC.UI.UIWorkspace.PixelScaling",
                delegate()
                {
                    return "2;2";
                });
            UIResourceManager.LoadResourceGroup("RC.App.SplashScreen");
            UIResourceManager.LoadResourceGroup("RC.App.CommonResources");

            UISprite mouseIcon = root.GraphicsPlatform.SpriteManager.LoadSprite("../../../../sprites/pointers/normal_pointer.png", new RCIntVector(2, 2));
            mouseIcon.TransparentColor = new RCColor(255, 0, 255);
            mouseIcon.Upload();
            UIPointer pointer = new UIPointer(mouseIcon, new RCIntVector(0, 0));

            display = new TestUIObject(new RCIntVector(2, 2), new RCIntVector(0, 0), new RCIntRectangle(0, 0, 400, 300));
            workspace = new MySensitiveAnimObject(new RCIntVector(40, 50), new RCIntRectangle(0, 0, 320, 200),
                                              "Workspace", RCColor.Gray, RCColor.Gray);
            display.Attach(workspace);

            MyButton button = new MyButton(new RCIntVector(5, 12), new RCIntVector(60, 20),
                                           RCColor.Brown, RCColor.Yellow, RCColor.White,
                                           "MyButton");
            //button.MouseSensor.Enter += delegate(UISensitiveObject sender) { Console.WriteLine("BUTTON ENTER"); };
            //button.MouseSensor.Move += delegate(UISensitiveObject sender, UIMouseEventArgs args) { Console.WriteLine("BUTTON MOVE"); };
            //button.MouseSensor.Leave += delegate(UISensitiveObject sender) { Console.WriteLine("BUTTON LEAVE"); };
            MyCheckbox checkbox = new MyCheckbox(new RCIntVector(70, 12), new RCIntVector(80, 20),
                                                 RCColor.Red, RCColor.Green, RCColor.LightRed, RCColor.LightGreen, RCColor.White,
                                                 "MyCheckbox");
            MyDropdownSelector selector = new MyDropdownSelector(new RCIntVector(5, 50), new RCIntVector(60, 20),
                                                                 new string[4] { "option0", "option1", "option2", "option3" },
                                                                 RCColor.WhiteHigh, RCColor.Red,
                                                                 RCColor.Green, RCColor.LightGreen, RCColor.LightBlue, RCColor.Gray);
            //selector.MouseSensor.Enter += delegate(UISensitiveObject sender) { Console.WriteLine("SELECTOR ENTER"); };
            //selector.MouseSensor.Move += delegate(UISensitiveObject sender, UIMouseEventArgs args) { Console.WriteLine("SELECTOR MOVE"); };
            //selector.MouseSensor.Leave += delegate(UISensitiveObject sender) { Console.WriteLine("SELECTOR LEAVE"); };
            MySlider sliderHorz = new MySlider(new RCIntVector(5, 80), new RCIntVector(80, 10),
                                               new UISlider.Settings()
                                               {
                                                   Alignment = UISlider.Alignment.Horizontal,
                                                   IntervalLength = 10,
                                                   SliderBottom = 3,
                                                   SliderLeft = 1,
                                                   SliderRight = 1,
                                                   SliderTop = 3,
                                                   TimeBetweenTrackings = 300,
                                                   TrackingValueChange = 3,
                                                   TrackPos = new RCIntVector(10, 5),
                                                   TrackSize = new RCIntVector(60, 1)
                                               },
                                               RCColor.Green, RCColor.LightGreen, RCColor.White);
            MySlider sliderVert = new MySlider(new RCIntVector(5, 100), new RCIntVector(10, 80),
                                               new UISlider.Settings()
                                               {
                                                   Alignment = UISlider.Alignment.Vertical,
                                                   IntervalLength = 10,
                                                   SliderBottom = 1,
                                                   SliderLeft = 3,
                                                   SliderRight = 3,
                                                   SliderTop = 1,
                                                   TimeBetweenTrackings = 300,
                                                   TrackingValueChange = 3,
                                                   TrackPos = new RCIntVector(5, 10),
                                                   TrackSize = new RCIntVector(60, 1)
                                               },
                                               RCColor.Green, RCColor.LightGreen, RCColor.White);
            workspace.Attach(button);
            workspace.Attach(checkbox);
            workspace.Attach(selector);
            workspace.Attach(sliderHorz);
            workspace.Attach(sliderVert);
            workspace.AttachSensitive(button);
            workspace.AttachSensitive(checkbox);
            workspace.AttachSensitive(selector);
            workspace.AttachSensitive(sliderHorz);
            workspace.AttachSensitive(sliderVert);
            button.Pressed += delegate(UISensitiveObject sender)
            {
                TraceManager.WriteAllTrace("BUTTON PRESSED", TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
                checkbox.IsEnabled = !checkbox.IsEnabled;
            };
            checkbox.CheckedStateChanged += delegate(UISensitiveObject sender)
            {
                TraceManager.WriteAllTrace("CHECKBOX STATE CHANGED", TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
                button.IsEnabled = checkbox.IsChecked;
            };
            selector.SelectedIndexChanged += delegate(UISensitiveObject sender)
            {
                TraceManager.WriteAllTrace("SELECTED INDEX CHANGED", TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
            };
            sliderHorz.SelectedValueChanged += delegate(UISensitiveObject sender)
            {
                TraceManager.WriteAllTrace(string.Format("SELECTED VALUE CHANGED (horizontal): {0}", sliderHorz.SelectedValue), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
            };
            sliderVert.SelectedValueChanged += delegate(UISensitiveObject sender)
            {
                TraceManager.WriteAllTrace(string.Format("SELECTED VALUE CHANGED (vertical): {0}", sliderVert.SelectedValue), TraceManager.GetTraceFilterID("RC.UI.Test.Info"));
            };

            UIMouseManager mouseMgr = new UIMouseManager(workspace);
            mouseMgr.SetDefaultMousePointer(pointer);

            root.GraphicsPlatform.RenderManager.Attach(display);
            root.GraphicsPlatform.RenderLoop.Start(new RCIntVector(800, 600));

            root.Dispose();
        }

        private static TestUIObject display;
        private static MySensitiveObject workspace;
    }

    class MyButton : UIButton
    {
        public MyButton(RCIntVector position,
                        RCIntVector size,
                        RCColor basicColor,
                        RCColor highlightedColor,
                        RCColor disabledColor,
                        string text)
            : base(position, size)
        {
            this.textStr = new UIString(text, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), RCColor.Black);
            this.basicBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(basicColor, this.Range.Size - new RCIntVector(2, 2), new RCIntVector(2, 2));
            this.highlightedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(highlightedColor, this.Range.Size - new RCIntVector(2, 2), new RCIntVector(2, 2));
            this.disabledBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(disabledColor, this.Range.Size - new RCIntVector(2, 2), new RCIntVector(2, 2));

            this.basicBackground.Upload();
            this.highlightedBackground.Upload();
            this.disabledBackground.Upload();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.IsEnabled)
            {
                renderContext.RenderSprite(this.IsHighlighted ? this.highlightedBackground : this.basicBackground,
                                           this.IsPushed ? new RCIntVector(2, 2) : new RCIntVector(0, 0));
            }
            else
            {
                renderContext.RenderSprite(this.disabledBackground, new RCIntVector(0, 0));
            }

            renderContext.RenderString(this.textStr,
                                       this.IsPushed ? new RCIntVector(7, this.textStr.Font.CharTopMaximum + 7) : new RCIntVector(5, this.textStr.Font.CharTopMaximum + 5));
        }

        private UIString textStr;
        private UISprite basicBackground;
        private UISprite highlightedBackground;
        private UISprite disabledBackground;
    }

    class MyCheckbox : UICheckbox
    {
        public MyCheckbox(RCIntVector position,
                          RCIntVector size,
                          RCColor basicUncheckedColor,
                          RCColor basicCheckedColor,
                          RCColor highlightedUncheckedColor,
                          RCColor highlightedCheckedColor,
                          RCColor disabledColor,
                          string text)
            : base(position, size, true)
        {
            this.textStr = new UIString(text, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), RCColor.Black);
            this.basicUncheckedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(basicUncheckedColor, new RCIntVector(this.Range.Size.Y - 2, this.Range.Size.Y - 2), new RCIntVector(2, 2));
            this.basicCheckedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(basicCheckedColor, new RCIntVector(this.Range.Size.Y - 2, this.Range.Size.Y - 2), new RCIntVector(2, 2));
            this.highlightedUncheckedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(highlightedUncheckedColor, new RCIntVector(this.Range.Size.Y - 2, this.Range.Size.Y - 2), new RCIntVector(2, 2));
            this.highlightedCheckedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(highlightedCheckedColor, new RCIntVector(this.Range.Size.Y - 2, this.Range.Size.Y - 2), new RCIntVector(2, 2));
            this.disabledBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(disabledColor, new RCIntVector(this.Range.Size.Y - 2, this.Range.Size.Y - 2), new RCIntVector(2, 2));

            this.basicUncheckedBackground.Upload();
            this.basicCheckedBackground.Upload();
            this.highlightedUncheckedBackground.Upload();
            this.highlightedCheckedBackground.Upload();
            this.disabledBackground.Upload();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.IsEnabled)
            {
                if (this.IsHighlighted)
                {
                    renderContext.RenderSprite(this.IsChecked ? this.highlightedCheckedBackground : this.highlightedUncheckedBackground,
                                               this.IsPushed ? new RCIntVector(2, 2) : new RCIntVector(0, 0));
                }
                else
                {
                    renderContext.RenderSprite(this.IsChecked ? this.basicCheckedBackground : this.basicUncheckedBackground,
                                               this.IsPushed ? new RCIntVector(2, 2) : new RCIntVector(0, 0));
                }
            }
            else
            {
                renderContext.RenderSprite(this.disabledBackground, new RCIntVector(0, 0));
            }

            renderContext.RenderString(this.textStr,
                                       new RCIntVector(this.Range.Size.Y, this.textStr.Font.CharTopMaximum + 5));
        }

        private UIString textStr;
        private UISprite basicUncheckedBackground;
        private UISprite basicCheckedBackground;
        private UISprite highlightedUncheckedBackground;
        private UISprite highlightedCheckedBackground;
        private UISprite disabledBackground;
    }

    class MyDropdownSelector : UIDropdownSelector
    {
        public MyDropdownSelector(RCIntVector position, RCIntVector size, string[] options,
                                  RCColor basicTextColor, RCColor highlightedTextColor,
                                  RCColor basicBackground, RCColor highlightedBackground,
                                  RCColor optListBackground, RCColor disabledBackground)
            : base(position, size, options.Length)
        {
            this.basicOptions = new UIString[options.Length];
            this.highlightedOptions = new UIString[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                this.basicOptions[i] = new UIString(options[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), basicTextColor);
                this.highlightedOptions[i] = new UIString(options[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6"), new RCIntVector(2, 2), highlightedTextColor);
            }

            this.basicBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(basicBackground, new RCIntVector(size.X, size.Y), new RCIntVector(2, 2));
            this.highlightedBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(highlightedBackground, new RCIntVector(size.X, size.Y), new RCIntVector(2, 2));
            this.optListBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(optListBackground, new RCIntVector(size.X, size.Y * this.basicOptions.Length), new RCIntVector(2, 2));
            this.disabledBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(disabledBackground, new RCIntVector(size.X, size.Y), new RCIntVector(2, 2));

            this.basicBackground.Upload();
            this.highlightedBackground.Upload();
            this.optListBackground.Upload();
            this.disabledBackground.Upload();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.IsEnabled)
            {
                if (this.CurrentStatus == Status.Normal || this.CurrentStatus == Status.Highlighted)
                {
                    renderContext.RenderSprite(this.CurrentStatus == Status.Normal ? this.basicBackground : this.highlightedBackground, new RCIntVector(0, 0));
                    renderContext.RenderString(this.CurrentStatus == Status.Normal ? this.basicOptions[this.SelectedIndex] : this.highlightedOptions[this.SelectedIndex], new RCIntVector(5, this.basicOptions[this.SelectedIndex].Font.CharTopMaximum + 5));
                }
                else
                {
                    renderContext.RenderSprite(this.highlightedBackground, new RCIntVector(0, 0));
                    renderContext.RenderString(this.highlightedOptions[this.SelectedIndex], new RCIntVector(5, this.highlightedOptions[this.SelectedIndex].Font.CharTopMaximum + 5));

                    renderContext.RenderSprite(this.optListBackground, new RCIntVector(this.OptionListRect.Location));
                    for (int i = 0; i < this.basicOptions.Length; i++)
                    {
                        renderContext.RenderString(i == this.HighlightedIndex ? this.highlightedOptions[i] : this.basicOptions[i],
                                                   new RCIntVector(5, (i + 1) * this.NormalRect.Height + this.basicOptions[this.SelectedIndex].Font.CharTopMaximum + 5));
                    }
                }
            }
            else
            {
                renderContext.RenderSprite(this.disabledBackground, new RCIntVector(0, 0));
                renderContext.RenderString(this.basicOptions[this.SelectedIndex], new RCIntVector(5, this.basicOptions[this.SelectedIndex].Font.CharTopMaximum + 5));
            }

        }

        private UIString[] basicOptions;
        private UIString[] highlightedOptions;
        private UISprite basicBackground;
        private UISprite highlightedBackground;
        private UISprite optListBackground;
        private UISprite disabledBackground;
    }

    class MySlider : UISlider
    {
        public MySlider(RCIntVector position, RCIntVector size, Settings settings,
                        RCColor trackColor, RCColor sliderColor, RCColor disabledColor)
            : base(position, size, settings)
        {
            this.trackBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(trackColor, settings.Alignment == Alignment.Horizontal ? new RCIntVector(settings.TrackSize.X, settings.TrackSize.Y * 2 + 1) : new RCIntVector(settings.TrackSize.Y * 2 + 1, settings.TrackSize.X), new RCIntVector(2, 2));
            this.sliderBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(sliderColor, new RCIntVector(settings.SliderLeft + settings.SliderRight + 1, settings.SliderTop + settings.SliderBottom + 1), new RCIntVector(2, 2));
            this.disabledTrackBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(disabledColor, settings.Alignment == Alignment.Horizontal ? new RCIntVector(settings.TrackSize.X, settings.TrackSize.Y * 2 + 1) : new RCIntVector(settings.TrackSize.Y * 2 + 1, settings.TrackSize.X), new RCIntVector(2, 2));
            this.disabledSliderBackground = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(disabledColor, new RCIntVector(settings.SliderLeft + settings.SliderRight + 1, settings.SliderTop + settings.SliderBottom + 1), new RCIntVector(2, 2));

            this.trackBackground.Upload();
            this.sliderBackground.Upload();
            this.disabledTrackBackground.Upload();
            this.disabledSliderBackground.Upload();
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.IsEnabled)
            {
                renderContext.RenderSprite(this.trackBackground, this.TrackRectangle.Location);
                renderContext.RenderSprite(this.sliderBackground, this.SliderRectangle.Location);
            }
            else
            {
                renderContext.RenderSprite(this.disabledTrackBackground, this.TrackRectangle.Location);
                renderContext.RenderSprite(this.disabledSliderBackground, this.SliderRectangle.Location);
            }

        }

        private UISprite trackBackground;
        private UISprite sliderBackground;
        private UISprite disabledTrackBackground;
        private UISprite disabledSliderBackground;
    }
}
