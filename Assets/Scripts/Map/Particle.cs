using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Map
{
	public class Particle
	{
		private int _stabilityRadius;

		public Particle (int stabilityRadius)
		{
			_stabilityRadius = stabilityRadius;
		}

		public void Drop(int[,] map, int x, int y)
		{
			if (map [x, y] == 0)
				map [x, y]++;
			else
				Agitate (map, x, y);						
		}

		public void Agitate(int[,] map, int x, int y)
		{
			var lowerNeighbours = new List<Position> ();

			for (var i = x - _stabilityRadius; i <= x + _stabilityRadius; i++) {
				for (var j = y - _stabilityRadius; j <= y + _stabilityRadius; j++) {
					CheckAndAddNeighbour (i, j, lowerNeighbours, x, y, map);
				}
			}

			if (!lowerNeighbours.Any ()) {
				map [x, y]++;
				return;
			}

			var rand = new Random ();
			var selectedIndex = rand.Next(lowerNeighbours.Count);
            var position = lowerNeighbours[selectedIndex];
			map [position.X, position.Y]++;
		}

		private void CheckAndAddNeighbour(int neighbourX, int neighbourY, List<Position> lowerNeighbours, int dropX, int dropY, int[,] map)
		{
			if (neighbourX < 0 || neighbourX >= map.GetLength (0) || neighbourY < 0 || neighbourY >= map.GetLength (1))
				return;
			if(map [neighbourX, neighbourY] < map [dropX, dropY])
				lowerNeighbours.Add (new Position(neighbourX, neighbourY));
		}
	}
}

