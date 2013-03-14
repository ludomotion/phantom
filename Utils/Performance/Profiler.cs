using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Phantom.Misc;

namespace Phantom.Utils.Performance
{
	public class Profiler : Component
	{
		public static Profiler Instance { get; private set; }

        public static SpriteFont font = null;

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

        private SpriteBatch batch;
        private Texture2D pixel;
        private int width = 400;
        private int height = 0;
        private int left = 1280 - 400;
        private int top = 0;
        private Color background;

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

		}

		internal void EndRender()
		{
			if (font != null)
			{
                if (batch == null)
                {
                    this.batch = new SpriteBatch(PhantomGame.Game.GraphicsDevice);
                    pixel = new Texture2D(PhantomGame.Game.GraphicsDevice, 1, 1);
                    pixel.SetData(new [] { Color.White });
                    background = Color.Black;
                    background.A = 128;
                }

                this.batch.Begin();

                Vector2 position = new Vector2(left, top);


                this.batch.Draw(pixel, new Microsoft.Xna.Framework.Rectangle(left, top, width, height), background);

                rootNode.Render(batch, font, ref position, left + width-240);
                height = (int)Math.Max(height, position.Y - top);

                lock (PhantomGame.Game.GlobalRenderLock)
                {
                    this.batch.End();
                }

			}
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
