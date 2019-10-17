using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Map;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tests
{
    public class MapOrganizationGeneratorTests
    {
        [Test]
        public void GenerateCountryOnMap_WithProvinceWithoutNeighbours_ThrowsInvalidOperationException()
        {
            var map = new Mock<IHexMap>();
            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));
            var province = new Mock<IProvince>();
            province.Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince>());
            var regions = new List<IProvince>
            {
                province.Object
            };

            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object);

            Assert.Throws<InvalidOperationException>(() => mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 1, Color.black, 1));
        }

        [Test]
        public void GenerateCountryOnMap_WithTwoProvinces_AddsProvincesToCountry()
        {
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(4);

            provinces[0].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[1].Object });
            provinces[1].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[0].Object, provinces[2].Object });
            provinces[2].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[1].Object, provinces[3].Object });
            provinces[3].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[2].Object });

            var regions = provinces.Select(p => p.Object).ToList();

            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 2, Color.black, 1);


            Assert.AreEqual(2, regions.Count);
            Assert.AreEqual(2, country.Object.Provinces.Count);
            foreach(var province in country.Object.Provinces)
            {
                var neighbour = country.Object.Provinces.Except(new[] { province }).Single();
                Assert.IsTrue(province.GetNeighbours(map.Object).Contains(neighbour));
            }
        }

        [Test]
        public void GenerateCountryOnMap_WithThreeProvinces_CreatesInCountryWithProvincesInRow()
        {
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(5, 5, map.Object);
            
            var regions = provinces.Select(p => p.Object).ToList();

            Func<int, int, int> random = (a, b) => {
                if (a == 2 && b == 23)
                    return 2;
                if (a == 0 && b == 5)
                    return 1;
                return 0;
            };

            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 3, Color.black, 1);

            Assert.AreEqual(3, country.Object.Provinces.Count);
            Assert.Contains(provinces[2].Object, country.Object.Provinces);
            Assert.Contains(provinces[3].Object, country.Object.Provinces);
            Assert.Contains(provinces[4].Object, country.Object.Provinces);
        }

        [Test]
        public void GenerateCountryOnMap_WithEightProvinces_AvoidsEnclosedWaterProvince()
        {
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(5, 5, map.Object);

            var regions = provinces.Select(p => p.Object).ToList();

            var randomStep = 0;
            Func<int, int, int> random = (a, b) => {
                randomStep++;
                switch(randomStep)
                {
                    // First region is at index 6
                    case 1:
                        return 6;
                    // First and second neighbours are the right ones
                    case 2:
                        return 4;
                    case 3:
                        return 3;
                    // Third and fourth neighbours are the bottom ones
                    case 4:
                        return 5;
                    case 5:
                        return 4;
                    // Fifth and sixth neighbours are the left ones
                    case 6:
                    case 7:
                        return 2;
                    // Seventh neighbour is the upper one
                    case 8:
                        return 1;
                    case 9:
                        return 2;
                    default:
                        return 0;
                }                
            };
            
            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 8, Color.black, 1);

            Assert.AreEqual(8, country.Object.Provinces.Count);
            Assert.Contains(provinces[6].Object, country.Object.Provinces, provinces[6].Object.Name);
            Assert.Contains(provinces[7].Object, country.Object.Provinces, provinces[7].Object.Name);
            Assert.Contains(provinces[8].Object, country.Object.Provinces, provinces[8].Object.Name);
            Assert.Contains(provinces[13].Object, country.Object.Provinces, provinces[13].Object.Name);
            Assert.Contains(provinces[18].Object, country.Object.Provinces, provinces[18].Object.Name);
            Assert.Contains(provinces[17].Object, country.Object.Provinces, provinces[17].Object.Name);
            Assert.Contains(provinces[16].Object, country.Object.Provinces, provinces[16].Object.Name);
            Assert.Contains(provinces[12].Object, country.Object.Provinces, $"{provinces[12].Object.Name} {string.Join(",", countryProvinces.Select(p => p.Name))}");
        }

        [Test]
        public void GenerateCountryOnMap_WithTenProvinces_AvoidsEnclosedWaterProvince()
        {
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(6, 6, map.Object);

            var regions = provinces.Select(p => p.Object).ToList();

            var randomStep = 0;
            Func<int, int, int> random = (a, b) => {
                randomStep++;
                switch (randomStep)
                {
                    // First region is at index 8
                    case 1:
                        return 8;
                    // First, second and third neighbours are the right ones
                    // Index 9
                    case 2:
                        return 4;
                    // Index 10
                    case 3:
                    // Index 11
                    case 4:
                        return 3;
                    // Fourth and fifth neighbours are the bottom ones
                    // Index 17
                    case 5:
                        return 3;
                    // Index 23
                    case 6:
                        return 2;
                    // Sixth, seventh and eighth neighbours are the left ones
                    // Index 22
                    case 7:
                        return 1;
                    // Index 21
                    case 8:
                        return 2;
                    // Index 20
                    case 9:
                        return 3;
                    // Index 14 -> Enclosing water
                    case 10:
                        return 1;
                    // Index 19
                    case 11:
                        return 3;
                    default:
                        return 0;
                }
            };

            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 10, Color.black, 1);

            Assert.AreEqual(10, country.Object.Provinces.Count);
            Assert.Contains(provinces[8].Object, country.Object.Provinces, provinces[6].Object.Name);
            Assert.Contains(provinces[9].Object, country.Object.Provinces, provinces[9].Object.Name);
            Assert.Contains(provinces[10].Object, country.Object.Provinces, provinces[10].Object.Name);
            Assert.Contains(provinces[11].Object, country.Object.Provinces, provinces[11].Object.Name);
            Assert.Contains(provinces[17].Object, country.Object.Provinces, provinces[17].Object.Name);
            Assert.Contains(provinces[23].Object, country.Object.Provinces, provinces[23].Object.Name);
            Assert.Contains(provinces[22].Object, country.Object.Provinces, provinces[22].Object.Name);
            Assert.Contains(provinces[21].Object, country.Object.Provinces, provinces[21].Object.Name);
            Assert.Contains(provinces[20].Object, country.Object.Provinces, provinces[20].Object.Name);
            Assert.Contains(provinces[19].Object, country.Object.Provinces, provinces[19].Object.Name);
        }

        [Test]
        public void GenerateContinentsList_WithProvinces_Returns_TwoContinents()
        {
            var hexMap = new Mock<IHexMap>();

            var provinces = GenerateProvinces(10, 10, hexMap.Object);
            var provinceObjects = provinces.Select(p => p.Object).ToList();

            var countries = GenerateCountries(3);                      

            provinceObjects[12].IsWater = false;
            provinceObjects[12].Owner = countries[0].Object;
            provinceObjects[13].IsWater = false;
            provinceObjects[13].Owner = countries[0].Object;
            provinceObjects[14].IsWater = false;
            provinceObjects[14].Owner = countries[1].Object;
            provinceObjects[22].IsWater = false;
            provinceObjects[22].Owner = countries[0].Object;
            provinceObjects[23].IsWater = false;
            provinceObjects[23].Owner = countries[0].Object;
            provinceObjects[33].IsWater = false;
            provinceObjects[33].Owner = countries[0].Object;
            provinceObjects[34].IsWater = false;
            provinceObjects[34].Owner = countries[1].Object;
            provinceObjects[42].IsWater = false;
            provinceObjects[42].Owner = countries[0].Object;
            provinceObjects[43].IsWater = false;
            provinceObjects[43].Owner = countries[0].Object;
            provinceObjects[44].IsWater = false;
            provinceObjects[44].Owner = countries[1].Object;

            provinceObjects[64].IsWater = false;
            provinceObjects[64].Owner = countries[2].Object;
            provinceObjects[65].IsWater = false;
            provinceObjects[65].Owner = countries[2].Object;
            provinceObjects[74].IsWater = false;
            provinceObjects[74].Owner = countries[2].Object;

            var parent = new GameObject();

            var continent = new Mock<IContinent>();
            continent.Setup(c => c.AddCountry(It.IsAny<ICountry>()));
            var container = new GameObject();
            var organisationFactory = new Mock<IOrganisationFactory>();
            organisationFactory.Setup(o => o.CreateContinent(container, It.IsAny<string>(), parent)).Returns(continent.Object);

            var mapOrganizationGenerator = new MapOrganizationGenerator(organisationFactory.Object, (a, b) => 0);

            var result = mapOrganizationGenerator.GenerateContinentsList(foo => container, new GameObject(), provinceObjects, hexMap.Object, parent);

            Assert.AreEqual(2, result.Count);
        }

        private IList<Mock<IProvince>> GenerateProvinces(int count)
        {
            return Enumerable.Range(0, count).Select(
                n => new Mock<IProvince>
                {
                    Name = $"Province {n}"
                }).ToList();
        }

        private IList<Mock<IProvince>> GenerateProvinces(int height, int width, IHexMap map)
        {
            var provinces = Enumerable.Range(0, height * width).Select(
                n => new Mock<IProvince>())
                .ToList();

            for (var i=0; i<height; i++)
            {
                for(var j=0; j<width; j++)
                {
                    var index = j + i * width;
                    var province = provinces[index];
                    province.Setup(m => m.Name).Returns($"Province {index}");
                    province.SetupProperty(p => p.IsWater);
                    province.Object.IsWater = true;
                    province.SetupProperty(p => p.Owner);
                    province.Setup(m => m.GetNeighbours(map)).Returns(() =>
                    {
                        var list = new List<IProvince>();
                        // Top left -> 0
                        if (index % width > 0 && index >= width)
                            list.Add(provinces[index - width - 1].Object);
                        // Top -> 1
                        if (index >= width)
                            list.Add(provinces[index - width].Object);
                        // Top right -> 2
                        if (index >= width && index % width < width - 1)
                            list.Add(provinces[index - width + 1].Object);
                        // Left -> 3
                        if (index % width > 0)
                            list.Add(provinces[index - 1].Object);
                        // Right -> 4
                        if (index % width < width - 1)
                            list.Add(provinces[index + 1].Object);
                        // Bottom left -> 5
                        if (index + width < provinces.Count && index % width > 0)
                            list.Add(provinces[index + width - 1].Object);
                        // Bottom -> 6
                        if (index + width < provinces.Count)
                            list.Add(provinces[index + width].Object);
                        // Bottom right -> 7
                        if (index + width < provinces.Count && index % width < width - 1)
                            list.Add(provinces[index + width + 1].Object);
                        return list;
                    });
                }
            }
            return provinces;
        }

        private IList<Mock<ICountry>> GenerateCountries(int count) =>
           Enumerable.Range(0, count).Select(i =>
           {
               var country = new Mock<ICountry>();

               Transform parent = null;
               country.Setup(c => c.GetParent()).Returns(parent);
               country.Setup(c => c.SetParent(It.IsAny<Transform>())).Callback<Transform>(t => parent = t);
               return country;
           }).ToList();
    }
}