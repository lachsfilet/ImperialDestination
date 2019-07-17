using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiEngine;
using VoronoiEngine.Elements;
using VoronoiEngine.Structures;

public class VoronoiGenerator : MonoBehaviour
{
    public GameObject HexTile;

    public GameObject MapStartPoint;

    public GameObject Camera;

    public GameObject Province;

    public int Height = 1;

    public int Width = 1;

    public int Regions = 1;

    public List<Color> TerrainColorMapping;

    public List<Color> RegionColors;

    private GameObject _mapObject;

    private HexGrid _hexGrid;

    private HexMap _map;

    private TileTerrainTypeMap _terrainMap;

    private ICollection<Position> _lines;

    // Start is called before the first frame update
    void Start()
    {
        _mapObject = new GameObject("Map");
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new HexMap(Height, Width);

        var voronoiMap = GenerateMap();
        SkinMap();

        var points = voronoiMap.Where(g => g is Site).Select(s => s.Point).ToList();
        DetectRegions(points, RegionColors);        
    }
    
    private VoronoiMap GenerateMap()
    {
        _terrainMap = new TileTerrainTypeMap(Width, Height);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _terrainMap.Add(TileTerrainType.Plain, x, y);
            }
        }
        CreateMap();

        var voronoiFactory = new VoronoiFactory();
        var voronoiMap = voronoiFactory.CreateVoronoiMap(Height, Width, Regions);
        _lines = voronoiMap.Where(g => g is HalfEdge).Cast<HalfEdge>().SelectMany(
            edge =>
            {
                var start = new Position(edge.Point.XInt, edge.Point.YInt);
                var end = new Position(edge.EndPoint.XInt, edge.EndPoint.YInt);
                var line = _map.DrawLine(start, end);
                return line;
            }).ToList();
        return voronoiMap;
    }

    private void CreateMap()
    {
        for (var y = 0; y < _hexGrid.Height; y++)
        {
            for (var x = 0; x < _hexGrid.Width; x++)
            {
                var terrain = _terrainMap.Get(x, y);
                var position = _hexGrid.Get(x, y);
                if (!terrain.HasValue)
                    continue;

                var tile = CreateTile(terrain.Value, position, x, y);
                tile.transform.SetParent(_mapObject.transform);
            }
        }
    }

    private GameObject CreateTile(TileTerrainType type, Vector3 position, int x, int y)
    {
        var hexTile = Instantiate(HexTile, position, MapStartPoint.transform.rotation);
        var tile = hexTile.GetComponent<Tile>();
        tile.TileTerrainType = type;
        tile.Position.X = x;
        tile.Position.Y = y;
        _map.AddTile(x, y, tile);
        return hexTile;
    }

    private void SkinMap()
    {
        var tiles = _mapObject.transform.GetComponentsInChildren<Tile>().ToList();
        tiles.ForEach(t =>
        {
            if (_lines.Contains(t.Position))
                t.SetColor(Color.black);
            else
                t.SetColor(TerrainColorMapping[(int)t.TileTerrainType]);
            t.ResetSelectionColor();
        });
    }

    private ICollection<ICollection<Tile>> DetectRegions(ICollection<Point> points, IList<Color> colors) =>
        points.OrderBy(p => p.X).ThenBy(p => p.Y).Select((point, index) =>
            {
                var color = colors[index % colors.Count];
                var tile = _map.GetTile(point.XInt, point.YInt);
                var provinceContainer = Instantiate(Province);
                var province = provinceContainer.GetComponent<Province>();
                province.Name = $"Region {index}";
                var region = FillRegion(province, tile, color, colors);
                province.DrawBorder(_map);
                return region;
            }).ToList();
    
    private ICollection<Tile> FillRegion(Province province, Tile start, Color color, ICollection<Color> colors)
    {
        var tileStack = new Stack<Tile>();
        var region = new List<Tile>();
        tileStack.Push(start);

        while (tileStack.Count > 0)
        {
            var tile = tileStack.Pop();
            if (region.Contains(tile))
                continue;

            if (tile == null)
                Debug.LogError($"Tile is {tile}, color is {color}, tileStack.Count is {tileStack.Count}");

            var oldColor = tile.Color;
            region.Add(tile);
            tile.SetColor(color);
            province.AddHexTile(tile);

            if (oldColor == Color.black)
                continue;

            foreach (var neighbour in _map.GetNeighboursWithDirection(tile))
            {
                if(!colors.Contains(neighbour.Neighbour.Color))
                    tileStack.Push(neighbour.Neighbour);
            }
        }
        return region;
    }
}
