using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using System.Diagnostics;

namespace Phantom.GameUI.Elements
{
    public class InventoryContainerItem : ContainerItem
    {
        protected InventoryContainer Inventory;
        public int InventoryX;
        public int InventoryY;
        internal int Width;
        internal int Height;

        public InventoryContainerItem(string name, string caption, Vector2 position, int width, int height, float slotSize, int inventoryX, int inventoryY)
            : base(name, caption, position, new OABB(new Vector2(width*slotSize*0.5f, height*slotSize*0.5f)))
        {
            this.Inventory = null;
            this.InventoryX = inventoryX;
            this.InventoryY = inventoryY;
            this.Width = width;
            this.Height = height;
        }

        public InventoryContainerItem(string name, string caption, int width, int height, float slotSize)
            : base(name, caption, new Vector2(float.NaN, float.NaN), new OABB(new Vector2(width * slotSize * 0.5f, height * slotSize * 0.5f)))
        {
            this.Inventory = null;
            this.InventoryX = -1;
            this.InventoryY = -1;
            this.Width = width;
            this.Height = height;
        }


        public InventoryContainerItem(string name, string caption, int width, int height, float slotSize, int inventoryX, int inventoryY)
            : this(name, caption, new Vector2(float.NaN, float.NaN), width, height, slotSize, inventoryX, inventoryY) { }

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

        public override void Dock(Container container)
        {
            if (container is InventoryContainer)
            {
                InventoryContainer inv = (InventoryContainer)container;
                if ((float.IsNaN(this.Position.X) || float.IsNaN(this.Position.Y)) && InventoryX >= 0 && InventoryY >= 0)
                    this.Position = inv.GetPositionInInventory(this);
                
                if (float.IsNaN(this.Position.X) || float.IsNaN(this.Position.Y))
                    AddToInventory(inv);
                else
                    DockAtLocation(inv);
                 
                
            } 
            else 
                base.Dock(container);
        }

        private void DockAtLocation(InventoryContainer inv)
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
            InventoryContainerItem other = inv.Slots[InventoryX, InventoryY];
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

            if (Parent != null)
                this.Parent.RemoveComponent(this);
            inv.AddComponent(this);
            this.Container = inv;
            this.Inventory = inv;
            State = UIContentState.Docked;
            for (int y = InventoryY; y < InventoryY + Height; y++)
                for (int x = InventoryX; x < InventoryX + Width; x++)
                    inv.Slots[x, y] = this;
            this.Position.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
            this.Position.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;
        }

        private void AddToInventory(InventoryContainer inv)
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

        public override void MoveTo(Container container)
        {
            base.MoveTo(container);
            if (container is InventoryContainer)
            {
                InventoryContainer inv = (InventoryContainer)container;
                this.targetPosition.X = inv.Position.X + (-0.5f * inv.Width + 0.5f * this.Width + this.InventoryX) * inv.SlotSize.X;
                this.targetPosition.Y = inv.Position.Y + (-0.5f * inv.Height + 0.5f * this.Height + this.InventoryY) * inv.SlotSize.Y;


            }

        }

        
    }
}
