﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HexGrid : IEnumerable<Vector3>
{

    private GameObject _hexTile;

    public int Height { get; private set; }

    public int Width { get; private set; }

    public Vector3[,] MapGrid { get; private set; }
    
    public HexGrid(int height, int width, GameObject hexTile)
    {
        Height = height;
        Width = width;
        _hexTile = hexTile;
        MapGrid = new Vector3[Width, Height];

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        var renderer = _hexTile.GetComponent<Renderer>();
        var size = renderer.bounds.size;

        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var even = (j & 1) == 0;
                var x = i * size.x + (even ? 0 : size.x / 2);
				var z = j * (float)(size.z * 3 / 4);
                var position = new Vector3(x, 0, -z);
                MapGrid[i, j] = position;
            }
        }
    }

    public Vector3 Get(int x, int y)
    {
        if(x < Width && y < Height)
            return MapGrid[x, y];
        return Vector3.zero;        
    }
       
    IEnumerator IEnumerable.GetEnumerator()
    {
        return MapGrid.GetEnumerator();
    }

    public IEnumerator<Vector3> GetEnumerator()
    {
        var iterator = MapGrid.GetEnumerator();
        while (iterator.MoveNext())
        {
            yield return (Vector3)iterator.Current;
        }
    }
}
