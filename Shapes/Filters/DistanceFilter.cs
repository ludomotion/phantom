using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Phantom.Core;

namespace Phantom.Shapes.Filters
{
	public class DistanceFilter : IFilter
	{
		// TODO: When position is set make the correct 'func'
		public Vector2 Position;
		public float Distance;
		public Entity Entity;

		private Func<Entity, bool> func;

		public DistanceFilter(Vector2 origin, float distance)
		{
			this.Position = origin;
			this.Distance = distance * distance;
			func = ContainsFromPosition;
		}

		public DistanceFilter(Entity origin, float distance)
		{
			this.Entity = origin;
			this.Distance = distance * distance;
			func = ContainsFromEntity;
		}

		private bool ContainsFromPosition(Entity e)
		{
			float distance = (e.Position - this.Position).LengthSquared();
			return distance < this.Distance;
		}
		private bool ContainsFromEntity(Entity e)
		{
			float distance = (e.Position - this.Entity.Position).LengthSquared();
            return distance < this.Distance;
		}

		public bool Contains(Core.Entity e)
		{
			return this.func(e);
		}
	}
}
