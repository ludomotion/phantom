using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Shapes.Filters
{
	public interface IFilter
	{
		bool Contains(Entity e);
	}
}
