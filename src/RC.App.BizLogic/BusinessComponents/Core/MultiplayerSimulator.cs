using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.App.BizLogic.BusinessComponents;
using RC.DssServices;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    class MultiplayerSimulator : ISimulator
    {
        public MultiplayerSimulator(IPlayerManager activeScenario)
        {
        }

        #region ISimulator methods

        /// <see cref="ISimulator.ExecuteNextFrame"/>
        bool ISimulator.ExecuteNextFrame(out RCPackage[] outgoingCmds)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.GuestCommand"/>
        void ISimulator.GuestCommand(int guestIndex, RCPackage command)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.GuestLeftDssDuringSim"/>
        void ISimulator.GuestLeftDssDuringSim(int guestIndex)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.HostCommand"/>
        void ISimulator.HostCommand(RCPackage command)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.HostLeftDssDuringSim"/>
        void ISimulator.HostLeftDssDuringSim()
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.SimulationError"/>
        void ISimulator.SimulationError(string reason, byte[] customData)
        {
            throw new NotImplementedException();
        }

        /// <see cref="ISimulator.StateHash"/>
        byte[] ISimulator.StateHash { get { throw new NotImplementedException(); } }

        #endregion ISimulator methods
    }
}
