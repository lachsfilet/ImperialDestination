﻿using Assets.Scripts.Map;

namespace Tests
{
    public class HexMapBuilder
    {
        private int _height;
        private int _width;
        private TileBuilder _tileBuilder;

        private HexMapBuilder()
        {
            _height = 1;
            _width = 1;
            _tileBuilder = TileBuilder.Create();
        }

        public HexMapBuilder WithHeight(int height)
        {
            _height = height;
            return this;
        }

        public HexMapBuilder WithWidth(int width)
        {
            _width = width;
            return this;
        }

        public HexMapBuilder WithTiles(TileBuilder tileBuilder)
        {
            _tileBuilder = tileBuilder;
            return this;
        }

        public static HexMapBuilder Create() => new HexMapBuilder();

        public HexMap Build()
        {
            var map = new HexMap(_height, _width);
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    map.AddTile(i, j, _tileBuilder.WithPosition(new Position(i, j)).Build());
                }
            }
            return map;
        }
    }
}
