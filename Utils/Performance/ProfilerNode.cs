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

		public string Name { get; set; }

		public int OpenProfiles { get; private set; }
		private Stopwatch watch;
		private int profileInstances;
		public int accumulator;

		private int maxTime;
		private int minTime;

		public ProfilerStats Stats
		{
			get { return stats; }
		}
		private ProfilerStats stats;

		public ProfilerNode( int depth )
		{
			this.Depth = depth;

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
			int delta = (int)this.watch.ElapsedMilliseconds;
			this.watch.Reset();
			this.accumulator += delta;

			this.maxTime = Math.Max(this.maxTime, delta);
			this.minTime = Math.Min(this.minTime, delta);

			this.OpenProfiles -= 1;
		}

		public void Reset()
		{
			profileInstances = 0;
			accumulator = 0;
			maxTime = 0;
			minTime = 1;
		}


		public void Compute()
		{
			this.stats = new ProfilerStats();
			this.stats.Calls = this.profileInstances;
			this.stats.TotalTime = this.accumulator;

			if (this.Parent != null)
			{
				if (this.Parent.accumulator > 0)
					this.stats.Percentage = (this.accumulator * 100f / (float)this.Parent.accumulator);
				else
					this.stats.Percentage = 0f;
			}
			else
			{
				this.stats.Percentage = 100f;
			}
			this.stats.Percentage = MathHelper.Clamp(this.stats.Percentage, 0, 100);

			this.stats.MinPercentage = Math.Min(this.stats.MinPercentage, this.stats.Percentage);
			this.stats.MaxPercentage = Math.Max(this.stats.MaxPercentage, this.stats.Percentage);

			float av = 0;
			if (this.profileInstances == 0)
				av = this.accumulator;
			else
				av = this.accumulator / (float)this.profileInstances;

			if (this.minTime == 0)
				this.stats.Min = 1;
			else
				this.stats.Min = av / (float)this.minTime;

			if (av == 0)
				this.stats.Max = 1;
			else
				this.stats.Max = (float)this.maxTime / av;
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
            info.X += 60;
            batch.DrawString(font, this.Stats.TotalTime.ToString("0.0") + "ms", info, Color.White);
            info.X += 60;
            batch.DrawString(font, (this.Stats.TotalTime / Math.Max(1,this.Stats.Calls)).ToString("0.0") + "ms", info, Color.White);
            info.X += 80;
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
