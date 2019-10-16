using Assets.Contracts.Map;
using Assets.Scripts.Map;
using NUnit.Framework;
using Moq;
using System.Linq;
using UnityEngine;
using System;
using Assets.Contracts.Organization;
using System.Collections.Generic;
using Assets.Contracts.Utilities;

namespace Tests
{
    
    public class HeightMapGeneratorTests
    {
        [Test]
        public void GenerateHeightMap_Generates_TwoMountainsWith28Hills()
        {
            var province = new Mock<IProvince>();
            var country = new Mock<ICountry>();
            var continent = new Mock<IContinent>();

            province.Setup(p => p.Owner).Returns(country.Object);
            country.Setup(c => c.Continent).Returns(continent.Object);
            country.Setup(c => c.Provinces).Returns(new List<IProvince> { province.Object });
            continent.Setup(c => c.Countries).Returns(new List<ICountry> { country.Object });

            var hexMap = HexMapBuilder.Create()
                .WithHeight(8)
                .WithWidth(8)
                .WithTiles(TileBuilder.Create().WithProvince(province.Object).WithType(TileTerrainType.Plain))
                .Build();

            province.Setup(p => p.HexTiles).Returns(hexMap.ToList());

            Assert.IsTrue(hexMap.All(t => t.TileTerrainType == TileTerrainType.Plain));
            
            var counter = 0;
            Func<int, int, int> random = (start, end) => {
                switch(counter++)
                {
                    case 2:
                        return 3;
                    default:
                        return 0;
                }
            };

            var generator = new HeightMapGenerator(random);

            generator.GenerateHeightMap(hexMap, 0);

            var mountains = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Mountains).ToList();
            var hills = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Hills).ToList();

            Assert.AreEqual(2, mountains.Count);
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(3, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(4, 4))));

            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 2))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(3, 2))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 2))));
            //Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 2))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(6, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(3, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 5))));            
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 6))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(3, 6))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 6))));
            //Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 6))));


            var comparer = new PositionComparer(hills.Count);
            Assert.AreEqual(22, hills.Count, $"The following tiles are hills: {string.Join(",", hills.OrderBy(h => h.Position, comparer).Select(h => h.Position.ToString()))}");
        }

        [Test]
        public void GenerateHeightMap_Generates_ThreeMountains()
        {
            var province = new Mock<IProvince>();
            var country = new Mock<ICountry>();
            var continent = new Mock<IContinent>();

            province.Setup(p => p.Owner).Returns(country.Object);
            country.Setup(c => c.Continent).Returns(continent.Object);
            country.Setup(c => c.Provinces).Returns(new List<IProvince> { province.Object });
            continent.Setup(c => c.Countries).Returns(new List<ICountry> { country.Object });

            var hexMap = HexMapBuilder.Create()
                .WithHeight(8)
                .WithWidth(8)
                .WithTiles(TileBuilder.Create().WithProvince(province.Object).WithType(TileTerrainType.Plain))
                .Build();

            province.Setup(p => p.HexTiles).Returns(hexMap.ToList());

            Assert.IsTrue(hexMap.All(t => t.TileTerrainType == TileTerrainType.Plain));

            var counter = 0;
            Func<int, int, int> random = (start, end) => {
                switch (counter++)
                {
                    case 2:
                        return 2;
                    case 3:
                        return 1;
                    default:
                        return 0;
                }
            };

            var generator = new HeightMapGenerator(random);

            generator.GenerateHeightMap(hexMap, 0);

            var mountains = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Mountains).ToList();
            var hills = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Hills).ToList();

            Assert.AreEqual(4, mountains.Count);
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(2, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(5, 4))));
        }
    }
}
