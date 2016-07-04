using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Assets.Scripts.Map;
using System;
using System.Linq;
using Assets.Scripts.Organization;
using Assets.Scripts.Economy.Resources;

//[ExecuteInEditMode]
public class Map : MonoBehaviour {
    public GameObject HexTile;
    public GameObject MapStartPoint;
    public GameObject Province;
    public GameObject Country;

    public Camera Camera;
    public List<Color> TerrainColorMapping;
    public Text TerrainText;
    public Text PositionText;
    public Text ContinentText;
    public Text TileCountText;
    public Text ProvinceText;
    public Text ProvinceCountText;
    public Text CountryText;
    public Text ResourcesText;

    public MapMode MapMode = MapMode.InGame;
    public int Height = 1;
    public int Width = 1;
    public int DesertBelt = 10;
    public int PoleBelt = 5;

    public int DropPoints = 5;
	public int MinParticles = 100;
	public int MaxParticles = 400;
	public int PassesCount = 4;
	public int ParticleStablityRadius = 1;
    public int FilterCount = 2;
    public int MinHillsValue = 10;
    public int MinMountainsValue = 20;

    public int MajorCountries = 4;
    public int MinorCountries = 4;
    public int ProvincesMajorCountries = 8;
    public int ProvincesMinorCountries = 4;
    public int MinProvinceSize = 8;

    public List<Country> Countries { get; private set; }
    
    private Tile _lastHovered;
    private Tile _selectedTile;
    private Country _selectedCountry;
    private HexGrid _hexGrid;
    private HexMap _map;
    private int _equator;
    private List<GameObject> _continents;
    private Dictionary<GameObject, List<Country>> _continentCountryMapping;
    private int _provinceCount;
    private int _tileCount;
    private int _tileCountProvinces;
    private int _tileCountMajorCountry;
    private int _tileCountMinorCountry;

    // Use this for initialization
    void Start() {
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new HexMap(Height, Width);
        _equator = (int)Math.Round(Height / 2m);
        _continents = new List<GameObject>();
        _continentCountryMapping = new Dictionary<GameObject, List<Country>>();

        GenerateMap();
        SetContinents();

        Countries = new List<Country>();
        _provinceCount = MajorCountries * ProvincesMajorCountries + MinorCountries * ProvincesMinorCountries;
        _tileCount = _continents.Sum(c => c.transform.childCount);
        _tileCountProvinces = _tileCount / _provinceCount;
        _tileCountMajorCountry = _tileCountProvinces * ProvincesMajorCountries;
        _tileCountMinorCountry = _tileCountProvinces * ProvincesMinorCountries;

        Debug.LogFormat("Tiles total: {0}", _tileCount);
        Debug.LogFormat("Provinces total: {0}", _provinceCount);
        Debug.LogFormat("Tiles per province: {0}", _tileCountProvinces);
        Debug.LogFormat("Tiles per major country: {0}", _tileCountMajorCountry);
        Debug.LogFormat("Tiles per minor country: {0}", _tileCountMinorCountry);
        Debug.LogFormat("Continents total: {0}", _continents.Count);

        SetupCountries();
        SetupProvinces();

        var resources = new Dictionary<Type, double>
        {
            { typeof(Coal), 0.3 },
            { typeof(IronOre), 0.2 },
            { typeof(Gold), 0.1 },
            { typeof(Gemstone), 0.05 },
            { typeof(Oil), 0.3 },
            { typeof(Wood), 1 },
            { typeof(Wool), 1 },
            { typeof(Grain), 1 },
            { typeof(Cotton), 1 },
            { typeof(Cattle), 1 },
            { typeof(Fruit), 1 },
            { typeof(Horse), 1 }
        };

        ResourceService.Instance.SpreadResources(_map, resources);
    }

