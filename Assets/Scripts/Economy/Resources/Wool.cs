using System.Collections.Generic;
using Assets.Contracts.Map;
using Assets.Contracts.Economy;

namespace Assets.Scripts.Economy.Resources
{
    public class Wool : IResource
    {
        private List<TileTerrainType> _possibleTerrainTypes;

        public Wool()
        {
            _possibleTerrainTypes = new List<TileTerrainType>
            {
                TileTerrainType.SheepMeadows
            };            
        }

        public int Modificator { get; set; }

        public string Name
        {
            get
            {
                return "Wool";
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
