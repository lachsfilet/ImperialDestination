using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using Assets.Contracts.Map;
using Helpers;

public class ProvinceFactoryTest
{
    [UnityTest]
    public IEnumerator CreateProvinces_WithLinePositionsAndHexMap_GeneratesProvinces()
    {
        var map = HexMapBuilder.New.WithWidth(13).WithHeight(9).Build();

        var lines = Enumerable.Range(0, 13).Select(i => new Position(i, 0))
            .Union(Enumerable.Range(0, 13).Select(i => new Position(i, 4)))
            .Union(Enumerable.Range(0, 13).Select(i => new Position(i, 8)))
            .Union(Enumerable.Range(0, 9).Select(i => new Position(0, i)))
            .Union(Enumerable.Range(0, 9).Select(i => new Position(4, i)))
            .Union(Enumerable.Range(0, 9).Select(i => new Position(8, i)))
            .Union(Enumerable.Range(0, 9).Select(i => new Position(12, i)))
            .Distinct().ToList();

        Assert.AreEqual(63, lines.Count);

        var provinceContainer = new GameObject();
        var provinceGameObject = new GameObject();

        Func<GameObject, GameObject> instantiate = o => provinceGameObject;


        var provinceHandle = Addressables.LoadAssetAsync<GameObject>("Province");
        yield return provinceHandle;

        Debug.Log(provinceHandle.OperationException);
        Assert.IsTrue(provinceHandle.IsDone);
        Assert.AreEqual(AsyncOperationStatus.Succeeded, provinceHandle.Status);


        //var province = new Mock<IProvince>();
        //var container = new GameObject();
        //var organisationFactory = new Mock<IOrganisationFactory>();
        //organisationFactory.Setup(o => o.CreateProvince(provinceGameObject, It.IsAny<string>())).Returns(province.Object);

        //var sites = new List<Point> { new Point(2,2), new Point(6,2), new Point(10,2), new Point(2,6), new Point(6,6), new Point(10,6) };

        //var provinceFactory = new ProvinceFactory(map, lines, instantiate, provinceGameObject, organisationFactory.Object);
        //var result = provinceFactory.CreateProvinces(sites);

        //Assert.IsNotNull(result);
    }
}
