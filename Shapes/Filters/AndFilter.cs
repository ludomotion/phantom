using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Filters
{
	public class AndFilter : IFilter
	{
		private IFilter[] filters;

		public AndFilter(params IFilter[] filters)
		{
			this.filters = filters;
		}

		public bool Contains(Core.Entity e)
		{
			for (int i = 0; i < filters.Length; i++)
			{
				if (!this.filters[i].Contains(e))
					return false;
			}
			return true;
		}
	}
}
