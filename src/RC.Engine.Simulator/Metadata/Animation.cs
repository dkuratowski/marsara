using RC.Engine.Maps.PublicInterfaces;
using System;
using System.Collections.Generic;

namespace RC.Engine.Simulator.Metadata
{
    /// <summary>
    /// Represents an animation definition of a scenario element type.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// The interface of animation instructions.
        /// </summary>
        public interface IInstruction
        {
            /// <summary>
            /// Executes the instruction with the given context.
            /// </summary>
            /// <param name="ctx">The context of the instruction execution.</param>
            /// <returns>A boolean value that indicates whether the execution can be stopped or not.</returns>
            bool Execute(IInstructionContext ctx);
        }

        /// <summary>
        /// Interface of an instruction context.
        /// </summary>
        public interface IInstructionContext
        {
            /// <summary>
            /// Gets the direction of the animation being played.
            /// </summary>
            MapDirection Direction { get; }

            /// <summary>
            /// Gets or sets the value of the instruction pointer in the context.
            /// </summary>
            int InstructionPointer { get; set; }

            /// <summary>
            /// Sets the list of the displayed sprites in the context.
            /// </summary>
            /// <param name="frame">The list of the displayed sprites.</param>
            void SetFrame(int[] frame);

            /// <summary>
            /// Gets or sets the given register of the context.
            /// </summary>
            /// <param name="regIdx">The index of the register to get or set.</param>
            /// <returns>The value of the register of the context.</returns>
            int this[int regIdx] { get; set; }
        }

        /// <summary>
        /// Constructs a new animation instance.
        /// </summary>
        /// <param name="instructions">The instructions defined for the animation.</param>
        public Animation(List<IInstruction> instructions)
        {
            if (instructions == null) { throw new ArgumentNullException("instructions"); }
            this.instructions = new List<IInstruction>(instructions);
        }

        /// <summary>
        /// Gets the instruction at the given index or null if no instruction exists with the given index.
        /// </summary>
        /// <param name="index">The index of the instruction to get.</param>
        /// <returns>
        /// The instruction at the given index or null if no instruction exists with the given index.
        /// </returns>
        public IInstruction this[int index] { get { return index < 0 || index >= this.instructions.Count ? null : this.instructions[index]; } }

        /// <summary>
        /// The instructions of the animation.
        /// </summary>
        private List<IInstruction> instructions;
    }

    /// <summary>
    /// Represents an animation instruction that shows a new frame and waits for a defined time.
    /// </summary>
    class NewFrameInstruction : Animation.IInstruction
    {
        /// <summary>
        /// Constructs a new frame instruction.
        /// </summary>
        /// <param name="spriteIndices">The indices of the sprites to be displayed mapped by the corresponding map directions.</param>
        /// <param name="duration">The waiting time in frames.</param>
        public NewFrameInstruction(Dictionary<MapDirection, int[]> spriteIndices, int duration)
        {
            if (spriteIndices == null || spriteIndices.Count == 0) { throw new ArgumentNullException("spriteIndices"); }
            if (duration < 1) { throw new ArgumentOutOfRangeException("duration"); }

            this.spriteIndices = new Dictionary<MapDirection, int[]>();
            foreach (KeyValuePair<MapDirection, int[]> item in spriteIndices)
            {
                this.spriteIndices.Add(item.Key, new int[item.Value.Length]);
                for (int i = 0; i < spriteIndices[item.Key].Length; i++) { this.spriteIndices[item.Key][i] = spriteIndices[item.Key][i]; }
            }
            this.duration = duration;
        }

        #region Animation.IInstruction members

        /// <see cref="Animation.IInstruction.Execute"/>
        public bool Execute(Animation.IInstructionContext ctx)
        {
            if (ctx[0] == 0) { ctx.SetFrame(this.spriteIndices[ctx.Direction]); }
            ctx[0]++;
            if (ctx[0] >= this.duration) { ctx.InstructionPointer++; }
            return true;
        }

        #endregion Animation.IInstruction members

        /// <summary>
        /// The indices of the sprites to be displayed mapped by the corresponding map directions.
        /// </summary>
        private Dictionary<MapDirection, int[]> spriteIndices;

        /// <summary>
        /// The waiting time in frames.
        /// </summary>
        private int duration;
    }

    /// <summary>
    /// Represents an animation instruction that jumps to another instruction given by a label.
    /// </summary>
    class GotoInstruction : Animation.IInstruction
    {
        /// <summary>
        /// Constructs a goto instruction.
        /// </summary>
        /// <param name="targetInstructionIdx">The index of the target instruction.</param>
        public GotoInstruction(int targetInstructionIdx)
        {
            if (targetInstructionIdx < 0) { throw new ArgumentOutOfRangeException("targetInstructionIdx"); }
            this.targetInstructionIdx = targetInstructionIdx;
        }

        #region Animation.IInstruction members

        /// <see cref="Animation.IInstruction.Execute"/>
        public bool Execute(Animation.IInstructionContext ctx)
        {
            ctx.InstructionPointer = this.targetInstructionIdx;
            return false;
        }

        #endregion Animation.IInstruction members

        /// <summary>
        /// The index of the target instruction.
        /// </summary>
        private int targetInstructionIdx;
    }
}
