using Assets.Contracts.Economy;
using Assets.Contracts.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Economy.Resources
{
    public class ResourceService
    {
        private static ResourceService _instance;

        public static ResourceService Instance
        {
            get
            {
                return _instance ?? (_instance = new ResourceService());
            }
        }

        private ResourceService()
        {
        }

        public void SpreadResources(IHexMap map, IDictionary<Type, double> resourceProbability)
        {
            var rand = new Random();
            foreach (var key in resourceProbability.Keys)
            {
                var resource = (IResource)Activator.CreateInstance(key);
                var terrainTypes = resource.PossibleTerrainTypes.ToList();
                foreach (var terrainType in terrainTypes)
                {
                    var tiles = map.GetTilesOfTerrainType(terrainType);
                    foreach (var tile in tiles)
                    {
                        var propability = resourceProbability[key];
                        var value = rand.NextDouble();
                        if (value < propability && tile.Resources.Count < GetResourceCapacity(terrainType))
                            tile.Resources.Add(resource);
                    }
                }
            }
        }

        private int GetResourceCapacity(TileTerrainType terrainType)
        {
            if (terrainType == TileTerrainType.Hills || terrainType == TileTerrainType.Mountains)
                return 2;
            return 1;
        }
    }
}