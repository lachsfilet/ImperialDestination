using Assets.Scripts.Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiEngine;
using VoronoiEngine.Elements;

public class VoronoiGenerator : MonoBehaviour
{
    public GameObject HexTile;

    public GameObject MapStartPoint;

    public GameObject Camera;

    public int Height = 1;

    public int Width = 1;

    public int Regions = 1;

    public List<Color> TerrainColorMapping;

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

        GenerateMap();
        SkinMap();
    }
    
    private void GenerateMap()
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
}
