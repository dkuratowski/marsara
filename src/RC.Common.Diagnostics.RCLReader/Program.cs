using System;
using System.Windows.Forms;
using RC.Common.Configuration;

namespace RC.Common.Diagnostics.RCLReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigurationManager.Initialize("../../../../config/RC.Common.Diagnostics.RCLReader/RC.Common.Diagnostics.RCLReader.root");

            EVENT_FORMAT = RCPackageFormatMap.Get("RC.Common.RclEvent");
            FORK_FORMAT = RCPackageFormatMap.Get("RC.Common.RclFork");
            JOIN_FORMAT = RCPackageFormatMap.Get("RC.Common.RclJoin");
            EXCEPTION_FORMAT = RCPackageFormatMap.Get("RC.Common.RclException");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm());
        }

        public static int EVENT_FORMAT;

        public static int FORK_FORMAT;

        public static int JOIN_FORMAT;

        public static int EXCEPTION_FORMAT;
    }
}
