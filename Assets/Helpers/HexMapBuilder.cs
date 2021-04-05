using Assets.Contracts.Map;
using Assets.Scripts.Map;

namespace Helpers
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
            _tileBuilder = TileBuilder.New;
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

        public static HexMapBuilder New => new HexMapBuilder();

        public HexMap Build()
        {
            var map = new HexMap(_height, _width);
            for (var i = 0; i < _height; i++)
            {
                for (var j = 0; j < _width; j++)
                {
                    map.AddTile(j, i, _tileBuilder.WithPosition(new Position(j, i)).Build());
                }
            }
            return map;
        }
    }
}
