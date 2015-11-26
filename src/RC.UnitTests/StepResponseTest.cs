using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.App.PresLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UnitTests
{
    /// <summary>
    /// Implements test cases for testing StepResponse class.
    /// </summary>    
    [TestClass]
    public class StepResponseTest
    {
        /// <summary>
        /// Contains test context informations.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Test for debugging
        /// </summary>
        [TestMethod]
        public void StepResponseDebugging()
        {
            StepResponse stepResponse = new StepResponse(5000);
            for (int t = 0; t < 3500; t++)
            {
                int stepResponseValue = stepResponse.GetValue(t);
                TestContext.WriteLine("StepResponse.GetValue({0}) = {1}", t, stepResponseValue);
            }
        }
    }
}
