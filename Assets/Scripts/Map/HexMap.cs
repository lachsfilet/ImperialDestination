﻿using Assets.Contracts;
using Assets.Contracts.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class HexMap : IHexMap
    {
        private Dictionary<Direction, Position>[] _directions;
        private TileBase[,] _map;
        
        public int Height { get; private set; }

        public int Width { get; private set; }

        public GameObject HexTile { get; private set; }

        public HexMap(int height, int width)
        {
            Height = height;
            Width = width;
            _map = new TileBase[width,height];
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

        public void AddTile(int x, int y, TileBase tile)
        {
            if(x < _map.GetLength(0) && y < _map.GetLength(1))
                _map[x, y] = tile;
        }

        public TileBase GetTile(int x, int y)
        {
            if (x < _map.GetLength(0) && y < _map.GetLength(1))
                return _map[x, y];
            return null;
        }

        public TileBase GetTile(Position position)
        {
            if (position is null)            
                throw new ArgumentNullException(nameof(position));            

            return _map[position.X, position.Y];
        }
        
        public IEnumerable<TileBase> GetTilesOfTerrainType(TileTerrainType terrainType)
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

        public TileBase GetNeighbour(TileBase hexTile, Direction direction)
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
        
        public TilePair GetPairWithDirection(TileBase hexTile, TileBase neighbour)
        {
            var neighbours = GetNeighboursWithDirection(hexTile);
            return neighbours.SingleOrDefault(n => n.Neighbour == neighbour);
        }

        public IEnumerable<TileBase> GetNeighbours(TileBase hexTile, bool reverse = false)
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

        public IEnumerable<TilePair> GetNeighboursWithDirection(TileBase hexTile, bool reverse = false)
        {
            var directions = Enum.GetValues(typeof(Direction)).Cast<Direction>();
            if (reverse)
                directions = directions.OrderByDescending(d => d);

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

        public TileBase GetNextNeighbour(TileBase hexTile, TileBase currentNeighbour)
        {
            var neighbours = GetNeighbours(hexTile).ToList();
            var index = neighbours.IndexOf(currentNeighbour);
            if (index < neighbours.Count - 1)
                return neighbours[++index];
            return neighbours[0];
        }
        
        public TilePair GetNextNeighbourWithDirection(TileBase hexTile, TileBase currentNeighbour, bool reverse = false)
        {
            var neighbours = GetNeighboursWithDirection(hexTile, reverse).ToList();
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
            var z = position.Y;
            var y = -x - z;
            return new Vector3(x, y, z);
        }

        public float GetDistance(Vector3 a, Vector3 b) =>
            Math.Max(Math.Abs(a.x - b.x), Math.Max(Math.Abs(a.y - b.y), Math.Abs(a.z - b.z)));

        public float GetDistance(Position a, Position b)
        {
            var ac = ConvertPositionToCube(a);
            var bc = ConvertPositionToCube(b);

            return GetDistance(ac, bc);
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;
               
        private Vector3 LerpCube(Vector3 a, Vector3 b, float t) =>
            new Vector3(Lerp(a.x, b.x, t),
                Lerp(a.y, b.y, t),
                Lerp(a.z, b.z, t));
                
        private Vector3 RoundCube(Vector3 cube)
        {
            var rx = (float)Math.Round(cube.x);
            var ry = (float)Math.Round(cube.y);
            var rz = (float)Math.Round(cube.z);

            var xDiff = Math.Abs(rx - cube.x);
            var yDiff = Math.Abs(ry - cube.y);
            var zDiff = Math.Abs(rz - cube.z);

            if (xDiff > yDiff && xDiff > zDiff)
                rx = -ry - rz;
            else if (yDiff > zDiff)
                ry = -rx - rz;
            else
                rz = -rx - ry;
            return new Vector3(rx, ry, rz);
        }
        
        public IEnumerable<Vector3> DrawLine(Vector3 a, Vector3 b)
        {
            var distance = GetDistance(a, b);
            for (var i = 0; i <= distance; i++)
            {
                yield return RoundCube(LerpCube(a, b, 1.0f / distance * i));
            }
        }

        public IEnumerable<Position> DrawLine(Position a, Position b)
        {
            var ac = ConvertPositionToCube(a);
            var bc = ConvertPositionToCube(b);

            return DrawLine(ac, bc).Select(c => ConvertCubeToPosition(c));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public IEnumerator<TileBase> GetEnumerator()
        {
            var iterator = _map.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (TileBase)iterator.Current;
            }
        }
    }
}

