using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using System.Diagnostics;

namespace Phantom.Graphics.Particles
{
    public class ParticleLayer : RenderLayer
    {
        private Sprite sprite;
        private int maxParticles;

        private LinkedList<Particle> particles;
        private Dictionary<Type, Queue<Particle>> graveyard;

        public ParticleLayer(float width, float height, Renderer renderer, Sprite sprite, int maxNumberOfParticles)
            :base(width, height, renderer)
        {
#if DEBUG
            if ((renderer.Options | Renderer.RenderOptions.Canvas) == Renderer.RenderOptions.Canvas)
                Debug.WriteLine("Renderer for a particle layer shouldn't have a canvas.");
#endif
            renderer.ChangeOptions(renderer.Policy, renderer.Options | Renderer.RenderOptions.NonPremultiplied);
            this.sprite = sprite;
            this.maxParticles = maxNumberOfParticles;
            this.particles = new LinkedList<Particle>();
            this.graveyard = new Dictionary<Type, Queue<Particle>>();
        }
        public ParticleLayer(Renderer renderer, Sprite sprite, int maxNumberOfParticles)
            : this(PhantomGame.Game.Width, PhantomGame.Game.Height, renderer, sprite, maxNumberOfParticles)
        {
        }

        public override void Update(float elapsed)
        {
            LinkedListNode<Particle> node = this.particles.First;
            while( node != null )
            {
                LinkedListNode<Particle> next = node.Next;
                Particle p = node.Value;
                if (p.Active)
                {
                    p.Integrate(elapsed);
                    if (p.Life <= 0 || !p.Active)
                    {
                        this.Bury(p);
                        this.particles.Remove(node);
                        continue;
                    }
                }
                node = next;
            }
            base.Update(elapsed);
        }

        public override void Render(RenderInfo info)
        {
            if (info != null)
            {
                foreach (Particle p in this.particles)
                    if (p.Active)
                        p.Render(info, this.sprite);
            } else 
                base.Render(info);
        }

        public void AddParticle(Particle p)
        {
            if (this.particles.Count >= this.maxParticles)
            {
                this.Bury(this.particles.First.Value);
                this.particles.RemoveFirst();
            }
            this.particles.AddLast(p);
        }

        private void Bury(Particle p)
        {
            p.Deactivate();
            Type t = p.GetType();
            if (!this.graveyard.ContainsKey(t))
                this.graveyard[t] = new Queue<Particle>();
            this.graveyard[t].Enqueue(p);
        }

        public T GetDeadOrCreate<T>() where T : Particle
        {
            Type t = typeof(T);
            if (!this.graveyard.ContainsKey(t))
                return CreateNewParticle<T>();
            if (this.graveyard[t].Count <= 0)
                return CreateNewParticle<T>();
            return this.graveyard[t].Dequeue() as T;
        }

        private T CreateNewParticle<T>() where T : Particle
        {
            T p = Activator.CreateInstance<T>();
            return p;
        }
    }
}
