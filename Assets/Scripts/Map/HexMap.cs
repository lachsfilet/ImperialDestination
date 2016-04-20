using System;
using System.Collections.Generic;

namespace Assets.Scripts.Map
{
	public class HexMap
	{
		//private readonly Dictionary<Direction, CubeCoord> _directions;

		private readonly Dictionary<Direction, Position>[] _directions;

		public HexGrid HexGrid { get; set; }

		public Tile CurrentTile { get; set; }

		public HexMap ()
		{
			/*
			_directions = new Dictionary<Direction, CubeCoord> {
				{ Direction.Northwest, CubeCoord.Create (0, +1, -1) },
				{ Direction.Northeast, CubeCoord.Create (+1, 0, -1) },
				{ Direction.East, CubeCoord.Create (+1, -1, 0) },
				{ Direction.Southeast, CubeCoord.Create (0, -1, +1) },
				{ Direction.Southwest, CubeCoord.Create (-1, 0, +1) },
				{ Direction.West, CubeCoord.Create (-1, +1, 0) }
			};
			*/

			_directions = new Dictionary<Direction, Position>[] {
				new Dictionary<Direction, Position> {
					{ Direction.Northeast, new Position (0, -1) },
					{ Direction.East, new Position (1, 0) },
					{ Direction.Southeast, new Position (0, 1) },
					{ Direction.Southwest, new Position (-1, 1) },
					{ Direction.West, new Position (-1, 0) },
					{ Direction.Northwest, new Position (-1, -1) },
				},
				new Dictionary<Direction, Position> {
					{ Direction.Northeast, new Position (1, -1) },
					{ Direction.East, new Position (1, 0) },
					{ Direction.Southeast, new Position (1, 1) },
					{ Direction.Southwest, new Position (0, 1) },
					{ Direction.West, new Position (-1, 0) },
					{ Direction.Northwest, new Position (0, -1) },
				}
			};
		}

		public Position GetNeighbour(Position start, Direction direction)
		{
			var parity = start.Y & 1;
			var dir = _directions [parity] [direction];
			return new Position (start.X + dir.X, start.Y + dir.Y);
		}

		/*
		public int GetDistance(CubeCoord a, CubeCoord b)
		{
			return Math.Max (Math.Abs (a.X - b.X), Math.Abs (a.Y - b.Y), Math.Abs (a.Z - b.Y));
		}
		*/
	}
}

