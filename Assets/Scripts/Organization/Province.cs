using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Organization
{
    public class Province
    {
        public string Name { get; set; }

        public Country Owner { get; set; }

        public List<Tile> HexTiles { get; set; }
    }
}
