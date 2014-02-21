using RC.Common;
using RC.Engine.Maps.Core;
using RC.Engine.Maps.PublicInterfaces;
using RC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.MotionControl.Test
{
    /// <summary>
    /// Displays the execution of the motion control.
    /// </summary>
    class RCMotionControlTestPage : UIPage
    {
        /// <summary>
        /// Constructs an RCMotionControlTestPage object.
        /// </summary>
        public RCMotionControlTestPage()
        {
            this.timeSinceLastUpdate = 0;
            this.brush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(UIColor.Green, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.brush.Upload();

            this.entities = new BspSearchTree<TestEntity>(new RCNumRectangle(HALF_VECT * (-1), MAP_SIZE), 16, 10);
            for (int i = 0; i < ENTITY_COUNT; i++)
            {
                RCNumVector size;
                RCNumVector position;
                TestEntity newEntity;

                do
                {
                    size = new RCNumVector(RandomService.DefaultGenerator.Next((int)(MIN_ENTITY_SIZE.X * 1000), (int)(MAX_ENTITY_SIZE.X * 1000)) / (RCNumber)1000,
                                                   RandomService.DefaultGenerator.Next((int)(MIN_ENTITY_SIZE.Y * 1000), (int)(MAX_ENTITY_SIZE.Y * 1000)) / (RCNumber)1000);
                    position = new RCNumVector(RandomService.DefaultGenerator.Next(MAP_SIZE.X), RandomService.DefaultGenerator.Next(MAP_SIZE.Y));
                    newEntity = new TestEntity(position, size, this.entities);
                } while (this.entities.GetContents(newEntity.BoundingBox).Count != 0);

                this.entities.AttachContent(newEntity);
            }

            this.MouseSensor.Move += this.OnMouseMove;
            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnUpdate);
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            foreach (TestEntity entity in this.entities.GetContents())
            {
                RCIntRectangle displayPos = new RCIntRectangle((RCIntVector)((entity.BoundingBox.Location + HALF_VECT) * new RCNumVector(CELL_SIZE, CELL_SIZE)),
                                                                (RCIntVector)(entity.BoundingBox.Size * new RCNumVector(CELL_SIZE, CELL_SIZE)));
                renderContext.RenderRectangle(this.brush, displayPos);
            }
            //renderContext.RenderRectangle(this.brush, new RCIntRectangle(0, 0, 20, 20));
        }
        
        /// <summary>
        /// Called when a mouse button has been pushed over the page.
        /// </summary>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            RCNumVector mapCoordinates = new RCNumVector(evtArgs.Position) / CELL_SIZE - HALF_VECT;
            foreach (TestEntity entity in this.entities.GetContents())
            {
                entity.SetGoal(mapCoordinates);
            }
        }
        
        /// <summary>
        /// Called by the framework on updates.
        /// </summary>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            timeSinceLastUpdate += evtArgs.TimeSinceLastUpdate;
            if (timeSinceLastUpdate >= TIME_BETWEEN_UPDATES)
            {
                timeSinceLastUpdate = 0;
                foreach (TestEntity entity in this.entities.GetContents()) { entity.UpdateVelocity(); }
                foreach (TestEntity entity in this.entities.GetContents()) { entity.UpdatePosition(); }
            }
        }

        /// <summary>
        /// The map content manager that stores the test entities.
        /// </summary>
        private ISearchTree<TestEntity> entities;

        /// <summary>
        /// The brush that is used to draw the test entities.
        /// </summary>
        private UISprite brush;

        /// <summary>
        /// The elapsed time since the last update.
        /// </summary>
        private int timeSinceLastUpdate;

        /// <summary>
        /// The size of the test map.
        /// </summary>
        private static readonly RCIntVector MAP_SIZE = new RCIntVector(256, 192);

        /// <summary>
        /// The minimum size of an entity.
        /// </summary>
        private static readonly RCNumVector MIN_ENTITY_SIZE = new RCNumVector(3, 3);

        /// <summary>
        /// The maximum size of an entity.
        /// </summary>
        private static readonly RCNumVector MAX_ENTITY_SIZE = new RCNumVector(10, 10);

        /// <summary>
        /// The half vector.
        /// </summary>
        private static readonly RCNumVector HALF_VECT = new RCNumVector((RCNumber)1 / (RCNumber)2, (RCNumber)1 / (RCNumber)2);

        /// <summary>
        /// The number of test entities.
        /// </summary>
        private const int ENTITY_COUNT = 40;

        /// <summary>
        /// The size of a cell in pixels.
        /// </summary>
        private const int CELL_SIZE = 4;

        /// <summary>
        /// The time between updates in milliseconds.
        /// </summary>
        private const int TIME_BETWEEN_UPDATES = 100;
    }
}
