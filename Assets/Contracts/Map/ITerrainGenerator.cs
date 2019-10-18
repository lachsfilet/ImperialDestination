using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Contracts.Map
{
    public interface ITerrainGenerator
    {
        int DesertBelt { get; set; }
        int PoleBelt { get; set; }

        void GenerateTerrain(IHexMap hexMap);
    }
}
