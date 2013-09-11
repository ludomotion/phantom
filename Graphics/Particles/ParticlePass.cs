using System;
using System.Collections.Generic;
using System.Text;
using Phantom.Core;
using System.Diagnostics;

namespace Phantom.Graphics.Particles
{
    public class ParticlePass : Component
    {
        private Sprite sprite;
        private int maxParticles;
        private int renderPass;
        private int particlePasses;

        private LinkedList<Particle> particles;
        private Dictionary<Type, Queue<Particle>> graveyard;

        public ParticlePass(Sprite sprite, int maxNumberOfParticles, int renderPass, int particlePasses)
            :base()
        {
            this.sprite = sprite;
            this.maxParticles = maxNumberOfParticles;
            this.particles = new LinkedList<Particle>();
            this.graveyard = new Dictionary<Type, Queue<Particle>>();
            this.renderPass = renderPass;
            this.particlePasses = particlePasses;
        }

        public override void Update(float elapsed)
        {
            LinkedListNode<Particle> node = this.particles.First;
            while (node != null)
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
                    }
                }
                node = next;
            }
            base.Update(elapsed);
        }

        public override void Render(RenderInfo info)
        {
            if (info.Pass == renderPass)
            {
                for (int i = 0; i < particlePasses; i++)
                {
                    info.Pass = i;
                    foreach (Particle p in this.particles)
                        if (p.Active)
                            p.Render(info, this.sprite);
                }
                info.Pass = renderPass;
            }
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
