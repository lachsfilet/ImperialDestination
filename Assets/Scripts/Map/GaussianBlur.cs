using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
	public class GaussianBlur
	{
		private readonly List<GaussValue> _kernel;

		public GaussianBlur()
		{
			_kernel = new List<GaussValue> { 
				new GaussValue(-3, 0.006), 
				new GaussValue(-2, 0.061), 
				new GaussValue(-1, 0.242), 
				new GaussValue(0, 0.383), 
				new GaussValue(1, 0.242), 
				new GaussValue(2, 0.061), 
				new GaussValue(3, 0.006)
			};
		}

		public int[,] Filter(int[,] sourceMap)
		{
			var width = sourceMap.GetLength (0);
			var height = sourceMap.GetLength (1);
			var intermediate = new int[width, height];
			var result = new int[width, height];

			for (var i = 0; i < width; i++) {
				for (var j = 0; j < height; j++) {
					intermediate [i, j] = ComputeX (sourceMap, i, j);
				}
			}

			for (var i = 0; i < width; i++) {
				for (var j = 0; j < height; j++) {
					result [i, j] = ComputeY (intermediate, i, j);
				}
			}
			return result;
		}

		private int ComputeX(int[,] map, int x, int y)
		{
			var value = 0;
			_kernel.ForEach (t => {
				var offset = x + t.Position >= 0 && x + t.Position < map.GetLength(0) ? t.Position : 0;
			    
				value += (int)Math.Round(t.Value * map[offset + x, y]);
			});
			return value;
		}

		private int ComputeY(int[,] map, int x, int y)
		{
			var value = 0;
			_kernel.ForEach (t => {
				var offset = y + t.Position >= 0 && y + t.Position < map.GetLength(1) ? t.Position : 0;

				value += (int)Math.Round(t.Value * map[x, y + offset]);
			});
			return value;
		}
	}
}

