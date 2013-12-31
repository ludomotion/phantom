using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Phantom.GameUI
{
    //DONE: Support simple tweening to make menu's more lively
    //TODO: Create containers and dragable items (inventory system)
    //TODO: Create a MenuInputTouch component
    //TODO: Include a default menu font in the library
    //TODO: Include a white sprite and use that to draw instead of the canvas based calls
    //TODO: Include a simple sprite frames based renderer for the default components
    //TODO: Implement basic back behavior

    /// <summary>
    /// A Menu layer implements basic functionality to keep track of menu controls.
    /// The constructor takes a renderer to visualize components added to the menu.
    /// By adding MenuControls components or instances from derived classes the menu
    /// is populated. By adding MenuInput components the menu can be controlled by
    /// several input devices.
    /// 
    /// The menu class implements the methods ConnectControls and WrapControls to
    /// automatically connect controls based on the menu's ordering and the controls'
    /// relative positions. These methods should be called it the menu is to be
    /// controlled by keyboard or gamepad.
    /// 
    /// To use the basic menu simply add a Menu instance to a gamestate, and add controls 
    /// and input handlers to it. For example a menu controlled by keyboard or mouse  
    /// might look something like this:
    /// 
    /// *** insert example here ***
    ///
    /// To respond to the controls. Respond to the MenuClicked and MenuOptionChanged messages 
    /// that pass the source control as its data parameter. The messages are passed to the game 
    /// state.
    /// </summary>
    
    public class UILayer : Layer
    {
        /// <summary>
        /// Menu ordering determines how controls are linked.
        /// </summary>
        public enum Ordering { TopToBottom, LeftToRight, TopToBottomLeftToRight, LeftToRightTopToBottom, TwoDimensional };

        //TODO this should be a font included in the library
        /// <summary>
        /// A font used by the default renderers of the MenuControls
        /// </summary>
        public static SpriteFont Font;

        public static float DefaultFontScale = 1f;

        //Default colors not the best solution but hey!
        public static Color ColorText = Color.Black;
        public static Color ColorTextHighLight = Color.Blue;
        public static Color ColorTextDisabled = Color.Gray;
        public static Color ColorFace = Color.Gray;
        public static Color ColorFaceHighLight = Color.Yellow;
        public static Color ColorFaceDisabled = Color.Silver;
        public static Color ColorShadow = Color.Black;
        public static Color ColorTextBox = Color.White;
        public static Color ColorTextBoxHighLight = Color.White;
        public static Color ColorTextBoxDisabled = Color.Silver;
        
        /// <summary>
        /// All the controls added to the menu.
        /// </summary>
        public List<UIElement> Controls;
        private UIElement[] selected;
        private UIElement focused;
        private Renderer renderer;

        /// <summary>
        /// Creates a menu class
        /// </summary>
        /// <param name="renderer">The renderer component that is responsible for rendering the controlss</param>
        /// <param name="maxPlayers">The number of players that can control the menu simultaneously</param>
        public UILayer(Renderer renderer, int playerCount)
        {
            selected = new UIElement[playerCount]; 
            this.renderer = renderer;
            AddComponent(renderer);
            Controls = new List<UIElement>();

#if DEBUG
            if (PhantomGame.Game.Console != null)
            {
                PhantomGame.Game.Console.Register("edit_ui", "Allows control over the ui elements with the mouse to reposition them.", delegate(string[] argv)
                {
                    for (int i = 0; i < this.Components.Count; i++)
                    {
                        if (Components[i] is UIMouseHandler)
                            RemoveComponent(Components[i]);
                    }
                    this.AddComponent(new UIDesigner());
                });
            }
#endif
        }

        /// <summary>
        /// Clears all components but retains the renderer
        /// </summary>
        public override void ClearComponents()
        {
            base.ClearComponents();
            Controls.Clear();
            AddComponent(renderer);
        }

        protected override void OnComponentAdded(Component child)
        {
            base.OnComponentAdded(child);
            if (child is UIElement)
                Controls = GetAllComponentsByTypeAsList<UIElement>();
        }

        protected override void OnComponentRemoved(Component child)
        {
            base.OnComponentRemoved(child);
            if (child is UIElement)
                Controls.Remove((UIElement)child);
        }


        public override void Render(RenderInfo info)
        {
            if (info != null)
                base.Render(info);
            else
                renderer.Render(null);
        }

        public void SetFocus(UIElement value)
        {
            if (this.focused != null)
                this.focused.Focus = false;
            focused = value;
            if (this.focused != null)
                this.focused.Focus = true;
        }

        public void FocusOnNext()
        {
            if (this.focused != null)
            {
                if (this.focused.Right != null)
                    SetFocus(this.focused.Right);
                else if (this.focused.Below != null)
                    SetFocus(this.focused.Below);
                else if (this.focused.Left != null)
                    SetFocus(this.focused.Left);
                else if (this.focused.Above != null)
                    SetFocus(this.focused.Above);
                else
                    SetFocus(null);
            }
        }

        public void FocusOnPrevious()
        {
            if (this.focused != null)
            {
                if (this.focused.Left != null)
                    SetFocus(this.focused.Left);
                else if (this.focused.Above != null)
                    SetFocus(this.focused.Above);
                else if (this.focused.Right != null)
                    SetFocus(this.focused.Right);
                else if (this.focused.Below != null)
                    SetFocus(this.focused.Below);
                else
                    SetFocus(null);
            }
        }

        /// <summary>
        /// Set the selected control for a controlling player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        public void SetSelected(int player, UIElement value)
        {
            if (player < 0 || player >= selected.Length)
                return;
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

        /// <summary>
        /// Get the selected control for a controlling player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public UIElement GetSelected(int player)
        {
            if (player < 0 || player >= selected.Length)
                return null;
            return selected[player];
        }

        /// <summary>
        /// Override to implement back behavior
        /// </summary>
        public virtual void Back()
        {
            //TODO 
        }

        /// <summary>
        /// Connect all controls based on a ordering and their relative positions
        /// </summary>
        /// <param name="ordering"></param>
        public void ConnectControls(Ordering ordering)
        {
            ConnectControls(ordering, float.MaxValue);
        }

        /// <summary>
        /// Connect all controls based on a ordering and their relative positions
        /// </summary>
        /// <param name="ordering"></param>
        /// <param name="maxDistance">Max distance for controls to be connected (distance betwene their positions, not their shapes)</param>
        public void ConnectControls(Ordering ordering, float maxDistance)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                switch (ordering)
                {
                    case Ordering.LeftToRight:
                        FindConnectionsLeftRight(Controls[i], maxDistance);
                        break;
                    case Ordering.TopToBottom:
                        FindConnectionsTopBottom(Controls[i], maxDistance);
                        break;
                    case Ordering.LeftToRightTopToBottom:
                    case Ordering.TopToBottomLeftToRight:
                    case Ordering.TwoDimensional:
                        FindConnectionsTopBottom(Controls[i], maxDistance);
                        FindConnectionsLeftRight(Controls[i], maxDistance);
                        break;
                }
            }
            switch (ordering)
            {
                case Ordering.LeftToRightTopToBottom:
                    RemoveConnectionsTopToBottom();
                    break;
                case Ordering.TopToBottomLeftToRight:
                    RemoveConnectionsLeftToRight();
                    break;
            }
        }

        private void FindConnectionsTopBottom(UIElement menuControl, float maxDistance)
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

        private void FindConnectionsLeftRight(UIElement menuControl, float maxDistance)
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
                    UIElement mc = Controls[i].Left;
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
                    UIElement mc = Controls[i].Above;
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

        /// <summary>
        /// Wrap controls after they have been connected. Connects the first to the last control
        /// </summary>
        /// <param name="ordering"></param>
        public void WrapControls(Ordering ordering)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                switch (ordering)
                {
                    case Ordering.LeftToRight:
                    case Ordering.LeftToRightTopToBottom:
                        WrapLeftRight(Controls[i]);
                        break;
                    case Ordering.TopToBottom:
                    case Ordering.TopToBottomLeftToRight:
                        WrapTopBottom(Controls[i]);
                        break;
                    case Ordering.TwoDimensional:
                        WrapTopBottom(Controls[i]);
                        WrapLeftRight(Controls[i]);
                        break;
                }
            }
        }

        private void WrapTopBottom(UIElement menuControl)
        {
            if (menuControl.Above == null)
            {
                UIElement mc = menuControl.Below;
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
                UIElement mc = menuControl.Above;
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

        private void WrapLeftRight(UIElement menuControl)
        {
            if (menuControl.Left == null)
            {
                UIElement mc = menuControl.Right;
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
                UIElement mc = menuControl.Left;
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

        /// <summary>
        /// Find the last control at a specific position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="exclude">This control is excluded from the search</param>
        /// <returns></returns>
        public UIElement GetControlAt(Vector2 position, UIElement exclude)
        {
            for (int i = Controls.Count-1; i >= 0; i--)
            {
                if (Controls[i] != exclude && Controls[i].Visible && Controls[i].Shape != null)
                {
                    if (Controls[i].InControl(position))
                    {
                        return Controls[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the last control at a specific position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public UIElement GetControlAt(Vector2 position)
        {
            return GetControlAt(position, null);
        }

        /// <summary>
        /// Returns the first control that can be used by a player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public UIElement GetFirstControl(int player)
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].Enabled && (Controls[i].PlayerMask & (1 << player)) > 0)
                    return Controls[i];
            }
            return null;
        }

    }
}
