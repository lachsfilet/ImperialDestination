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
            var province = new Mock<IProvince>();
            province.Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince>());
            var regions = new List<IProvince>
            {
                province.Object
            };

            var mapOrganizationGenerator = new MapOrganizationGenerator();

            Assert.Throws<InvalidOperationException>(() => mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 1, Color.black, 1));
        }

        [Test]
        public void GenerateCountryOnMap_WithTwoProvinces_AddsProvincesToCountry()
        {
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);

            var provinces = GenerateProvinces(4);

            provinces[0].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[1].Object });
            provinces[1].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[0].Object, provinces[2].Object });
            provinces[2].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[1].Object, provinces[3].Object });
            provinces[3].Setup(p => p.GetNeighbours(map.Object)).Returns(new List<IProvince> { provinces[2].Object });

            var regions = provinces.Select(p => p.Object).ToList();

            var mapOrganizationGenerator = new MapOrganizationGenerator();

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 2, Color.black, 1);


            Assert.AreEqual(2, regions.Count);
            Assert.AreEqual(2, country.Object.Provinces.Count);
            foreach(var province in country.Object.Provinces)
            {
                var neighbour = country.Object.Provinces.Except(new[] { province }).Single();
                Assert.IsTrue(province.GetNeighbours(map.Object).Contains(neighbour));
            }
        }

        private IList<Mock<IProvince>> GenerateProvinces(int count)
        {
            return Enumerable.Range(0, count).Select(
                n => new Mock<IProvince>
                {
                    Name = $"Province {n}"
                }).ToList();
        }
    }
}