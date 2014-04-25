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

namespace Phantom.GameUI
{
	/// <summary>
	/// Implements mouse input for menu controls. 
	/// </summary>
	public class UITouchHandler : UIBaseHandler
	{
		private TouchController touch;
		private Dictionary<int, UIElement> touchmap = new Dictionary<int, UIElement> ();

		public UITouchHandler()
			: base(0) { }

		public UITouchHandler(int player)
			: base(player) { }


		public override void OnAdd(Component parent)
		{
			base.OnAdd(parent);
			layer = parent as UILayer;
			if (layer == null)
				throw new Exception("UITouchHandler can only be added to a Menu component.");
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
			foreach (TouchLocation l in this.touch.CurrentTouchCollection) {

				UIElement focus = layer.GetControlAt (l.Position);

				if (l.State == TouchLocationState.Pressed) {
					touchmap [l.Id] = focus;
					this.layer.SetSelected(player, focus);
					if (focus != null)
					{
						focus.StartPress(player);
						if (focus.OnMouseDown != null)
							focus.OnMouseDown(focus, l.Position, UIMouseButton.Left);
					}
				} else if (l.State == TouchLocationState.Released && touchmap.ContainsKey(l.Id)) {
					UIElement started = touchmap [l.Id];
					touchmap.Remove (l.Id);
					if (layer.GetSelected (player) != null) {
						layer.GetSelected (player).EndPress (player);
					}
					if (focus == started && focus != null) {
						focus.ClickAt (l.Position, player);
					}
				}
			}
		}
	}
}