    // Update is called once per frame
    void Update () {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (!Physics.Raycast(ray, out hit))
            return;

        var hexTile = hit.collider.gameObject;
        var tile = hexTile.GetComponent<Tile>();

        if (Input.GetMouseButtonDown(0))
        {
            if (MapMode == MapMode.Overview)
            {
                var country = tile.Province.Owner;
                if (_selectedCountry != null && _selectedCountry != country)
                {
                    _selectedCountry.Provinces.ForEach(p =>
                    {
                        p.HexTiles.ToList().ForEach(
                            t =>
                            {
                                t.Deselect();
                            });
                    });
                }
                if (country == _selectedCountry)
                    return;

                country.Provinces.ForEach(p =>
                {
                    p.HexTiles.ToList().ForEach(
                        t =>
                        {
                            t.Select();
                        });
                });
                _selectedCountry = country;
                return;
            }

            if(_selectedTile != null)
                _selectedTile.Deselect();
            tile.Select();
            _selectedTile = tile;
            TerrainText.text = _selectedTile.TileTerrainType.ToString();
            PositionText.text = string.Format("x: {0}, y: {1}", _selectedTile.Position.X, _selectedTile.Position.Y);
            ContinentText.text = tile.transform.parent != null ? tile.transform.parent.name : "None";
            TileCountText.text = tile.transform.parent != null ? tile.transform.parent.childCount.ToString() : "None";
            ProvinceText.text = tile.Province != null ? tile.Province.Name : "None";
            ProvinceCountText.text = tile.Province != null ? tile.Province.HexTiles.Count().ToString() : "None";
            CountryText.text = tile.Province != null && tile.Province.Owner != null ? tile.Province.Owner.Name : "None";
            ResourcesText.text = string.Join(",", tile.Resources.Select(r => r.Name).ToArray());
            return;
        }

        if (_lastHovered != null && tile != _lastHovered)
        {
            _lastHovered.Leave(); 
        }
        tile.Hover();
        _lastHovered = tile;        
    }

    private void GenerateMap()
    {
        var mapGenerator = new ParticleDeposition
        {
            DropPoints = DropPoints,
            MaxParticles = MaxParticles,
            MinParticles = MinParticles,
            ParticleStablityRadius = ParticleStablityRadius,
            PassesCount = PassesCount
        };

        var mapFactory = new MapFactory(mapGenerator)
        {
            DesertBelt = DesertBelt,
            FilterCount = FilterCount,
            MinHillsValue = MinHillsValue,
            MinMountainsValue = MinMountainsValue,
            PoleBelt = PoleBelt
        };

        var terrainMap = mapFactory.CreateMap(_hexGrid.Width, _hexGrid.Height);

        for (var y = 0; y < _hexGrid.Height; y++)
        {
            for (var x = 0; x < _hexGrid.Width; x++)
            {
                var terrain = terrainMap.Get(x, y);
                var position = _hexGrid.Get(x, y);
                if(terrain.HasValue)
                    CreateTile(terrain.Value, position, x, y);
            }
        }
    }

    private void SetContinents()
    {
        GameObject continent = null;
        
        for (var y = 0; y < _hexGrid.Height; y++)
        {
            for (var x = 0; x < _hexGrid.Width; x++)
            {
                var hexTile = _map.GetTile(x, y);
                if (hexTile.TileTerrainType == TileTerrainType.Water)
                    continue;

                if (hexTile.transform.parent != null)
                    continue;

                continent = new GameObject(string.Format("Continent {0}", _continents.Count + 1));
                AddTilesToContinent(hexTile, continent);
                _continents.Add(continent);
            }
        }
    }

