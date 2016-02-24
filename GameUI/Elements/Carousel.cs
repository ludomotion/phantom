using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.GameUI.Elements
{
    public class Carousel : UIElement
    {
        public readonly UIElementOrientation ElementOrientation;
        public readonly int VisibleOptions;
        public readonly bool Wrap;
        private int selectedElement;
        protected List<UIElement> elements;
        private Vector2 offset;
        private float fallOff;
        private UIElement hovering;

        public Carousel(string name, Vector2 position, OABB shape, UIElementOrientation orientation, int visibleOptions, bool wrap, float offset, float fallOff)
            : base(name, position, shape)
        {
            this.ElementOrientation = orientation;
            this.VisibleOptions = visibleOptions;
            this.Wrap = wrap;
            this.fallOff = fallOff;
            elements = new List<UIElement>();
            selectedElement = -1;
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

        protected override void OnComponentAdded(Core.Component component)
        {
            base.OnComponentAdded(component);
            if (component is UIElement)
            {
                if (selectedElement < elements.Count - 1)
                    elements.Insert(selectedElement, component as UIElement);
                else
                {
                    elements.Add(component as UIElement);
                    selectedElement++;
                }

                SelectionChanged();
            }
        }

        protected override void OnComponentRemoved(Core.Component component)
        {
            base.OnComponentRemoved(component);
            if (component is UIElement)
            {
                int position = elements.IndexOf(component as UIElement);
                if (position <= selectedElement)
                    selectedElement--;
                elements.RemoveAt(position);
                if (selectedElement < 0 && elements.Count > 0)
                {
                    if (Wrap)
                        selectedElement = elements.Count - 1;
                    else
                        selectedElement = 0;

                    SelectionChanged();
                }
            }
        }

        public virtual UIElement GetElementAt(Vector2 position)
        {
            if (selectedElement >= 0)
            {
                if (elements[selectedElement].Shape.InShape(position))
                    return elements[selectedElement];
            }

            for (int i = 1; i <= VisibleOptions; i++)
            {
                int index = selectedElement + i;
                if (index >= elements.Count && Wrap)
                    index -= elements.Count;
                if (index < elements.Count && elements[index].Shape.InShape(position))
                    return elements[index];

                index = selectedElement - i;
                if (index < 0 && Wrap)
                    index += elements.Count;
                if (index >= 0 && elements[index].Shape.InShape(position))
                    return elements[index];
            }

            return null;
        }

        public override void ClickAt(Vector2 position, UIMouseButton button)
        {
            if (hovering != null)
                hovering.ClickAt(position, button);
            else
            {
                switch (ElementOrientation)
                {
                    case UIElementOrientation.LeftRight:
                        if (position.X < 0)
                            PreviousOption();
                        else
                            NextOption();
                        break;
                    case UIElementOrientation.TopDown:
                        if (position.Y < 0)
                            PreviousOption();
                        else
                            NextOption();
                        break;
                }

            }

            base.ClickAt(position, button);
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (hovering != null)
                hovering.Selected = this.Selected;
        }

        public override void PreviousOption()
        {
            if (selectedElement < 0)
                return;
            elements[selectedElement].CancelPress(-1);
            elements[selectedElement].Selected = 0;
            selectedElement--;
            if (selectedElement < 0)
            {
                if (Wrap)
                    selectedElement = elements.Count - 1;
                else
                    selectedElement = 0;
            }

            SelectionChanged();
        }

        public override void NextOption()
        {
            if (selectedElement < 0)
                return;
            elements[selectedElement].CancelPress(-1);
            elements[selectedElement].Selected = 0;
            selectedElement++;
            if (selectedElement >= elements.Count)
            {
                if (Wrap)
                    selectedElement = 0;
                else
                    selectedElement = elements.Count - 1;
            }

            SelectionChanged();
        }

        public override void StartPress(int player)
        {
            if (hovering!=null)
                hovering.StartPress(player);
            base.StartPress(player);
        }

        public override void CancelPress(int player)
        {
            if (hovering != null)
                hovering.CancelPress(player);
            base.CancelPress(player);
        }

        public override void EndPress(int player)
        {
            if (hovering != null)
                hovering.EndPress(player);
            base.EndPress(player);
        }

        public override void Activate()
        {
            if (hovering != null)
                hovering.EndPress(-1);
            base.Activate();
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

            //base.Render(info);
        }

        protected void RenderElements(Graphics.RenderInfo info)
        {
            if (selectedElement >= 0)
            {
                for (int i = VisibleOptions; i > 0; i--)
                {
                    int index = selectedElement + i;
                    if (index >= elements.Count && Wrap)
                        index -= elements.Count;
                    if (index < elements.Count)
                    {
                        elements[index].Position = this.Position + offset * i * (float)Math.Pow(fallOff, i);
                        elements[index].Render(info);
                    }

                    index = selectedElement - i;
                    if (index < 0 && Wrap)
                        index += elements.Count;
                    if (index >= 0)
                    {
                        elements[index].Position = this.Position - offset * i * (float)Math.Pow(fallOff, i);
                        elements[index].Render(info);
                    }
                }


                elements[selectedElement].Position = this.Position;
                elements[selectedElement].Render(info);
            }
        }



        internal void UpdateMouse(Vector2 mousePosition)
        {
            UIElement h = GetElementAt(mousePosition);

            if (h != hovering)
            {
                if (hovering != null)
                    hovering.Selected = 0;
                hovering = h;
            }
        }

        public virtual void SelectionChanged()
        {
        }

        public UIElement GetSelectedElement()
        {
            if (selectedElement >= 0)
                return elements[selectedElement];
            else
                return null;
        }



        

        public void SetSelection(int index, int player)
        {
            if (index >= 0 && index < elements.Count)
            {
                elements[selectedElement].Selected = 0;
                selectedElement = index;
                elements[selectedElement].Selected = player;
                SelectionChanged();
            }
        }

        public void ClearContents()
        {
            for (int i = Components.Count - 1; i >= 0; i--)
            {
                if (Components[i] is UIElement)
                    RemoveComponent(Components[i]);
            }
        }
    }
}
