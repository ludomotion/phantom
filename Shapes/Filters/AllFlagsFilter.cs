using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Filters
{
	public class AllFlagsFilter : IFilter
	{
		public uint Mask;

		public AllFlagsFilter(uint mask)
		{
			this.Mask = mask;
		}

		public bool Contains(Core.Entity e)
		{
			return (e.Flags & this.Mask) == this.Mask;
		}
	}
}
