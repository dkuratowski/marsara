using RC.Engine.Maps.PublicInterfaces;
using System;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

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
        /// <param name="dirValueSrc">The direction value source of the animation being played.</param>
        public AnimationPlayer(Animation animation, IValueRead<MapDirection> dirValueSrc)
        {
            if (animation == null) { throw new ArgumentNullException("animation"); }
            this.directionValueSrc = dirValueSrc;
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
                if (currInstruction == null) { throw new InvalidOperationException("AnimationPlayer has reached the end of the animation!"); }
                stepComplete = currInstruction.Execute(this);
            }
        }

        #region Animation.IInstructionContext members

        /// <see cref="Animation.IInstructionContext.Direction"/>
        public MapDirection Direction { get { return this.directionValueSrc.Read(); } }

        /// <see cref="Animation.IInstructionContext.InstructionPointer"/>
        int Animation.IInstructionContext.InstructionPointer
        {
            get { return this.instructionPointer;}
            set
            {
                this.registers = new int[REGISTER_COUNT];
                this.instructionPointer = value;
            }
        }

        /// <see cref="Animation.IInstructionContext.SetFrame"/>
        void Animation.IInstructionContext.SetFrame(int[] frame)
        {
            if (frame == null) { throw new ArgumentNullException("frame"); }
            this.currentFrame = new int[frame.Length];
            for (int i = 0; i < frame.Length; i++) { this.currentFrame[i] = frame[i]; }
        }

        /// <see cref="Animation.IInstructionContext.this"/>
        int Animation.IInstructionContext.this[int regIdx]
        {
            get { return this.registers[regIdx]; }
            set { this.registers[regIdx] = value; }
        }

        #endregion Animation.IInstructionContext members

        #region Internal members

        /// <summary>
        /// Gets whether this AnimationPlayer has reached the end of the animation.
        /// </summary>
        internal bool IsFinished { get { return this.animation[this.instructionPointer] == null; } }

        #endregion Internal members

        /// <summary>
        /// The direction value source of the animation being played.
        /// </summary>
        private readonly IValueRead<MapDirection> directionValueSrc;

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
        private readonly Animation animation;

        /// <summary>
        /// The number of registers.
        /// </summary>
        private const int REGISTER_COUNT = 4;
    }
}
