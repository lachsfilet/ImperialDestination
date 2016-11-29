namespace Assets.Scripts.Map
{
    public class TilePair
    {
        public Tile HexTile { get; set; }
        public Tile Neighbour { get; set; }
        public Direction Direction { get; set; }
    }
}
