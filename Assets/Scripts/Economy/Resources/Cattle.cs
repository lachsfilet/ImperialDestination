using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Cattle : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Cattle()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.CattleMeadows
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Cattle";
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
