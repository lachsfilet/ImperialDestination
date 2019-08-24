using Assets.Scripts.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class Province : MonoBehaviour
    {
        private List<Tile> _hexTiles;

        private List<TilePair> _borderRoute;

        private IList<Province> _neighbours;

        public Province()
        {
            _hexTiles = new List<Tile>();
        }

        public string Name { get; set; }

        public Country Owner { get; set; }

        public Tile Capital { get; private set; }

        public bool IsCapital { get; set; }

        public IEnumerable<Tile> HexTiles => _hexTiles;

        public void AddHexTile(Tile hexTile)
        {
            if (hexTile == null)
                return;

            _hexTiles.Add(hexTile);
            hexTile.Province = this;
        }

        public void SetCapital(HexMap map)
        {
            var tiles = HexTiles.ToList();
            var innerTiles = tiles.Where(t => map.GetNeighbours(t).All(n => n.Province == this)).ToList();
            if (!innerTiles.Any())
                innerTiles = tiles;
            var rand = new System.Random();
            var index = rand.Next(innerTiles.Count);
            Capital = innerTiles[index];
        }

        public IList<Province> GetNeighbours(HexMap map)
        {
            if (_neighbours != null)
                return _neighbours;

            TraceBorder(map);

            _neighbours = _borderRoute.Select(b=>b.Neighbour.Province).Distinct().ToList();
            return _neighbours;
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

        private void TraceBorder(HexMap map)
        {
            if (_borderRoute != null)
                return;

            // Get first tile with any neighbour of another province
            var tiles = HexTiles.ToList();
            var firstBorderTile = tiles.Where(t => map.GetNeighbours(t).Where(n => n.Province != this).Any()).First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstBorderTile).ToList();
            var neighbourPair = neighbourPairs.Where(n => n.Neighbour.Province != this).First();
            _borderRoute = new List<TilePair>();

            TraceBorder(neighbourPair, _borderRoute, map);
        }

        private void TraceBorder(TilePair tilePair, List<TilePair> borderRoute, HexMap map)
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
