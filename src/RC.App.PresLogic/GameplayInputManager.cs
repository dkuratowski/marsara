using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Manager class for handling all input event during gameplay.
    /// </summary>
    class GameplayInputManager
    {
        /// <summary>
        /// Constructs a GameplayInputManager instance.
        /// </summary>
        public GameplayInputManager()
        {
            this.inputHandlers = new Dictionary<string, InputHandler>();
            this.inputHandlerSet = new HashSet<InputHandler>();
        }

        /// <summary>
        /// Starts and adds the given input handler to the manager.
        /// </summary>
        /// <param name="handlerName">The name of the handler to start and add.</param>
        /// <param name="handler">The handler to start and add.</param>
        public void StartAndAddInputHandler(string handlerName, InputHandler handler)
        {
            if (handlerName == null) { throw new ArgumentNullException("handlerName"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }
            if (this.inputHandlerSet.Contains(handler)) { throw new InvalidOperationException("Input handler already added!"); }
            if (this.inputHandlers.ContainsKey(handlerName)) { throw new InvalidOperationException(string.Format("Input handler with name '{0}' already added!", handlerName)); }
            if (this.inputHandlerSet.Any(hdl => hdl.State != InputHandler.StateEnum.StandBy)) { throw new InvalidOperationException("Input handler can only be added when every other input handlers are standing by!"); }

            handler.Start();
            handler.ProcessingStarted += this.OnHandlerProcessingEvt;
            handler.ProcessingFinished += this.OnHandlerProcessingEvt;
            this.inputHandlers.Add(handlerName, handler);
            this.inputHandlerSet.Add(handler);
        }

        /// <summary>
        /// Stops and removes the given input handler.
        /// </summary>
        /// <param name="handlerName">The name of the handler to stop and remove.</param>
        public void StopAndRemoveInputHandler(string handlerName)
        {
            if (handlerName == null) { throw new ArgumentNullException("handlerName"); }
            if (!this.inputHandlers.ContainsKey(handlerName)) { throw new InvalidOperationException(string.Format("Input handler with name '{0}' doesn't exist!", handlerName)); }
            if (this.inputHandlerSet.Any(hdl => hdl.State != InputHandler.StateEnum.StandBy)) { throw new InvalidOperationException("Input handler can only be removed when every other input handlers are standing by!"); }

            InputHandler handlerToRemove = this.inputHandlers[handlerName];
            this.inputHandlers.Remove(handlerName);
            this.inputHandlerSet.Remove(handlerToRemove);
            handlerToRemove.ProcessingStarted -= this.OnHandlerProcessingEvt;
            handlerToRemove.ProcessingFinished -= this.OnHandlerProcessingEvt;
            handlerToRemove.Stop();
        }

        /// <summary>
        /// This event is raised when one of the input handlers has started or stopped input event processing.
        /// </summary>
        private void OnHandlerProcessingEvt(object sender, EventArgs args)
        {
            InputHandler handler = sender as InputHandler;
            if (handler == null) { throw new ArgumentException("sender"); }
            foreach (InputHandler otherHandler in this.inputHandlerSet.Where(item => item != handler))
            {
                if (handler.State == InputHandler.StateEnum.Processing)
                {
                    /// In case of processing start, we have to stop any other handlers.
                    otherHandler.Stop();
                }
                else if (handler.State == InputHandler.StateEnum.StandBy)
                {
                    /// In case of processing finished, we have to start other handlers.
                    otherHandler.Start();
                }
                else
                {
                    /// Unexpected case.
                    throw new InvalidOperationException("Invalid handler state!");
                }
            }
        }

        /// <summary>
        /// List of the registered input handlers mapped by their name.
        /// </summary>
        private Dictionary<string, InputHandler> inputHandlers;

        /// <summary>
        /// List of the registered input handlers.
        /// </summary>
        private HashSet<InputHandler> inputHandlerSet;
    }
}
