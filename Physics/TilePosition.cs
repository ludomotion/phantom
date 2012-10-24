using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;

namespace Phantom.Physics
{
    internal class TilePosition : EntityComponent
    {
        private TiledIntegrator integrator;
        public TiledIntegrator.Tile Tile;

        public TilePosition(TiledIntegrator integrator)
        {
            this.SetIntegrater(integrator);
        }

        public void SetIntegrater(TiledIntegrator integrator)
        {
            this.integrator = integrator;
        }

        public override void OnAdd(Component parent)
        {
            base.OnAdd(parent);
            this.SetTile(this.integrator.GetTile(this.Entity.Position));
        }

        public void SetTile(TiledIntegrator.Tile tile)
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
            this.SetTile(this.integrator.GetTile(this.Entity.Position));
            base.Integrate(elapsed);
        }

    }
}
