using System;
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
            //RCNumRectangle rectangleA = new RCNumRectangle(-1, -1, 2, 2);
            //RCNumRectangle rectangleB = new RCNumRectangle(4, -1, 2, 2);
            //RCNumVector velocityA = new RCNumVector(3, 3);
            //RCNumVector velocityB = new RCNumVector(-3, 0);

            //RCNumber ttc = MotionController.CalculateTimeToCollision(rectangleA, velocityA, rectangleB, velocityB);

            //RCNumRectangle newRectA = rectangleA + velocityA * ttc;
            //RCNumRectangle newRectB = rectangleB + velocityB * ttc;

            //ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            ConfigurationManager.Initialize("..\\..\\..\\RC.App.Starter\\bin\\Debug\\RC.App.root");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
