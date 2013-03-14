using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Phantom.Utils.Performance
{
	public class ProfilerNode
	{
		public ProfilerNode Parent { get; set; }
		public List<ProfilerNode> Childern { get; private set; }
		public readonly int Depth;
		public readonly int ID;

		public string Name { get; set; }

		public int OpenProfiles { get; private set; }
		private Stopwatch watch;
		private int profileInstances;
		private long accumulator;

		private long maxTime;
		private long minTime;

		private ProfilerStats Stats;

		public ProfilerNode( int depth, int id )
		{
			this.Depth = depth;
			this.ID = id;

			this.Parent = null;
			this.Childern = new List<ProfilerNode>();

			this.watch = new Stopwatch();
			this.Reset();
		}

		public void Begin()
		{
			this.OpenProfiles += 1;
			this.profileInstances += 1;
			this.watch.Start();
		}

		public void End()
		{
			this.watch.Stop();
			long delta = this.watch.ElapsedMilliseconds;
			this.accumulator += delta;

			this.maxTime = Math.Max(this.maxTime, delta);
			this.minTime = Math.Min(this.minTime, delta);

			this.OpenProfiles -= 1;
		}

		public void Reset()
		{
			profileInstances = 0;
			accumulator = 0;
			maxTime = long.MinValue;
			minTime = long.MaxValue;
		}


		public void Compute()
		{
			Stats = new ProfilerStats();
			Stats.Calls = profileInstances;
			Stats.TotalTime = accumulator;

			if (Parent != null)
			{
				if (Parent.accumulator > 0)
					Stats.Percentage = accumulator * 100f / (float)Parent.accumulator;
				else
					Stats.Percentage = 0f;
			}
			else
			{
				Stats.Percentage = 100f;
			}
			Stats.Percentage = MathHelper.Clamp(Stats.Percentage, 0, 100);

			Stats.MinPercentage = Math.Min(Stats.MinPercentage, Stats.Percentage);
			Stats.MaxPercentage = Math.Max(Stats.MaxPercentage, Stats.Percentage);

			float av = 0;
			if (profileInstances == 0)
				av = accumulator;
			else
				av = accumulator / (float)profileInstances;

			if (minTime == 0)
				Stats.Min = 1;
			else
				Stats.Min = av / (float)minTime;

			if (av == 0)
				Stats.Max = 1;
			else
				Stats.Max = (float)maxTime / av;
		}

		public ProfilerNode GetChildByName(string name)
		{
			for (int i = this.Childern.Count - 1; i >= 0; i -= 1)
				if (this.Childern[i].Name == name)
					return this.Childern[i];
			return null;
		}

        internal void Render(SpriteBatch batch, SpriteFont font, ref Vector2 position, float infoX)
        {
            batch.DrawString(font, this.Name, position, Color.White);
            Vector2 info = new Vector2(infoX, position.Y);
            batch.DrawString(font, this.Stats.Calls.ToString(), info, Color.White);
            info.X += 30;
            batch.DrawString(font, this.Stats.TotalTime.ToString("0.0")+"ms", info, Color.White);
            info.X += 100;
            batch.DrawString(font, this.Stats.Percentage.ToString("0.0") + "%", info, Color.White);
            //info.X += 40;
            position.Y += 16;
            position.X += 8;
            for (int i = 0; i<this.Childern.Count; i++) 
            {
                this.Childern[i].Render(batch, font, ref position, infoX);
            }
            position.X -= 8;
        }
    }
}
