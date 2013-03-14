using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Utils.Performance
{
	public class Profiler : Component
	{
		public  static Profiler Instance { get; private set; }

		public static void Initialize(PhantomGame game, int frequency)
		{
			Profiler.Instance = new Profiler(frequency);
			game.InsertComponent(0, Profiler.Instance);
		}

		private ProfilerNode rootNode;
		private ProfilerNode currNode;
		private List<ProfilerNode> nodes;

		private readonly int frameFrequency;
		private int frameCounter;
		private bool dataReady;

		private Profiler(int frameFrequency)
		{
			this.frameFrequency = frameFrequency;
			this.frameCounter = 0;

			this.nodes = new List<ProfilerNode>();

			this.rootNode = this.currNode = new ProfilerNode(0, 0);
			this.rootNode.Name = "root";
			this.nodes.Add(this.currNode);
		}

		public override void Update(float elapsed)
		{
			if (rootNode.OpenProfiles > 0)
			{
				rootNode.End();
			}
			rootNode.Begin();

			this.Begin("update");

			this.frameCounter += 1;
			this.dataReady = this.frameCounter == this.frameFrequency;
			if (this.dataReady)
			{
				this.frameCounter = 0;
				int count = this.nodes.Count;
				for (int i = 0; i < count; i++)
					this.nodes[i].Compute();
				for (int i = 0; i < count; i++)
					this.nodes[i].Reset();
			}
		}

		internal void EndUpdate()
		{
			this.End("update");
		}

		internal void BeginRender()
		{
			this.Begin("render");

			if (this.dataReady)
			{
				// TODO: Render previous stats
			}
		}

		internal void EndRender()
		{
			this.End("render");
		}

		public void Begin(string name)
		{
			if (currNode.Name != name)
			{
				ProfilerNode n = currNode.GetChildByName(name);
				if (n == null)
				{
					n = new ProfilerNode(currNode.Depth + 1, this.nodes.Count);
					this.nodes.Add(n);
					currNode.Childern.Add(n);
					n.Parent = currNode;
				}
				if (n.Name == null || n.Name.Length == 0)
					n.Name = name;
				currNode = n;
				currNode.Begin();
			}
			else
			{
				currNode.End();
				currNode.Begin();
			}
		}

		public void End(string name)
		{
			currNode.End();
			ProfilerNode n = currNode.Parent;
			if (n != null)
			{
				currNode = n;
			}
		}
	}

	public static class ProfilerHelpers
	{
		public static void BeginProfiling(this Component component, string name)
		{
			Profiler.Instance.Begin(name);
		}
		public static void EndProfiling(this Component component, string name)
		{
			Profiler.Instance.End(name);
		}
	}
}
