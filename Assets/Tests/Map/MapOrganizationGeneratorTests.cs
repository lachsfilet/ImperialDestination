using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using Assets.Tests.Helpers;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using VoronoiEngine.Elements;

namespace Tests
{
    public class MapOrganizationGeneratorTests
    {
        [Test]
        public void GenerateCountryOnMap_WithProvinceWithoutNeighbours_ThrowsInvalidOperationException()
        {
            var mapObject = new GameObject();
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

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object);

            Assert.Throws<InvalidOperationException>(() => mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 1, 1));
        }

        [Test]
        public void GenerateCountryOnMap_WithTwoProvinces_AddsProvincesToCountry()
        {
            var mapObject = new GameObject();
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

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 2, 1);

            Assert.AreEqual(2, regions.Count);
            Assert.AreEqual(2, country.Object.Provinces.Count);
            foreach (var province in country.Object.Provinces)
            {
                var neighbour = country.Object.Provinces.Except(new[] { province }).Single();
                Assert.IsTrue(province.GetNeighbours(map.Object).Contains(neighbour));
            }
        }

        [Test]
        public void GenerateCountryOnMap_WithThreeProvinces_CreatesInCountryWithProvincesInRow()
        {
            var mapObject = new GameObject();
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(5, 5, map.Object);

            var regions = provinces.Select(p => p.Object).ToList();

            Func<int, int, int> random = (a, b) =>
            {
                if (a == 2 && b == 23)
                    return 2;
                if (a == 0 && b == 5)
                    return 1;
                return 0;
            };

            var organisationFactory = new Mock<IOrganisationFactory>();

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 3, 1);

            Assert.AreEqual(3, country.Object.Provinces.Count);
            Assert.Contains(provinces[2].Object, country.Object.Provinces);
            Assert.Contains(provinces[3].Object, country.Object.Provinces);
            Assert.Contains(provinces[4].Object, country.Object.Provinces);
        }

        [Test]
        public void GenerateCountryOnMap_WithEightProvinces_AvoidsEnclosedWaterProvince()
        {
            var mapObject = new GameObject();
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(5, 5, map.Object);

            var regions = provinces.Select(p => p.Object).ToList();

            var randomStep = 0;
            Func<int, int, int> random = (a, b) =>
            {
                randomStep++;
                switch (randomStep)
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

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 8, 1);

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
            var mapObject = new GameObject();
            var map = new Mock<IHexMap>();

            var country = new Mock<ICountry>();
            var countryProvinces = new List<IProvince>();
            country.Setup(c => c.Provinces).Returns(countryProvinces);
            country.Setup(c => c.AddProvince(It.IsAny<IProvince>())).Callback((IProvince p) => countryProvinces.Add(p));

            var provinces = GenerateProvinces(6, 6, map.Object);

            var regions = provinces.Select(p => p.Object).ToList();

            var randomStep = 0;
            Func<int, int, int> random = (a, b) =>
            {
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

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object, random);

            mapOrganizationGenerator.GenerateCountryOnMap(country.Object, regions, map.Object, 10, 1);

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
        public void GenerateContinentsList_WithProvinces_Returns_CreatesTwoContinents()
        {
            var mapObject = new GameObject();
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

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapObject, organisationFactory.Object, (a, b) => 0);

            mapOrganizationGenerator.GenerateContinentsList(foo => container, new GameObject(), provinceObjects, hexMap.Object, parent);

            organisationFactory.Verify(o => o.CreateContinent(container, It.IsAny<string>(), parent), Times.Exactly(2));
        }

        [UnityTest]
        public IEnumerator CreateProvinces_With250SitesAnd75x100HexMap_GeneratesCountriesWithoutGaps()
        {
            var mapStartPoint = new GameObject();

            var provinceHandle = Addressables.LoadAssetAsync<GameObject>("Province");
            yield return provinceHandle;

            Assert.IsTrue(provinceHandle.IsDone);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, provinceHandle.Status);
            Assert.IsNotNull(provinceHandle.Result);

            var tileHandle = Addressables.LoadAssetAsync<GameObject>("HexTile");
            yield return tileHandle;

            Assert.IsTrue(tileHandle.IsDone);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, tileHandle.Status);
            Assert.IsNotNull(tileHandle.Result);

            var map = new HexMap(75, 100);

            var points = new List<Point> { new Point(55.7534977778576, 27.5999848300591), new Point(34.1073204433114, 11.6943974037163), new Point(23.9220833759392, 62.735852623701), new Point(47.206688565764, 6.28203753953894), new Point(34.9480648105722, 64.1744775754281), new Point(79.2493699310577, 22.1725175828545), new Point(14.9052525604634, 34.0334284529246), new Point(24.8402489604616, 3.74914301268251), new Point(33.2360914043319, 29.6707651175888), new Point(70.3049171722051, 70.5152757319227), new Point(37.6130736417198, 12.8141014980218), new Point(23.1825671527454, 11.3289682517429), new Point(90.9040163410381, 43.5538312716195), new Point(79.4694560060601, 52.0455507235814), new Point(17.5418697272157, 69.18415325097), new Point(96.1449361830693, 46.5688857997623), new Point(42.824633709539, 15.1376228104986), new Point(32.4161522241385, 71.4878505102768), new Point(11.1073480221011, 21.8525218823238), new Point(35.220485934159, 17.0384362605579), new Point(51.9735490488697, 71.9470319151632), new Point(25.5007672843061, 32.2276772811206), new Point(49.3342768639951, 19.5365616872611), new Point(42.0043869009262, 4.87816693954084), new Point(42.8809934211341, 55.4719746082425), new Point(2.21093669869515, 67.5013460812631), new Point(93.8764508417232, 65.7688367561292), new Point(5.34556264772339, 28.8602301743162), new Point(13.741736876658, 14.1653924836616), new Point(23.7411909963662, 48.3273219588806), new Point(4.46094142294533, 57.2402568912321), new Point(89.8624777918972, 30.9386426680436), new Point(88.7101081408142, 12.668016976988), new Point(85.8385992729285, 70.0036770831811), new Point(13.1150561115309, 21.0796083086541), new Point(92.9083973597309, 2.99061714717681), new Point(4.47418631775034, 35.5127139489691), new Point(42.3644549708648, 30.1677908069304), new Point(90.642422557642, 59.1007062471941), new Point(64.8220455268501, 48.4118949716966), new Point(16.2635765570512, 54.5232446205445), new Point(26.7475136279815, 31.6401300363429), new Point(90.3462412992242, 4.36456581315238), new Point(67.4251666471479, 40.241495511607), new Point(14.9061219230788, 69.3700250235247), new Point(81.8974144816852, 68.1370696742726), new Point(5.49393962998592, 51.9991018706928), new Point(18.3337233040173, 55.6086585594381), new Point(6.39098382433457, 62.1225369182055), new Point(53.4052979514959, 24.2081091637761), new Point(39.2981521735425, 35.3935414084203), new Point(92.4111401994764, 10.9728514202744), new Point(80.3041092107557, 72.662002203363), new Point(85.8316685747503, 34.6575624377735), new Point(45.6235833920648, 47.8278822832871), new Point(46.9678853782676, 25.1708950545503), new Point(53.1289525256161, 43.3340457730619), new Point(48.3905433609106, 22.8111964430712), new Point(49.8306678886668, 35.5298028586105), new Point(50.0446455134287, 61.9439998678602), new Point(48.5225857950387, 23.9206898035112), new Point(78.7474460665823, 14.433148231559), new Point(45.3036831204331, 1.30961118047573), new Point(97.5070211479939, 25.3452394173226), new Point(29.1765291412252, 50.0577017218143), new Point(33.9686282011534, 5.63038499356731), new Point(25.6618631089394, 24.2447329472027), new Point(25.4691127210246, 50.7630713715977), new Point(79.5321194955763, 9.02023957437847), new Point(61.0252539459734, 68.8695316970672), new Point(19.0074127158185, 15.7410469752462), new Point(8.21301195454458, 37.3140674369941), new Point(45.8379589099614, 36.0453541539821), new Point(38.0273974440188, 34.7725353645033), new Point(21.1758226552866, 64.892798753871), new Point(24.5646996435545, 43.2472961615991), new Point(33.0652349582246, 51.9751117322478), new Point(82.8639386267699, 5.45368656863164), new Point(67.6033728446827, 54.6418454528981), new Point(68.3762126985827, 58.7156825730184), new Point(98.7029547727215, 38.8117653759251), new Point(17.1896692552556, 58.0447747530624), new Point(30.017322421082, 29.4769232689761), new Point(8.01271270728331, 15.46781742548), new Point(32.5048133807745, 59.4076378072648), new Point(49.9855622323163, 28.2269263222008), new Point(85.5078606891948, 42.5375770705461), new Point(63.863614134427, 20.5283605887221), new Point(23.7889919717745, 21.5476599259058), new Point(72.4712529133406, 71.8180049461396), new Point(62.7246499460771, 38.1115006451083), new Point(12.7319976462666, 30.9159199981559), new Point(42.404387795089, 56.1433956251216), new Point(73.4673071277641, 41.2064829148382), new Point(48.5715824782716, 13.5838320309221), new Point(0.309384801103447, 4.80640548318923), new Point(42.6914051103831, 60.8991537098303), new Point(92.4333106057874, 49.3957956439796), new Point(3.86929899541163, 22.0942442836679), new Point(19.9028488797615, 4.78579349200511), new Point(40.2101259581792, 12.9567018761098), new Point(14.0152001474123, 30.4207548845656), new Point(36.8304793438085, 64.1345663406581), new Point(71.3500084999716, 56.177851080046), new Point(5.67065696682346, 41.4103410110857), new Point(82.5071562754489, 28.9841541196146), new Point(0.63067546283392, 53.2914873740131), new Point(15.8274118745827, 35.6664655486431), new Point(39.1918506269305, 55.9329236875907), new Point(35.2501081583323, 40.2937948714447), new Point(64.9680361645147, 38.7828174926261), new Point(71.121848942303, 10.7126951425861), new Point(79.168400914021, 32.5565400237946), new Point(94.0716449693179, 3.01581286593145), new Point(63.4750695291325, 72.3406432039759), new Point(23.5943185489598, 69.687426483113), new Point(8.92442588597742, 49.3846379785727), new Point(55.8707184967914, 33.9283900027761), new Point(36.9718074723947, 53.0960918409266), new Point(4.53000822129194, 41.5616427182973), new Point(3.76075051201542, 72.7371057629292), new Point(49.4569350408655, 68.8956903363092), new Point(34.6658081983523, 45.2471032064627), new Point(5.79951565656788, 52.6192598802127), new Point(65.6457802507308, 33.4250999546727), new Point(26.8917138156908, 20.9592796019089), new Point(93.3084078427909, 48.583082068983), new Point(40.4345794531678, 65.2732362538917), new Point(45.2121170811412, 73.763313048409), new Point(60.5296360261411, 68.9733132230925), new Point(90.7300045945356, 50.3165167199059), new Point(11.6817852941723, 5.89802968031635), new Point(31.8060025334386, 65.9845893187377), new Point(29.8516721929664, 25.0867760037476), new Point(15.4246497468206, 51.9022006913564), new Point(10.7441396805198, 50.1988268095063), new Point(70.2991734688632, 58.7869229758097), new Point(50.1458128970795, 49.8248606630717), new Point(2.42818565872879, 58.2660957417759), new Point(78.6660407086211, 13.9292772747247), new Point(37.7558554563466, 17.6969191877623), new Point(37.3888883131504, 14.0209116125577), new Point(69.8883358761148, 70.6768776973136), new Point(98.9220681278603, 32.087700916495), new Point(25.6340578424903, 50.4887149457302), new Point(0.178566841491762, 27.8721794057042), new Point(10.5043246799634, 8.41275352817623), new Point(63.8727336367, 26.4945493193784), new Point(85.6286670102871, 31.0697803502296), new Point(60.3652588624345, 33.9272269103337), new Point(30.6816145291932, 53.2482154971213), new Point(54.9713871199504, 71.1663623243414), new Point(33.9732237723531, 43.9020501840404), new Point(56.7409438363933, 10.9587556137511), new Point(14.850472573587, 54.3989664029325), new Point(84.8968195998561, 49.5288200562488), new Point(20.017310531818, 31.3150971286535), new Point(54.5192527745474, 50.1214680849209), new Point(29.9264516229399, 68.710927900258), new Point(37.002271398903, 27.0973060145496), new Point(88.3546121769373, 64.2256702176415), new Point(7.92798032468556, 35.040821228661), new Point(69.258161110458, 54.1587936888257), new Point(56.6535652408626, 44.1341034081458), new Point(14.7746362903969, 10.9881795127821), new Point(80.0289168497682, 44.7452271118505), new Point(17.5853286839953, 24.873652003181), new Point(86.9534996626682, 48.9733330258044), new Point(67.7042209825964, 30.7808875258923), new Point(83.7844823039065, 8.33771602638891), new Point(37.8132574073101, 62.982992279801), new Point(28.077667841724, 38.761844509636), new Point(81.6488936383505, 24.6046541485026), new Point(17.2711457001377, 50.536620308895), new Point(55.7356472158505, 71.6217873606932), new Point(5.51895895298522, 69.2943383498557), new Point(4.56405636694471, 18.0242006098964), new Point(56.0345157166172, 32.7080618546848), new Point(63.3094971647065, 6.04890247716052), new Point(25.950891188323, 45.235333171317), new Point(2.08025865400222, 72.2783192574411), new Point(98.8864016252972, 0.11217372404047), new Point(91.7480787433442, 18.7130016799611), new Point(48.676200850716, 64.0887821317132), new Point(53.9410221716114, 65.7784316743624), new Point(98.3501755093924, 65.5883793437799), new Point(8.13959691400621, 19.3205474434982), new Point(97.4385438824252, 3.05984914538444), new Point(57.8458639359315, 41.1641205815431), new Point(64.2300428702636, 27.1640723381024), new Point(55.1845377484265, 65.8104890630629), new Point(3.3086262626148, 13.4218801695024), new Point(9.18144336816922, 64.5252560631024), new Point(55.2978015259364, 69.1203885074334), new Point(76.2423970984492, 32.2874833477137), new Point(2.20888080224808, 44.5582930047802), new Point(15.2289448260464, 17.3914515810979), new Point(9.0742533272478, 29.9861357742856), new Point(84.3746285868691, 20.875173955632), new Point(59.1089835060337, 42.3174844739574), new Point(83.2408364560645, 22.8209631390967), new Point(13.6687806740723, 49.8825571536471), new Point(96.4681616353188, 12.5369031059262), new Point(84.5649908783682, 58.0909694368443), new Point(25.6749136902741, 38.8245363397638), new Point(84.9122188156062, 6.15812982439908), new Point(42.8624077410728, 66.2335634996339), new Point(5.33300257536257, 9.74445174901954), new Point(93.3880440953132, 60.4932079140531), new Point(89.9686268842633, 4.05411592687206), new Point(68.7371601810386, 41.2638424314856), new Point(9.49800061830226, 8.03206857854131), new Point(85.7919940053448, 28.9559795981999), new Point(72.517670782524, 62.6926915667451), new Point(16.7735060904052, 42.8467152085373), new Point(18.718953377902, 22.363148582337), new Point(94.961934569274, 5.69285060078504), new Point(52.100287347613, 18.428085278919), new Point(74.3749817211064, 26.5905574264892), new Point(20.4881375103668, 68.3259939571498), new Point(1.75933817902549, 57.2427407946637), new Point(66.796957421022, 18.164730178269), new Point(57.2831980955243, 69.4929527954631), new Point(16.7493626301872, 19.149688188522), new Point(17.9339394047549, 71.0383234652869), new Point(45.7285429540689, 2.57469628219246), new Point(36.9589707050281, 40.1283565927895), new Point(86.8767717503369, 20.956580778098), new Point(21.0327473650839, 2.18499980409862), new Point(79.9427784029128, 43.1515351595131), new Point(43.2546725618908, 0.517503635267496), new Point(60.9427908588866, 50.3446344185363), new Point(33.2219803739441, 41.1065907343787), new Point(12.2766445201247, 26.345716801633), new Point(36.5094800370324, 59.5581994743823), new Point(37.661514537717, 43.2146472508156), new Point(60.4419649692448, 72.9232827038147), new Point(3.66441150692497, 68.1365506537895), new Point(87.4073723183979, 62.9347854354115), new Point(52.4624071649566, 68.634396259037), new Point(21.9890425079451, 35.055439467102), new Point(65.9387155291339, 15.4994487639048), new Point(11.5934138789742, 11.4172486669464), new Point(91.9592956416119, 30.8532914690922), new Point(19.7393391885512, 17.9870683504208), new Point(3.6210423999564, 53.4084208763244), new Point(54.6310754514444, 46.1628302522762), new Point(48.1544274618637, 0.797557225822265), new Point(15.6387137177581, 73.5844044618236), new Point(90.0794829726589, 1.71115329661926) };

            var lines = TestMapHelpers.GenerateMap(75, 100, points, map);
            var grid = TestMapHelpers.CreateMap(mapStartPoint, map, tileHandle.Result, TileTerrainType.Water);

            Assert.IsNotNull(grid);
            yield return null;

            var organizationFactory = new OrganisationFactory();
            var provinceFactory = new ProvinceFactory(map, lines, UnityEngine.Object.Instantiate, provinceHandle.Result, organizationFactory);

            var provinces = provinceFactory.CreateProvinces(points);
            yield return null;

            Assert.IsNotNull(provinces);
            Assert.AreEqual(250, provinces.Count);

            var provinceless = map.Where(t => t.Province == null).ToList();

            //Debug.Log(provinceless.Count);
            foreach (var tile in provinceless)
            {
                Debug.Log($"{tile} is on line: {lines.Contains(tile.Position)}");
                foreach (var neighbour in map.GetNeighbours(tile))
                {
                    Debug.Log($"Neighbour: {neighbour} is on line: {lines.Contains(neighbour.Position)}");
                }
            }

            //foreach (var province in result)
            //    Debug.Log($"Province {province.Name} has {province.HexTiles.Count()} tiles");

            Assert.False(provinceless.Any());

            var countryHandle = Addressables.LoadAssetAsync<GameObject>("Country");
            yield return countryHandle;

            Assert.IsTrue(countryHandle.IsDone);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, countryHandle.Status);
            Assert.IsNotNull(countryHandle.Result);

            var fantasiaObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var fantasia = organizationFactory.CreateCountry(fantasiaObject, "Fantasia", CountryType.Minor, Color.red);
            yield return null;

            fantasia.AddProvince(provinces.Single(p => p.Name == "Region 203"));
            fantasia.AddProvince(provinces.Single(p => p.Name == "Region 214"));
            fantasia.AddProvince(provinces.Single(p => p.Name == "Region 212"));
            fantasia.AddProvince(provinces.Single(p => p.Name == "Region 221"));

            var janlandObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var janland = organizationFactory.CreateCountry(janlandObject, "Janland", CountryType.Major, Color.blue);
            yield return null;


            var step = 0;
            Func<int, int, int> random = (a, b) =>
            {
                step++;
                switch(step)
                {
                    case 1:
                        return 84;
                    case 2:
                        return 5;
                    case 3:
                        return 3;
                    default:
                        return a;
                }                    
            };

            var restProvinces = provinces.Where(p => !p.HexTiles.Any(h => h.Position.X == 0 || h.Position.Y == 0 || h.Position.X == map.Width - 1 || h.Position.Y == map.Height - 1)).ToList();
            foreach (var province in fantasia.Provinces)
            {
                restProvinces.Remove(province);
            }

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapStartPoint, organizationFactory, random);            
            mapOrganizationGenerator.GenerateCountryOnMap(janland, restProvinces, map, 8, 1f / 2);

            TestMapHelpers.LogMap(map);
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

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
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