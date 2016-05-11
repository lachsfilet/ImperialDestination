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
            var tiles = HexTiles.ToList();
            tiles.ForEach(t =>
            {
                var neighbours = map.GetNeighbours(t).ToList();
                var borderEdges = neighbours.Where(n => n.TileTerrainType != TileTerrainType.Water && n.Province != this).
                    Select(n => neighbours.IndexOf(n)).
                    Select(i => (Direction)i).ToList();
                t.SetBorders(borderEdges);
            });
        }

        //public void SetBorders(List<Direction> directions)
        //{
        //    //var mesh = GetComponent<MeshFilter>().mesh;
        //    var lineRenderer = GetComponent<LineRenderer>();
        //    var sortedDirections = SortDirections(directions);
        //    var vectors = sortedDirections.SelectMany(d => _edges[d]).Distinct().ToArray();
        //    lineRenderer.SetVertexCount(vectors.Length);
        //    lineRenderer.SetPositions(vectors);
        //}

        private List<Direction> SortDirections(List<Direction> list)
        {
            var enumerator = list.GetEnumerator();
            var count = 0;
            var indices = new List<int>();
            enumerator.MoveNext();
            while (true)
            {
                count++;

                var a = enumerator.Current;
                if (!enumerator.MoveNext())
                    break;
                var b = enumerator.Current;

                if (a - b < -1)
                {
                    indices.Add(count);
                    count = 0;
                }
            }
            indices.Add(count);

            var lists = ChunkList(list, indices);
            var result = lists.Reverse().SelectMany(l => l.ToList()).ToList();
            return result;
        }

        private IEnumerable<IEnumerable<Direction>> ChunkList(List<Direction> list, IEnumerable<int> indices)
        {
            foreach (var i in indices)
            {
                yield return list.Take(i);
                list = list.Skip(i).ToList();
            }
        }
    }
}
