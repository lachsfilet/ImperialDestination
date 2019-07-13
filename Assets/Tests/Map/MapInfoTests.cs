using System.Collections.Generic;
using Assets.Scripts.Economy;
using Assets.Scripts.Economy.Resources;
using Assets.Scripts.Map;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class MapInfoTests
    {
        [Test]
        public void TestMapInfoToJson()
        {
            var tileInfo = new TileInfo
            {
                Position = new Position { X = 1, Y = 1 },
                TileTerrainType = TileTerrainType.City,
                Resources = new List<IResource>() { new Gold() }
            };

            var json = JsonConvert.SerializeObject(tileInfo);

            Assert.IsNotNull(json);

            var expected = "{\"Position\":{\"X\":1,\"Y\":1},\"TileTerrainType\":9,\"Resources\":[{\"Modificator\":0,\"Name\":\"Gold\",\"Price\":0,\"PossibleTerrainTypes\":[5,6]}],\"ProvinceInfo\":null}";

            Assert.AreEqual(expected, json, string.Format("The Json output is {0} instead of {1}.", json, expected));
            Debug.Log(System.Environment.Version);
        }
    }
}