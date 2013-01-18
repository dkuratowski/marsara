using System;
using System.Windows.Forms;
using RC.Common.Diagnostics;
using RC.Common.Configuration;

namespace RC.DssServices.Test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigurationManager.Initialize("../../../../config/RC.DssServices.Test/RC.DssServices.Test.root");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
