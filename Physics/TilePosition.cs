using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics
{
    internal class TilePosition : EntityComponent
    {
        private TiledIntegrater integrater;
        public TiledIntegrater.Tile Tile;

        public TilePosition(TiledIntegrater integrater)
        {
            this.SetIntegrater(integrater);
        }

        public void SetIntegrater(TiledIntegrater integrater)
        {
            this.integrater = integrater;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            this.SetTile(this.integrater.GetTile(this.Entity.Position));
        }

        public void SetTile(TiledIntegrater.Tile tile)
        {
            if (this.Tile == tile)
                return;
            if (this.Tile != null)
                this.Tile.Entities.Remove(this.Entity);
            this.Tile = tile;
            if( this.Tile != null )
                this.Tile.Entities.Add(this.Entity);
        }

        public override void Integrate(float elapsed)
        {
            this.SetTile(this.integrater.GetTile(this.Entity.Position));
            base.Integrate(elapsed);
        }

    }
}
