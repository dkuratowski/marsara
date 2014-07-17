using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Constants defined for reading and writing command workflow XML definitions.
    /// </summary>
    class XmlCommandWorkflowConstants
    {
        public const string SPRITEPALETTE_ELEM = "spritePalette";
        public const string COMMANDWORKFLOW_ELEM = "commandWorkflow";
        public const string COMMANDWORKFLOW_NAME_ATTR = "name";
        public const string COMMANDWORKFLOW_ACTIVATOR_ELEM = "activator";
        public const string SHOWBUTTON_ELEM = "showButton";
        public const string SHOWBUTTON_SPRITE_ATTR = "sprite";
        public const string SHOWBUTTON_PANELPOSITION_ATTR = "panelPosition";
        public const string CANCELWORKFLOW_ELEM = "cancelWorkflow";
        public const string FINISHWORKFLOW_ELEM = "finishWorkflow";
        public const string SELECTPOSITION_ELEM = "selectPosition";
        public const string SELECTBUILDPOSITION_ELEM = "selectBuildPosition";
        public const string SELECTTARGETTYPE_ELEM = "selectTargetType";
        public const string SELECTTARGETTYPE_TYPE_ATTR = "type";
    }
}
