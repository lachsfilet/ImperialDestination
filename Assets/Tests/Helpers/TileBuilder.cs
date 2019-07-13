using Assets.Scripts.Map;
using UnityEngine;

namespace Tests
{
    public class TileBuilder
    {
        private TileTerrainType _type;
        private Position _position;

        private TileBuilder()
        {
            _type = TileTerrainType.Water;
            _position = new Position(0, 0);
        }

        public static TileBuilder Create() => new TileBuilder();

        public TileBuilder WithType(TileTerrainType type)
        {
            _type = type;
            return this;
        }

        public TileBuilder WithPosition(Position position)
        {
            _position = position;
            return this;
        }

        public Tile Build()
        {
            var hexTile = new GameObject();            
            var tile = hexTile.AddComponent<Tile>();
            tile.TileTerrainType = _type;
            tile.Position = _position;
            return tile;
        }
    }
}