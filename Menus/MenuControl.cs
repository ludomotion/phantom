using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Phantom.Shapes;

namespace Phantom.Menus
{
    public enum ClickType { Select, NextOption, PreviousOption }

    public class MenuControl : Component
    {
        public string Name;
        //TODO: Change to int so that multiple controllers might control the same menu
        public bool Selected;
        public bool Enabled = true;

        protected bool pressed;
        protected Menu menu;
        public MenuControl Left;
        public MenuControl Right;
        public MenuControl Above;
        public MenuControl Below;
        public Vector2 Position;
        public Shape Shape;

        protected float selectSpeed = 4;
        protected float deselectSpeed = 4;
        protected float currentSelected = 0;

        public MenuControl(string name, Vector2 position, Shape shape)
        {
            this.Name = name;
            this.Position = position;
            this.Shape = shape;
            this.Shape.SetStubEntity(new Entity(position));
        }

        public override void OnAncestryChanged()
        {
            base.OnAncestryChanged();
            menu = GetAncestor<Menu>();
        }

        public override void Update(float elapsed)
        {
            base.Update(elapsed);
            if (Selected)
                currentSelected += Math.Min(1 - currentSelected, elapsed * selectSpeed);
            else
                currentSelected -= Math.Min(currentSelected, elapsed * deselectSpeed);
        }

        public virtual void StartPress() 
        {
            if (Enabled)
                pressed = true;
        }

        public virtual void EndPress()
        {
            if (pressed)
            {
                pressed = false;
                Click(ClickType.Select);
            }
        }

        public virtual void CancelPress()
        {
            pressed = false;
        }

        public virtual void Click(ClickType type)
        {
        }

        public virtual void ClickAt(Vector2 position)
        {
        }


    }
}
