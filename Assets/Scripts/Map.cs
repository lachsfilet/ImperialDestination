using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Map;
using UnityEngine.UI;
using System;
using System.Linq;

//[ExecuteInEditMode]
public class Map : MonoBehaviour {
    public GameObject HexTile;
    public GameObject MapStartPoint;
    public Camera Camera;
    public List<Color> TerrainColorMapping;
    public Text TerrainText;
    public Text PositionText;
    public Text AreaText;

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


    private Tile _lastHovered;
    private Tile _selectedTile;
    private HexGrid _hexGrid;
    private Tile[,] _map;
    private List<GameObject> _areas;

    private int _equator;
    private Dictionary<Direction, Position>[] _directions;


    // Use this for initialization
    void Start() {
        _hexGrid = new HexGrid(Height, Width, HexTile);
        _map = new Tile[Width, Height];
        _equator = (int)Math.Round(Height / 2m);
        _areas = new List<GameObject>();
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

        //var sectorFactory = new SectorFactory();
        //_sectors = sectorFactory.CreateSectors(Height, Width, Continents).ToList();
        //_sectors.ForEach(s => GenerateContinent(s));
        GenerateMap();
        SetContinents();
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
            AreaText.text = tile.transform.parent != null ? tile.transform.parent.name : "None";
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

                continent = new GameObject(string.Format("Continent {0}", _areas.Count + 1));
                AddTilesToContinent(hexTile, continent);
                _areas.Add(continent);
            }
        }
    }

    //private void GenerateContinent(Sector sector)
    //{
    //    Debug.LogFormat("Sector left: {0}, top: {1}, right: {2}, bottom: {3}, center x: {4}, center y: {5}, height: {6}, width: {7}", sector.Left, sector.Top, sector.Right, sector.Bottom, sector.Center.X, sector.Center.Y, sector.Height, sector.Width);
    //    var random = new System.Random(22);
    //    for(var i = 0; i < sector.Width; i++)
    //    {
    //        for (var j = 0; j < sector.Height; j++)
    //        {
    //            var x = i + sector.Left;
    //            var y = j + sector.Bottom;
    //            var position = _hexGrid.Get(x, y);
    //            var distanceX = Math.Abs(x - sector.Center.X);
    //            var distanceY = Math.Abs(y - sector.Center.Y);
    //            var valueX = random.NextDouble();
    //            var valueY = random.NextDouble();
    //            var probabilityX = 1d / (double)distanceX;
    //            var probabilityY = 1d / (double)distanceY;
    //            var type = TileTerrainType.Water;
    //            if (valueX <= probabilityX && valueY <= probabilityY)
    //                // Create land tiles
    //                type = (TileTerrainType)UnityEngine.Random.Range(1, 6);
    //            CreateTile(type, position, x, y);
    //        }
    //    }
    //}

    //private void GenerateContinent(Sector sector)
    //{
    //    var tileCount = sector.Height * sector.Width * (1 - WaterProportion);
    //    System.Random random = new System.Random();
    //    Debug.LogFormat("TileCount: {0}", tileCount);
    //    for (var i = 0; i < tileCount; i++)
    //    {
    //        //Debug.Log(string.Format("Step: {0}", i));
    //        var u1 = random.NextDouble();
    //        var u2 = random.NextDouble();

    //        // Use Box-Muller-method to get the tile positions around the sector center
    //        var z1 = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
    //        var x = (int)Math.Abs(z1 * sector.Center.X);
    //        //Debug.Log(string.Format("u1: {0}, u2: {1}, z1: {2}, x: {3}", u1, u2, z1, x));

    //        var z2 = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
    //        var y = (int)Math.Abs(z2 * sector.Center.Y);
    //        //Debug.Log(string.Format("u1: {0}, u2: {1}, z2: {2}, y: {3}", u1, u2, z1, y));

    //        // Create land tiles
    //        var type = UnityEngine.Random.Range(1, 6);
    //        var position = _hexGrid.Get(x, y);
    //        CreateTile((TileTerrainType)type, position, x, y);
    //    }
    //}

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
