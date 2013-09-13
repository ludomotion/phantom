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

namespace Phantom.GameUI
{
	public class UITouchHandler : UIBaseHandler
	{
		/// <summary>
		/// Previous state of the touch controls.
		/// </summary>
		protected TouchCollection previous;
		/// <summary>
		/// Current state of the touch controls.
		/// </summary>
		protected TouchCollection current;

		/// <summary>
		/// The element currently touched.
		/// </summary>
		protected UIElement hover;

		/// <summary>
		/// The element we started a touch even over.
		/// </summary>
		private UIElement touched;

		/// <summary>
		/// The element we are dragging.
		/// </summary>
		private UIContent draggingContent;

		protected Vector2 touchPosition;
		protected Vector2 previousTouchPosition;

		private Renderer renderer;

		public UITouchHandler()
			: base(0) { }

		public UITouchHandler(int player)
			: base(player) { }

		public override void OnAdd(Component parent)
		{
			if (!TouchPanel.GetState().IsConnected)
				throw new Exception("UITouchHandler: NO available Touch input");

			base.OnAdd(parent);
			current = TouchPanel.GetState();
			layer = parent as UILayer;
			if (layer == null)
				throw new Exception("UITouchHandler can only be added to a Menu component.");

			this.renderer = layer.GetComponentByType<Renderer>();
		}

		public override Component.MessageResult HandleMessage(int message, object data)
		{
			switch (message)
			{
				case (Messages.UIActivated):
					current = TouchPanel.GetState ();
					if (previous.Count > 0)
					{						
						foreach (TouchLocation tloc in previous)
						{
							if (tloc.State == TouchLocationState.Pressed)
							{
								layer.SetSelected (player, layer.GetControlAt (tloc.Position));
								break;
							}
						}
					}
					break;
			}

			return base.HandleMessage(message, data);
		}

		public Vector2 GetCurrentPosition()
		{
			//if (current != null)
			//{
				if (current.Count > 0) {						
					foreach (TouchLocation tloc in  current) {
						if (tloc.State == TouchLocationState.Pressed) {
							return new Vector2 (tloc.Position.X, tloc.Position.Y);
						}
					}
				}
			//}

			return Vector2.Zero;
		}

		private Vector2 GetPreviousPosition()
		{
			//if (previous != null)
			//{
				if (previous.Count > 0) {						
					foreach (TouchLocation tloc in  previous) {
						if (tloc.State == TouchLocationState.Pressed) {
							return new Vector2 (tloc.Position.X, tloc.Position.Y);
						}
					}
				}
			//}

			return Vector2.Zero;
		}

		public UIElement GetHoverElement()
		{
			return hover;
		}

		public override void Update(float elapsed)
		{
			base.Update(elapsed);
			previous = current;
			current = TouchPanel.GetState ();

			// NO NEED to handle hover element
			touchPosition = GetCurrentPosition();
			previousTouchPosition = GetPreviousPosition();

			// No touches
			if (touchPosition == Vector2.Zero && previousTouchPosition == Vector2.Zero)
				return;

			// If there is a renderer attached, update the coordinates depending on the transformation
			if (this.renderer != null)
			{
				Matrix renderMatrix = this.renderer.CreateMatrix();
				if (touchPosition != Vector2.Zero)
					touchPosition = Vector2.Transform(touchPosition, Matrix.Invert(renderMatrix));
				if (previousTouchPosition != Vector2.Zero)
					previousTouchPosition = Vector2.Transform(previousTouchPosition, Matrix.Invert (renderMatrix));
			}

			// Get the current selected object
			hover = layer.GetControlAt(touchPosition, draggingContent);

			if (hover != null && (hover.PlayerMask & (1 << player)) == 0)
				hover = null;

			UIContent content = (hover as UIContent);
			if (content != null && content.Container != null)
				hover = content.Container;
			if (hover is UIInventory)
				((UIInventory)hover).UpdateMouse(touchPosition);
			if (hover is UICarousel)
				((UICarousel)hover).UpdateMouse (touchPosition);
			if (hover is UICarouselContainer)
				((UICarouselContainer)hover).UpdateMouse (touchPosition);  

			//Start Touching
			if (touchPosition != Vector2.Zero && previousTouchPosition == Vector2.Zero)
			{
				layer.SetSelected(player, hover);
				if (hover != null)
				{
					hover.StartPress(player);
					hover.ClickAt(touchPosition - hover.Position, player);
					touched = hover;
				}
			}

			// End Touching
			if (touchPosition == Vector2.Zero && previousTouchPosition != Vector2.Zero)
			{
				if (draggingContent != null)
				{
					//end drag
					if (hover is UICarousel)
						hover = ((UICarousel) hover).GetElementAt(previousTouchPosition);

					UIContainer container = hover as UIContainer;
					if (container != null)
					{
						draggingContent.Dock(container);
					}
					else
					{
						draggingContent.DropAt(previousTouchPosition);
					}

				}
				else
				{
					if (layer.GetSelected(player) != null)
						layer.GetSelected(player).EndPress(player);
				}
				touched = null;
				draggingContent = null;
			}

			if (previousTouchPosition != Vector2.Zero && touchPosition != Vector2.Zero
			    && previousTouchPosition.X != touchPosition.X && previousTouchPosition.Y != touchPosition.Y)
			{
				// I am eventually dragging
				//Check which item I am hovering and select it
				layer.SetSelected(player, hover);

				//if dragging update the position
				if (draggingContent != null)
				{
					draggingContent.Position = touchPosition;
					draggingContent.Selected = 1;
				}
				else
				{
					//if pressing the left button at the same location pass the info
					if (touched != null && hover == touched) 
					{
						touched.MoveMouseTo(touchPosition - touched.Position, player);

						if (hover is UICarousel)
							hover = ((UICarousel)hover).GetElementAt(touchPosition);

						//check if I can start dragging something;
						if (hover is UIContainer && (hover as UIContainer).GetContentAt(touchPosition) != null && hover.Enabled)
						{
							layer.GetSelected(player).CancelPress(player);
							draggingContent = (hover as UIContainer).GetContentAt(touchPosition);
							draggingContent.Undock();
						}

						if (hover is UIContent && hover.Enabled)
						{
							draggingContent = hover as UIContent;
							draggingContent.Undock();
						}

					}
				}
			}
		}

	}
}
#endif