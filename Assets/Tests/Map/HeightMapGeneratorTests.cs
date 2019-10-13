using Assets.Contracts.Map;
using Assets.Scripts.Map;
using NUnit.Framework;
using Moq;
using System.Linq;
using UnityEngine;
using System;
using Assets.Contracts.Organization;
using System.Collections.Generic;

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
            
            var counter = 0;
            Func<int, int, int> random = (start, end) => {
                switch(counter++)
                {
                    case 0:
                        return 0;
                    case 1:
                        return 0;
                    case 2:
                        return 15;
                    case 3:
                        return 12;
                    default:
                        Assert.Inconclusive("This is not allowed to happen!");
                        return 0;
                }
            };

            var generator = new HeightMapGenerator(random);

            generator.GenerateHeightMap(hexMap, 0);

            var mountains = hexMap.Where(m => m.TileTerrainType == TileTerrainType.Mountains).ToList();

            Assert.AreEqual(2, mountains.Count);
        }
    }
}
