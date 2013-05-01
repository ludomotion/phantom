using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Misc;
using Phantom.Core;

namespace Phantom.Shapes.Filters
{
	public class AngleFilter : IFilter
	{
		public Vector2 Position;
		public Entity Entity;
		public float Orientation;

		private float halfarc;

		public float Arc
		{
			get
			{
				return this.halfarc * 2;
			}
			set
			{
				this.halfarc = value * .5f;
			}
		}

		private Func<Entity, bool> func;

		public AngleFilter(Vector2 origin, float orientation, float arc)
		{
			this.Position = origin;
			this.Orientation = orientation;
			this.halfarc = arc * .5f;
			func = ContainsFormPosition;
		}

		public AngleFilter(Entity origin, float arc)
		{
			this.Entity = origin;
			this.halfarc = arc * .5f;
			func = ContainsFormEntity;
		}

		public bool ContainsFormPosition(Core.Entity e)
		{
			Vector2 delta = e.Position - this.Position;
			float a = delta.Angle();
			return PhantomUtils.AngleDifference(a, Orientation) <= this.halfarc;
		}

		public bool ContainsFormEntity(Core.Entity e)
		{
			Vector2 delta = e.Position - this.Entity.Position;
			float a = delta.Angle();
			return PhantomUtils.AngleDifference(a, this.Entity.Orientation) <= this.halfarc;
		}

		public bool Contains(Entity e)
		{
			return this.func(e);
		}
	}
}
