using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common.Configuration;
using System.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Represents a series of UISprites that can be played as an animation.
    /// </summary>
    public class UIAnimation : IDisposable
    {
        /// <summary>
        /// Constructs a UIAnimation object.
        /// </summary>
        public UIAnimation(XElement animationFileRoot)
        {
            this.phaseEndpoints = new List<int>();
            this.phaseSprites = new List<UISprite>();
            this.isLooped = false;
            this.isPlaying = false;
            this.currentTimepoint = 0;

            int currentTimepoint = 0;
            foreach (XElement animPhaseElem in animationFileRoot.Elements(ANIMATION_PHASE_ELEM))
            {
                XAttribute durationAttr = animPhaseElem.Attribute(ANIMATION_PHASE_DURATION_ATTR);
                XAttribute spriteAttr = animPhaseElem.Attribute(ANIMATION_PHASE_SPRITE_ATTR);
                if (durationAttr != null && spriteAttr != null)
                {
                    int duration = XmlHelper.LoadInt(durationAttr.Value);
                    if (duration > 0)
                    {
                        /// TODO: check whether the sprites of the animation has the same size.
                        UISprite phaseSprite = UIResourceManager.GetResource<UISprite>(spriteAttr.Value);
                        currentTimepoint += duration;
                        this.phaseEndpoints.Add(currentTimepoint);
                        this.phaseSprites.Add(phaseSprite);
                    }
                    else
                    {
                        throw new ConfigurationException("Duration of animation phase must be positive!");
                    }
                }
                else
                {
                    throw new ConfigurationException("No duration and sprite has been defined for animation phase!");
                }
            }
        }

        #region Public properties and methods

        /// <summary>
        /// Starts playing the animation.
        /// </summary>
        public void Start()
        {
            if (!this.isPlaying)
            {
                UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.Update;
                this.isPlaying = true;
            }
        }

        /// <summary>
        /// Stops playing the animation.
        /// </summary>
        public void Stop()
        {
            if (this.isPlaying)
            {
                UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.Update;
                this.isPlaying = false;
            }
        }

        /// <summary>
        /// Resets the animation to it's initial state.
        /// </summary>
        /// <param name="loop">True if the animation has to be looped.</param>
        public void Reset(bool loop)
        {
            if (this.isPlaying) { throw new InvalidOperationException("Animation is being played!"); }
            this.isLooped = loop;
            this.currentTimepoint = 0;
        }

        /// <summary>
        /// Gets whether this animation is being looped.
        /// </summary>
        public bool IsLooped { get { return isLooped; } }

        /// <summary>
        /// Gets whether this animation is being played.
        /// </summary>
        public bool IsPlaying { get { return isPlaying; } }

        /// <summary>
        /// Gets the current timepoint inside this animation.
        /// </summary>
        public int CurrentTimepoint { get { return currentTimepoint; } }

        /// <summary>
        /// Gets the duration of this animation.
        /// </summary>
        public int Duration { get { return this.phaseEndpoints[this.phaseEndpoints.Count - 1]; } }

        /// <summary>
        /// Gets the sprite at the current timepoint.
        /// </summary>
        public UISprite CurrentSprite
        {
            get
            {
                for (int i = 0; i < this.phaseEndpoints.Count; i++)
                {
                    int phaseEndpoint = this.phaseEndpoints[i];
                    int phaseStartpoint = i > 0 ? this.phaseEndpoints[i - 1] : 0;
                    if (this.currentTimepoint >= phaseStartpoint && this.currentTimepoint < phaseEndpoint)
                    {
                        return this.phaseSprites[i];
                    }
                }
                return this.phaseSprites[this.phaseSprites.Count - 1];
            }
        }

        #endregion Public properties and methods

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.Stop();
            this.Reset(false);
            this.phaseEndpoints.Clear();
            this.phaseSprites.Clear();
        }

        #endregion IDisposable members

        /// <summary>
        /// Internal method called by the framework on every update while the animation is being played.
        /// </summary>
        /// <param name="evtArgs">The details of the update event.</param>
        private void Update()
        {
            this.currentTimepoint += UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
            if (this.isLooped)
            {
                this.currentTimepoint %= this.phaseEndpoints[this.phaseEndpoints.Count - 1];
            }
        }

        /// <summary>
        /// List of the endpoints of the phases in this UIAnimation.
        /// </summary>
        private List<int> phaseEndpoints;

        /// <summary>
        /// List of the sprites of the phases in this UIAnimation.
        /// </summary>
        private List<UISprite> phaseSprites;

        /// <summary>
        /// This flag indicates whether the animation has to be looped or not.
        /// </summary>
        private bool isLooped;

        /// <summary>
        /// This flag indicates whether the animation is being played or not.
        /// </summary>
        private bool isPlaying;

        /// <summary>
        /// The current timepoint inside the animation.
        /// </summary>
        private int currentTimepoint;

        /// <summary>
        /// Supported XML elements and attributes in animation files.
        /// </summary>
        private const string ANIMATION_ELEM = "animation";
        private const string ANIMATION_PHASE_ELEM = "animationPhase";
        private const string ANIMATION_PHASE_DURATION_ATTR = "duration";
        private const string ANIMATION_PHASE_SPRITE_ATTR = "sprite";
    }
}
