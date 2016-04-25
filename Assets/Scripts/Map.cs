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
    public Camera Camera;
    public List<Color> TerrainColorMapping;
    public Text TerrainText;
    public Text PositionText;
    public Text ContinentText;
    public Text TileCountText;

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

    public List<Country> Countries { get; private set; }

    private Tile _lastHovered;
    private Tile _selectedTile;
    private HexGrid _hexGrid;
    private Dictionary<Direction, Position>[] _directions;
    private Tile[,] _map;
    private int _equator;
    private List<GameObject> _continents;
    private int _provinceCount;
    private int _tileCount;
    private int _tileCountProvinces;

    // Use this for initialization
    void Start() {
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new Tile[Width, Height];
        _equator = (int)Math.Round(Height / 2m);
        _continents = new List<GameObject>();
        _directions = new Dictionary<Direction, Position>[] {
                new Dictionary<Direction, Position> {
                    { Direction.Northeast, new Position (0, -1) },
                    { Direction.East, new Position (1, 0) },
                    { Direction.Southeast, new Position (0, 1) },
                    { Direction.Southwest, new Position (-1, 1) },
                    { Direction.West, new Position (-1, 0) },
                    { Direction.Northwest, new Position (-1, -1) },
                },
                new Dictionary<Direction, Position> {
                    { Direction.Northeast, new Position (1, -1) },
                    { Direction.East, new Position (1, 0) },
                    { Direction.Southeast, new Position (1, 1) },
                    { Direction.Southwest, new Position (0, 1) },
                    { Direction.West, new Position (-1, 0) },
                    { Direction.Northwest, new Position (0, -1) },
                }
        };

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
        
        GenerateMap();
        SetContinents();
        SetupCountries();
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

                if (value == 0)
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

                // In the map border regions only use water
                //if (i < 2 || j < 2 || i > _hexGrid.Height - 3 || j > _hexGrid.Width - 3)
                //{
                //    CreateTile(TileTerrainType.Water, position, j, i);
                //    continue;
                //}
                //var water = rand.NextDouble();
                //if(water <= WaterProportion)
                //{
                //    CreateTile(TileTerrainType.Water, position, j, i);
                //    continue;
                //}

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
                var hexTile = _map[x, y];
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
            Countries.Add(new Country { Name = String.Format("Country {0}", 1), CountryType = CountryType.Major });
        }
        for (var i = 0; i < MinorCountries; i++)
        {
            Countries.Add(new Country { Name = String.Format("Country {0}", 1), CountryType = CountryType.Minor });
        }
    }

    public void SetupProvinces()
    {
        var province = new Province();
        _continents.ForEach(continent =>
        {
            var hexTile = continent.GetComponentsInChildren<Tile>().First();
            hexTile.Province = province;

            var count = 0;
            while(count <= _tileCountProvinces)
            {
                foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
                {
                    hexTile = GetNeighbour(hexTile, direction);
                    if (hexTile == null || hexTile.Province != null || hexTile.TileTerrainType == TileTerrainType.Water)
                        continue;
                    hexTile.Province = province;
                    count++;
                }
            }
        });
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
        _map[x, y] = tile;
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
        
    private Tile GetNeighbour(Tile hexTile, Direction direction)
    {
        var parity = hexTile.Position.Y & 1;
        var position = _directions[parity][direction];
        var neighbour = hexTile.Position + position;
        if (neighbour.X < 0 || neighbour.X >= Width || neighbour.Y < 0 || neighbour.Y >= Height)
            return null;
        return _map[neighbour.X, neighbour.Y];
    }

    private void AddTilesToContinent(Tile hexTile, GameObject continent)
    {
        hexTile.transform.SetParent(continent.transform);

        foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
        {
            var neighbour = GetNeighbour(hexTile, direction);
            if (neighbour == null || neighbour.transform.parent != null || neighbour.TileTerrainType == TileTerrainType.Water)
                continue;
            AddTilesToContinent(neighbour, continent);
        }
    }
}
