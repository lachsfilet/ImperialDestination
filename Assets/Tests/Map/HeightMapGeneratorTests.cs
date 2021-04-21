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

            var points = new List<Point> { new Point(2.6633606300053, 3.55330520707802), new Point(2.69534677206322, 9.33034982407947), new Point(2.03974293919268, 17.8234329511521), new Point(2.91181309423959, 24.9718190026338), new Point(2.70386597686627, 32.0751650957741), new Point(2.20339637864539, 37.1732124510096), new Point(2.12762569362653, 46.6342709295611), new Point(2.13495687681015, 51.9609112888393), new Point(2.95671706551533, 60.7073158420191), new Point(2.05912399108481, 66.01525996533), new Point(7.63103244576186, 3.58555222423074), new Point(7.41547998758754, 11.3765056768323), new Point(7.38242338475884, 18.7003961478827), new Point(7.63254812528964, 24.2696265225623), new Point(7.7748910592752, 31.2907937687313), new Point(7.16970971188029, 39.8388112228544), new Point(7.83970738008605, 44.1552266777285), new Point(7.44020923526967, 53.199836453516), new Point(7.47195288607476, 59.52647667263), new Point(7.90693582450363, 67.5638184252027), new Point(12.4269722045525, 2.6720062488094), new Point(12.0259932731399, 11.123768331075), new Point(12.3146777736557, 18.8858945662556), new Point(12.2764404259047, 24.3524026904965), new Point(12.7421987330272, 31.836060363723), new Point(12.7290271025752, 38.5426271234372), new Point(12.5323412621079, 46.154814456196), new Point(12.408867134903, 51.4045296657852), new Point(12.1022884147718, 60.7095346393574), new Point(12.7276932232677, 65.4188326696953), new Point(17.9752628584277, 3.46581276015649), new Point(17.8823819416027, 9.8208041618675), new Point(17.5220119867111, 16.7713779130817), new Point(17.2180301035838, 23.2276504031511), new Point(17.437881074584, 31.2050341769145), new Point(17.8483508768717, 38.3436745239159), new Point(17.9955027895027, 44.6135535480518), new Point(17.9114477838909, 52.2210910884762), new Point(17.502524134937, 60.1226716316876), new Point(17.4774907852884, 66.261375588021), new Point(22.6236916233896, 2.54524168537242), new Point(22.6810101078269, 9.48861281410261), new Point(22.7012374870019, 17.9665020122037), new Point(22.4139291455103, 23.3643076840622), new Point(22.6428750910065, 30.9160980400239), new Point(22.6309904030669, 38.7063240188669), new Point(22.7811323342757, 46.3019748038156), new Point(22.8793432833065, 53.815103853501), new Point(22.3416201413337, 60.2571756575523), new Point(22.1859107903139, 65.1726554078854), new Point(27.7439347518347, 2.60167087456289), new Point(27.0129198222481, 10.0215620221671), new Point(27.7636692443693, 16.8434067838096), new Point(27.518706251643, 23.9834702382719), new Point(27.8068237015078, 30.0055103162329), new Point(27.6323190869914, 38.1841936126278), new Point(27.4256874278307, 44.921704081782), new Point(27.5159192665089, 53.3805468899107), new Point(27.8075941455586, 60.1585330107056), new Point(27.5723639002872, 67.5995339479295), new Point(32.843196825517, 2.28978262668931), new Point(32.4330083767106, 9.39754518372824), new Point(32.7706876559047, 16.8387286117481), new Point(32.6667591736963, 23.6845335646926), new Point(32.3251745660441, 32.9192394981716), new Point(32.0654102214917, 39.2503967472587), new Point(32.5216464202486, 45.2598169479798), new Point(32.6765237775056, 52.2694039956058), new Point(32.1688274062093, 59.0214683013137), new Point(32.3992016936649, 66.2603056771962), new Point(37.1367944190916, 2.25831719825897), new Point(37.3146121931796, 10.9231149567864), new Point(37.6730469263499, 16.7087775965728), new Point(37.1430872451249, 24.4216929210451), new Point(37.2514056680963, 30.2574829600088), new Point(37.1307738056084, 38.8663274132956), new Point(37.1800279855635, 44.9581984230122), new Point(37.2143549771115, 52.9420216288148), new Point(37.767548581477, 60.6412142830161), new Point(37.569833018151, 66.4522791213599), new Point(42.6143764209069, 2.87158519023638), new Point(42.0026488467132, 11.5012316063518), new Point(42.936426550586, 18.6465605137155), new Point(42.8730847839606, 23.0172453355124), new Point(42.6307109164217, 30.2783538039207), new Point(42.6246882237609, 38.4013141353621), new Point(42.3203093099037, 44.4567860236656), new Point(42.7297168968849, 53.2712732782919), new Point(42.7819820157168, 60.3759101733453), new Point(42.4594681348929, 66.6032853711412), new Point(47.1364889587911, 2.57858460004376), new Point(47.9767721868012, 9.71804073812349), new Point(47.8423060271155, 17.3299032511794), new Point(47.5701042635227, 23.6067408377336), new Point(47.2055840055484, 30.0875497037021), new Point(47.6555860837249, 37.8652679356119), new Point(47.7706564156202, 46.7453259596347), new Point(47.8057254714918, 53.5387980670383), new Point(47.0834568860398, 59.442604973187), new Point(47.7046117683428, 67.3725802653342), new Point(52.363174414897, 2.41201639986225), new Point(52.8431867239267, 9.4758632003683), new Point(52.4611394295754, 17.9910072814631), new Point(52.3017998278615, 23.083297933025), new Point(52.5896825774525, 31.3717916530426), new Point(52.8653585272214, 39.9267355710858), new Point(52.4209366261125, 44.1062136833119), new Point(52.3476040816622, 52.264544662677), new Point(52.3137562099443, 60.2899058020161), new Point(52.9914378416685, 66.4793764005785), new Point(57.3120825743825, 4.01251351414831), new Point(57.8001644396224, 10.804583045563), new Point(57.437198517582, 17.0087967356708), new Point(57.6964482961672, 24.2156019700764), new Point(57.3465365363967, 32.6503526236165), new Point(57.6736342067242, 37.9033410739635), new Point(57.0873582894389, 46.0058241393444), new Point(57.1712540421501, 53.9974504765111), new Point(57.3496748094213, 60.4544533600353), new Point(57.7782074831325, 65.5359107998833), new Point(62.141501255865, 3.62701485754317), new Point(62.6125643684587, 11.2449603636027), new Point(62.163824707346, 18.0400226800889), new Point(62.8674860190914, 25.2057555635487), new Point(62.5675045715494, 31.1526317517984), new Point(62.5325098124018, 37.9604648561033), new Point(62.4894304827272, 44.63445890445), new Point(62.4787775140623, 53.0770097305425), new Point(62.3371743603317, 58.724977126217), new Point(62.7214225492074, 65.3746215530553), new Point(67.9672207976539, 4.68979751350814), new Point(67.6812294412783, 11.1186951087409), new Point(67.7836296776233, 17.6927345077008), new Point(67.8882202342563, 25.2569857389932), new Point(67.8641505189539, 31.0453617684754), new Point(67.0718400781377, 38.4868152697975), new Point(67.9850944508729, 46.4384027251221), new Point(67.3516242109945, 53.3092328656042), new Point(67.0582734695907, 58.1555329552645), new Point(67.4377029745084, 67.2715728726571), new Point(72.4687795594655, 4.88358932495284), new Point(72.8376960851428, 11.8869778573918), new Point(72.350941062137, 17.0604377640693), new Point(72.8116832085939, 25.6276159638668), new Point(72.1898305323859, 31.4367523959078), new Point(72.6619757985054, 37.3240473020468), new Point(72.0967285708043, 45.9600008963421), new Point(72.2114161356405, 51.7347062540868), new Point(72.8611088445695, 60.7189979966353), new Point(72.9646904435776, 66.7987396143371), new Point(77.7917873257733, 2.34547231828117), new Point(77.8711013355623, 9.65715194617265), new Point(77.3123705057951, 16.110714167408), new Point(77.3350605011615, 25.0298887230595), new Point(77.8598620904888, 32.1270996765825), new Point(77.6340631072568, 37.8559901257306), new Point(77.8039560675639, 46.8926947749652), new Point(77.6560942915529, 51.0180730680088), new Point(77.0589030860266, 60.156710466443), new Point(77.3552906407766, 66.2904512688008), new Point(82.7525616273063, 2.03704473890227), new Point(82.5624980472785, 10.1556991609538), new Point(82.6864781243198, 18.8914729323664), new Point(82.3988765191282, 23.9952824562766), new Point(82.6013847727335, 30.3201663788968), new Point(82.9086354439653, 37.4558224121369), new Point(82.0871538748439, 46.514369309188), new Point(82.9654036317791, 52.0608663601153), new Point(82.0900951060886, 59.8559363092556), new Point(82.6499554466689, 65.9421090190029), new Point(87.0184187535282, 2.40516090272235), new Point(87.0160098974668, 10.4423919205751), new Point(87.8448543580458, 17.1299372697854), new Point(87.304059699785, 23.3974909374479), new Point(87.9972393405611, 30.6161753389128), new Point(87.185998998669, 37.4266160672654), new Point(87.5510420247684, 45.603620061932), new Point(87.8470182441394, 53.3383172384176), new Point(87.5526593921486, 59.4576048457332), new Point(87.5325725244044, 66.0456385850188), new Point(92.6380282033412, 2.00989904720797), new Point(92.0752448016197, 11.2594198925697), new Point(92.8003977815622, 17.4463669096336), new Point(92.1981761670663, 25.8974988935038), new Point(92.998827959876, 30.9074165145435), new Point(92.9159292499143, 38.3207729488242), new Point(92.0689480421454, 44.123762903327), new Point(92.4117316694054, 52.8525239791966), new Point(92.996338348834, 58.2451022212604), new Point(92.5403786956055, 65.929497103174), new Point(97.6597646110038, 2.7991386199366), new Point(97.3345214781978, 10.1879789792877), new Point(97.9207231276299, 18.2992902646304), new Point(97.6008987792772, 24.6578415625998), new Point(97.9911048589233, 30.5578936569197), new Point(97.8009627278899, 38.8126804725326), new Point(97.0860729120141, 44.9042277549879), new Point(97.6760081363264, 51.0453572133767), new Point(97.0598088326211, 58.7886097998305), new Point(97.6803995252961, 66.0881961645038) };
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
            Assert.AreEqual(200, result.Count);

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

            fantasia.AddProvince(result.Single(p => p.Name == "Region 149"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 153"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 150"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 148"));
            yield return null;

            foreach(var tile in fantasia.Provinces.SelectMany(p=>p.HexTiles))
            {
                tile.TileTerrainType = TileTerrainType.Plain;
            }
            yield return null;


            var janlandObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var janland = organizationFactory.CreateCountry(janlandObject, "Janland", CountryType.Major, Color.blue);
            yield return null;

            janland.AddProvince(result.Single(p => p.Name == "Region 64"));
            janland.AddProvince(result.Single(p => p.Name == "Region 59"));
            janland.AddProvince(result.Single(p => p.Name == "Region 61"));
            janland.AddProvince(result.Single(p => p.Name == "Region 67"));
            janland.AddProvince(result.Single(p => p.Name == "Region 53"));
            janland.AddProvince(result.Single(p => p.Name == "Region 51"));
            janland.AddProvince(result.Single(p => p.Name == "Region 44"));
            janland.AddProvince(result.Single(p => p.Name == "Region 39"));

            foreach (var tile in janland.Provinces.SelectMany(p => p.HexTiles))
            {
                tile.TileTerrainType = TileTerrainType.Plain;
            }
            yield return null;


            // LogMap(map);
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

            var continent1 = janland.Continent;
            var continent2 = fantasia.Continent;

            CollectionAssert.AreEquivalent(janland.Provinces.SelectMany(p => p.HexTiles), continent1.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)));
            CollectionAssert.AreEquivalent(fantasia.Provinces.SelectMany(p => p.HexTiles), continent2.Countries.SelectMany(c => c.Provinces.SelectMany(p => p.HexTiles)));

            var heightMapGenerator = new HeightMapGenerator(0.05);
            yield return null;
            var terrainGenerator = new TerrainGenerator(heightMapGenerator);
            yield return null;

            terrainGenerator.DesertBelt = 10;
            terrainGenerator.PoleBelt = 5;

            //TestMapHelpers.LogMap(map);

            terrainGenerator.GenerateTerrain(map);
            yield return null;
        }
    }
}