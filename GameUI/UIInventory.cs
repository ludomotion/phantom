using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Shapes;
using Phantom.Misc;
using System.Diagnostics;

namespace Phantom.GameUI
{
    public class UIInventory : UIContainer
    {
        public UIInventoryItem[,] Slots;
        public readonly int Width;
        public readonly int Height;
        internal Vector2 Size;
        internal Vector2 SlotSize;
        internal UIInventoryItem Hovering;

        public UIInventory(string name, string caption, Vector2 position, OABB shape, int width, int height)
            : base(name, caption, position, shape)
        {
            this.Width = Math.Max(1, width);
            this.Height = Math.Max(1, height);
            this.Size = shape.HalfSize * 2;
            this.SlotSize = this.Size;
            SlotSize.X /= this.Width;
            SlotSize.Y /= this.Height;

            Slots = new UIInventoryItem[this.Width, this.Height];
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++)
                    Slots[x, y] = null;
        }

        public override void Render(Graphics.RenderInfo info)
        {
            if (UILayer.Font != null && Visible)
            {
                Vector2 size = UILayer.Font.MeasureString(Caption);
                Color text = Color.Lerp(UILayer.ColorShadow, UILayer.ColorFaceHighLight, this.currentSelected);

                if (!Enabled)
                    text = UILayer.ColorFace;

				PhantomUtils.DrawShape(info, this.Position, this.Shape, Color.Transparent, text, 2);

                size.X *= 0.5f;
                size.Y = -this.Shape.RoughWidth * 0.5f;
                info.Batch.DrawString(UILayer.Font, Caption, Position, text, 0, size, UILayer.DefaultFontScale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);

                for (int y = 0; y < this.Height; y++)
                {
                    for (int x = 0; x < this.Width; x++)
                    {
                        Vector2 p = Position;
                        p.X += (x - this.Width * 0.5f + 0.5f) * this.SlotSize.X;
                        p.Y += (y - this.Height * 0.5f + 0.5f) * this.SlotSize.Y;
                        RenderSlot(info, p, x, y);
                    }
                }
            }
            for (int i = 0; i < Components.Count; i++)
                Components[i].Render(info);
        }

        protected virtual void RenderSlot(Graphics.RenderInfo info, Vector2 position, int x, int y)
        {
            Color empty = UILayer.ColorFace;
            Color full = UILayer.ColorFaceDisabled;

            Vector2 s = SlotSize * 0.5f;
            s.X -= 1;
            s.Y -= 1;

            if (!Enabled)
                empty = UILayer.ColorFaceDisabled;

            info.Canvas.FillColor = (Slots[x, y] == null) ? empty : full;

            info.Canvas.FillRect(position, s, 0);
        }

        public Vector2 GetPositionInInventory(UIInventoryItem item)
        {
            Vector2 result = new Vector2(item.InventoryX * SlotSize.X, item.InventoryY * SlotSize.Y);
            result.X += item.Width * SlotSize.X * 0.5f;
            result.Y += item.Height * SlotSize.Y * 0.5f;
            result.X -= Size.X * 0.5f;
            result.Y -= Size.Y * 0.5f;
            result += this.Position;
            return result;
        }

        public void GetInventoryPosition(UIInventoryItem item, ref int x, ref int y)
        {
            //determine the inventory position;
            Vector2 delta = item.Position - this.Position;
            delta.X -= item.Width * SlotSize.X * 0.5f;
            delta.Y -= item.Height * SlotSize.Y * 0.5f;
            delta.X += Size.X * 0.5f;
            delta.Y += Size.Y * 0.5f;
            x = (int)Math.Round(delta.X / SlotSize.X);
            y = (int)Math.Round(delta.Y / SlotSize.Y);
        }

        public void GetInventoryPosition(Vector2 position, ref int x, ref int y)
        {
            //determine the inventory position;
            Vector2 delta = position - this.Position;
            delta.X -= SlotSize.X * 0.5f;
            delta.Y -= SlotSize.Y * 0.5f;
            delta.X += Size.X * 0.5f;
            delta.Y += Size.Y * 0.5f;
            x = (int)Math.Round(delta.X / SlotSize.X);
            y = (int)Math.Round(delta.Y / SlotSize.Y);
        }

        public override bool CanAccept(UIContent content)
        {
            if (!this.Enabled) 
                return false;
            if (!(content is UIInventoryItem)) 
                return false;
            UIInventoryItem item = (UIInventoryItem)content;

            int invX = -1;
            int invY = -1;
            GetInventoryPosition(item, ref invX, ref invY);

            if (invX < 0 || invX + item.Width > this.Width || invY < 0 || invY + item.Height > this.Height)
                return false;

            int empty = 0;
            for (int y = invY; y < invY + item.Height; y++)
                for (int x = invX; x < invX + item.Width; x++)
                    if (Slots[x, y] == null) 
                        empty++;

            if (empty < item.Width * item.Height)
                return false;

            return true;
        }

        public bool FindEmptySpotFor(UIInventoryItem item)
        {
            if (!this.Enabled)
                return false;

            for (int invY = 0; invY < this.Height - item.Height + 1; invY++)
            {
                for (int invX = 0; invX < this.Height - item.Height + 1; invX++)
                {
                    int empty = 0;
                    for (int y = invY; y < invY + item.Height; y++)
                        for (int x = invX; x < invX + item.Width; x++)
                            if (Slots[x, y] == null)
                                empty++;

                    if (empty == item.Width * item.Height)
                    {
                        item.InventoryX = invX;
                        item.InventoryY = invY;
                        return true;
                    }
                }
            }
        


            return false;
        }


        public override UIContent GetContentAt(Vector2 position)
        {
            int x = -1;
            int y = -1;
            GetInventoryPosition(position, ref x, ref y);
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;
            return Slots[x, y];
        }

        protected override void OnComponentRemoved(Core.Component component)
        {
            base.OnComponentRemoved(component);
            if (component is UIInventoryItem)
            {
                UIInventoryItem item = (UIInventoryItem)component;
                if (item.InventoryX >= 0 && item.InventoryY >= 0)
                    for (int y = item.InventoryY; y < item.InventoryY + item.Height; y++)
                        for (int x = item.InventoryX; x < item.InventoryX + item.Width; x++)
                            Slots[x, y] = null;
            }
        }

        internal void UpdateMouse(Vector2 mousePosition)
        {
            int x = -1;
            int y = -1;
            GetInventoryPosition(mousePosition, ref x, ref y);
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                Hovering = null;
            else
                Hovering = Slots[x, y];
        }



    }
}
