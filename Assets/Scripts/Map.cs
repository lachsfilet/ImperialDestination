using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Map;
using UnityEngine.UI;
using System;
using System.Linq;
using Assets.Scripts.Organization;

//[ExecuteInEditMode]
public class Map : MonoBehaviour {
    public GameObject HexTile;
    public GameObject MapStartPoint;
    public GameObject Province;

    public Camera Camera;
    public List<Color> TerrainColorMapping;
    public Text TerrainText;
    public Text PositionText;
    public Text ContinentText;
    public Text TileCountText;
    public Text ProvinceText;
    public Text ProvinceCountText;
    public Text CountryText;

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
    }

    // Update is called once per frame
    void Update () {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return;

        var hexTile = hit.collider.gameObject;
        var tile = hexTile.GetComponent<Tile>();

        if (Input.GetMouseButtonDown(0))
        {
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
        var map = mapGenerator.Generate(_hexGrid.Width, _hexGrid.Height);

        var gaussianBlur = new GaussianBlur();
        int[,] blurredMap = map;
        for (var i = 0; i < FilterCount; i++)
        {
            blurredMap = gaussianBlur.Filter(blurredMap);
        }

        for (var y = 0; y < _hexGrid.Height; y++)
        {
            for (var x = 0; x < _hexGrid.Width; x++)
            {
                var value = blurredMap[x, y];
                var position = _hexGrid.Get(x, y);

                // In the map border regions only use water
                if (value == 0 || (x < 2 || y < 2 || y > _hexGrid.Height - 3 || x > _hexGrid.Width - 3))
                {
                    CreateTile(TileTerrainType.Water, position, x, y);
                    continue;
                }
                
                if (value >= MinHillsValue && value < MinMountainsValue)
                {
                    CreateTile(TileTerrainType.Hills, position, x, y);
                    continue;
                }

                if (value >= MinMountainsValue)
                {
                    CreateTile(TileTerrainType.Mountains, position, x, y);
                    continue;
                }

                if (isWithinDesertBelt(y))
                {
                    CreateTile(TileTerrainType.Desert, position, x, y);
                    continue;
                }

                if (isWithinPoleBelt(y))
                {
                    CreateTile(TileTerrainType.Tundra, position, x, y);
                    continue;
                }

                // Create land tiles
                CreateTile(TileTerrainType.Plain, position, x, y);
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

        for (var i = 0; i < MajorCountries; i++)
        {
            Countries.Add(new Country { Name = String.Format("Major Country {0}", i), CountryType = CountryType.Major });
        }
        for (var i = 0; i < MinorCountries; i++)
        {
            Countries.Add(new Country { Name = String.Format("Minor Country {0}", i), CountryType = CountryType.Minor });
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

    public void SetupProvinces()
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
                Debug.LogFormat("Tile x: {0}, y: {1} remaining", hexTile.Position.X, hexTile.Position.Y);
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
            
            // Draw border lines of provinces
            provinces.Where(p=>p.transform.parent != null).ToList().ForEach(p => p.DrawBorder(_map));
        });

        var colors = new List<Color> { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan, Color.gray, Color.grey };
        var index = 0;
        Countries.ForEach(c =>
        {
            var colorIndex = index % colors.Count;
            var color = colors[colorIndex];
            c.Provinces.ForEach(p => p.HexTiles.ToList().ForEach(t => t.SetColor(color)));
            index++;
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

    private bool isWithinDesertBelt(int y)
    {
        var upper = _equator + DesertBelt / 2;
        var lower = _equator - DesertBelt / 2;
        return (y < upper && y > lower);
    }

    private bool isWithinPoleBelt(int y)
    {
        return (y < PoleBelt || y >= Height - PoleBelt);
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
