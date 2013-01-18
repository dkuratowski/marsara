using System;

namespace RC.DssServices
{
    /// <summary>
    /// This class is responsible for monitoring the answers for the previously sent commits.
    /// </summary>
    class CommitMonitor
    {
        /// <summary>
        /// Creates a CommitMonitor object with the given ticket.
        /// </summary>
        /// <param name="ticket">The ticket of the commit you want to monitor.</param>
        /// <param name="answerTimeoutClock">The clock that is used to measure commit answer timeout for this commit.</param>
        /// <param name="localOpID">The ID of the local operator.</param>
        /// <param name="opCount">The number of operators in the DSS.</param>
        public CommitMonitor(int ticket, AlarmClock answerTimeoutClock, int localOpID, int opCount, bool[] opFlags)
        {
            if (opCount <= 0) { throw new ArgumentOutOfRangeException("opCount"); }
            if (localOpID < 0 || localOpID >= opCount) { throw new ArgumentOutOfRangeException("localOpID"); }
            if (answerTimeoutClock == null) { throw new ArgumentNullException("answerTimeoutClock"); }
            if (opFlags == null || opFlags.Length != opCount) { throw new ArgumentException("opFlags"); }

            this.commitTimeStamp = DssRoot.Time;
            this.commitAnswered = false;
            this.ticket = ticket;
            this.commitAwTimeoutClock = answerTimeoutClock;
            this.localOperatorID = localOpID;
            this.operatorFlags = opFlags;

            this.answerFlags = new bool[opCount];
            for (int i = 0; i < opCount; i++)
            {
                this.answerFlags[i] = (i == this.localOperatorID);
            }

            Refresh();
        }

        /// <summary>
        /// Call this function when a commit answer has arrived from an operator.
        /// </summary>
        /// <param name="senderOp">The ID of the operator who sent the answer.</param>
        /// <param name="pingTime">The measured ping time of the answer.</param>
        /// <returns>
        /// False if the commit has already been answered by the given operator, true otherwise.
        /// </returns>
        public bool AnswerArrived(int senderOp, out int pingTime)
        {
            if (senderOp < 0 || senderOp >= this.answerFlags.Length) { throw new ArgumentOutOfRangeException("senderOp"); }

            if (!this.answerFlags[senderOp])
            {
                this.answerFlags[senderOp] = true;
                Refresh();

                pingTime = DssRoot.Time - this.commitTimeStamp;
                return true;
            }
            else
            {
                pingTime = -1;
                return false;
            }
        }

        /// <summary>
        /// Call this function for each CommitMonitor when the operator or answer flags have been changed.
        /// </summary>
        public void Refresh()
        {
            bool nonAnsweredOpFound = false;
            for (int i = 0; i < this.answerFlags.Length; i++)
            {
                if (this.operatorFlags[i] && !this.answerFlags[i])
                {
                    nonAnsweredOpFound = true;
                    break;
                }
            }

            if (!nonAnsweredOpFound)
            {
                /// The commit has been successfully answered by every operator
                this.commitAnswered = true;
            }
        }

        /// <summary>
        /// Gets the ticket of this commit.
        /// </summary>
        public int Ticket { get { return this.ticket; } }

        /// <summary>
        /// Gets whether the commit has been answered by the other operators or not.
        /// </summary>
        public bool IsCommitAnswered { get { return this.commitAnswered; } }

        /// <summary>
        /// Gets the alarm clock that measures the commit answer timeout for this commit.
        /// </summary>
        public AlarmClock CommitAwTimeoutClock { get { return this.commitAwTimeoutClock; } }

        /// <summary>
        /// Gets the local time when the commit has been sent.
        /// </summary>
        public int TimeStamp { get { return this.commitTimeStamp; } }

        /// <summary>
        /// The ticket of the commit you want to monitor.
        /// </summary>
        private int ticket;

        /// <summary>
        /// This clock is used to measure commit answer timeout for this commit.
        /// </summary>
        private AlarmClock commitAwTimeoutClock;

        /// <summary>
        /// The ID of the local operator.
        /// </summary>
        private int localOperatorID;

        /// <summary>
        /// These flags indicate the operators who have already sent an answer for this commit.
        /// </summary>
        private bool[] answerFlags;

        /// <summary>
        /// These flags indicate which operators are participating in the simulation and which aren't.
        /// See DssSimulationMgr.operatorFlags for more informations.
        /// </summary>
        private bool[] operatorFlags;

        /// <summary>
        /// This flag becomes true if every operator has answered this commit.
        /// </summary>
        private bool commitAnswered;

        /// <summary>
        /// The local time when the commit has been sent.
        /// </summary>
        private int commitTimeStamp;
    }
}
