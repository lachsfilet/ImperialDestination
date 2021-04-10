using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Game;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class Country : MonoBehaviour, ICountry
    {
        public string Name { get; set; }

        public Player Player { get; set; }

        public Color Color { get; set; }

        public CountryType CountryType { get; set; }

        public List<IProvince> Provinces { get; set; }

        public IContinent Continent { get; set; }

        public TileBase Capital { get; private set; }

        public Country()
        {
            Provinces = new List<IProvince>();
        }

        public void AddProvince(IProvince province)
        {
            Provinces.Add(province);
            province.IsWater = false;
            var provinceObject = (Province)province;
            provinceObject.transform.SetParent(transform);
            province.Owner = this;
        }

        public void RemoveProvince(IProvince province)
        {
            Provinces.Remove(province);
            province.Owner = null;
        }

        public void SetCapital(IHexMap map)
        {
            // First try to set any harbor city
            var allCities = Provinces.Select(p => p.Capital).ToList();
            var harborCities = allCities.Where(t => map.GetNeighbours(t).Where(n => n.TileTerrainType == TileTerrainType.Water).Any()).ToList();
            var cities = harborCities.Any() ? harborCities : allCities;
            var rand = new System.Random();
            var index = rand.Next(cities.Count);
            Capital = cities[index];
            Capital.Province.IsCapital = true;
        }

        public void DrawBorder(IHexMap map)
        {
            // Get first tile with any neighbour of another province
            var tiles = Provinces.SelectMany(p => p.HexTiles).ToList();
            var firstBorderTile = tiles.Where(t => map.GetNeighbours(t).Where(n => n.Province == null || (Country)n.Province.Owner != this).Any()).First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstBorderTile).ToList();
            var neighbourPair = neighbourPairs.Where(n => n.Neighbour.Province == null || (Country)n.Neighbour.Province.Owner != this).First();
            var borderRoute = new List<TilePair>();

            TraceBorder(neighbourPair, borderRoute, map);
            var vectors = borderRoute.SelectMany(p => p.HexTile.GetVertices(p.Direction)).Distinct().ToArray();

            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = vectors.Length;
            //lineRenderer.SetVertexCount(vectors.Length);
            lineRenderer.SetPositions(vectors);
        }

        private void TraceBorder(TilePair tilePair, List<TilePair> borderRoute, IHexMap map)
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

        public Transform GetParent() => this.transform.parent;

        public void SetParent(Transform transform)
        {
            this.transform.parent = transform;
        }
    }
}