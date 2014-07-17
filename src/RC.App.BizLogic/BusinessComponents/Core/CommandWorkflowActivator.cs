using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    abstract class CommandWorkflowActivator
    {
    }

    class UniversalQuantorWA : CommandWorkflowActivator { }
    class ExistentialQuantorWA : CommandWorkflowActivator { }
    class IndividualCommandWA : CommandWorkflowActivator { }
}
