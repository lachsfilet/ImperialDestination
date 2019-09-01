using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Economy;
using Assets.Scripts.Organization;
using System.Linq;
using Assets.Contracts.Map;
using Assets.Contracts.Economy;
using Assets.Contracts.Infrastructure;
using Assets.Contracts;
using Assets.Contracts.Organization;

public class Tile : TileBase
{
    //public TileTerrainType TileTerrainType;

    //public bool IsSelected = false;

    //public Color DefaultSelectionColor = Color.yellow;

    //public Color SelectionColor = Color.yellow;

    //public Color HoverColor = Color.red;
    
    //public List<IResource> Resources;

    //public List<IBuilding> Buildings;

    //public ICountry Owner;

    //public IProvince Province;

    //public Position Position = new Position();

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
        Resources = new List<IResource>();
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


    public override void Select(Color color)
    {
        IsSelected = true;
        SelectionColor = color;
        _renderer.material.color = color;
    }

    public override void Deselect()
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

    public override void SetColor(Color color)
    {
        if(_renderer == null)
            _renderer = GetComponent<Renderer>();
        _renderer.material.color = color;
        _color = color;
    }

    public IEnumerable<Vector3> GetVertices(IEnumerable<Direction> directions)
    {
        return directions.SelectMany(d => _edges[d]).Distinct();
    }

    public override IEnumerable<Vector3> GetVertices(Direction direction, bool relative = false)
    {
        var pair = _edges[direction];
        foreach (var vector in pair)
        {
            if (relative)
                yield return vector;

            var position = this.transform.position;
            var result = position + vector;
            yield return result;
        }
    } 
    
    public void ResetSelectionColor()
    {
        SelectionColor = DefaultSelectionColor;
    }   
}
