using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Marshals invocations from a background task to the UI thread synchronously.
    /// </summary>
    class Marshal: TriggeredScheduler
    {
        /// <summary>
        /// Constructs a Marshal instance.
        /// </summary>
        public Marshal() : base(0)
        {
            this.AddScheduledFunction(this.Execute);
        }

        public TReturn Invoke(Func<TReturn> func)
        {
            this.Trigger();
        }

        private void Execute()
        {

        }
    }
}
