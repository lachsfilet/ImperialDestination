using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class HexMap : IEnumerable<Tile>
    {
        private Dictionary<Direction, Position>[] _directions;
        private Tile[,] _map;
        
        public int Height { get; private set; }
        public int Width { get; private set; }
        public GameObject HexTile { get; private set; }

        public HexMap(int height, int width)
        {
            Height = height;
            Width = width;
            _map = new Tile[width,height];
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

        public void AddTile(int x, int y, Tile tile)
        {
            if(x < _map.GetLength(0) && y < _map.GetLength(1))
                _map[x, y] = tile;
        }

        public Tile GetTile(int x, int y)
        {
            if (x < _map.GetLength(0) && y < _map.GetLength(1))
                return _map[x, y];
            return null;
        }

        public IEnumerable<Tile> GetTilesOfTerrainType(TileTerrainType terrainType)
        {
            for(var i = 0; i < _map.GetLength(0); i++)
            {
                for (var j = 0; j < _map.GetLength(1); j++)
                {
                    var tile = _map[i, j];
                    if (tile.TileTerrainType == terrainType)
                        yield return tile;
                }
            }
        }

        public Tile GetNeighbour(Tile hexTile, Direction direction)
        {
            if (hexTile == null)
                return null;

            var parity = hexTile.Position.Y & 1;
            var position = _directions[parity][direction];
            var neighbour = hexTile.Position + position;

            if (neighbour.X < 0 || neighbour.X >= Width || neighbour.Y < 0 || neighbour.Y >= Height)
                return null;
            return _map[neighbour.X, neighbour.Y];
        }
        
        public TilePair GetPairWithDirection(Tile hexTile, Tile neighbour)
        {
            var neighbours = GetNeighboursWithDirection(hexTile);
            return neighbours.Where(n => n.Neighbour == neighbour).SingleOrDefault();
        }

        public IEnumerable<Tile> GetNeighbours(Tile hexTile, bool reverse = false)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            if (reverse)
                directions = directions.OrderByDescending(d => d);
            foreach (var direction in directions)
            {
                var neighbour = GetNeighbour(hexTile, direction);
                if (neighbour != null)
                    yield return neighbour;
            }
        }

        public IEnumerable<TilePair> GetNeighboursWithDirection(Tile hexTile)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            foreach (var direction in directions)
            {
                var neighbour = GetNeighbour(hexTile, direction);
                if (neighbour != null)
                    yield return new TilePair
                    {
                        HexTile = hexTile,
                        Neighbour = neighbour,
                        Direction = direction
                    };
            }
        }

        public Tile GetNextNeighbour(Tile hexTile, Tile currentNeighbour)
        {
            var neighbours = GetNeighbours(hexTile).ToList();
            var index = neighbours.IndexOf(currentNeighbour);
            if (index < neighbours.Count - 1)
                return neighbours[++index];
            return neighbours[0];
        }
        
        public TilePair GetNextNeighbourWithDirection(Tile hexTile, Tile currentNeighbour)
        {
            var neighbours = GetNeighboursWithDirection(hexTile).ToList();
            var index = neighbours.Select(n=>n.Neighbour).ToList().IndexOf(currentNeighbour);
            if (index < neighbours.Count - 1)
                return neighbours[++index];
            return neighbours[0];
        }

        public Position ConvertCubeToPosition(Vector3 cube)
        {
            // Convert cube to odd-r offset
            var x = (int)(cube.x + (cube.z - ((int)cube.z & 1)) / 2);
            var y = (int)cube.z;
            return new Position(x, y);
        }

        public Vector3 ConvertPositionToCube(Position position)
        {
            // Convert odd-r offset to cube
            var x = position.X - (position.Y - (position.Y & 1)) / 2;
            var z = position.X;
            var y = -x - z;
            return new Vector3(x, y, z);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public IEnumerator<Tile> GetEnumerator()
        {
            var iterator = _map.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (Tile)iterator.Current;
            }
        }
    }
}

