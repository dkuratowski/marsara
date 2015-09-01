using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Views
{
    /// <summary>
    /// Enumerates the possible conditions of a map object.
    /// </summary>
    public enum MapObjectConditionEnum
    {
        Undefined = -1,     // The conditions of the map object is undefined.
        Excellent = 0,      // The map object has excellent conditions.
        Moderate = 1,       // The map object has moderate conditions.
        Critical = 2        // The map object has critical conditions.
    }
}
