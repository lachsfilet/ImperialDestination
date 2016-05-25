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

        Debug.LogFormat("Tiles total: {0}", _tileCount);
        Debug.LogFormat("Countries total: {0}", Countries.Count);
        Debug.LogFormat("Provinces total: {0}", _provinceCount);
        Debug.LogFormat("Tiles per province: {0}", _tileCountProvinces);
        Debug.LogFormat("Tiles per major country: {0}", _tileCountProvinces * ProvincesMajorCountries);
        Debug.LogFormat("Tiles per minor country: {0}", _tileCountProvinces * ProvincesMinorCountries);

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
        for(var i = 0; i < MajorCountries; i++)
        {
            Countries.Add(new Country { Name = String.Format("Country {0}", i), CountryType = CountryType.Major });
        }
        for (var i = 0; i < MinorCountries; i++)
        {
            Countries.Add(new Country { Name = String.Format("Country {0}", i), CountryType = CountryType.Minor });
        }

        // Spread countries over the continents
        var majorCountryStack = new Stack<Country>(Countries.Where(c => c.CountryType == CountryType.Major));
        var minorCountryStack = new Stack<Country>(Countries.Where(c => c.CountryType == CountryType.Minor));
        var continents = _continents.OrderByDescending(c => c.transform.childCount).ToList();
        continents.ForEach(c =>
        {
            var countries = new List<Country>();
            var tileCount = c.transform.childCount;
            var provinceCount = (int)Math.Round((decimal)_tileCountProvinces / tileCount);

            var majorCountryCount = provinceCount / ProvincesMajorCountries;
            for (var i = 0; i < majorCountryCount; i++)
                if (majorCountryStack.Any())
                    countries.Add(majorCountryStack.Pop());

            var majorCountryRemainder = provinceCount % ProvincesMajorCountries;
            var minorCountryCount = provinceCount / ProvincesMinorCountries;
            for (var i = 0; i < minorCountryCount; i++)
            if (minorCountryStack.Any())
                    countries.Add(minorCountryStack.Pop());

            _continentCountryMapping.Add(c, countries);
        });

        //var emptyContinents = _continentCountryMapping.Where(c => !c.Value.Any()).Select(c => c.Key).ToList();
        //emptyContinents.ForEach(c => Debug.LogFormat("Continent without countries: {0}", c.name));
    }

    public void SetupProvinces()
    {
        _continents.ForEach(continent =>
        {
            var continentIndex = _continents.IndexOf(continent);
            var continentTiles = continent.GetComponentsInChildren<Tile>().ToList();
            var emptyTiles = continentTiles;
            var provinces = new List<Province>();
            var count = 0;
            do
            {
                var provinceContainer = Instantiate(Province);
                var province = provinceContainer.GetComponent<Province>();
                province.Name = string.Format("Province {0}_{1}", continentIndex, count++);
                var hexTile = emptyTiles.First();
                var tiles = CreateProvince(province, hexTile);

                if (province.HexTiles.Any())
                {
                    provinces.Add(province);
                    province.transform.SetParent(continent.transform);
                }
                emptyTiles = emptyTiles.Where(tile => !tiles.Contains(tile)).ToList();
            }
            while (emptyTiles.Count >= MinProvinceSize);

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

            // Draw border lines of provinces
            provinces.ForEach(p => p.DrawBorder(_map));
        });
    }

    private List<Tile> CreateProvince(Province province, Tile hexTile)
    {
        var provinceTiles = new List<Tile>();
        var provinceSize = (int)Math.Round(Math.Sqrt(_tileCountProvinces));
        provinceTiles.Add(hexTile);
        var count = 1;
        while (count < _tileCountProvinces)
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
                return provinceTiles;
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
