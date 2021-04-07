using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class Province : MonoBehaviour, IProvince
    {
        private List<TileBase> _hexTiles;

        private List<TilePair> _borderRoute;

        private IList<IProvince> _neighbours;

        public Province()
        {
            _hexTiles = new List<TileBase>();
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                var textMesh = GetComponent<TextMesh>();
                textMesh.text = _name;
            }
        }

        public ICountry Owner { get; set; }

        public TileBase Capital { get; private set; }

        public bool IsCapital { get; set; }

        public IEnumerable<TileBase> HexTiles => _hexTiles;

        public bool IsWater { get; set; }

        public void AddHexTile(TileBase hexTile)
        {
            if (hexTile == null)
                throw new ArgumentNullException(nameof(hexTile));

            _hexTiles.Add(hexTile);
            hexTile.Province = this;
        }

        public void SetCapital(IHexMap map)
        {
            var tiles = HexTiles.ToList();
            var innerTiles = tiles.Where(t => map.GetNeighbours(t).All(n => (Province)n.Province == this)).ToList();
            if (!innerTiles.Any())
                innerTiles = tiles;
            var rand = new System.Random();
            var index = rand.Next(innerTiles.Count);
            Capital = innerTiles[index];
            Capital.TileTerrainType = TileTerrainType.City;
        }

        public void ResetProvince(GameObject mapObject)
        {
            IsWater = true;
            IsCapital = false;
            Owner.RemoveProvince(this);
            Capital = null;
            foreach (var tile in HexTiles)
            {
                tile.TileTerrainType = TileTerrainType.Water;
                tile.transform.SetParent(mapObject.transform);
            }
        }

        public IList<IProvince> GetNeighbours(IHexMap map)
        {
            if (_neighbours != null)
                return _neighbours;

            TraceBorder(map);

            _neighbours = _borderRoute.Select(b => b.Neighbour.Province).Distinct().ToList();
            return _neighbours;
        }

        public void ArrangePosition()
        {
            var positions = _hexTiles.Select(h => h.transform.position).ToList();
            var minX = positions.Min(p => p.x);
            var minZ = positions.Min(p => p.z);
            var maxX = positions.Max(p => p.x);
            var maxZ = positions.Max(p => p.z);

            var x = (maxX + minX) / 2f;
            var z = (maxZ + minZ) / 2f;

            this.transform.position = new Vector3(x, 0, z);
        }

        public void DrawBorder(IHexMap map)
        {
            TraceBorder(map);

            var vectors = _borderRoute.SelectMany(p => p.HexTile.GetVertices(p.Direction)).Distinct().ToArray();

            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = vectors.Length;
            //lineRenderer.SetVertexCount(vectors.Length);
            lineRenderer.SetPositions(vectors);
        }

        public override string ToString()
            => Name;

        private void TraceBorder(IHexMap map)
        {
            if (_borderRoute != null)
                return;

            // Get first tile with any neighbour of another province
            var tiles = HexTiles.ToList();
            var firstBorderTile = tiles.Where(t => map.GetNeighbours(t).Where(n => (Province)n.Province != this).Any()).First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstBorderTile).ToList();
            var neighbourPair = neighbourPairs.Where(n => (Province)n.Neighbour.Province != this).First();
            _borderRoute = new List<TilePair>();

            TraceBorder(neighbourPair, map, false);
        }

        private void TraceBorder(TilePair tilePair, IHexMap map, bool reverse)
        {
            // Abort if current combination is already stored
            if (_borderRoute.Any(p => p.Neighbour == tilePair.Neighbour && p.HexTile == tilePair.HexTile))
                return;

            if(reverse)
                _borderRoute.Insert(0, tilePair);
            else
                _borderRoute.Add(tilePair);

            var nextPair = map.GetNextNeighbourWithDirection(tilePair.HexTile, tilePair.Neighbour, reverse);

            if (nextPair.Neighbour.Province != tilePair.HexTile.Province)
            {
                // Move on with current tile and next neighbour province tile
                TraceBorder(nextPair, map, reverse);
                return;
            }

            // Take next own province tile and current neighbour province tile
            var newCurrentProvinceTile = nextPair.Neighbour;
            var lastNeighbourProvinceTile = tilePair.Neighbour;//_borderRoute.Where(t => t.HexTile == tilePair.HexTile).Select(t => t.Neighbour).Last();
            var newPair = map.GetPairWithDirection(newCurrentProvinceTile, lastNeighbourProvinceTile);

            if (newPair != null)
                TraceBorder(newPair, map, reverse);

            //Debug.Log($"{this}, border: {string.Join(", ", _borderRoute.Select(b => "tile: " + b.HexTile + ", neighbour: " + b.Neighbour))}");
            //Debug.Log($"{this}: Last pair: tile: {tilePair.HexTile}, neighbour {tilePair.Neighbour}");
            //Debug.Log($"{this}: Next pair: tile: {nextPair.HexTile}, neighbour {nextPair.Neighbour}");

            // Dead end -> go backwards from the first list entry
            var firstPair = _borderRoute.First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstPair.HexTile, true).ToList();
            newPair = neighbourPairs.Where(n => (Province)n.Neighbour.Province != this).First();
            TraceBorder(newPair, map, true);
        }
    }
}