    private void SetupCountries()
    {
        // Get ratio of total tiles for the continents
        var continentTileRatio = _continents.Select(c => new
        {
            Continent = c,
            TileCount = c.transform.childCount,
            Countries = new List<Country>()
        }).OrderByDescending(c => c.TileCount).ToList();

        for (var i = 0; i < MajorCountries + MinorCountries; i++)
        {
            var countryContainer = Instantiate(Country);
            var country = countryContainer.GetComponent<Country>();
            var isMajor = i < MajorCountries;
            country.Name = string.Format(isMajor ? "Major Country {0}" : "Minor Country {0}", i);
            country.CountryType = isMajor ? CountryType.Major: CountryType.Minor;
            Countries.Add(country);
        }
        
        // Spread countries over the continents (or fit countries into the continents)
        var countries = Countries.OrderByDescending(c => c.CountryType).Select(c =>
        new
        {
            Country = c,
            TileCount = c.CountryType == CountryType.Major ? _tileCountMajorCountry : _tileCountMinorCountry
        }).ToList();

        countries.ForEach(country =>
        {
            var continent = continentTileRatio.Where(con =>
                con.TileCount - (con.Countries.Sum(x => x.CountryType == CountryType.Major ? _tileCountMajorCountry : _tileCountMinorCountry))
                >= country.TileCount
            ).FirstOrDefault();

            if (continent == null)
                continent = continentTileRatio.OrderByDescending(con => con.TileCount - (con.Countries.Sum(x => x.CountryType == CountryType.Major ? _tileCountMajorCountry : _tileCountMinorCountry))).First();

            if (!_continentCountryMapping.ContainsKey(continent.Continent))
                _continentCountryMapping.Add(continent.Continent, new List<Country>());
            _continentCountryMapping[continent.Continent].Add(country.Country);
            continent.Countries.Add(country.Country);
        });

        Debug.LogFormat("Countries total: {0}", Countries.Count);
        foreach (var key in _continentCountryMapping.Keys)
        {
            _continentCountryMapping[key].ForEach(c => Debug.LogFormat("Continent: {0}, Tiles: {1}, Country: {2}", key.name, key.transform.childCount, c.Name));
        }
    }

    private void SetupProvinces()
    {
        _continents.ForEach(continent =>
        {
            var continentIndex = _continents.IndexOf(continent);
            var continentTiles = continent.GetComponentsInChildren<Tile>().ToList();
            var countries = _continentCountryMapping[continent];
            var majorCountries = countries.Where(c => c.CountryType == CountryType.Major).Count();
            var minorCountries = countries.Where(c => c.CountryType == CountryType.Minor).Count();
            var provinceCount = (majorCountries * ProvincesMajorCountries) + (minorCountries * ProvincesMinorCountries);
            var tileCountProvinces = continentTiles.Count / provinceCount;
            
            var emptyTiles = continentTiles;
            var provinces = new List<Province>();
            var count = 0;
            var countryStack = new Stack<Country>(countries);
            var country = countryStack.Pop();
            do
            {
                var provinceContainer = Instantiate(Province);
                var province = provinceContainer.GetComponent<Province>();
                province.Name = string.Format("Province {0}_{1}", continentIndex, count++);

                if(country != null)
                {
                    var countryProvinceCount = country.Provinces.Count;

                    if ((country.CountryType == CountryType.Major && countryProvinceCount == ProvincesMajorCountries)
                    || (country.CountryType == CountryType.Minor && countryProvinceCount == ProvincesMinorCountries))
                        if (countryStack.Count > 0)
                            country = countryStack.Pop();
                        else
                            country = null;
                }              
                var hexTile = emptyTiles.First();
                var tiles = CreateProvince(province, hexTile, tileCountProvinces);

                if (province.HexTiles.Any())
                {
                    provinces.Add(province);
                    province.transform.SetParent(continent.transform);
                    province.Owner = country;
                    if (country != null)
                        country.Provinces.Add(province);
                }
                emptyTiles = emptyTiles.Where(tile => !tiles.Contains(tile)).ToList();
            }
            while (emptyTiles.Count >= MinProvinceSize);

            // Add ownerless province tiles to empty pool again
            provinces.Where(p => p.Owner == null).ToList().ForEach(p =>
            {
                p.HexTiles.ToList().ForEach(t => t.Province = null);
                emptyTiles.AddRange(p.HexTiles);
                p.transform.parent = null;
            });

            // Add remaining tiles to existing provinces
            emptyTiles = continentTiles.Where(tile => tile.Province == null).ToList();
            while (emptyTiles.Any())
            {
                var hexTile = emptyTiles.First();
                //Debug.LogFormat("Tile x: {0}, y: {1} remaining", hexTile.Position.X, hexTile.Position.Y);
                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    var neighbour = _map.GetNeighbour(hexTile, direction);
                    if (neighbour == null || neighbour.Province == null)
                        continue;

                    neighbour.Province.AddHexTile(hexTile);
                    emptyTiles.Remove(hexTile);
                    break;
                }
            }

            provinces.Where(p => p.transform.parent != null).ToList().ForEach(p =>
            {
                // Draw border lines of provinces
                p.DrawBorder(_map);

                // Set province capitals
                p.SetCapital(_map);
                var tile = p.Capital;
                tile.TileTerrainType = TileTerrainType.City;
                tile.SetColor(TerrainColorMapping[(int)TileTerrainType.City]);
            });
        });

