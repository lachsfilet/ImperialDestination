using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Map;
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
        public string Name {
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
                return;

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

        public IList<IProvince> GetNeighbours(IHexMap map)
        {
            if (_neighbours != null)
                return _neighbours;

            TraceBorder(map);

            _neighbours = _borderRoute.Select(b => b.Neighbour.Province).Where(p => p != null).Distinct().ToList();
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

        public void DrawBorder(HexMap map)
        {
            TraceBorder(map);

            var vectors = _borderRoute.SelectMany(p => p.HexTile.GetVertices(p.Direction)).Distinct().ToArray();

            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = vectors.Length;
            //lineRenderer.SetVertexCount(vectors.Length);
            lineRenderer.SetPositions(vectors);
        }

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

            TraceBorder(neighbourPair, _borderRoute, map);
        }

        private void TraceBorder(TilePair tilePair, List<TilePair> borderRoute, IHexMap map)
        {
            // Abort if current combination is already stored
            if (borderRoute.Any(p => p.Neighbour == tilePair.Neighbour && p.HexTile == tilePair.HexTile))
                return;

            borderRoute.Add(tilePair);

            var nextPair = map.GetNextNeighbourWithDirection(tilePair.HexTile, tilePair.Neighbour);

            if(nextPair.Neighbour.Province != tilePair.HexTile.Province)
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
