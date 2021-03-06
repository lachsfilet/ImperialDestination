﻿using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using Assets.Scripts;
using Assets.Scripts.Economy.Resources;
using Assets.Scripts.Game;
using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.VersionControl;
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

    public GameObject Continent;

    public int Height = 1;

    public int Width = 1;

    public int Regions = 1;

    public int MajorCountries = 7;

    public int MinorCountries = 16;

    public int ProvincesMajorCountries = 8;

    public int ProvincesMinorCountries = 4;

    public int DesertBelt = 10;

    public int PoleBelt = 5;

    public double MountainRatio = 0.25;

    public List<Color> TerrainColorMapping;

    public MapMode MapMode = MapMode.InGame;

    public List<Color> CountryColors;

    // Private fields
    private GameObject _mapObject;

    private HexGrid _hexGrid;

    private HexMap _map;

    private TileTerrainTypeMap _terrainMap;

    private ICollection<Position> _lines;

    private ICollection<Position> _vertices;

    private ICollection<IProvince> _regions;

    private CountryGenerator _countryGenerator;

    private IMapOrganizationGenerator _mapOrganizationGenerator;

    private ITerrainGenerator _terrainGenerator;

    private IOrganisationFactory _organisationFactory;

    private ProvinceFactory _provinceFactory;

    private List<string> _countryNames;

    // Start is called before the first frame update
    private void Start()
    {
        var points = new List<Point>();
        try
        {
            Debug.Log(Application.dataPath);
            Debug.Log(Application.persistentDataPath);

            _mapObject = new GameObject("Map");

            if (LoadMapFromCache())
                return;

            _hexGrid = new HexGrid(Height, Width, HexTile);
            _map = new HexMap(Height, Width);
            _organisationFactory = new OrganisationFactory();
            _mapOrganizationGenerator = new MapOrganizationGenerator(_mapObject, _organisationFactory);
            _countryGenerator = new CountryGenerator(_organisationFactory, _mapOrganizationGenerator);

            var heightMapGenerator = new HeightMapGenerator(MountainRatio);
            _terrainGenerator = new TerrainGenerator(heightMapGenerator);
            _terrainGenerator.DesertBelt = DesertBelt;
            _terrainGenerator.PoleBelt = PoleBelt;

            var voronoiMap = GenerateMap();
            points = voronoiMap.Where(g => g is Site).Select(s => s.Point).ToList();

            _provinceFactory = new ProvinceFactory(_map, _lines, Instantiate, Province, _organisationFactory);
            _regions = _provinceFactory.CreateProvinces(points);

            Debug.Log("Map check after region detection");
            CheckMap(points);

            var majorCountryNames = SettingsLoader.Instance.MajorCountryNames;
            var minorCountryNames = SettingsLoader.Instance.MinorCountryNames;
            _countryNames = majorCountryNames.Union(minorCountryNames).ToList();

            _countryGenerator.GenerateCountries(_regions, _map, MajorCountries, MinorCountries, ProvincesMajorCountries, ProvincesMinorCountries, majorCountryNames, minorCountryNames, Instantiate, Country, CountryColors);
            _mapOrganizationGenerator.GenerateContinentsList(Instantiate, Continent, _regions, _map, _mapObject);
            _terrainGenerator.GenerateTerrain(_map);

            var resources = SettingsLoader.Instance.ResourceSettings;
            ResourceService.Instance.SpreadResources(_map, resources);

            if (MapMode == MapMode.InGame)
                SkinMap();
            else
                ColorCountries();

            StoreMapIntoCache();

            var siteCommands = string.Join(", ", points.Select(p => $"new Point({p.X}, {p.Y})"));
            var siteListComand = $"var points = new List<Point> {{ {siteCommands} }};";
            Debug.Log(siteListComand);
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            LogMap();

            var siteCommands = string.Join(", ", points.Select(p => $"new Point({p.X}, {p.Y})"));
            var siteListComand = $"var points = new List<Point> {{ {siteCommands} }};";                       
            Debug.Log(siteListComand);

            Debug.Log("var lines  = new List<Position>()");
            Debug.Log("{");
            foreach (var line in _lines)
            {
                Debug.Log($"new Position({line.X}, {line.Y})");
            }
            Debug.Log("}");
        }
    }

    private void CheckMap(List<Point> points)
    {
        var provinceless = _map.Where(t => t.Province == null);
        if (!provinceless.Any())
            return;

        Debug.LogError("Tiles without province found!");

        var siteCommands = string.Join(", ", points.Select(p => $"new Point({p.X}, {p.Y})"));
        var siteListComand = $"var points = new List<Point> {{ {siteCommands} }};";
        Debug.Log(siteListComand);

        var hexGridCommand = $"new HexGrid({Height}, {Width}, HexTile);";
        Debug.Log(hexGridCommand);

        var mapCommand = $"new HexMap({Height}, {Width});";
        Debug.Log(mapCommand);

        Debug.Log(points.Count);
        Debug.Log(_lines.Count);

        foreach (var tile in provinceless)
            tile.SetColor(Color.yellow);

        foreach (var line in _lines)
        {
            if (line.X < 0 || line.X >= Width || line.Y < 0 || line.Y >= Height)
                continue;
            var tile = _map.GetTile(line);
            tile.SetColor(Color.red);
        }

        foreach (var point in points)
        {
            var tile = _map.GetTile(point.XInt, point.YInt);
            if(provinceless.Contains(tile))
                tile.SetColor(Color.cyan);
            else
                tile.SetColor(Color.black);
        }

        foreach(var vertex in _vertices)
        {
            if (vertex.X < 0 || vertex.X >= Width || vertex.Y < 0 || vertex.Y >= Height)
                continue;
            var tile = _map.GetTile(vertex);
            tile.SetColor(Color.blue);
        }
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
        var voronoiMap = voronoiFactory.CreateVoronoiMap(Height, Width, Regions);
        _lines = voronoiMap.Where(g => g is HalfEdge).Cast<HalfEdge>().SelectMany(
            edge =>
            {
                var start = new Position(edge.Point.XInt, edge.Point.YInt);
                var end = new Position(edge.EndPoint.XInt, edge.EndPoint.YInt);
                var line = _map.DrawLine(start, end);
                return line;
            }).ToList();
        _vertices = voronoiMap.Where(g => g is Vertex).Cast<Vertex>().Select(v => new Position(v.Point.XInt, v.Point.YInt)).ToList();
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
        var tiles = _mapObject.transform.GetComponentsInChildren<Tile>().ToList();
        tiles.ForEach(t =>
        {
            if (t.Province != null && t.Province.IsCapital && t.TileTerrainType == TileTerrainType.City)
                t.SetColor(Color.red);
            else
                t.SetColor(TerrainColorMapping[(int)t.TileTerrainType]);
            t.ResetSelectionColor();
        });
    }

    private void ColorCountries()
    {
        var countries = _map.Where(t=>t.Province!= null).Select(t => t.Province.Owner).Where(c => c != null).Distinct().ToList();
        countries.ForEach(c =>
        {
            c.Provinces.ForEach(p => p.HexTiles.ToList().ForEach(t =>
            {
                var renderer = t.GetComponent<Renderer>();
                renderer.material.color = c.Color;
            }));
        });

        var waterTiles = _map.Where(t => t.Province != null).Where(t => t.TileTerrainType == TileTerrainType.Water).ToList();
        waterTiles.ForEach(t =>
        {
            t.SetColor(TerrainColorMapping[(int)t.TileTerrainType]);
        });
    }

    private void LogMap()
    {
        for (var y = 0; y < _map.Height; y++)
        {
            var row = "";
            for (var x = 0; x < _map.Width; x++)
            {
                var tile = _map.GetTile(x, y);
                var province = int.Parse(tile.Province.Name.Split(' ')[1]);
                var country = tile.Province.Owner != null ? tile.Province.Owner.Name.First() : '0';
                if (y % 2 != 0 && x == 0)
                    row += "  ";
                row += $"{country}{province:000} ";
            }
            Debug.Log(row);
        }
    }

    private void SetupMap(MapInfo mapInfo)
    {
        var continents = new List<GameObject>();
        foreach (var continentInfo in mapInfo.Tiles.Where(t => t.ProvinceInfo.ContinentInfo != null).Select(t => t.ProvinceInfo.ContinentInfo).Distinct())
        {
            var continent = new GameObject(continentInfo.Name);
            continent.transform.SetParent(_mapObject.transform);
            continents.Add(continent);
        }

        var countries = new Dictionary<string, Country>();
        foreach (var countryInfo in mapInfo.Tiles.Where(t => t.ProvinceInfo.OwnerInfo != null).Select(t => t.ProvinceInfo.OwnerInfo).Distinct().ToList())
        {
            var countryContainer = Instantiate(Country);
            var country = countryContainer.GetComponent<Country>();
            var countryName = countryInfo.Name;
            country.Name = countryName;
            countries.Add(countryName, country);
        }

        var provinces = new Dictionary<string, Province>();
        foreach (var provinceInfo in mapInfo.Tiles.Select(t => t.ProvinceInfo).Distinct())
        {
            var provinceContainer = Instantiate(Province);
            var province = provinceContainer.GetComponent<Province>();
            province.Name = provinceInfo.Name;
            provinces.Add(provinceInfo.Name, province);

            if(provinceInfo.OwnerInfo==null)            
                continue;
            
            var country = countries[provinceInfo.OwnerInfo.Name];
            country.Provinces.Add(province);
            province.Owner = country;
            province.IsCapital = provinceInfo.IsCapital;
        }

        foreach (var tileInfo in mapInfo.Tiles)
        {
            var tile = _map.GetTile(tileInfo.Position.X, tileInfo.Position.Y);
            tile.TileTerrainType = tileInfo.TileTerrainType;
            tile.Resources = tileInfo.Resources.ToList();

            var provinceInfo = tileInfo.ProvinceInfo;
            provinces[provinceInfo.Name].AddHexTile(tile);

            if (provinceInfo.ContinentInfo == null)
                continue;

            var continent = continents.Single(c => c.name == provinceInfo.ContinentInfo.Name);
            tile.transform.SetParent(continent.transform);
        }

        foreach (var province in provinces.Values)
        {
            province.DrawBorder(_map);
        }
        foreach (var country in countries.Values)
        {
            country.DrawBorder(_map);
        }
    }

    private void StoreMapIntoCache()
    {
        var mapInfo = MapInfo.Create(_map, _terrainMap);
        GameCache.Instance.SetMapInfo(mapInfo);
        GameCache.Instance.SetCountryNames(_countryNames);
    }

    private bool LoadMapFromCache()
    {
        if (GameCache.Instance.ContainsMapInfo || MapMode != MapMode.InGame)
            return false;

        var mapInfo = GameCache.Instance.CurrentGame.MapInfo;

        Height = mapInfo.Map.GetLength(1);
        Width = mapInfo.Map.GetLength(0);
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new HexMap(Height, Width);
        _terrainMap = new TileTerrainTypeMap(mapInfo.Map);

        CreateMap();
        SetupMap(GameCache.Instance.CurrentGame.MapInfo);
        SkinMap();

        return true;
    }
}