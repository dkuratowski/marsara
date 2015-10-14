using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.Configuration;
using RC.Engine.Simulator.ComponentInterfaces;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a command input listener that is waiting for trigger from the command panel.
    /// </summary>
    abstract class ButtonListener : CommandInputListener, IButtonListener
    {
        #region IButtonListener members

        /// <see cref="IButtonListener.CommandPanelSlot"/>
        public RCIntVector CommandPanelSlot { get { return this.panelPosition; } }

        /// <see cref="IButtonListener.ButtonSprite"/>
        public SpriteRenderInfo ButtonSprite
        {
            get
            {
                return new SpriteRenderInfo
                {
                    SpriteGroup = SpriteGroupEnum.CommandButtonSpriteGroup,
                    Index = this.SpritePalette.Index,
                    Section = this.spriteSection,
                    DisplayCoords = new RCIntVector(0, 0)
                };
            }
        }

        /// <see cref="IButtonListener.ButtonAvailability"/>
        public virtual AvailabilityEnum ButtonAvailability { get { return AvailabilityEnum.Enabled; } }

        /// <see cref="IButtonListener.IsHighlighted"/>
        public virtual bool IsHighlighted { get { return false; } }

        /// <see cref="IButtonListener.Priority"/>
        public int Priority { get { return this.priority; } }

        #endregion IButtonListener members

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            XAttribute spriteNameAttr = listenerElem.Attribute(SPRITENAME_ATTR);
            if (spriteNameAttr == null) { throw new InvalidOperationException("Sprite name not defined for a button listener!"); }

            XAttribute panelPosAttr = listenerElem.Attribute(PANELPOSITION_ATTR);
            if (panelPosAttr == null) { throw new InvalidOperationException("Panel position not defined for a button listener!"); }

            XAttribute priorityAttr = listenerElem.Attribute(PRIORITY_ATTR);
            
            int spriteIndex = this.SpritePalette.GetSpriteIndex(spriteNameAttr.Value);
            this.spriteSection = this.SpritePalette.GetSection(spriteIndex);
            this.spriteOffset = this.SpritePalette.GetOffset(spriteIndex);
            this.panelPosition = XmlHelper.LoadIntVector(panelPosAttr.Value);
            this.priority = priorityAttr != null ? XmlHelper.LoadInt(priorityAttr.Value) : 0;

            if (this.priority < 0) { throw new InvalidOperationException("Priority shall be non-negative!"); }
        }

        /// <summary>
        /// The position of the button on the command panel that this listener is attached.
        /// </summary>
        private RCIntVector panelPosition;

        /// <summary>
        /// The section of the sprite of this button listener inside the sprite palette.
        /// </summary>
        private RCIntRectangle spriteSection;

        /// <summary>
        /// The offset of the sprite of this button listener.
        /// </summary>
        private RCIntVector spriteOffset;

        /// <summary>
        /// The priority of this button listener.
        /// </summary>
        private int priority;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string SPRITENAME_ATTR = "sprite";
        private const string PANELPOSITION_ATTR = "panelPosition";
        private const string PRIORITY_ATTR = "priority";
    }
}
