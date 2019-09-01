using Assets.Contracts;

namespace Assets.Contracts.Map
{
    public class TilePair
    {
        public TileBase HexTile { get; set; }
        public TileBase Neighbour { get; set; }
        public Direction Direction { get; set; }
    }
}
