using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Contracts.Utilities;
using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using Assets.Tests.Helpers;
using Helpers;
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

        [UnityTest]
        public IEnumerator GenerateHeightMap_With75x100MapAnd2Countries_GeneratesHeighMap()
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

            var points = new List<Point> { new Point(0.888977197878518, 5.62189431005246), new Point(2.43869395993589, 13.2290489972704), new Point(0.803911121936474, 20.722116483246), new Point(0.307733145685742, 21.4258438862981), new Point(0.372129180176244, 33.5820341192102), new Point(2.51086461055598, 40.3149855441018), new Point(2.28659348342875, 47.3048257074807), new Point(2.3515645066982, 50.0484544532599), new Point(1.92155625946892, 61.311981388513), new Point(0.887307138595407, 65.1869759667604), new Point(4.27423679562017, 1.42291418994913), new Point(5.33994299096053, 7.4565105347226), new Point(4.91132826447083, 16.87748606637), new Point(3.22075553900597, 22.9641546998006), new Point(3.3287483664829, 29.8292698952506), new Point(5.38785550994233, 37.3262777153059), new Point(4.76539842633782, 45.0657733632558), new Point(4.34895839604966, 50.9681192561836), new Point(4.68200985560287, 61.0916425539607), new Point(4.58012883531867, 68.9643305190673), new Point(6.5680821298473, 6.59376687630721), new Point(7.8124951137288, 9.21394470297449), new Point(7.98948490572604, 18.9070018105707), new Point(6.0770955523835, 21.0318549135848), new Point(7.76774218807357, 30.8250893358258), new Point(6.00721386960112, 37.0315516661068), new Point(6.44504572658103, 45.8200720133353), new Point(8.43169215248511, 49.6513659384341), new Point(6.06944028477624, 61.2337753717945), new Point(6.75826416293078, 68.9983065514817), new Point(11.6601515252423, 5.75388930679946), new Point(11.8537561562163, 13.0390315251607), new Point(9.00444482686205, 17.5324063759914), new Point(9.51245252113438, 23.2696114314113), new Point(9.92453833572778, 31.5188645927789), new Point(10.7673277672228, 41.3919887181334), new Point(9.69643461690118, 45.106052804322), new Point(9.36919328447859, 52.3794523134732), new Point(10.7973252533923, 59.2459222759334), new Point(10.2061624662979, 69.5527641398612), new Point(13.1561127617751, 0.483241344095786), new Point(14.0740383975553, 11.9419901859676), new Point(12.7767589426491, 17.5401111904253), new Point(12.5519304371215, 21.2991909814529), new Point(13.8822107174817, 30.4962036523485), new Point(12.7740393964453, 40.6909142912789), new Point(13.4238683601952, 46.6886607309285), new Point(14.8959901257865, 51.5716356628442), new Point(14.9721439336297, 61.1901237402065), new Point(12.9443886172699, 64.1097250707027), new Point(16.5905482548245, 4.91027819081688), new Point(17.0891137700943, 7.60586717939278), new Point(17.443424805274, 14.6248436442692), new Point(15.1742303828589, 25.9323872243671), new Point(16.2679804355223, 30.2944960423254), new Point(16.4818315773652, 38.9161050552112), new Point(16.036884121614, 44.2165189069773), new Point(16.4145900460028, 54.7237788814696), new Point(15.3919141625017, 57.716774178537), new Point(16.8113974252769, 66.4702600722528), new Point(18.9619580097319, 0.896531567860642), new Point(20.0904798959803, 13.6610524643497), new Point(18.7340454290314, 19.4328504989077), new Point(19.3155553761476, 24.9363533751743), new Point(19.4351753617801, 33.2898153342725), new Point(19.4761927721445, 41.0036351843754), new Point(18.4467209295587, 43.7046540173258), new Point(19.2177792336875, 53.787318253325), new Point(19.1179896630896, 60.2335557482362), new Point(20.6745709887122, 64.1875938885787), new Point(22.3429601273234, 3.35545046224978), new Point(22.8602442708147, 9.09732896513181), new Point(23.587969987927, 18.8899868698278), new Point(22.402079146077, 22.105840600145), new Point(21.594835981538, 29.0335314120322), new Point(21.7102154072887, 40.2182283332656), new Point(21.4129413205259, 47.7333133680435), new Point(23.5136384491407, 51.8147476933965), new Point(21.0139250429412, 61.213546332537), new Point(23.5256119335655, 63.2684740467316), new Point(25.9522347571106, 3.47018418482979), new Point(24.6787048772344, 11.6688056819461), new Point(26.5071626745663, 18.4539718294768), new Point(25.2316098083889, 21.7147422664402), new Point(26.7321574407314, 33.5133918540149), new Point(25.3350584289688, 38.6809106309344), new Point(25.061189373518, 45.039000103734), new Point(24.1442941893564, 54.8891182271247), new Point(25.5239835439827, 58.7804574588223), new Point(24.2667667825086, 66.8227857979121), new Point(27.9262853534549, 1.96378861086619), new Point(28.0921726078224, 9.31521109878794), new Point(28.5568483073995, 19.226221468405), new Point(29.1600451870635, 23.3090354675935), new Point(28.2169275582847, 28.0267371852075), new Point(29.0377827799124, 39.3950962146721), new Point(29.2887691013928, 47.9721915935968), new Point(28.5567340522803, 55.6633894455914), new Point(27.7593453208727, 62.6717642832882), new Point(28.3916925962044, 64.5846248090196), new Point(30.8640988501087, 2.55677574107273), new Point(30.741774245045, 9.87455876957372), new Point(32.1078836047593, 18.9762586527393), new Point(31.1751941978816, 25.9245296441598), new Point(32.3128401112337, 34.9760381542035), new Point(32.6823368960444, 39.2520343080405), new Point(31.9676102171501, 44.0703087137408), new Point(32.1887749522872, 51.5914258223918), new Point(30.3950491856761, 62.268433614759), new Point(30.4440684134346, 63.6238247722498), new Point(35.7487998827122, 0.564720751980655), new Point(35.089390555904, 13.3482968431657), new Point(33.3250994432369, 19.7200401195884), new Point(35.2887640261505, 24.5039169446118), new Point(35.7456945636057, 28.6732969962402), new Point(34.7645624907522, 38.8841973193382), new Point(35.7999320290051, 48.0378310168338), new Point(33.774237920425, 55.604556750322), new Point(33.0998494453262, 62.6738439820119), new Point(34.8843497232927, 68.6563873978594), new Point(37.0646150028634, 0.115575792321738), new Point(37.6767465177349, 9.86345948179414), new Point(36.329121897616, 14.5027921784217), new Point(38.9160024462808, 24.2701525228425), new Point(36.3707357306828, 29.0409536329289), new Point(38.4606891481488, 37.5033721339439), new Point(38.4152591365461, 44.6234579522272), new Point(36.930325035439, 50.451509898273), new Point(38.0063704759844, 58.3825097286061), new Point(36.9068543151519, 69.1983549581833), new Point(41.843918253595, 6.23820285044527), new Point(41.3648181410343, 11.7233299299718), new Point(40.2251681467635, 16.374588771432), new Point(41.0223683887265, 26.5532594898498), new Point(39.1130372426068, 33.5025848050148), new Point(40.0107616214132, 36.2479374512322), new Point(39.8021218258897, 44.3797722334879), new Point(41.202738470958, 55.2982906006734), new Point(40.5652766616853, 58.0117851486484), new Point(41.391576749455, 63.2557731891311), new Point(44.5354433192571, 2.07029687150861), new Point(43.1309429598651, 12.3656670154844), new Point(42.8278667073827, 15.613940344478), new Point(44.249396857456, 24.9097167821181), new Point(43.6573168289183, 28.3550452493853), new Point(42.8241538455822, 39.8504723104883), new Point(43.9533645342818, 47.1984610628329), new Point(44.5802477740591, 52.2458988229027), new Point(42.6847143413893, 62.1890786114098), new Point(42.0020287740054, 68.3933623230054), new Point(46.1931139734542, 1.25074621813872), new Point(46.2884566957543, 9.36246523324515), new Point(47.4353613641278, 18.4936705611151), new Point(45.2707519458005, 24.2558324184529), new Point(46.9216006514251, 30.872467815351), new Point(47.0567990090031, 37.6697459619817), new Point(46.6294988485191, 43.8602202128899), new Point(45.1209945199643, 53.5655123868797), new Point(46.5425725474686, 56.1522419802622), new Point(46.1964175483195, 64.2447853313036), new Point(49.7682840445863, 0.809142791577216), new Point(48.7226515238744, 13.9087083166971), new Point(49.5340967851384, 18.5079714728091), new Point(49.5814658019605, 24.6731565309098), new Point(49.4396546117215, 29.8938242759992), new Point(50.0145530588993, 39.4632335991893), new Point(48.8601648611297, 42.1869834257229), new Point(49.916503107602, 53.6652881934612), new Point(50.5988133920351, 56.2764016731067), new Point(50.864210213937, 67.4380184227778), new Point(51.4209367732615, 1.99276024894452), new Point(53.7539922551969, 7.99460478545847), new Point(52.0953632407334, 20.2532796777102), new Point(51.4492749988331, 23.697630367101), new Point(53.1069173641069, 29.4550657525915), new Point(53.8485269480611, 36.4454248163129), new Point(52.4560447137132, 47.7599484747089), new Point(51.5431569509875, 50.9747898131492), new Point(53.8133825696136, 59.0976906400629), new Point(53.4783883148238, 64.1745383768224), new Point(54.3443930099459, 3.95859800929138), new Point(56.955702139044, 9.58991057732604), new Point(56.2165045762511, 14.3359640884846), new Point(54.2840119797196, 24.6151805541549), new Point(55.8807341264937, 30.6170323531223), new Point(56.3578347737705, 38.3229342709868), new Point(54.2010363490326, 47.3536225233011), new Point(54.1125426544401, 51.1332358550901), new Point(54.5927832269076, 56.6915821035819), new Point(55.8655117521368, 67.3262794876128), new Point(58.4827127752279, 0.992456504140262), new Point(59.4672925777115, 8.90412834142527), new Point(57.9631120273672, 17.2551989877854), new Point(57.4567696859393, 24.247102867927), new Point(59.5369788028938, 30.4970507712555), new Point(59.1860266561555, 37.9662072746857), new Point(57.6400695450791, 47.5086187890305), new Point(58.5576148855303, 53.0235877949854), new Point(57.8983379676465, 59.8360713514667), new Point(58.2097546175168, 65.2198834732267), new Point(62.3959650250133, 4.43312777645566), new Point(62.2753931736925, 9.29910816312726), new Point(61.265764834483, 17.3236286078224), new Point(60.4184314293873, 26.0438151420298), new Point(62.3560066918638, 34.1341303638807), new Point(60.253555264442, 40.8729772255164), new Point(62.4620699507473, 43.4566265803094), new Point(62.0206608148388, 55.8111271857336), new Point(62.2292494262705, 60.9925099830108), new Point(60.1468496486297, 64.9166450234673), new Point(65.6872149234112, 4.41559175141882), new Point(65.012384291744, 8.89132885722971), new Point(64.9355586380398, 16.5354071974454), new Point(63.7840451424867, 22.9604660253788), new Point(63.7367947673131, 31.2390117320414), new Point(63.0159059702493, 39.5470895113177), new Point(65.0866377125898, 44.132977281759), new Point(65.4581547553922, 55.0265353657429), new Point(64.1499453280819, 59.457907851533), new Point(65.5502901689849, 68.9236327893676), new Point(66.9766538510922, 3.02198079182859), new Point(68.0356088229621, 13.7854868428714), new Point(66.8166093518104, 20.2277516388464), new Point(67.8091617384037, 21.036869024875), new Point(66.3401758090314, 31.8233075471703), new Point(68.495145343009, 39.1036119414976), new Point(67.4971809305703, 42.9061157507385), new Point(67.2641637079716, 50.1749933069455), new Point(68.8875276124512, 59.417921881386), new Point(68.0497734318719, 63.4180440830151), new Point(69.6403671180086, 6.01587397652486), new Point(70.4248402935568, 9.25833721564074), new Point(71.6130070428425, 17.044539439047), new Point(69.4303525269173, 24.6118526033181), new Point(70.1661137790261, 33.688339868881), new Point(70.7691091800896, 36.7436741430981), new Point(71.5893569665911, 43.4190163907684), new Point(71.8447654446795, 52.2481873627976), new Point(71.8567876419317, 60.9100765045314), new Point(71.993960158431, 68.2885518499131), new Point(74.2774413462158, 1.79256964092728), new Point(72.5000239161309, 11.8958802432268), new Point(73.8930025421516, 16.9335019890841), new Point(72.4515717171373, 24.9627055786376), new Point(74.6499694127822, 32.5607011223029), new Point(74.6952732585814, 41.2652838841385), new Point(74.4321870759279, 47.5326329779498), new Point(73.8983326488632, 52.2629850093569), new Point(74.8702943315126, 58.4914167046041), new Point(72.2950587469596, 68.8608823818438) };
            yield return null;

            var lines = TestMapHelpers.GenerateMap(75, 100, points, map);
            var grid = TestMapHelpers.CreateMap(mapStartPoint, map, tileHandle.Result, TileTerrainType.Water);

            Assert.IsNotNull(grid);
            yield return null;

            var organizationFactory = new OrganisationFactory();
            var provinceFactory = new ProvinceFactory(map, lines, UnityEngine.Object.Instantiate, provinceHandle.Result, organizationFactory);

            var result = provinceFactory.CreateProvinces(points);
            yield return null;

            Assert.IsNotNull(result);
            Assert.AreEqual(250, result.Count);

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

            fantasia.AddProvince(result.Single(p => p.Name == "Region 192"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 194"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 186"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 218"));

            // Fantasia
            // Add Province: Region 192
            // Add Province: Region 194
            // Add Province: Region 186
            // Add Province: Region 218
            yield return null;

            var janlandObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var janland = organizationFactory.CreateCountry(janlandObject, "Janland", CountryType.Major, Color.blue);
            yield return null;

            janland.AddProvince(result.Single(p => p.Name == "Region 116"));
            janland.AddProvince(result.Single(p => p.Name == "Region 104"));
            janland.AddProvince(result.Single(p => p.Name == "Region 118"));
            janland.AddProvince(result.Single(p => p.Name == "Region 108"));
            janland.AddProvince(result.Single(p => p.Name == "Region 128"));
            janland.AddProvince(result.Single(p => p.Name == "Region 130"));
            janland.AddProvince(result.Single(p => p.Name == "Region 145"));
            janland.AddProvince(result.Single(p => p.Name == "Region 147"));

            // Janland
            // Add Province: Region 116
            // Add Province: Region 104
            // Add Province: Region 118
            // Add Province: Region 108
            // Add Province: Region 128
            // Add Province: Region 130
            // Add Province: Region 145
            // Add Province: Region 147

            // LogMap(map);
            foreach (var province in fantasia.Provinces)
            {
                Debug.Log($"Neighbours of {province}: {string.Join(", ", province.GetNeighbours(map))}");
            }
            yield return null;

            var continentHandle = Addressables.LoadAssetAsync<GameObject>("Continent");
            yield return continentHandle;

            Assert.IsTrue(continentHandle.IsDone);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, provinceHandle.Status);
            Assert.IsNotNull(continentHandle.Result);
            Assert.IsNotNull(continentHandle.Result.GetComponent<Continent>());

            var continentObject = UnityEngine.Object.Instantiate(continentHandle.Result);
            Assert.IsNotNull(continentObject.GetComponent<Continent>());
            yield return null;

            var mapOrganizationGenerator = new MapOrganizationGenerator(mapStartPoint, organizationFactory);
            mapOrganizationGenerator.GenerateContinentsList(UnityEngine.Object.Instantiate, continentHandle.Result, result, map, mapStartPoint);
            yield return null;

            var continents = mapStartPoint.transform.GetComponentsInChildren<Continent>();
            Assert.IsNotNull(continents);
            Assert.AreEqual(2, continents.Length);
            yield return null;

            var continent1 = janland.Provinces.Select(p => p.Owner.Continent).First();
            var continent2 = fantasia.Provinces.Select(p => p.Owner.Continent).First();

            CollectionAssert.AreEquivalent(janland.Provinces.SelectMany(p => p.HexTiles), continent1.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)));
            CollectionAssert.AreEquivalent(fantasia.Provinces.SelectMany(p => p.HexTiles), continent2.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)));

            var countryLess = map.Where(t => t.TileTerrainType != TileTerrainType.Water).Select(t => t.Province).Where(p => p.Owner == null).ToList();

            Debug.Log(string.Join(", ", countryLess.Select(p => p.Name)));
            CollectionAssert.IsEmpty(countryLess);

            var continentLess = countryLess.Where(p => p.Owner.Continent == null).Select(p => p.Owner).ToList();
            Debug.Log(string.Join(", ", continentLess.Select(c => c.Name)));
            CollectionAssert.IsEmpty(continentLess);
            yield return null;

            var heightMapGenerator = new HeightMapGenerator(0.05);
            yield return null;
            var terrainGenerator = new TerrainGenerator(heightMapGenerator);
            yield return null;

            terrainGenerator.DesertBelt = 10;
            terrainGenerator.PoleBelt = 5;

            TestMapHelpers.LogMap(map);

            terrainGenerator.GenerateTerrain(map);
            yield return null;
        }
    }
}