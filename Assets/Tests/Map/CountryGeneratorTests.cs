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
    public class CountryGeneratorTests
    {
        [Test]
        public void GenerateCountries_WithMajorAndMinorCountry_CreatesCountry()
        {
            var provinces = new List<IProvince>();
            var map = new Mock<IHexMap>();
            var majorCountryNames = new string[] { "Janland" };
            var minorCountryNames = new string[] { "Fantasia" };

            var original = new GameObject();
            var countryContainer = new GameObject();
            var country = new Mock<ICountry>();
            
            var instantiate = new Mock<Func<GameObject, GameObject>>();
            instantiate.Setup(f => f.Invoke(original)).Returns(countryContainer);

            var colors = new List<Color> { Color.blue };

            var organisationFactory = new Mock<IOrganisationFactory>();
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, majorCountryNames[0], CountryType.Major, Color.blue)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, minorCountryNames[0], CountryType.Minor, Color.grey)).Returns(country.Object);

            var mapOrganizationGenerator = new Mock<IMapOrganizationGenerator>();
            mapOrganizationGenerator.Setup(m => m.GenerateCountryOnMap(country.Object, It.IsAny<IList<IProvince>>(), map.Object, 0, 0.5f));

            var countryGenerator = new CountryGenerator(organisationFactory.Object, mapOrganizationGenerator.Object);
            countryGenerator.GenerateCountries(provinces, map.Object, 1, 1, 4, 2, majorCountryNames, minorCountryNames, instantiate.Object, original, colors);

            instantiate.Verify(f => f.Invoke(original), Times.Exactly(2));

            organisationFactory.Verify(o => o.CreateCountry(countryContainer, majorCountryNames[0], CountryType.Major, Color.blue), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, minorCountryNames[0], CountryType.Minor, Color.grey), Times.Once);
        }
        
        [Test]
        public void GenerateCountries_WithMajorAndMinorCountryWithEmptyNames_CreatesCountry()
        {
            var provinces = new List<IProvince>();
            var map = new Mock<IHexMap>();

            var original = new GameObject();
            var countryContainer = new GameObject();
            var country = new Mock<ICountry>();

            var instantiate = new Mock<Func<GameObject, GameObject>>();
            instantiate.Setup(f => f.Invoke(original)).Returns(countryContainer);

            var colors = new List<Color> { Color.blue, Color.green };

            var organisationFactory = new Mock<IOrganisationFactory>();
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Major 0", CountryType.Major, Color.blue)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Major 1", CountryType.Major, Color.green)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Minor 0", CountryType.Minor, Color.grey)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Minor 1", CountryType.Minor, Color.grey)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Minor 2", CountryType.Minor, Color.grey)).Returns(country.Object);
            organisationFactory.Setup(o => o.CreateCountry(countryContainer, "Minor 3", CountryType.Minor, Color.grey)).Returns(country.Object);

            var mapOrganizationGenerator = new Mock<IMapOrganizationGenerator>();
            mapOrganizationGenerator.Setup(m => m.GenerateCountryOnMap(country.Object, It.IsAny<IList<IProvince>>(), map.Object, 0, 0.5f));

            var countryGenerator = new CountryGenerator(organisationFactory.Object, mapOrganizationGenerator.Object);
            countryGenerator.GenerateCountries(provinces, map.Object, 2, 4, 4, 2, null, null, instantiate.Object, original, colors);

            instantiate.Verify(f => f.Invoke(original), Times.Exactly(6));

            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Major 0", CountryType.Major, Color.blue), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Major 1", CountryType.Major, Color.green), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Minor 0", CountryType.Minor, Color.grey), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Minor 1", CountryType.Minor, Color.grey), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Minor 2", CountryType.Minor, Color.grey), Times.Once);
            organisationFactory.Verify(o => o.CreateCountry(countryContainer, "Minor 3", CountryType.Minor, Color.grey), Times.Once);
        }
    }
}