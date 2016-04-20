using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Economy;
using Assets.Scripts.Infrastructure;
using Assets.Scripts.Organization;
using Assets.Scripts.Map;

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
}
