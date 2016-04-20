using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
	public class ParticleDeposition : IMapGenerator
    {
		public int DropPoints { get; set; }
		public int MinParticles { get; set; }
		public int MaxParticles { get; set; }
		public int PassesCount { get; set; }
		public int ParticleStablityRadius { get; set; }

		public ParticleDeposition ()
		{
			DropPoints = 10;
			MinParticles = 100;
			MaxParticles = 400;
			PassesCount = 4;
			ParticleStablityRadius = 1;
		}

		public int[,] Generate(int width, int height)
		{
			var map = new int[width, height];

			for (var i = 0; i < PassesCount; i++) {
				var drops = CreateDrops ();
				var dropX = width / 2;
				var dropY = height / 2;

				var rand = new Random ();

				drops.ForEach (drop => {
					drop.ForEach(particle => {
							particle.Drop(map, dropX, dropY);
					});

					dropX = i == 0 ? rand.Next(width - 4) + 2 : (rand.Next(width / 2) ) + width / 4;
					dropY = i == 0 ? rand.Next(height - 4) + 2 : (rand.Next(height / 2) ) + height / 4;

					ChangeVariablesForNextPass();
				});
			}

			return map;
		}

		private List<List<Particle>> CreateDrops()
		{
			var drops = new List<List<Particle>> ();
			var rand = new Random ();
			for (var i = 0; i < DropPoints; i++) {
                var current = rand.Next(MinParticles, MaxParticles);
				drops.Add(new List<Particle>());
				for (var j = 0; j < current; j++) {
					drops [i].Add (new Particle (ParticleStablityRadius));
				}
			}
			return drops;
		}

		private void ChangeVariablesForNextPass()
		{
			DropPoints /= 2;
			PassesCount--;
			MinParticles = (int)Math.Round (MinParticles * 1.1);
			MaxParticles = (int)Math.Round (MinParticles * 1.1);
		}
	}
}

