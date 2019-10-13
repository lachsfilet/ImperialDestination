using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoronoiEngine;
using VoronoiEngine.Elements;
using VoronoiEngine.Structures;
using VoronoiEngine.Utilities;

public class VoronoiGenerator : MonoBehaviour
{
    // Unity fields
    public GameObject HexTile;

    public GameObject MapStartPoint;

    public GameObject Camera;

    public GameObject Province;

    public GameObject Country;

    public int Height = 1;

    public int Width = 1;

    public int Regions = 1;

    public int MajorCountries = 7;

    public int MinorCountries = 16;

    public int ProvincesMajorCountries = 8;

    public int ProvincesMinorCountries = 4;

    public int DesertBelt = 10;

    public int PoleBelt = 5;

    public List<Color> TerrainColorMapping;

    // Private fields
    private GameObject _mapObject;

    private HexGrid _hexGrid;

    private HexMap _map;

    private TileTerrainTypeMap _terrainMap;

    private ICollection<Position> _lines;

    private ICollection<IProvince> _regions;

    private ICollection<ICountry> _countries;

    private IMapOrganizationGenerator _mapOrganizationGenerator;

    private int _landRegionCount;

    // Start is called before the first frame update
    private void Start()
    {
        _mapObject = new GameObject("Map");
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new HexMap(Height, Width);
        _landRegionCount = MajorCountries * ProvincesMajorCountries + MinorCountries * ProvincesMinorCountries;
        _mapOrganizationGenerator = new MapOrganizationGenerator();

        var voronoiMap = GenerateMap();
        var points = voronoiMap.Where(g => g is Site).Select(s => s.Point).ToList();
        _regions = DetectRegions(points);
        GenerateCountries();
        _mapOrganizationGenerator.GenerateContinentsList(_regions, _map, _mapObject);
        SkinMap();
    }

    private VoronoiMap GenerateMap()
    {
        _terrainMap = new TileTerrainTypeMap(Width, Height);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _terrainMap.Add(TileTerrainType.Water, x, y);
            }
        }
        CreateMap();

        var voronoiFactory = new VoronoiFactory(new EvenlySpreadSiteGenerator());
        var voronoiMap = voronoiFactory.CreateVoronoiMap(Height - 1, Width - 1, Regions);
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
        var hexTile = TileFactory.Instance.CreateTile(Instantiate, HexTile, type, position, MapStartPoint.transform.rotation, x, y, _map);
        return hexTile;
    }

    private void SkinMap()
    {
        var tiles = _mapObject.transform.GetComponentsInChildren<Tile>().Where(t => t.TileTerrainType == TileTerrainType.Water).ToList();
        tiles.ForEach(t =>
        {
            t.SetColor(TerrainColorMapping[(int)t.TileTerrainType]);
            t.ResetSelectionColor();
        });
    }

    private ICollection<IProvince> DetectRegions(ICollection<Point> points) =>
        points.OrderBy(p => p.X * Height.CountDigits() * 10 + p.Y).Select((point, index) =>
            {
                var tile = _map.GetTile(point.XInt, point.YInt);
                if (tile == null)
                    Debug.LogError($"Tile is NULL at X: {point.XInt}, Y: {point.YInt} (X: {point.X}, Y: {point.Y})");
                var provinceContainer = Instantiate(Province);
                var province = provinceContainer.GetComponent<Province>();
                province.Name = $"Region {index}";
                FillRegion(province, tile);
                province.DrawBorder(_map);
                province.ArrangePosition();
                province.IsWater = true;
                return province;
            }).Cast<IProvince>().ToList();

    private void FillRegion(Province province, TileBase start)
    {
        var tileStack = new Stack<TileBase>();
        tileStack.Push(start);

        while (tileStack.Count > 0)
        {
            var tile = tileStack.Pop();
            if (province.HexTiles.Contains(tile))
                continue;

            if (tile == null)
            {
                Debug.LogError($"Tile is null, tileStack.Count is {tileStack.Count}, province is {province.Name}");
            }

            province.AddHexTile(tile);

            if (_lines.Contains(tile.Position))
                continue;

            foreach (var neighbour in _map.GetNeighboursWithDirection(tile))
            {
                if (neighbour.Neighbour == null)
                    Debug.LogError($"Neighbour is null, tile is {tile}, tileStack.Count is {tileStack.Count}, province is {province.Name}");

                if (neighbour.Neighbour.Province == null)
                {
                    tileStack.Push(neighbour.Neighbour);
                }
            }
        }
    }

    private void GenerateCountries()
    {
        var color = new Color(1, 1, 1, 1);

        var regions = _regions.Where(p => !p.HexTiles.Any(h => h.Position.X == 0 || h.Position.Y == 0 || h.Position.X == Width - 1 || h.Position.Y == Height - 1)).ToList();

        var majorCountries = Enumerable.Range(1, MajorCountries).Select(n => new { number = n, isMajor = true });
        var minorCountries = Enumerable.Range(1, MinorCountries).Select(n => new { number = n, isMajor = false });
        var countries = majorCountries.Concat(minorCountries).Shuffle().ToList();
        var step = 1f / countries.Count;

        var count = 0;
        foreach (var countryInfo in countries)
        {
            var regionCount = countryInfo.isMajor ? ProvincesMajorCountries : ProvincesMinorCountries;
            var prefix = countryInfo.isMajor ? "Major" : "Minor";
            var countryContainer = Instantiate(Country);
            var country = OrganisationFactory.Instance.CreateCountry(
                countryContainer,
                $"{prefix} Country {count}",
                countryInfo.isMajor ? CountryType.Major : CountryType.Minor,
                color);

            _mapOrganizationGenerator.GenerateCountryOnMap(country, regions, _map, regionCount, color, step);

            color = new Color(color.r - step, color.g - step, color.b - step, 1);
            count++;
        }
    }
}