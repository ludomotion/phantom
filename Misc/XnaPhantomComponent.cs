using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Phantom.Misc
{
    public class XnaPhantomComponent : IGameComponent, IUpdateable, IDrawable
    {
        public bool Enabled
        {
            get { return true; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public int UpdateOrder
        {
            get { return 0; }
        }

        public int DrawOrder
        {
            get { return 0; }
        }

#pragma warning disable 67
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
#pragma warning restore 67

        private PhantomGame phantom;
        
        public XnaPhantomComponent( PhantomGame phantom )
        {
            this.phantom = phantom;
        }

        public void Initialize()
        {
            this.phantom.XnaInitialize();
        }

        public void Update(GameTime gameTime)
        {
            this.phantom.XnaUpdate(gameTime);
        }


        public void Draw(GameTime gameTime)
        {
            this.phantom.XnaRender(gameTime);
        }

    }
}
