using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI
{
    public struct MouseCommand
    {
        public string Name;
        public Vector2 Position;
        public Entity Entity;
        public MouseCommand(string name, Vector2 position)
        {
            this.Position = position;
            this.Name = name;
            this.Entity = null;
        }
        public MouseCommand(string name, Vector2 position, Entity entity)
        {
            this.Position = position;
            this.Name = name;
            this.Entity = entity;
        }
        public MouseCommand(string name, Entity entity)
        {
            this.Position = entity.Position;
            this.Name = name;
            this.Entity = entity;
        }

    }
    public class UIMouseEntityHandler : UIMouseHandler
    {
        
        private EntityLayer entityLayer;
        private bool selecting = false;
        private List<Entity> selected;

        public int Capacity = -1;
        public string Command = "";

        public UIMouseEntityHandler(EntityLayer entityLayer)
            : base(0)
        {
            this.entityLayer = entityLayer;
            selecting = false;
            selected = new List<Entity>();
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            Parent.HandleMessage(Messages.ToolSelected, "Select");

        }

        public override void Update(float elapsed)
        {
            if (!selecting)
                base.Update(elapsed);
            else
            {
                previous = current;
                current = Mouse.GetState();
                mousePosition = new Vector2(current.X, current.Y);
            }

            if (hover == null)
            {
                //not hovering over anything
                if (current.LeftButton == ButtonState.Pressed && previous.LeftButton != ButtonState.Pressed)
                {
                    if (Command == "Select")
                    {
                        ClearSelected();
                        List<Entity> entities = entityLayer.GetEntitiesAt(mousePosition);
                        bool selected = false;
                        for (int i = 0; i < entities.Count; i++)
                        {
                            if (AddSelected(entities[i]))
                            {
                                selected = true;
                                break;
                            }
                        }
                        if (!selected)
                        {
                            mouseDownPosition = mousePosition;
                            selecting = true;
                        }
                    }
                    else
                    {
                        InstructSelected(new MouseCommand(Command, mousePosition, entityLayer.GetEntityAt(mousePosition)));
                    }
                }
                if (current.RightButton == ButtonState.Pressed && previous.RightButton != ButtonState.Pressed)
                {
                    InstructSelected(new MouseCommand("Right", mousePosition, entityLayer.GetEntityAt(mousePosition)));
                }
            }
            if (selecting)
            {
                if (current.LeftButton != ButtonState.Pressed && previous.LeftButton == ButtonState.Pressed)
                {
                    SelectAll(mouseDownPosition, mousePosition);
                    selecting = false;
                }
            }
        }
        
        private void InstructSelected(MouseCommand mouseCommand)
        {
            foreach (Entity e in selected)
                e.HandleMessage(Messages.DoMouseCommand, mouseCommand);
            Parent.HandleMessage(Messages.ToolSelected, "Select");
        }

        private void SelectAll(Vector2 corner1, Vector2 corner2)
        {
            Vector2 topLeft = new Vector2(Math.Min(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y));
            Vector2 bottomRight = new Vector2(Math.Max(corner1.X, corner2.X), Math.Max(corner1.Y, corner2.Y));
			List<Entity> entities = new List<Entity>(entityLayer.GetEntitiesInRect(topLeft, bottomRight, false));
            for (int i = 0; i < entities.Count; i++)
                AddSelected(entities[i]);
        }

        private bool AddSelected(Entity entity)
        {
            if (entity.HandleMessage(Messages.Select, this).Handled)
            {
                selected.Add(entity);
                return true;
            }
            return false;
        }

        private void ClearSelected()
        {
            while (selected.Count > 0)
            {
                selected[0].HandleMessage(Messages.Deselect, this);
                selected.RemoveAt(0);
            }
        }

        public override void Render(Graphics.RenderInfo info)
        {
            base.Render(info);
            if (selecting)
            {
                info.Canvas.StrokeColor = Color.White;
                info.Canvas.LineWidth = 2;
                Vector2 hs = mousePosition - mouseDownPosition;
                hs *= 0.5f;
                info.Canvas.StrokeRect(mouseDownPosition + hs, hs, 0);
            }
        }

        public override void HandleMessage(Message message)
        {
            string cmd = null;
            if (message.Is<string>(Messages.ToolSelected, ref cmd))
            {
                this.Command = cmd;
                message.Handle();
            }
        }

    }
}
