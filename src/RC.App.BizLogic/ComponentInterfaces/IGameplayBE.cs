using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.App.BizLogic.PublicInterfaces;

namespace RC.App.BizLogic.ComponentInterfaces
{
    /// <summary>
    /// Interface to the gameplay backend component. The Gameplay page of the UI communicates directly with this interface.
    /// </summary>
    [ComponentInterface]
    public interface IGameplayBE
    {
        /// <summary>
        /// Temporary method for testing.
        /// </summary>
        /// TODO: remove this method when no longer necessary.
        void StartTestScenario();

        /// <summary>
        /// Temporary method for testing.
        /// </summary>
        /// TODO: remove this method when no longer necessary.
        void StopTestScenario();
    }
}
