using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class HeightMapGenerator : IHeightMapGenerator
    {
        private const int MOUNTAIN_COUNT_MIN = 5;

        private double _ratio;

        private Func<int, int, int> _random;

        public HeightMapGenerator(double ratio) : this(ratio, UnityEngine.Random.Range)
        {
        }

        public HeightMapGenerator(double ratio, Func<int, int, int> random)
        {
            _ratio = ratio;
            _random = random;
        }

        public void GenerateHeightMap(IHexMap hexMap, int ratio)
        {
            var heightMap = new Dictionary<TileBase, TileTerrainType>();
            var continents = hexMap.Where(t => t.TileTerrainType != TileTerrainType.Water).Select(t => t.Province.Owner.Continent).Distinct();

            foreach (var continent in continents)
            {
                var sectors = SliceContinentSectors(continent);

                var index = _random(0, 4);
                var startSector = sectors[index];
                sectors.Remove(startSector);

                index = _random(0, 3);
                var endSector = sectors[index];

                // Determine start of mountains
                index = _random(0, startSector.Count);
                var startTile = startSector[index];

                // Determine end of mountains
                index = _random(0, endSector.Count);
                var endTile = endSector[index];

                var mountains = hexMap.DrawLine(startTile.Position, endTile.Position).ToList();
                var mountainTiles = mountains.Select(m => hexMap.GetTile(m))
                    .Where(t => t.TileTerrainType != TileTerrainType.Water).ToList();

                //Debug.Log($"Continent: {continent.TileCount} Ratio: {_ratio} Mountains: {mountainTiles.Count} Mountains to extend {(int)Math.Round(continent.TileCount * _ratio) - mountainTiles.Count}");
                var count = Math.Max((int)Math.Round(continent.TileCount * _ratio) - mountainTiles.Count, MOUNTAIN_COUNT_MIN);
                ExtendMountains(mountainTiles, hexMap, count);

                foreach (var mountain in mountainTiles)
                    mountain.TileTerrainType = TileTerrainType.Mountains;

                var hills = GenerateSurroundingHills(mountainTiles, hexMap);
                foreach (var hill in hills)
                    hill.TileTerrainType = TileTerrainType.Hills;
            }
        }

        private List<List<TileBase>> SliceContinentSectors(IContinent continent)
        {
            var tiles = continent.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)).ToList();

            var west = tiles.Min(t => t.Position.X);
            var east = tiles.Max(t => t.Position.X);

            var north = tiles.Max(t => t.Position.Y);
            var south = tiles.Min(t => t.Position.Y);

            var centerX = west / 2 + east / 2;
            var centerY = north / 2 + south / 2;

            var comparer = new PositionComparer(east);

            Func<TileBase, bool> predicate = t => t.TileTerrainType == TileTerrainType.Plain;

            var sectors = new List<List<TileBase>>
                {
                    tiles.Where(predicate).Where(t => t.Position.X <= centerX && t.Position.Y > centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(predicate).Where(t => t.Position.X > centerX && t.Position.Y > centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(predicate).Where(t => t.Position.X <= centerX && t.Position.Y <= centerY).OrderBy(p=>p.Position, comparer).ToList(),
                    tiles.Where(predicate).Where(t => t.Position.X > centerX && t.Position.Y <= centerY).OrderBy(p=>p.Position, comparer).ToList()
                };

            if (sectors.Any(s => !s.Any()))
            {
                Debug.LogError("Empty continent sector detected!");
                Debug.Log(string.Join(",", sectors.Select((s, i) => $"Sector {i}: {string.Join(", ", s.Select(t => t.ToString()))}")));
            }

            return sectors;
        }

        private static ICollection<TileBase> GenerateSurroundingHills(ICollection<TileBase> mountains, IHexMap map)
        {
            var range = 2;

            var result = new List<TileBase>();

            foreach (var mountain in mountains)
            {
                var subresult = new List<TileBase>();
                //Debug.Log($"Mountain at {mountain.Position}");

                var fringes = new List<List<TileBase>>
                {
                    new List<TileBase> { mountain }
                };

                for (var i = 1; i <= range; i++)
                {
                    foreach (var tile in fringes[i - 1])
                    {
                        //Debug.Log($"Fringe {i - 1}, Tile: {tile.TileTerrainType} at {tile.Position}");

                        fringes.Add(new List<TileBase>());
                        var neighbours = map.GetNeighbours(tile).ToList();
                        foreach (var neighbour in neighbours
                            .Where(n => n.TileTerrainType != TileTerrainType.Water
                                && n.TileTerrainType != TileTerrainType.City
                                && !mountains.Contains(n)
                                && !subresult.Contains(n)))
                        {
                            subresult.Add(neighbour);

                            //Debug.Log($"Fringe {i}, Add tile: {neighbour.TileTerrainType} at {neighbour.Position}");
                            fringes[i].Add(neighbour);
                        }
                    }
                }
                result.AddRange(subresult.Where(t => !result.Contains(t)));
            }
            return result;
        }

        private void ExtendMountains(ICollection<TileBase> mountains, IHexMap map, int count)
        {
            //Debug.Log($"Extend mountains: {count}");
            for (var i = 0; i < count; i++)
            {
                var index = _random(0, mountains.Count - 1);
                var field = mountains.ElementAt(index);

                var neighbours = map.GetNeighbours(field)
                    .Where(n => n.TileTerrainType != TileTerrainType.Water
                        && n.TileTerrainType != TileTerrainType.City
                        && !mountains.Contains(n)).ToList();
                if (neighbours.Count == 0)
                {
                    i--;
                    continue;
                }

                index = _random(0, neighbours.Count - 1);
                var neighbour = neighbours[index];
                //Debug.Log($"Add mountain {i}: {neighbour.Position}");
                mountains.Add(neighbour);
            }
        }
    }
}