using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Shapes.Filters
{
	public class DelegateFilter : IFilter
	{
		public Func<Entity, bool> Function;

		public DelegateFilter(Func<Entity, bool> function)
		{
			this.Function = function;
		}

		public bool Contains(Core.Entity e)
		{
			return this.Function(e);
		}
	}
}
