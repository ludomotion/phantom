using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phantom.Utils.Performance
{
	public struct ProfilerStats
	{
		public int Calls;
		public int TotalTime;
		public float Percentage;
		public float Min;
		public float Max;
		public float MinPercentage;
		public float MaxPercentage;
	}
}
