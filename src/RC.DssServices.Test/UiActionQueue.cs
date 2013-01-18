using System;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// Enumerates the possible actions coming from the UI to the DSS-thread.
    /// </summary>
    enum UiActionType
    {
        OpenBtnPressed = 0,         /// (operatorID)
        CloseBtnPressed = 1,        /// (operatorID)
        NewColorSelected = 2,       /// (operatorID, newColor)
        StartSimBtnPressed = 3,
        UpKeyPressed = 4,
        DownKeyPressed = 5,
        LeftKeyPressed = 6,
        RightKeyPressed = 7,
        LeaveBtnPressed = 8
    }

    /// <summary>
    /// If an action is performed by the user on the UI, this action must be sent to the DSS-thread for processing
    /// using this class.
    /// </summary>
    class UiActionQueue
    {
        /// <summary>
        /// Contructs a UiActionQueue object.
        /// </summary>
        public UiActionQueue()
        {
            this.actionFifo = new Fifo<UiActionType>(1024);
            this.param0Fifo = new Fifo<int>(1024);
            this.param1Fifo = new Fifo<int>(1024);
        }

        /// <summary>
        /// Posts an action to the UiActionQueue. This function can be called from the context of the UI-thread.
        /// </summary>
        /// <param name="uiAction">The action to post.</param>
        /// <param name="param0">The first parameter of the action.</param>
        /// <param name="param1">The second parameter of the action.</param>
        public void PostAction(UiActionType uiAction, int param0, int param1)
        {
            lock (this.actionFifo)
            {
                this.actionFifo.Push(uiAction);
                this.param0Fifo.Push(param0);
                this.param1Fifo.Push(param1);
            }
        }

        /// <summary>
        /// Empties the action queue and retrieves every actions and their argument.
        /// </summary>
        /// <param name="uiActions">List of the retrieved actions.</param>
        /// <param name="firstParams">List of the first parameter of the actions.</param>
        /// <param name="secondParams">List of the second parameter of the actions.</param>
        public void GetAllActions(out UiActionType[] uiActions, out int[] firstParams, out int[] secondParams)
        {
            uiActions = null;
            firstParams = null;
            secondParams = null;

            lock (this.actionFifo)
            {
                if (this.actionFifo.Length == this.param0Fifo.Length && this.actionFifo.Length == this.param1Fifo.Length)
                {
                    uiActions = new UiActionType[this.actionFifo.Length];
                    firstParams = new int[this.param0Fifo.Length];
                    secondParams = new int[this.param1Fifo.Length];

                    for (int i = 0; i < uiActions.Length; i++)
                    {
                        uiActions[i] = this.actionFifo.Get();
                        firstParams[i] = this.param0Fifo.Get();
                        secondParams[i] = this.param1Fifo.Get();
                    }
                }
                else
                {
                    throw new Exception("Inconsistent state in UiActionQueue!");
                }
            }
        }
        
        /// <summary>
        /// The FIFO that stores the action arrived from the UI.
        /// </summary>
        private Fifo<UiActionType> actionFifo;

        /// <summary>
        /// The FIFO that stores the first parameter of the actions.
        /// </summary>
        private Fifo<int> param0Fifo;

        /// <summary>
        /// The FIFO that stores the second parameter of the actions.
        /// </summary>
        private Fifo<int> param1Fifo;
    }
}