        Countries.ForEach(c => 
        {
            c.DrawBorder(_map);
            c.SetCapital(_map);
            var capital = c.Provinces.Where(p => p.IsCapital).SelectMany(p => p.HexTiles.Where(t => t.TileTerrainType == TileTerrainType.City)).Single();
            capital.SetColor(Color.red);
        });
    }

    private List<Tile> CreateProvince(Province province, Tile hexTile, int tileCountProvinces)
    {
        var provinceTiles = new List<Tile>();
        var provinceSize = (int)Math.Round(Math.Sqrt(tileCountProvinces));
        provinceTiles.Add(hexTile);
        var count = 1;
        while (count < tileCountProvinces)
        {
            Tile nextTile = null;
            var neighbours = count <= provinceSize ? _map.GetNeighbours(hexTile, true) : _map.GetNeighbours(hexTile);
            foreach (var neighbour in neighbours)
            {
                if (neighbour.Province != null 
                    || neighbour.TileTerrainType == TileTerrainType.Water 
                    || provinceTiles.Contains(neighbour))
                    continue;
                provinceTiles.Add(neighbour);
                nextTile = neighbour;
                count++;
            }
            if (nextTile == null)
            {
                if (provinceTiles.Count >= MinProvinceSize)
                    provinceTiles.ForEach(province.AddHexTile);
                return provinceTiles;
            }
            hexTile = nextTile;            
        }

        provinceTiles.ForEach(province.AddHexTile);
        return provinceTiles;
    }

    private GameObject CreateTile(TileTerrainType type, Vector3 position, int x, int y)
    {
        var hexTile = (GameObject)Instantiate(HexTile, position, MapStartPoint.transform.rotation);
        var renderer = hexTile.GetComponent<Renderer>();
        renderer.material.color = TerrainColorMapping[(int)type];
        var tile = hexTile.GetComponent<Tile>();
        tile.TileTerrainType = type;
        tile.Position.X = x;
        tile.Position.Y = y;
        if(x >= Width || y >= Height)
        {
            Debug.LogFormat("Index out of bounds: x: {0}, y; {1}, width: {2}, height: {3}", x, y, Width, Height);
            return null;
        }
        _map.AddTile(x, y, tile);
        return hexTile;
    }

    private void AddTilesToContinent(Tile hexTile, GameObject continent)
    {
        hexTile.transform.SetParent(continent.transform);

        foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
        {
            var neighbour = _map.GetNeighbour(hexTile, direction);
            if (neighbour == null || neighbour.transform.parent != null || neighbour.TileTerrainType == TileTerrainType.Water)
                continue;
            AddTilesToContinent(neighbour, continent);
        }
    }
}