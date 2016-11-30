using Assets.Scripts.Map;
using System.Collections.Generic;

namespace Assets.Scripts.Economy.Resources
{
    public class Grain : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Grain()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.GrainField,
            };
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Grain";
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
