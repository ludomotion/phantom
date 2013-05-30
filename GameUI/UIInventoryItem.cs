using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using System.Diagnostics;

namespace Phantom.GameUI
{
    public class UIInventoryItem : UIContent
    {
        protected UIInventory Inventory;
        internal int InventoryX;
        internal int InventoryY;
        internal int Width;
        internal int Height;

        public UIInventoryItem(string name, string caption, Vector2 position, int width, int height, UIInventory inventory, int inventoryX, int inventoryY, UIContainer container)
            : base(name, caption, position, new OABB(new Vector2(width*inventory.SlotSize.X*0.5f, height*inventory.SlotSize.Y*0.5f)), container)
        {
            this.Inventory = inventory;
            this.InventoryX = inventoryX;
            this.InventoryY = inventoryY;
            this.Width = width;
            this.Height = height;
            if (container is UIInventory)
            {
                UIInventory inv = (UIInventory)container;
                if (inventoryX >= 0 && inventoryY >= 0)
                {
                    this.Position.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
                    this.Position.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;
                }
                Dock(container);
            }
        }

        public UIInventoryItem(string name, string caption, int width, int height, UIInventory inventory, UIContainer container)
            : this(name, caption, container.Position, width, height, inventory, -1, -1, container) { }

        public UIInventoryItem(string name, string caption, int width, int height, UIInventory inventory)
            : this(name, caption, new Vector2(float.NaN, float.NaN), width, height, inventory, -1, -1, inventory) { }

        public UIInventoryItem(string name, string caption, int width, int height, UIInventory inventory, int inventoryX, int inventoryY)
            : this(name, caption, inventory.Position, width, height, inventory, inventoryX, inventoryY, inventory) { }

        public override void Update(float elapsed)
        {
            if (State == UIContentState.Docked && Container == Inventory)
            {
                Enabled = Inventory.Enabled;
                Selected = Inventory.Selected;
                if (Inventory.Hovering != this)
                    Selected = 0;
            } 
            base.Update(elapsed);
        }

        public override void Dock(UIContainer container)
        {
            if (container is UIInventory)
            {
                UIInventory inv = (UIInventory)container;
                if (float.IsNaN(this.Position.X) || float.IsNaN(this.Position.Y))
                    AddToInventory(inv);
                else
                    DockAtLocation(inv);
                 
                
            } 
            else 
                base.Dock(container);
        }

        private void DockAtLocation(UIInventory inv)
        {
            if (!CanDockAt(inv) || !inv.CanAccept(this))
            {
                if (LastContainer != null)
                    MoveTo(LastContainer);
                else
                    MoveTo(LastPosition);
                return;
            }
            inv.GetInventoryPosition(this, ref InventoryX, ref InventoryY);
            UIInventoryItem other = inv.Slots[InventoryX, InventoryY];
            if (other != null)
            {
                //its not empty, check if it is the same and then try to stack
                if (this.StackSize > 1 && this.Name == other.Name)
                {
                    int s = other.Count + this.Count;
                    if (s <= other.StackSize)
                    {
                        //this stacks fits with the other stack
                        this.Destroyed = true;
                        other.Count = s;
                    }
                    else
                    {
                        //return any left-overs
                        other.Count = other.StackSize;
                        this.Count = s - other.StackSize;
                        if (LastContainer != null)
                            MoveTo(LastContainer);
                        else
                            MoveTo(LastPosition);
                    }
                    return;
                }
            }
            this.Container = inv;
            this.Inventory = inv;
            State = UIContentState.Docked;
            for (int y = InventoryY; y < InventoryY + Height; y++)
                for (int x = InventoryX; x < InventoryX + Width; x++)
                    inv.Slots[x, y] = this;
            this.Position.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
            this.Position.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;
        }

        private void AddToInventory(UIInventory inv)
        {
            if (!CanDockAt(inv) || !inv.FindEmptySpotFor(this))
            {
                if (LastContainer != null)
                    MoveTo(LastContainer);
                else
                    MoveTo(LastPosition);
                return;
            }

            this.Container = inv;
            this.Inventory = inv;
            State = UIContentState.Docked;
            for (int y = InventoryY; y < InventoryY + Height; y++)
                for (int x = InventoryX; x < InventoryX + Width; x++)
                    inv.Slots[x, y] = this;
            this.Position.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
            this.Position.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;
        }

        public override void MoveTo(UIContainer container)
        {
            base.MoveTo(container);
            if (container is UIInventory)
            {
                UIInventory inv = (UIInventory)container;
                this.targetPosition.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
                this.targetPosition.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;


            }

        }

        
    }
}
