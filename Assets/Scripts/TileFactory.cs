using Assets.Contracts.Map;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class TileFactory
    {
        public static TileFactory _instance;

        private TileFactory()
        {
        }

        public static TileFactory Instance => _instance ?? (_instance = new TileFactory());

        public GameObject CreateTile(Func<GameObject, Vector3, Quaternion, GameObject> instantiate, GameObject tileObject, TileTerrainType type, Vector3 position, Quaternion rotation, int x, int y, IHexMap map)
        {
            var hexTile = instantiate(tileObject, position, rotation);
            var tile = hexTile.GetComponent<Tile>();
            tile.TileTerrainType = type;
            tile.Position.X = x;
            tile.Position.Y = y;
            map.AddTile(x, y, tile);
            return hexTile;
        }
    }
}