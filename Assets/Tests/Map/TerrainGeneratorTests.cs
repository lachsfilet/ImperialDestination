using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Contracts;
using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Map;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TerrainGeneratorTests
    {        
        [Test]
        public void GenerateTerrain_WithHexMap_CreatesTerrainMap()
        {            
            var heightMapGeneratorMock = new Mock<IHeightMapGenerator>();
            var hexMap = HexMapBuilder.Create().WithHeight(100).WithWidth(100).Build();

            var waterProvinceMock = new Mock<IProvince>();
            waterProvinceMock.Setup(p => p.IsWater).Returns(true);

            var landProvinceMock = new Mock<IProvince>();
            landProvinceMock.Setup(p => p.IsWater).Returns(false);

            var plainTiles = hexMap.Where(t => t.Position.X > 0 || t.Position.X < 99 || t.Position.Y > 0 || t.Position.Y < 99).ToList();
            plainTiles.ForEach(t => {
                t.TileTerrainType = TileTerrainType.Plain;
                t.Province = landProvinceMock.Object;
            });
            
            var waterTiles = hexMap.Except(plainTiles).ToList();
            waterTiles.ForEach(t => t.Province = waterProvinceMock.Object);

            var terrainGenerator = new TerrainGenerator(heightMapGeneratorMock.Object);
            terrainGenerator.GenerateTerrain(hexMap);

            Assert.IsTrue(waterTiles.All(t => t.TileTerrainType == TileTerrainType.Water));
            Assert.IsTrue(plainTiles.All(t => t.TileTerrainType != TileTerrainType.Tundra));
            Assert.IsTrue(plainTiles.All(t => t.TileTerrainType != TileTerrainType.Desert));
            Assert.IsTrue(plainTiles.All(t => t.TileTerrainType != TileTerrainType.Mountains));
            Assert.IsTrue(plainTiles.All(t => t.TileTerrainType != TileTerrainType.Hills));

            Assert.IsTrue(AreEqual(0.55m, GetTerrainRatio(plainTiles, TileTerrainType.Plain)), TileTerrainType.Plain.ToString());
            Assert.IsTrue(AreEqual(0.02m, GetTerrainRatio(plainTiles, TileTerrainType.Marsh)), TileTerrainType.Marsh.ToString());
            Assert.IsTrue(AreEqual(0.1m, GetTerrainRatio(plainTiles, TileTerrainType.Forest)), TileTerrainType.Forest.ToString());
            Assert.IsTrue(AreEqual(0.05m, GetTerrainRatio(plainTiles, TileTerrainType.Orchard)), TileTerrainType.Orchard.ToString());
            Assert.IsTrue(AreEqual(0.1m, GetTerrainRatio(plainTiles, TileTerrainType.Bosk)), TileTerrainType.Bosk.ToString());
            Assert.IsTrue(AreEqual(0.05m, GetTerrainRatio(plainTiles, TileTerrainType.SheepMeadows)), TileTerrainType.SheepMeadows.ToString());
            Assert.IsTrue(AreEqual(0.01m, GetTerrainRatio(plainTiles, TileTerrainType.StudFarm)), TileTerrainType.StudFarm.ToString());
            Assert.IsTrue(AreEqual(0.05m, GetTerrainRatio(plainTiles, TileTerrainType.CattleMeadows)), TileTerrainType.CattleMeadows.ToString());
            Assert.IsTrue(AreEqual(0.05m, GetTerrainRatio(plainTiles, TileTerrainType.CottonField)), TileTerrainType.CottonField.ToString());
            Assert.IsTrue(AreEqual(0.02m, GetTerrainRatio(plainTiles, TileTerrainType.GrainField)), TileTerrainType.GrainField.ToString());
        }

        [Test]
        [TestCase(TileTerrainType.Water)]
        [TestCase(TileTerrainType.Mountains)]
        [TestCase(TileTerrainType.Hills)]
        [TestCase(TileTerrainType.City)]
        public void GenerateTerrain_WithHexMapWithUnchangeable_CreatesTerrainMap(TileTerrainType tileTerrainType)
        {
            var heightMapGeneratorMock = new Mock<IHeightMapGenerator>();

            var provinceMock = new Mock<IProvince>();
            provinceMock.Setup(p => p.IsWater).Returns(tileTerrainType == TileTerrainType.Water);

            var hexMap = HexMapBuilder.Create()
                .WithHeight(10)
                .WithWidth(10)
                .WithTiles(
                    TileBuilder.Create()
                    .WithType(tileTerrainType)
                    .WithProvince(provinceMock.Object)
                ).Build();

            var terrainGenerator = new TerrainGenerator(heightMapGeneratorMock.Object);
            terrainGenerator.DesertBelt = 1;
            terrainGenerator.PoleBelt = 1;
            terrainGenerator.GenerateTerrain(hexMap);

            Assert.IsTrue(hexMap.All(t => t.TileTerrainType == tileTerrainType), $"The map does not only consist of {tileTerrainType}");
        }

        [Test]
        public void GenerateTerrain_WithHexMapAndTundraAndPole_CreatesTerrainMap()
        {
            var heightMapGeneratorMock = new Mock<IHeightMapGenerator>();

            var provinceMock = new Mock<IProvince>();
            provinceMock.Setup(p => p.IsWater).Returns(false);

            var hexMap = HexMapBuilder.Create()
                .WithHeight(10)
                .WithWidth(10)
                .WithTiles(
                    TileBuilder.Create()
                    .WithType(TileTerrainType.Plain)
                    .WithProvince(provinceMock.Object)
                ).Build();

            var terrainGenerator = new TerrainGenerator(heightMapGeneratorMock.Object);
            terrainGenerator.DesertBelt = 1;
            terrainGenerator.PoleBelt = 1;
            terrainGenerator.GenerateTerrain(hexMap);

            var tundra = hexMap.Where(t => t.Position.Y == 0 || t.Position.Y == 9);
            var desert = hexMap.Where(t => t.Position.Y == 5);

            Assert.IsTrue(tundra.All(t => t.TileTerrainType == TileTerrainType.Tundra), TileTerrainType.Tundra.ToString());
            Assert.IsTrue(desert.All(t => t.TileTerrainType == TileTerrainType.Desert), TileTerrainType.Desert.ToString());
            Assert.IsFalse(hexMap.Except(tundra).Except(desert).Any(t => t.TileTerrainType == TileTerrainType.Desert || t.TileTerrainType == TileTerrainType.Tundra), "Desert and tundra must not exist outside the belts");
        }

        private decimal GetTerrainRatio(ICollection<TileBase> map, TileTerrainType tileTerrainType) 
            => map.Where(t => t.TileTerrainType == tileTerrainType).Count() / (decimal)map.Count();

        private bool AreEqual(decimal a, decimal b) 
            => Math.Abs(a - b) <= 0.01m;
    }
}
