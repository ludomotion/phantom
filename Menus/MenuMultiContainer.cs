using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public class MenuMultiContainer : MenuContainer
    {
        public int Capacity;
        public List<MenuContainerContent> Contents {get; private set;}

        public MenuMultiContainer(string name, string caption, Vector2 position, Shape shape, int capacity)
            : base(name, caption, position, shape)
        {
            this.Contents = new List<MenuContainerContent>();
            this.Capacity = capacity;
        }

        public override bool CanAccept(MenuContainerContent content)
        {
            if (!this.Enabled)
                return false;
            MenuContainerContent currentContent = GetContentAt(content.Position);

            if (currentContent != null && content.LastContainer != this && Contents.Count == Capacity)
            {
                //try swap
                if (content.LastContainer != null)
                {
                    if (content.LastContainer.CanAccept(currentContent) && currentContent.CanDockAt(content.LastContainer))
                        currentContent.MoveTo(content.LastContainer);
                }
                else
                {
                    if (currentContent.CanFloat)
                        currentContent.MoveTo(content.LastPosition);
                }
            }
            if (Contents.Count >= Capacity)
                return false;
            

            return true;
        }

        public override MenuContainerContent GetContentAt(Vector2 position)
        {
            for (int i = Contents.Count - 1; i >= 0; i--)
            {
                if (Contents[i].Shape.InShape(position))
                    return Contents[i];
            }
            return null;
        }

        public override void RemoveContent(MenuContainerContent content)
        {
            Contents.Remove(content);
        }

        public override void AddContent(MenuContainerContent content)
        {
            Contents.Add(content);
        }

    }
}
