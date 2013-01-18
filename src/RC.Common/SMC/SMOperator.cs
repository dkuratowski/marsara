using System;
using System.Collections.Generic;

namespace RC.Common.SMC
{
    /// <summary>
    /// Enumerates the possible types of an SMOperator.
    /// </summary>
    public enum SMOperatorType
    {
        AND = 0,    /// Logical AND operator
        OR = 1,     /// Logical OR operator
        NOT = 2     /// Logical NOT operator
    }

    /// <summary>
    /// Base class of operators between SMStates that are used to define complex conditions for firing 
    /// internal triggers.
    /// </summary>
    public sealed class SMOperator
    {
        /// <summary>
        /// Constructs an operator with the given included states and other operators.
        /// </summary>
        public SMOperator(SMOperatorType operatorType, ISMState[] includedStates, SMOperator[] includedOperators)
        {
            Init(operatorType, includedStates, includedOperators);
        }

        /// <summary>
        /// Constructs an operator with the given included states.
        /// </summary>
        public SMOperator(SMOperatorType operatorType, ISMState[] includedStates)
        {
            Init(operatorType, includedStates, null);
        }

        /// <summary>
        /// Constructs an operator with the given included operators.
        /// </summary>
        public SMOperator(SMOperatorType operatorType, SMOperator[] includedOperators)
        {
            Init(operatorType, null, includedOperators);
        }

        /// <summary>
        /// Evaluates this operator on the given states.
        /// </summary>
        /// <param name="statesToCheck">The states to evaluate the operator.</param>
        /// <returns>True if the operator is satisfied, false otherwise.</returns>
        public bool Evaluate(Dictionary<ISMState, bool> statesToCheck)
        {
            if (statesToCheck == null) { throw new ArgumentNullException("statesToCheck"); }

            if (SMOperatorType.AND == this.operatorType)
            {
                foreach (ISMState state in this.includedStates)
                {
                    if (statesToCheck.ContainsKey(state))
                    {
                        if (!statesToCheck[state])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new SMException("The state '" + state.Name + "' was not found in statesToCheck!");
                    }
                }

                foreach (SMOperator op in this.includedOperators)
                {
                    if (!op.Evaluate(statesToCheck)) { return false; }
                }
                return true;
            }
            else if (SMOperatorType.OR == this.operatorType)
            {
                foreach (ISMState state in this.includedStates)
                {
                    if (statesToCheck.ContainsKey(state))
                    {
                        if (statesToCheck[state])
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new SMException("The state '" + state.Name + "' was not found in statesToCheck!");
                    }
                }

                foreach (SMOperator op in this.includedOperators)
                {
                    if (op.Evaluate(statesToCheck)) { return true; }
                }
                return false;
            }
            else if (SMOperatorType.NOT == this.operatorType)
            {
                if (this.includedStates.Count != 0)
                {
                    foreach (ISMState state in this.includedStates)
                    {
                        if (statesToCheck.ContainsKey(state))
                        {
                            return !statesToCheck[state];
                        }
                        else
                        {
                            throw new SMException("The state '" + state.Name + "' was not found in statesToCheck!");
                        }
                    }
                    return false;
                }
                else if (this.includedOperators.Count != 0)
                {
                    foreach (SMOperator op in this.includedOperators)
                    {
                        return !op.Evaluate(statesToCheck);
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new SMException("Unexpected SMOperatorType!");
            }
        }

        /// <summary>
        /// Collects all included states from this operator and the included operators.
        /// </summary>
        /// <param name="states">This set will contain the states.</param>
        public void CollectAllStates(ref HashSet<ISMState> states)
        {
            if (states == null) { throw new ArgumentNullException("states"); }

            foreach (SMOperator op in this.includedOperators)
            {
                op.CollectAllStates(ref states);
            }

            foreach (ISMState state in this.includedStates)
            {
                states.Add(state);
            }
        }

        /// <summary>
        /// Internal initialization method.
        /// </summary>
        private void Init(SMOperatorType operatorType, ISMState[] inclStates, SMOperator[] inclOperators)
        {
            this.operatorType = operatorType;

            int numOfOperands = 0;

            this.includedStates = new HashSet<ISMState>();
            if (null != inclStates)
            {
                foreach (ISMState state in inclStates)
                {
                    if (!this.includedStates.Add(state)) { throw new SMException("Duplicated operands!"); }
                }
                numOfOperands += this.includedStates.Count;
            }
            this.includedOperators = new HashSet<SMOperator>();
            if (null != inclOperators)
            {
                foreach (SMOperator op in inclOperators)
                {
                    if (!this.includedOperators.Add(op)) { throw new SMException("Duplicated operands!"); }
                }
                numOfOperands += this.includedOperators.Count;
            }

            if (this.operatorType == SMOperatorType.NOT)
            {
                if (numOfOperands != 1)
                {
                    throw new SMException("A logical NOT operator must have exactly 1 operand!");
                }
            }
            else
            {
                if (numOfOperands < 1)
                {
                    throw new SMException("A logical AND or OR operator must have at least 1 operand!");
                }
            }
        }

        /// <summary>
        /// The included states of this operator.
        /// </summary>
        private HashSet<ISMState> includedStates;

        /// <summary>
        /// The included operators of this operator.
        /// </summary>
        private HashSet<SMOperator> includedOperators;

        /// <summary>
        /// The type of this operator.
        /// </summary>
        private SMOperatorType operatorType;
    }
}
