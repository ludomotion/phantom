using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Shapes.Filters
{
	public class AnyFlagsFilter : IFilter
	{
		public uint Mask;

		public AnyFlagsFilter(uint mask)
		{
			this.Mask = mask;
		}

		public bool Contains(Core.Entity e)
		{
			return (e.Flags & this.Mask) != 0;
		}
	}
}
