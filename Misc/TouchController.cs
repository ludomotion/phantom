using System;
using Phantom.Core;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;
using Phantom.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Phantom
{
	public class TouchController : Component
	{
		public TouchCollection CurrentTouchCollection {
			get {
				return this.touchCollection;
			}
		}
		public Renderer.ViewportPolicy ViewportPolicy 
		{
			get {
				return this.viewportPolicy;
			}
			set {
				this.viewportPolicy = value;
				Renderer r = new Renderer (0, viewportPolicy);
				this.renderinfo = r.BuildRenderInfo ();
				this.invertedWorld = Matrix.Invert (this.renderinfo.World);
			}
		}

		private TouchCollection touchCollection;
		private Renderer.ViewportPolicy viewportPolicy;
		private RenderInfo renderinfo;
		private Matrix invertedWorld;
		private MouseState previousMouse;
		private TouchLocationState mousePrevState;
		private int mouseID;

		public TouchController ( Renderer.ViewportPolicy viewportPolicy = Renderer.ViewportPolicy.None)
		{
			this.ViewportPolicy = viewportPolicy;
			previousMouse = Mouse.GetState ();
		}

		public override void Update (float elapsed)
		{
			this.touchCollection = TouchPanel.GetState ();
			MouseState mouse = Mouse.GetState ();
			Vector2 currentMousePosition = new Vector2 (mouse.X, mouse.Y);
			Vector2 previousMousePosition = new Vector2 (previousMouse.X, previousMouse.Y);
			TouchLocationState mouseState = TouchLocationState.Invalid;

			if (mouse.LeftButton == ButtonState.Pressed) {
				if ((currentMousePosition - previousMousePosition).LengthSquared () > 0) {
					mouseState = TouchLocationState.Moved;
				}
				if (previousMouse.LeftButton != ButtonState.Pressed) {
					mouseID--;
					mouseState = TouchLocationState.Pressed;
				}
			} else if(previousMouse.LeftButton == ButtonState.Pressed) {
				mouseState = TouchLocationState.Released;
			}

			TouchLocation[] result = new TouchLocation[this.touchCollection.Count + (mouseState!=TouchLocationState.Invalid?1:0)];

			for (int i = 0; i < this.touchCollection.Count; i++) {
				TouchLocation p, l = this.touchCollection [i];
				if (l.TryGetPreviousLocation (out p)) {
					result [i] = new TouchLocation (l.Id, l.State, Vector2.Transform (l.Position, this.invertedWorld), p.State, Vector2.Transform (p.Position, this.invertedWorld));
				} else {
					result [i] = new TouchLocation (l.Id, l.State, Vector2.Transform (l.Position, this.invertedWorld));
				}
			}

			if (mouseState != TouchLocationState.Invalid) {
				result [result.Length - 1] = new TouchLocation (mouseID, mouseState, Vector2.Transform (currentMousePosition, this.invertedWorld), mousePrevState, Vector2.Transform (previousMousePosition, this.invertedWorld));
			}

			this.touchCollection = new TouchCollection (result);

			foreach (TouchLocation l in this.touchCollection) {
				Debug.WriteLine (l);
			}

			mousePrevState = mouseState;
			previousMouse = mouse;
			base.Update (elapsed);
		}

		public Vector2 ConvertTouchToGame( Vector2 touch ) 
		{
			return Vector2.Transform (touch, this.invertedWorld);
		}
	}
}

