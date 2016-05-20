using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Map
{
    public class TilePair
    {
        public Tile HexTile { get; set; }
        public Tile Neighbour { get; set; }
        public Direction Direction { get; set; }
    }
}
