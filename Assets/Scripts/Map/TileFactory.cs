using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class TileFactory : MonoBehaviour
    {
        public GameObject MapStartPoint;
        public List<Color> TerrainColorMapping;
        public GameObject HexTile;

        public GameObject CreateTile(TileTerrainType type, Vector3 position, int x, int y)
        {
            var hexTile = (GameObject)Instantiate(HexTile, position, MapStartPoint.transform.rotation);
            var renderer = hexTile.GetComponent<Renderer>();
            renderer.material.color = TerrainColorMapping[(int)type];
            var tile = hexTile.GetComponent<Tile>();
            tile.TileTerrainType = type;
            tile.Position.X = x;
            tile.Position.Y = y;
            return hexTile;
        }
    }
}
