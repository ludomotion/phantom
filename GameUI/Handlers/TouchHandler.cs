#if TOUCH
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Phantom.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Phantom.GameUI.Elements;

namespace Phantom.GameUI.Handlers
{
	/// <summary>
	/// Implements mouse input for menu controls. 
	/// </summary>
	public class TouchHandler : BaseInputHandler
	{
        public delegate void UITouchAction(TouchHandler handler, int touchID, Vector3 data);

		private TouchController touch;
		private Dictionary<int, UIElement> touchmap = new Dictionary<int, UIElement>();
        private Dictionary<int, Vector3> swipemap = new Dictionary<int, Vector3>();
        private Dictionary<int, Vector3> dragmap = new Dictionary<int, Vector3>();
        private float time = 0;
        //private bool doSwipes;

        public UITouchAction OnSwipe;
        public UITouchAction OnDrag;

		public TouchHandler()
			: this(null, null) { }

        public TouchHandler(UITouchAction onSwipe, UITouchAction onDrag)
            : base(0) 
        {
            this.OnSwipe = onSwipe;
            this.OnDrag = onDrag;
        }

		public override void OnAdd(Component parent)
		{
			base.OnAdd(parent);
			layer = parent as UILayer;
			if (layer == null)
				throw new Exception("UITouchHandler can only be added to a UILayer instance.");
		}

		public override void OnAncestryChanged ()
		{
			this.touch = PhantomGame.Game.GetComponentByType<TouchController> ();
			if(this.touch == null) {
				Renderer.ViewportPolicy p = Renderer.ViewportPolicy.None;
				Renderer r = layer.GetComponentByType<Renderer> ();
				if (r != null) {
					p = r.Policy;
				}
				PhantomGame.Game.AddComponent (this.touch = new TouchController (p));
			}
			base.OnAncestryChanged ();
		}

		public override void Update(float elapsed)
		{
			base.Update(elapsed);
            time += elapsed;
			foreach (TouchLocation l in this.touch.CurrentTouchCollection) {

				UIElement focus = layer.GetControlAt (l.Position);

                bool swiped = false;
                //swipes
                if (OnSwipe!=null)
                {
                    if (l.State == TouchLocationState.Pressed)
                    {
                        swipemap[l.Id] = new Vector3(l.Position.X, l.Position.Y, time);
                    }
                    else if (l.State == TouchLocationState.Released && swipemap.ContainsKey(l.Id))
                    {
                        Vector3 swipe = swipemap[l.Id];
                        swipe.X = l.Position.X - swipe.X;
                        swipe.Y = l.Position.Y - swipe.Y;
                        swipe.Z = time - swipe.Z;

                        if (Math.Abs(swipe.X) > 20 || Math.Abs(swipe.Y) > 20)
                        {
                            swiped = true;
                            OnSwipe(this, l.Id, swipe);
                        }
                    }
                }

                //drags
                if (OnDrag != null)
                {
                    if (l.State == TouchLocationState.Pressed)
                    {
                        dragmap[l.Id] = new Vector3(l.Position.X, l.Position.Y, 0);
                    }

                    if (l.State == TouchLocationState.Moved && dragmap.ContainsKey(l.Id))
                    {
                        Vector3 drag = dragmap[l.Id];

                        Vector3 dragDelta = new Vector3();
                        dragDelta.X = l.Position.X - drag.X;
                        dragDelta.Y = l.Position.Y - drag.Y;
                        dragDelta.Z = 0;

                        if (drag.Z > 0 || dragDelta.Length() > 20)
                        {
                            drag.X = l.Position.X;
                            drag.Y = l.Position.Y;
                            drag.Z = 1;
                            dragmap[l.Id] = drag;
                            OnDrag(this, l.Id, dragDelta);
                        }
                    }
                }



                //elements
				if (l.State == TouchLocationState.Pressed) {
					touchmap [l.Id] = focus;
                    this.layer.SetSelected(player, focus);
                    if (focus != null && focus.CanUse(player))
					{
						focus.StartPress(player);
                        if (focus.OnMouseDown != null)
                        {
                            focus.OnMouseDown(focus, l.Position, UIMouseButton.Left);
                        }
					}
				} else if (l.State == TouchLocationState.Released && touchmap.ContainsKey(l.Id)) {
					UIElement started = touchmap [l.Id];
					touchmap.Remove (l.Id);
                    UIElement element = layer.GetSelected(player);
					if (element != null && element.CanUse(player)) 
                    {
                        if (focus == started && focus != null && !swiped)
                            element.EndPress(player);
                        else
                            element.CancelPress(player);
					}
					if (focus == started && focus != null && !swiped) {
						focus.ClickAt (l.Position, UIMouseButton.Left);
					}
				}

                
			}
		}
	}
}
#endif
