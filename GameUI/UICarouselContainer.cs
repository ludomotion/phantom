using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Misc;

namespace Phantom.GameUI
{
    public class UICarouselContainer : UIContainer
    {
        public readonly UIElementOrientation ElementOrientation;
        public readonly int VisibleOptions;
        public readonly bool Wrap;
        public int Capacity;
        public List<UIContent> Contents {get; private set;}
        private int selectedContent;
        private Vector2 offset;
        private UIContent hovering;

        public UICarouselContainer(string name, string caption, Vector2 position, OABB shape, int capacity, UIElementOrientation orientation, int visibleOptions, bool wrap, float offset)
            : base(name, caption, position, shape)
        {
            this.ElementOrientation = orientation;
            this.Contents = new List<UIContent>();
            this.Capacity = capacity;
            this.VisibleOptions = visibleOptions;
            this.Wrap = wrap;
            this.selectedContent = -1;
            this.hovering = null;

            switch (orientation)
            {
                default:
                case UIElementOrientation.LeftRight:
                    this.offset = new Vector2(offset, 0);
                    break;
                case UIElementOrientation.TopDown:
                    this.offset = new Vector2(0, offset);
                    break;
            }
        }

        public override bool CanAccept(UIContent content)
        {
            if (!this.Enabled)
                return false;
            if (Contents.Count >= Capacity)
                return false;
            

            return true;
        }

        public override UIContent GetContentAt(Vector2 position)
        {
            if (selectedContent >= 0)
            {
                if (Contents[selectedContent].Shape.InShape(position))
                    return Contents[selectedContent];
            }

            for (int i = 1; i <=VisibleOptions; i++)
            {
                int index = selectedContent + i;
                if (index >= Contents.Count && Wrap)
                    index -= Contents.Count;
                if (index < Contents.Count && Contents[index].Shape.InShape(position))
                    return Contents[index];

                index = selectedContent - i;
                if (index < 0 && Wrap)
                    index += Contents.Count;
                if (index >= 0 && Contents[index].Shape.InShape(position))
                    return Contents[index];
            }

            return null;
        }

        protected override void OnComponentAdded(Core.Component component)
        {
            base.OnComponentAdded(component);
            if (component is UIContent)
            {
                if (selectedContent < Contents.Count - 1)
                    Contents.Insert(selectedContent, component as UIContent);
                else
                {
                    Contents.Add(component as UIContent);
                    selectedContent++;
                }
            }
        }

        protected override void OnComponentRemoved(Core.Component component)
        {
            base.OnComponentRemoved(component);
            if (component is UIContent)
            {
                int index = Contents.IndexOf(component as UIContent);
                if (index >= 0)
                {
                    Contents.RemoveAt(index);
                    if (index <= selectedContent)
                        selectedContent--;
                    if (selectedContent < 0 && Contents.Count > 0)
                        selectedContent = 0;
                }
            }

        }

        public override void ClickAt(Vector2 position, int player)
        {
            if (hovering != null)
                hovering.ClickAt(position, player);
            else
            {
                switch (ElementOrientation)
                {
                    case UIElementOrientation.LeftRight:
                        if (position.X < 0)
                            Previous(player);
                        else
                            Next(player);
                        break;
                    case UIElementOrientation.TopDown:
                        if (position.Y < 0)
                            Previous(player);
                        else
                            Next(player);
                        break;
                }

            }
            
            base.ClickAt(position, player);
        }

        private void Previous(int player)
        {
            if (selectedContent < 0)
                return;
            selectedContent--;
            if (selectedContent < 0)
            {
                if (Wrap)
                    selectedContent = Contents.Count - 1;
                else
                    selectedContent = 0;
            }
        }

        private void Next(int player)
        {
            if (selectedContent < 0)
                return;
            selectedContent++;
            if (selectedContent >= Contents.Count)
            {
                if (Wrap)
                    selectedContent = 0;
                else
                    selectedContent = Contents.Count - 1;
            }
        }


        internal void UpdateMouse(Vector2 mousePosition)
        {
            UIContent h = GetContentAt(mousePosition);

            if (h != hovering)
            {
                if (hovering != null)
                    hovering.Selected = 0;
                hovering = h;
            }
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (hovering != null)
                hovering.Selected = this.Selected;
        }

        public override void Render(Graphics.RenderInfo info)
        {
            if (Visible)
            {
                Color border = Color.Lerp(UILayer.ColorFace, UILayer.ColorFaceHighLight, this.currentSelected);

                if (!Enabled)
                {
                    border = UILayer.ColorFaceDisabled;
                }

                PhantomUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, border, 2);


                RenderElements(info);
            }
        }

        protected void RenderElements(Graphics.RenderInfo info)
        {
            if (selectedContent >= 0)
            {
                for (int i = VisibleOptions; i > 0; i--)
                {
                    int index = selectedContent + i;
                    if (index >= Contents.Count && Wrap)
                        index -= Contents.Count;
                    if (index < Contents.Count)
                    {
                        Contents[index].Position = this.Position + offset * i;
                        Contents[index].Render(info);
                    }

                    index = selectedContent - i;
                    if (index < 0 && Wrap)
                        index += Contents.Count;
                    if (index >= 0)
                    {
                        Contents[index].Position = this.Position - offset * i;
                        Contents[index].Render(info);
                    }
                }


                Contents[selectedContent].Position = this.Position;
                Contents[selectedContent].Render(info);
            }
        }
    }
}
