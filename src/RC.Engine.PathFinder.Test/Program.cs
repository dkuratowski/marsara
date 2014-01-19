using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RC.Common.Configuration;

namespace RC.Engine.PathFinder.Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            ConfigurationManager.Initialize("..\\..\\..\\RC.App.Starter\\bin\\Debug\\RC.App.root");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
