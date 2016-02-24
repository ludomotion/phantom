using Microsoft.Xna.Framework.Graphics;
using Phantom.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Phantom.Assets
{
    public class PhantomAssets
    {
        /// <summary>
        /// A simple 1x1 white pixel.
        /// </summary>
        public static readonly Texture2D White;

        /// <summary>
        /// A plain sans font 16 pixels in height.
        /// </summary>
        public static readonly Phont Sans16;

        /// <summary>
        /// Courier New, Bold, 20 pixels in height.
        /// </summary>
        public static readonly Phont Courier20Bold;

        static PhantomAssets()
        {
            Stream stream;
            Assembly assembly = Assembly.GetAssembly(typeof(PhantomAssets));

            stream = assembly.GetManifestResourceStream("Phantom.Assets.white.png");
            White = Texture2D.FromStream(PhantomGame.Game.GraphicsDevice, stream);

            stream = assembly.GetManifestResourceStream("Phantom.Assets.courier20bold.png");
            Texture2D courier20tex = Texture2D.FromStream(PhantomGame.Game.GraphicsDevice, stream);
            Courier20Bold = new PhontMono(courier20tex, 0.8f);

            stream = assembly.GetManifestResourceStream("Phantom.Assets.sans16.png");
            Texture2D sans16tex = Texture2D.FromStream(PhantomGame.Game.GraphicsDevice, stream);
            Sans16 = new Phont(sans16tex, 0.45f, 0.6f, 0.1f, 0.0f, 0.10f, 0.9f);
        }
    }
}
