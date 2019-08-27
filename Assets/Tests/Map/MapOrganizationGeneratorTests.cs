using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Map;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    }
}