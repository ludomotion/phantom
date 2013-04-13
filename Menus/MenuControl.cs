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
        public int Selected;
        public bool Enabled = true;

        protected int pressed;
        protected Menu menu;
        public MenuControl Left;
        public MenuControl Right;
        public MenuControl Above;
        public MenuControl Below;
        public Vector2 Position;
        public Shape Shape;
        public int PlayerMask = 255;

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
            if (Selected>0)
                currentSelected += Math.Min(1 - currentSelected, elapsed * selectSpeed);
            else
                currentSelected -= Math.Min(currentSelected, elapsed * deselectSpeed);
        }

        public virtual void StartPress(int player) 
        {
            if (Enabled)
                pressed |= 1 << player;
        }

        public virtual void EndPress(int player)
        {
            int pl = 1 << player;
            if ((pressed & pl) == pl)
            {
                pressed &= 255 - pl;
                Click(ClickType.Select, player);
            }
        }

        public virtual void CancelPress(int player)
        {
            int pl = 1 << player;
            pressed &= 255-pl;
        }

        public virtual void Click(ClickType type, int player)
        {
        }

        public virtual void ClickAt(Vector2 position, int player)
        {
        }


    }
}
