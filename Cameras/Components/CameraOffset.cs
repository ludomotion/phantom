using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Cameras.Components
{
	public class CameraOffset : CameraComponent
	{
		private Vector2 offset;

		public CameraOffset(Vector2 offset)
		{
			this.offset = offset;
		}

		public override void Update(float elapsed)
		{
			base.Update(elapsed);
			if (this.Camera != null )
			{
				this.Camera.Target += this.offset;
			}
		}
	}
}
