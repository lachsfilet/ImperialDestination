using UnityEngine;
using NUnit.Framework;
using Assets.Scripts.Map;
using System.Collections.Generic;
using Assets.Scripts.Economy;
using Assets.Scripts.Economy.Resources;
using Newtonsoft.Json;

[TestFixture]
public class GameInfoTest
{
    [Test]
    public void TestMapInfoToJson()
    {
        var tileInfo = new TileInfo {
            Position = new Position { X = 1, Y = 1 },
            TileTerrainType = TileTerrainType.City,
            Resources = new List<IResource>() { new Gold() }
        };
        
        var json = JsonConvert.SerializeObject(tileInfo);

        Assert.IsNotNull(json);
        Assert.IsTrue(json.Length > 2, "The Json output is only two characters long.");

        var expected = "{\"Position\":{\"X\":1,\"Y\":1},\"TileTerrainType\":9,\"ProvinceInfo\":{}}";
        Assert.AreEqual(expected, json, string.Format("The Json output is {0} instead of {1}.", json, expected));
        Debug.Log(System.Environment.Version);
    }
}
