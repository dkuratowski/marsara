using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// A mocked graphics platform used for testing.
    /// </summary>
    class MonoGameMockGraphicsPlatform : UIGraphicsPlatformBase
    {
        /// <summary>
        /// Constructs a MonoGameMockGraphicsPlatform object.
        /// </summary>
        public MonoGameMockGraphicsPlatform()
        {
        }

        #region UIGraphicsPlatformBase members

        /// <see cref="UIGraphicsPlatformBase.CreateRenderLoop_i"/>
        protected override UIRenderLoopBase CreateRenderLoop_i()
        {
            throw new NotImplementedException();
        }

        /// <see cref="UIGraphicsPlatformBase.CreateSpriteManager_i"/>
        protected override UISpriteManagerBase CreateSpriteManager_i()
        {
            throw new NotImplementedException();
        }

        #endregion UIGraphicsPlatformBase members
    }
}
