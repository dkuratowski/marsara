using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace RC.App.Starter
{
    /// <summary>
    /// Helper class for showing or hiding the console window. By default it is hidden.
    /// </summary>
    static class ConsoleHelper
    {
        /// <summary>
        /// Hides the console window.
        /// </summary>
        public static void HideConsole()
        {
            if (!isConsoleHidden)
            {
                IntPtr consoleHdl = GetConsoleWindow();
                ShowWindow(consoleHdl, SW_HIDE);
                isConsoleHidden = true;
            }
        }

        /// <summary>
        /// Shows the console window.
        /// </summary>
        public static void ShowConsole()
        {
            if (isConsoleHidden)
            {
                IntPtr consoleHdl = GetConsoleWindow();
                ShowWindow(consoleHdl, SW_SHOW);
                isConsoleHidden = false;
            }
        }

        /// <summary>
        /// Gets whether the console window is hidden or not.
        /// </summary>
        public static bool IsConsoleHidden { get { return isConsoleHidden; } }

        /// <summary>
        /// Internal Win32 API method.
        /// </summary>
        [DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// Internal Win32 API method.
        /// </summary>
        [DllImport("user32")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        /// <summary>
        /// Internal Win32 API parameters.
        /// </summary>
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_NORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_MAX = 10;

        /// <summary>
        /// True if the console window is hidden, false otherwise.
        /// </summary>
        private static bool isConsoleHidden = false;
    }
}
