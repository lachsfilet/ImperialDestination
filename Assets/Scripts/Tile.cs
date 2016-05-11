using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Economy;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.Organization;
using Assets.Scripts.Map;
using System.Linq;
using Assets.Scripts;

public class Tile : MonoBehaviour {

    public TileTerrainType TileTerrainType;

    public bool IsSelected = false;

    public Color SelectionColor = Color.yellow;

    public Color HoverColor = Color.red;
    
    public List<IResource> Resources;

    public List<IBuilding> Buildings;

    public Country Owner;

    public Province Province;

    public Position Position = new Position();

    private Color _color;

    private Renderer _renderer;

    private Dictionary<Direction, List<Vector3>> _edges;

    public Color Color { get { return _color; } }

    void Awake()
    {
        _edges = new Dictionary<Direction, List<Vector3>>
        {
            {
                Direction.Northeast, new List<Vector3>
                {
                    new Vector3 (0, 0, 4 ),
                    new Vector3 (4, 0, 2 )
                }
            },
            {
                Direction.East, new List<Vector3>
                {
                    new Vector3 (4, 0, 2 ),
                    new Vector3 (4, 0, -2 )
                }
            },
            {
                Direction.Southeast,new List<Vector3>
                {
                    new Vector3 (4, 0, -2 ),
                    new Vector3 (0, 0, -4 )
                }
            },
            {
                Direction.Southwest, new List<Vector3>
                {
                    new Vector3 (0, 0, -4 ),
                    new Vector3 (-4, 0, -2 )
                }
            },
            {
                Direction.West, new List<Vector3>
                {
                    new Vector3 (-4, 0, -2 ),
                    new Vector3 (-4, 0, 2 )
                }
            },
            {
                Direction.Northwest, new List<Vector3>
                {
                    new Vector3 (-4, 0, 2 ),
                    new Vector3 (0, 0, 4 )
                }
            }
        };
    }

    // Use this for initialization
	void Start () {
        _renderer = GetComponent<Renderer>();
        _color = _renderer.material.color;
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    public void Select()
    {
        IsSelected = true;
        _renderer.material.color = SelectionColor;
    }

    public void Deselect()
    {
        IsSelected = false;
        _renderer.material.color = _color;
    }

    public void Hover()
    {
        _renderer.material.color = HoverColor;
    }

    public void Leave()
    {
        _renderer.material.color = IsSelected ? SelectionColor : _color;
    }

    public void SetColor(Color color)
    {
        if(_renderer == null)
            _renderer = GetComponent<Renderer>();
        _renderer.material.color = color;
        _color = color;
    }

    public void SetBorders(List<Direction> directions)
    {
        //var mesh = GetComponent<MeshFilter>().mesh;
        var lineRenderer = GetComponent<LineRenderer>();
        var sortedDirections = SortDirections(directions);
        var vectors = sortedDirections.SelectMany(d => _edges[d]).Distinct().ToArray();
        lineRenderer.SetVertexCount(vectors.Length);
        lineRenderer.SetPositions(vectors);
    }

    private List<Direction> SortDirections(List<Direction> list)
    {
        var enumerator = list.GetEnumerator();
        var count = 0;
        var indices = new List<int>();
        enumerator.MoveNext();
        while (true)
        {
            count++;

            var a = enumerator.Current;
            if (!enumerator.MoveNext())
                break;
            var b = enumerator.Current;

            if (a - b < -1)
            {
                indices.Add(count);
                count = 0;
            }
        }
        indices.Add(count);

        var lists = ChunkList(list, indices);
        var result = lists.Reverse().SelectMany(l => l.ToList()).ToList();
        return result;
    }

    private IEnumerable<IEnumerable<Direction>> ChunkList(List<Direction> list, IEnumerable<int> indices)
    {
        foreach (var i in indices)
        {
            yield return list.Take(i);
            list = list.Skip(i).ToList();
        }
    }
}
