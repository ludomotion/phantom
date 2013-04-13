using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.Menus
{
    public enum MenuOrientation { TopToBottom, LeftToRight, TopToBottomLeftToRight, LeftToRightTopToBottom, TwoDimensional };

    public class Menu : GameState
    {
        //TODO this should be a font included in the library
        public static SpriteFont Font;
        public static Color ColorText = Color.Black;
        public static Color ColorTextHighLight = Color.Blue;
        public static Color ColorTextDisabled = Color.Gray;
        public static Color ColorFace = Color.Gray;
        public static Color ColorFaceHighLight = Color.Yellow;
        public static Color ColorFaceDisabled = Color.Silver;
        public static Color ColorShadow = Color.Black;


        public MenuOrientation Orientation;

        public List<MenuControl> Controls;
        private MenuControl[] selected;
        private Renderer renderer;
        public int Leader = 0;

        public Menu(Renderer renderer, MenuOrientation orientation)
        {
            selected = new MenuControl[4]; 
            this.Orientation = orientation;
            AddComponent(renderer);
            OnlyOnTop = true;
            Controls = new List<MenuControl>();
            this.renderer = renderer;
        }

        public override void ClearComponents()
        {
            base.ClearComponents();
            Controls.Clear();
        }

        protected override void OnComponentAdded(Component child)
        {
            base.OnComponentAdded(child);
            if (child is MenuControl)
                Controls.Add((MenuControl)child);
        }

        protected override void OnComponentRemoved(Component child)
        {
            base.OnComponentRemoved(child);
            if (child is MenuControl)
                Controls.Remove((MenuControl)child);
        }

        public override void BackOnTop()
        {
            base.BackOnTop();
            HandleMessage(Messages.MenuActivated, null);
        }

        public override void Render(RenderInfo info)
        {
            if (info != null)
                base.Render(info);
            else
                renderer.Render(null);
        }

        public void SetSelected(int player, MenuControl value)
        {
            if (selected[player] == value)
                return;
            if (selected[player] != null)
            {
                selected[player].CancelPress(player);
                selected[player].Selected &= 255 - (1 << player);
            }
            selected[player] = value;
            if (selected[player] != null)
                selected[player].Selected |= 1 << player;
        }

        public MenuControl GetSelected(int player)
        {
            return selected[player];
        }


        public virtual void Back()
        {
            //TODO 
        }

        public void ConnectControls()
        {
            ConnectControls(float.MaxValue);
        }

        public void ConnectControls(float maxDistance)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                switch (Orientation)
                {
                    case MenuOrientation.LeftToRight:
                        FindConnectionsLeftRight(Controls[i], maxDistance);
                        break;
                    case MenuOrientation.TopToBottom:
                        FindConnectionsTopBottom(Controls[i], maxDistance);
                        break;
                    case MenuOrientation.LeftToRightTopToBottom:
                    case MenuOrientation.TopToBottomLeftToRight:
                    case MenuOrientation.TwoDimensional:
                        FindConnectionsTopBottom(Controls[i], maxDistance);
                        FindConnectionsLeftRight(Controls[i], maxDistance);
                        break;
                }
            }
            switch (Orientation)
            {
                case MenuOrientation.LeftToRightTopToBottom:
                    RemoveConnectionsTopToBottom();
                    break;
                case MenuOrientation.TopToBottomLeftToRight:
                    RemoveConnectionsLeftToRight();
                    break;
            }
        }

        private void FindConnectionsTopBottom(MenuControl menuControl, float maxDistance)
        {
            menuControl.Above = null;
            menuControl.Below = null;
            float distanceAbove = maxDistance * maxDistance;
            float distanceBelow = maxDistance * maxDistance;
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i] != menuControl)
                {
                    Vector2 d = Controls[i].Position - menuControl.Position;
                    if (Math.Abs(d.X)<Math.Abs(d.Y)) 
                    {
                        float distance = d.LengthSquared();
                        if (d.Y > 0 && distance < distanceBelow)
                        {
                            distanceBelow = distance;
                            menuControl.Below = Controls[i];
                        } 
                        else if (d.Y < 0 && distance < distanceAbove)
                        {
                            distanceAbove = distance;
                            menuControl.Above = Controls[i];
                        }
                    }
                }
            }
        }

        private void FindConnectionsLeftRight(MenuControl menuControl, float maxDistance)
        {
            menuControl.Left = null;
            menuControl.Right = null;
            float distanceLeft = maxDistance * maxDistance;
            float distanceRight = maxDistance * maxDistance;
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i] != menuControl)
                {
                    Vector2 d = Controls[i].Position - menuControl.Position;
                    if (Math.Abs(d.X) > Math.Abs(d.Y))
                    {
                        float distance = d.LengthSquared();
                        if (d.X > 0 && distance < distanceRight)
                        {
                            distanceRight = distance;
                            menuControl.Right = Controls[i];
                        }
                        else if (d.X < 0 && distance < distanceLeft)
                        {
                            distanceLeft = distance;
                            menuControl.Left = Controls[i];
                        }
                    }
                }
            }
        }

        private void RemoveConnectionsTopToBottom()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Right == null && Controls[i].Left != null)
                {
                    MenuControl mc = Controls[i].Left;
                    int max = 100;
                    while (mc.Left != null && max > 0 && mc.Left.Below != null)
                    {
                        mc = mc.Left;
                        max--;
                    }

                    Controls[i].Right = mc.Below;
                }
            }

            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Right != null && Controls[i].Right.Left == null)
                    Controls[i].Right.Left = Controls[i];
            }

            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Above = null;
                Controls[i].Below = null;
            }
        }

        private void RemoveConnectionsLeftToRight()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Below == null && Controls[i].Above != null)
                {
                    MenuControl mc = Controls[i].Above;
                    int max = 100;
                    while (mc.Above != null && max > 0 && mc.Above.Right != null)
                    {
                        mc = mc.Above;
                        max--;
                    }

                    Controls[i].Below = mc.Right;
                }
            }

            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Below != null && Controls[i].Below.Above == null)
                    Controls[i].Below.Above = Controls[i];
            } 
            
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Left = null;
                Controls[i].Right = null;
            }
        }

        public void WrapControls()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                switch (Orientation)
                {
                    case MenuOrientation.LeftToRight:
                    case MenuOrientation.LeftToRightTopToBottom:
                        WrapLeftRight(Controls[i]);
                        break;
                    case MenuOrientation.TopToBottom:
                    case MenuOrientation.TopToBottomLeftToRight:
                        WrapTopBottom(Controls[i]);
                        break;
                    case MenuOrientation.TwoDimensional:
                        WrapTopBottom(Controls[i]);
                        WrapLeftRight(Controls[i]);
                        break;
                }
            }
        }

        private void WrapTopBottom(MenuControl menuControl)
        {
            if (menuControl.Above == null)
            {
                MenuControl mc = menuControl.Below;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Below != null && max > 0)
                    {
                        mc = mc.Below;
                        max--;
                    }
                    menuControl.Above = mc;
                    mc.Below = menuControl;
                }
            }
            if (menuControl.Below == null)
            {
                MenuControl mc = menuControl.Above;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Above != null && max > 0)
                    {
                        mc = mc.Above;
                        max--;
                    }
                    menuControl.Below = mc;
                    mc.Above = menuControl;
                }
            }
        }

        private void WrapLeftRight(MenuControl menuControl)
        {
            if (menuControl.Left == null)
            {
                MenuControl mc = menuControl.Right;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Right != null && max > 0)
                    {
                        mc = mc.Right;
                        max--;
                    }
                    menuControl.Left = mc;
                    mc.Right = menuControl;
                }
            }
            if (menuControl.Right == null)
            {
                MenuControl mc = menuControl.Left;
                if (mc != null)
                {
                    int max = 100;
                    while (mc.Left != null && max > 0)
                    {
                        mc = mc.Left;
                        max--;
                    }
                    menuControl.Right = mc;
                    mc.Left = menuControl;
                }
            }
        }


        public MenuControl GetControlAt(Vector2 position)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Shape != null)
                {
                    if (Controls[i].Shape.InShape(position))
                        return Controls[i];
                }
            }
            return null;
        }

        public MenuControl GetFirstControl(int player)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Enabled && (!Controls[i].MustBeLeader || player == Leader))
                    return Controls[i];
            }
            return null;
        }
    }
}
