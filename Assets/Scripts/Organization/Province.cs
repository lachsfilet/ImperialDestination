using Assets.Scripts.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Organization
{
    public class Province : MonoBehaviour
    {
        private List<Tile> _hexTiles;

        public Province()
        {
            _hexTiles = new List<Tile>();
        }

        public string Name { get; set; }

        public Country Owner { get; set; }

        public IEnumerable<Tile> HexTiles
        {
            get
            {
                return _hexTiles;
            }
        }

        public void AddHexTile(Tile hexTile)
        {
            if (hexTile == null)
                return;

            _hexTiles.Add(hexTile);
            hexTile.Province = this;
        }

        public void DrawBorder(HexMap map)
        {
            // Get first tile with any neighbour of another province
            var tiles = HexTiles.ToList();
            var firstBorderTile = tiles.Where(t => map.GetNeighbours(t).Where(n => n.Province != this).Any()).First();
            var neighbourPairs = map.GetNeighboursWithDirection(firstBorderTile).ToList();
            var neighbourPair = neighbourPairs.Where(n => n.Neighbour.Province != this).First();
            var borderRoute = new Dictionary<Tile, List<TilePair>>();

            TraceBorder(neighbourPair, borderRoute, map);
            var vectors = borderRoute.Keys.Select(t => t.transform.position).ToArray();

            var lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.SetVertexCount(vectors.Length);
            lineRenderer.SetPositions(vectors);

            //borderRoute.Keys.ToList().ForEach(b => b.SetColor(Color.magenta));

            //var vertices = tiles.SelectMany(t =>
            //{
            //    var neighbours = map.GetNeighbours(t).ToList();
            //    var edges = neighbours.Where(n => n.TileTerrainType != TileTerrainType.Water && n.Province != this).
            //        Select(n => neighbours.IndexOf(n)).
            //        Select(i => (Direction)i);
            //    return t.GetVertices(edges);
            //});
        }

        private void TraceBorder(TilePair tilePair, Dictionary<Tile, List<TilePair>> borderRoute, HexMap map)
        {
            // Abort if current combination is already stored
            if (borderRoute.ContainsKey(tilePair.HexTile) && borderRoute[tilePair.HexTile].Contains(tilePair))
                return;

            if (!borderRoute.ContainsKey(tilePair.HexTile))
                borderRoute.Add(tilePair.HexTile, new List<TilePair>());
            borderRoute[tilePair.HexTile].Add(tilePair);

            var nextPair = map.GetNextNeighbourWithDirection(tilePair.HexTile, tilePair.Neighbour);

            if(nextPair.Neighbour.Province != tilePair.HexTile.Province)
            {
                // Move on with current tile and next neighbour province tile
                TraceBorder(nextPair, borderRoute, map);
                return;
            }

            // Take next own province tile and current neighbour province tile
            var newCurrentProvinceTile = nextPair.Neighbour;
            var lastNeighbourProvinceTile = borderRoute[tilePair.HexTile].Select(t => t.Neighbour).Last();
            var newPair = map.GetPairWithDirection(newCurrentProvinceTile, lastNeighbourProvinceTile);
            if (newPair == null)
                return;
            TraceBorder(newPair, borderRoute, map);
        }
    }
}
