using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Phantom.GameUI
{
    public class UIMouseEntityHandler : UIMouseHandler
    {
        private EntityLayer entityLayer;
        private bool selecting = false;
        private List<Entity> selected;

        public UIMouseEntityHandler(EntityLayer entityLayer)
            : base(0)
        {
            this.entityLayer = entityLayer;
            selecting = false;
            selected = new List<Entity>();
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

        private void SelectAll(Vector2 corner1, Vector2 corner2)
        {
            Vector2 topLeft = new Vector2(Math.Min(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y));
            Vector2 bottomRight = new Vector2(Math.Max(corner1.X, corner2.X), Math.Max(corner1.Y, corner2.Y));
            List<Entity> entities = entityLayer.GetEntitiesInRect(topLeft, bottomRight);
            for (int i = 0; i < entities.Count; i++)
                AddSelected(entities[i]);
        }

        private bool AddSelected(Entity entity)
        {
            if (entity.HandleMessage(Messages.Select, this) == MessageResult.HANDLED)
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

    }
}
