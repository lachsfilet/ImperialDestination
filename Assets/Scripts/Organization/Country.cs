using Assets.Scripts.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Game;

namespace Assets.Scripts.Organization
{
    public class Country : MonoBehaviour
    {
        public string Name { get; set; }

        public Player Player { get; set; }

        public Color Color { get; set; }

        public CountryType CountryType { get; set; }

        public List<Province> Provinces { get; set; }

        public Continent Continent { get; set; }

        public Tile Capital { get; private set; }

        public Country()
        {
            Provinces = new List<Province>();
        }

        public void SetCapital(HexMap map)
        {
            // First try to set any harbor city
            var cities = Provinces.SelectMany(p => p.HexTiles.Where(t => t.TileTerrainType == TileTerrainType.City 
                && map.GetNeighbours(t).Where(n => n.TileTerrainType == TileTerrainType.Water).Any())).ToList();
            if(!cities.Any())
                cities = Provinces.SelectMany(p => p.HexTiles.Where(t => t.TileTerrainType == TileTerrainType.City)).ToList();
            var rand = new System.Random();
            var index = rand.Next(cities.Count);
            Capital = cities[index];
            Capital.Province.IsCapital = true;
        }

        public void DrawBorder(HexMap map)
        {
            // Get first tile with any neighbour of another province
            var tiles = Provinces.SelectMany(p => p.HexTiles).ToList();
            var firstBorderTile = tiles.Where(t => map.GetNeighbours(t).Where(n => n.Province == null || n.Province.Owner != this).Any()).First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstBorderTile).ToList();
            var neighbourPair = neighbourPairs.Where(n => n.Neighbour.Province == null || n.Neighbour.Province.Owner != this).First();
            var borderRoute = new List<TilePair>();

            TraceBorder(neighbourPair, borderRoute, map);
            var vectors = borderRoute.SelectMany(p => p.HexTile.GetVertices(p.Direction)).Distinct().ToArray();

            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = vectors.Length;
            //lineRenderer.SetVertexCount(vectors.Length);
            lineRenderer.SetPositions(vectors);
        }

        private void TraceBorder(TilePair tilePair, List<TilePair> borderRoute, HexMap map)
        {
            // Abort if current combination is already stored
            if (borderRoute.Any(p => p.Neighbour == tilePair.Neighbour && p.HexTile == tilePair.HexTile))
                return;

            borderRoute.Add(tilePair);

            var nextPair = map.GetNextNeighbourWithDirection(tilePair.HexTile, tilePair.Neighbour);

            if (nextPair.Neighbour.Province == null || nextPair.Neighbour.Province.Owner != tilePair.HexTile.Province.Owner)
            {
                // Move on with current tile and next neighbour province tile
                TraceBorder(nextPair, borderRoute, map);
                return;
            }

            // Take next own province tile and current neighbour province tile
            var newCurrentProvinceTile = nextPair.Neighbour;
            var lastNeighbourProvinceTile = borderRoute.Where(t => t.HexTile == tilePair.HexTile).Select(t => t.Neighbour).Last();
            var newPair = map.GetPairWithDirection(newCurrentProvinceTile, lastNeighbourProvinceTile);
            if (newPair == null)
                return;
            TraceBorder(newPair, borderRoute, map);
        }
    }
}
