using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.GameUI.Elements
{
    public class MultiContainer : Container
    {
        public int Capacity;
        public List<ContainerItem> Contents {get; private set;}

        public MultiContainer(string name, string caption, Vector2 position, Shape shape, int capacity)
            : base(name, caption, position, shape)
        {
            this.Contents = new List<ContainerItem>();
            this.Capacity = capacity;
        }

        public override bool CanAccept(ContainerItem content)
        {
            if (!this.Enabled)
                return false;
            ContainerItem currentContent = GetContentAt(content.Position);

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

        public override ContainerItem GetContentAt(Vector2 position)
        {
            for (int i = Contents.Count - 1; i >= 0; i--)
            {
                if (Contents[i].Shape.InShape(position))
                    return Contents[i];
            }
            return null;
        }

        protected override void OnComponentAdded(Core.Component component)
        {
            base.OnComponentAdded(component);
            if (component is ContainerItem)
                Contents.Add(component as ContainerItem);
        }

        protected override void OnComponentRemoved(Core.Component component)
        {
            base.OnComponentRemoved(component);
            if (component is ContainerItem)
                Contents.Remove(component as ContainerItem);

        }
    }
}
