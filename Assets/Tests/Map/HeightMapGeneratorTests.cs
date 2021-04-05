using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using Assets.Scripts.Map;
using Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class HeightMapGeneratorTests
    {
        [Test]
        public void GenerateHeightMap_Generates_TwoMountainsWith38Hills()
        {
            var province = new Mock<IProvince>();
            var country = new Mock<ICountry>();
            var continent = new Mock<IContinent>();

            province.Setup(p => p.Owner).Returns(country.Object);
            country.Setup(c => c.Continent).Returns(continent.Object);
            country.Setup(c => c.Provinces).Returns(new List<IProvince> { province.Object });
            continent.Setup(c => c.Countries).Returns(new List<ICountry> { country.Object });
            continent.Setup(c => c.TileCount).Returns(64);

            var hexMap = HexMapBuilder.New
                .WithHeight(8)
                .WithWidth(8)
                .WithTiles(TileBuilder.New.WithProvince(province.Object).WithType(TileTerrainType.Plain))
                .Build();

            province.Setup(p => p.HexTiles).Returns(hexMap.ToList());

            Assert.IsTrue(hexMap.All(t => t.TileTerrainType == TileTerrainType.Plain));

            var counter = 0;
            Func<int, int, int> random = (start, end) =>
            {
                counter++;

                if (counter < 3 || counter == 4)
                    return 0;
                
                if (counter == 3)
                    return 3;

                if(counter % 2 == 0)
                    return end;
                return start;
            };

            var generator = new HeightMapGenerator(0.25, random);

            generator.GenerateHeightMap(hexMap, 0);

            var mountains = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Mountains).ToList();
            var hills = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Hills).ToList();

            Assert.AreEqual(16, mountains.Count);
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(3, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(4, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(2, 3))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(2, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(2, 5))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(3, 5))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(3, 3))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(4, 2))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(4, 1))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(5, 0))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(6, 0))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(7, 0))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(7, 1))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(7, 2))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(7, 3))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(7, 4))));

            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 2))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(3, 2))));
            //Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 2))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 3))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(6, 4))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(1, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 5))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(2, 6))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(3, 6))));
            Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(4, 6))));
            //Assert.IsTrue(hills.Any(h => h.Position.Equals(new Position(5, 6))));

            var comparer = new PositionComparer(hills.Count);
            Assert.AreEqual(38, hills.Count, $"The following tiles are hills: {string.Join(",", hills.OrderBy(h => h.Position, comparer).Select(h => h.Position.ToString()))}");
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
            continent.Setup(c => c.TileCount).Returns(64);

            var hexMap = HexMapBuilder.New
                .WithHeight(8)
                .WithWidth(8)
                .WithTiles(TileBuilder.New.WithProvince(province.Object).WithType(TileTerrainType.Plain))
                .Build();

            province.Setup(p => p.HexTiles).Returns(hexMap.ToList());

            Assert.IsTrue(hexMap.All(t => t.TileTerrainType == TileTerrainType.Plain));

            var counter = 0;
            Func<int, int, int> random = (start, end) =>
            {
                counter++;
                if (counter == 3)
                    return 2;

                if(counter == 4)
                    return 1;

                if(counter < 3)
                    return 0;

                if (counter % 2 == 0)
                    return end;
                return start;
            };

            var generator = new HeightMapGenerator(0.25, random);

            generator.GenerateHeightMap(hexMap, 0);

            var mountains = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Mountains).ToList();
            var hills = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Hills).ToList();

            Assert.AreEqual(16, mountains.Count);
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(2, 4))));
            Assert.IsTrue(mountains.Any(h => h.Position.Equals(new Position(5, 4))));
        }
    }
}