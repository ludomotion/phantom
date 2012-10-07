using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Phantom.Misc;

namespace Phantom
{
    public class PhantomGame : Component, IDisposable
    {
        public static PhantomGame Game { get; private set; }

        public string Name { get; protected set; }
        public Color BackgroundColor { get; protected set; }

        public float Width { get; private set; }
        public float Height { get; private set; }

        private Microsoft.Xna.Framework.Game XnaGame;
        private GraphicsDeviceManager graphics;
        public GraphicsDevice GraphicsDevice { get; private set; }

        public IList<GameState> StateStack { get; protected set; }

        public PhantomGame( float width, float height )
        {
            PhantomGame.Game = this;

            this.Width = width;
            this.Height = height;

            this.Name = "PhantomGame"; // TODO: Get project name from assembly
            this.BackgroundColor = 0x123456.ToColor();

            this.XnaGame = new Microsoft.Xna.Framework.Game();
            this.XnaGame.Window.Title = this.Name;
            this.XnaGame.Components.Add(new XnaPhantomComponent(this));
            this.graphics = new GraphicsDeviceManager(this.XnaGame);
            this.XnaGame.Content.RootDirectory = "Content";

            this.StateStack = new List<GameState>();
        }

        public void Dispose()
        {
        }

        public void Run()
        {
            this.XnaGame.Run();
        }

        protected virtual void Initialize()
        {
        }

        internal void XnaInitialize()
        {
            this.GraphicsDevice = this.XnaGame.GraphicsDevice;
            this.Initialize();
        }

        internal void XnaUpdate(GameTime gameTime)
        {
            if (this.StateStack.Count == 0)
                return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = this.StateStack.Count - 1; i >= 0; i--)
            {
                this.StateStack[i].Update(elapsed);
                if (!this.StateStack[i].Propagate)
                    return;
            }
        }

        internal void XnaRender(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(0x123456.ToColor());
            if (this.StateStack.Count == 0)
                return;

            int i;
            for (i = this.StateStack.Count - 1; i >= 0 && this.StateStack[i].Transparent; i--);
            for (int j = i; j < this.StateStack.Count; j++)
                this.StateStack[j].Render(null);
        }

        public override void OnComponentAdded(Component component)
        {
            base.OnComponentAdded(component);
            if (component is GameState)
            {
                this.StateStack.Add(component as GameState);
            }
        }

    }
}
