using Assets.Contracts.Map;
using Assets.Scripts;
using Assets.Scripts.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiEngine;
using VoronoiEngine.Elements;

namespace Assets.Tests.Helpers
{
    public class TestMapHelpers
    {
        public static HexGrid CreateMap(GameObject mapStartPoint, HexMap map, GameObject hexTile, TileTerrainType type = TileTerrainType.Plain)
        {
            mapStartPoint.transform.position = new Vector3(0, 0, 0);
            mapStartPoint.transform.localScale = new Vector3(1, 1, 1);

            var hexGrid = new HexGrid(map.Height, map.Width, hexTile);
            var mapObject = new GameObject("Map");
            for (var y = 0; y < hexGrid.Height; y++)
            {
                for (var x = 0; x < hexGrid.Width; x++)
                {
                    var position = hexGrid.Get(x, y);
                    var tile = CreateTile(hexTile, mapStartPoint, map, type, position, x, y);
                    tile.transform.SetParent(mapObject.transform);
                }
            }
            return hexGrid;
        }

        public static List<Position> GenerateMap(int height, int width, List<Point> points, HexMap map)
        {
            var sites = points.Select(p => new Site() { Point = p }).ToList();
            var voronoiFactory = new VoronoiFactory();
            var voronoiMap = voronoiFactory.CreateVoronoiMap(height - 1, width - 1, sites);

            var lines = voronoiMap.Where(g => g is HalfEdge).Cast<HalfEdge>().SelectMany(
                edge =>
                {
                    var start = new Position(edge.Point.XInt, edge.Point.YInt);
                    var end = new Position(edge.EndPoint.XInt, edge.EndPoint.YInt);
                    var line = map.DrawLine(start, end);
                    return line;
                }).ToList();
            return lines;
        }

        public static void LogMap(HexMap map)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var row = "";
                for (var x = 0; x < map.Width; x++)
                {
                    var tile = map.GetTile(x, y);
                    var province = int.Parse(tile.Province.Name.Split(' ')[1]);
                    var country = tile.Province.Owner != null ? tile.Province.Owner.Name.First() : '0';
                    if (y % 2 != 0 && x == 0)
                        row += "  ";
                    row += $"{country}{province:000} ";
                }
                Debug.Log(row);
            }
        }

        private static GameObject CreateTile(GameObject tile, GameObject mapStartPoint, HexMap map, TileTerrainType type, Vector3 position, int x, int y)
        {
            var hexTile = TileFactory.Instance.CreateTile(UnityEngine.Object.Instantiate, tile, type, position, mapStartPoint.transform.rotation, x, y, map);
            hexTile.GetComponent<Tile>().Setup();
            return hexTile;
        }
    }
}