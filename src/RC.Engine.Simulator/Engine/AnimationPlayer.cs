using RC.Engine.Maps.PublicInterfaces;
using System;
using RC.Engine.Simulator.Metadata;

namespace RC.Engine.Simulator.Engine
{
    /// <summary>
    /// Represents an active animation of an entity.
    /// </summary>
    public class AnimationPlayer : Animation.IInstructionContext
    {
        /// <summary>
        /// Constructs an animation player that plays the given animation in the given direction.
        /// </summary>
        /// <param name="animation">The animation to be played.</param>
        /// <param name="direction">The direction of the animation.</param>
        public AnimationPlayer(Animation animation, MapDirection direction)
        {
            if (animation == null) { throw new ArgumentNullException("animation"); }
            this.direction = direction;
            this.instructionPointer = 0;
            this.animation = animation;
            this.registers = new int[REGISTER_COUNT];
            this.currentFrame = new int[0];
            this.Step();
        }

        /// <summary>
        /// Gets the indices of the sprites that shall be displayed in the current frame.
        /// </summary>
        public int[] CurrentFrame { get { return this.currentFrame; } }

        /// <summary>
        /// Steps the animation to the next frame.
        /// </summary>
        public void Step()
        {
            bool stepComplete = false;
            while (!stepComplete)
            {
                Animation.IInstruction currInstruction = this.animation[this.instructionPointer];
                stepComplete = currInstruction != null ? currInstruction.Execute(this) : true;
            }
        }

        #region Animation.IInstructionContext members

        /// <see cref="Animation.IInstructionContext.Direction"/>
        public MapDirection Direction { get { return this.direction; } }

        /// <see cref="Animation.IInstructionContext.SetInstructionPointer"/>
        int Animation.IInstructionContext.InstructionPointer
        {
            get { return this.instructionPointer;}
            set
            {
                this.registers = new int[REGISTER_COUNT];
                this.instructionPointer = value;
            }
        }

        /// <see cref="Animation.IInstructionContext.SetInstructionPointer"/>
        void Animation.IInstructionContext.SetFrame(int[] frame)
        {
            if (frame == null) { throw new ArgumentNullException("frame"); }
            this.currentFrame = new int[frame.Length];
            for (int i = 0; i < frame.Length; i++) { this.currentFrame[i] = frame[i]; }
        }

        /// <see cref="Animation.IInstructionContext.this[]"/>
        int Animation.IInstructionContext.this[int regIdx]
        {
            get { return this.registers[regIdx]; }
            set { this.registers[regIdx] = value; }
        }

        #endregion Animation.IInstructionContext members

        /// <summary>
        /// The direction of the animation being played.
        /// </summary>
        private MapDirection direction;

        /// <summary>
        /// The index of the next instruction.
        /// </summary>
        private int instructionPointer;

        /// <summary>
        /// The indices of the sprites that shall be displayed in the current frame.
        /// </summary>
        private int[] currentFrame;

        /// <summary>
        /// The registers used by the animation instructions.
        /// </summary>
        private int[] registers;

        /// <summary>
        /// The animation to be played.
        /// </summary>
        private Animation animation;

        /// <summary>
        /// The number of registers.
        /// </summary>
        private const int REGISTER_COUNT = 4;
    }
}
