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
            var neighbours = map.GetNeighbours(firstBorderTile).ToList();
            var neighbour = neighbours.Where(n => n.Province != this).First();
            var borderRoute = new Dictionary<Tile, List<Tile>>();

            TraceBorder(firstBorderTile, neighbour, borderRoute, map);
            borderRoute.Keys.ToList().ForEach(b => b.SetColor(Color.magenta));
            
            //var vertices = tiles.SelectMany(t =>
            //{
            //    var neighbours = map.GetNeighbours(t).ToList();
            //    var edges = neighbours.Where(n => n.TileTerrainType != TileTerrainType.Water && n.Province != this).
            //        Select(n => neighbours.IndexOf(n)).
            //        Select(i => (Direction)i);
            //    return t.GetVertices(edges);
            //});
        }

        private void TraceBorder(Tile currentProvinceTile, Tile neighbourProvinceTile, Dictionary<Tile, List<Tile>> borderRoute, HexMap map)
        {
            // Abort if current combination is already stored
            if (borderRoute.ContainsKey(currentProvinceTile) && borderRoute[currentProvinceTile].Contains(neighbourProvinceTile))
                return;

            if (!borderRoute.ContainsKey(currentProvinceTile))
                borderRoute.Add(currentProvinceTile, new List<Tile>());
            borderRoute[currentProvinceTile].Add(neighbourProvinceTile);

            var nextNeighbour = map.GetNextNeighbour(currentProvinceTile, neighbourProvinceTile);

            if(nextNeighbour.Province != currentProvinceTile.Province)
            {
                // Move on with current tile and next neighbour province tile
                TraceBorder(currentProvinceTile, nextNeighbour, borderRoute, map);
                return;
            }

            // Take next own province tile and current neighbour province tile
            var newCurrentProvinceTile = nextNeighbour;
            var lastNeighbourProvinceTile = borderRoute[currentProvinceTile].Last();
            TraceBorder(newCurrentProvinceTile, lastNeighbourProvinceTile, borderRoute, map);
        }
    }
}
