using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.GameUI
{
    public class UIMultiContainer : UIContainer
    {
        public int Capacity;
        public List<UIContent> Contents {get; private set;}

        public UIMultiContainer(string name, string caption, Vector2 position, Shape shape, int capacity)
            : base(name, caption, position, shape)
        {
            this.Contents = new List<UIContent>();
            this.Capacity = capacity;
        }

        public override bool CanAccept(UIContent content)
        {
            if (!this.Enabled)
                return false;
            UIContent currentContent = GetContentAt(content.Position);

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

        public override UIContent GetContentAt(Vector2 position)
        {
            for (int i = Contents.Count - 1; i >= 0; i--)
            {
                if (Contents[i].Shape.InShape(position))
                    return Contents[i];
            }
            return null;
        }

        public override void RemoveContent(UIContent content)
        {
            Contents.Remove(content);
        }

        public override void AddContent(UIContent content)
        {
            Contents.Add(content);
        }

    }
}
