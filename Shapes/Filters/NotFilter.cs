using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Filters
{
	public class NotFilter : IFilter
	{
		private IFilter filter;

		public NotFilter(IFilter filter)
		{
			this.filter = filter;

		}
		public bool Contains(Core.Entity e)
		{
			return !this.filter.Contains(e);
		}
	}
}
