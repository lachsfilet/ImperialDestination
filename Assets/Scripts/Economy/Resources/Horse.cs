using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Horse : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Horse()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.StudFarm
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Horse";
            }
        }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }

        public decimal Price { get; set; }
    }
}
