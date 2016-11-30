using System.Collections.Generic;
using Assets.Scripts.Map;

namespace Assets.Scripts.Economy.Resources
{
    public class Cotton : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Cotton()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.CottonField
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Cotton";
            }
        }

        public IEnumerable<TileTerrainType> PossibleTerrainTypes
        {
            get
            {
                return _possibleTerrainTypes;
            }
        }

        public int Price { get; set; }
    }
}
