using RC.Common.Configuration;
using RC.Common.Diagnostics;

namespace RC.DssServices.Test
{
    static class TestClientMessages
    {
        /// <summary>
        /// Defines the RCPackageFormats of the test program.
        /// </summary>
        static TestClientMessages()
        {
            RESET = RCPackageFormatMap.Get("RC.DssServices.Test.Reset");
            COLOR_CHANGE_REQUEST = RCPackageFormatMap.Get("RC.DssServices.Test.ColorChangeRequest");
            COLOR_CHANGE_NOTIFICATION = RCPackageFormatMap.Get("RC.DssServices.Test.ColorChangeNotification");
            COMMAND = RCPackageFormatMap.Get("RC.DssServices.Test.Command");
        }

        /// <summary>
        /// The RCPackageFormats of the test program.
        /// </summary>
        public static readonly int RESET;
        public static readonly int COLOR_CHANGE_REQUEST;
        public static readonly int COLOR_CHANGE_NOTIFICATION;
        public static readonly int COMMAND;
    }

    static class TestClientTraceFilters
    {
        public static readonly int TEST_INFO = TraceManager.GetTraceFilterID("RC.DssServices.Test.Info");
    }
}
