using Assets.Contracts.Map;
using Assets.Contracts.Organization;
using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Organization;
using Assets.Tests.Helpers;
using Helpers;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using VoronoiEngine;
using VoronoiEngine.Elements;

namespace Tests
{
    public class ProvinceFactoryTests
    {
        [Test]
        public void TestRandom()
        {
            var collection = new List<int>();
            var index = UnityEngine.Random.Range(0, collection.Count);
            Assert.AreEqual(0, index);
            var result = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => result = collection[index]);
        }

        [UnityTest]
        public IEnumerator CreateProvinces_WithLinePositionsAndHexMap_GeneratesProvinces()
        {
            var mapStartPoint = new GameObject();
            var map = HexMapBuilder.New.WithWidth(13).WithHeight(9).Build();

            var lines = Enumerable.Range(0, 13).Select(i => new Position(i, 0))
                .Union(Enumerable.Range(0, 13).Select(i => new Position(i, 4)))
                .Union(Enumerable.Range(0, 13).Select(i => new Position(i, 8)))
                .Union(Enumerable.Range(0, 9).Select(i => new Position(0, i)))
                .Union(Enumerable.Range(0, 9).Select(i => new Position(4, i)))
                .Union(Enumerable.Range(0, 9).Select(i => new Position(8, i)))
                .Union(Enumerable.Range(0, 9).Select(i => new Position(12, i)))
                .Distinct().ToList();

            Assert.AreEqual(63, lines.Count);

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

            TestMapHelpers.CreateMap(mapStartPoint, map, tileHandle.Result);
            yield return null;

            var organizationFactory = new OrganisationFactory();
            var sites = new List<Point> { new Point(2, 2), new Point(6, 2), new Point(10, 2), new Point(2, 6), new Point(6, 6), new Point(10, 6) };

            var provinceFactory = new ProvinceFactory(map, lines, UnityEngine.Object.Instantiate, provinceHandle.Result, organizationFactory);
            var result = provinceFactory.CreateProvinces(sites);
            yield return null;

            Assert.IsNotNull(result);
            Assert.AreEqual(6, result.Count);

            var provinceless = map.Where(t => t.Province == null).ToList();

            Debug.Log(provinceless.Count);
            foreach (var tile in provinceless)
                Debug.Log(tile);

            Assert.False(provinceless.Any());

            foreach (var province in result)
            {
                var neighbours = province.GetNeighbours(map);
                Debug.Log($"Neighbours of {province}: {string.Join(", ", neighbours.Select(n => n))}");
            }

            TestMapHelpers.LogMap(map);

            var provinces = result.ToList();
            Assert.AreEqual(new[] { "Region 1", "Region 2" }, provinces[0].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());
            Assert.AreEqual(new[] { "Region 0", "Region 2", "Region 3", "Region 4" }, provinces[1].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());
            Assert.AreEqual(new[] { "Region 0", "Region 1", "Region 4" }, provinces[2].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());
            Assert.AreEqual(new[] { "Region 1", "Region 4", "Region 5" }, provinces[3].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());
            Assert.AreEqual(new[] { "Region 1", "Region 2", "Region 3", "Region 5" }, provinces[4].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());
            Assert.AreEqual(new[] { "Region 3", "Region 4" }, provinces[5].GetNeighbours(map).Select(p => p.Name).OrderBy(n => n).ToArray());

            //  0 0 0 0 0 1 1 1 1 3 3 3 3
            //   0 0 0 0 0 1 1 1 1 3 3 3 3
            //  0 0 0 0 0 1 1 1 1 3 3 3 3
            //   0 0 0 0 0 1 1 1 1 3 3 3 3
            //  0 0 0 0 0 1 1 1 1 3 3 3 3
            //   2 2 2 2 2 4 4 4 4 5 5 5 5
            //  2 2 2 2 2 4 4 4 4 5 5 5 5
            //   2 2 2 2 2 4 4 4 4 5 5 5 5
            //  2 2 2 2 2 4 4 4 4 5 5 5 5
        }

        [UnityTest]
        public IEnumerator CreateProvinces_With20SitesAnd20x50HexMap_GeneratesProvinces()
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

            //var lines = new List<Position> { new Position(17, 19), new Position(17, 20), new Position(17, 21), new Position(17, 22), new Position(17, 23), new Position(17, 24), new Position(16, 25), new Position(17, 26), new Position(16, 27), new Position(17, 28), new Position(16, 29), new Position(16, 30), new Position(16, 31), new Position(16, 32), new Position(16, 33), new Position(16, 34), new Position(15, 35), new Position(16, 36), new Position(15, 37), new Position(16, 38), new Position(15, 39), new Position(15, 40), new Position(15, 41), new Position(15, 42), new Position(15, 43), new Position(15, 44), new Position(14, 45), new Position(15, 46), new Position(14, 47), new Position(15, 48), new Position(14, 49), new Position(14, 50), new Position(14, 51), new Position(14, 52), new Position(14, 53), new Position(14, 54), new Position(13, 55), new Position(14, 56), new Position(13, 57), new Position(14, 58), new Position(13, 59), new Position(13, 60), new Position(13, 61), new Position(13, 62), new Position(13, 63), new Position(13, 64), new Position(12, 65), new Position(13, 66), new Position(12, 67), new Position(13, 68), new Position(12, 69), new Position(12, 70), new Position(12, 71), new Position(12, 72), new Position(12, 73), new Position(12, 74), new Position(11, 75), new Position(12, 76), new Position(11, 77), new Position(12, 78), new Position(11, 79), new Position(11, 80), new Position(11, 81), new Position(11, 82), new Position(10, 83), new Position(11, 84), new Position(10, 85), new Position(11, 86), new Position(10, 87), new Position(10, 88), new Position(10, 89), new Position(10, 90), new Position(10, 91), new Position(10, 92), new Position(9, 93), new Position(10, 94), new Position(9, 95), new Position(10, 96), new Position(9, 97), new Position(9, 98), new Position(9, 99), new Position(9, 100), new Position(9, 101), new Position(9, 102), new Position(8, 103), new Position(9, 104), new Position(8, 105), new Position(9, 106), new Position(8, 107), new Position(8, 108), new Position(8, 109), new Position(8, 110), new Position(8, 111), new Position(8, 112), new Position(7, 113), new Position(8, 114), new Position(7, 115), new Position(8, 116), new Position(7, 117), new Position(7, 118), new Position(7, 119), new Position(7, 120), new Position(7, 121), new Position(7, 122), new Position(6, 123), new Position(7, 124), new Position(6, 125), new Position(7, 126), new Position(6, 127), new Position(6, 128), new Position(6, 129), new Position(6, 130), new Position(6, 131), new Position(6, 132), new Position(5, 133), new Position(6, 134), new Position(5, 135), new Position(6, 136), new Position(5, 137), new Position(5, 138), new Position(5, 139), new Position(5, 140), new Position(5, 141), new Position(5, 142), new Position(8, 10), new Position(7, 11), new Position(8, 12), new Position(7, 13), new Position(8, 14), new Position(7, 15), new Position(8, 16), new Position(7, 17), new Position(8, 18), new Position(7, 19), new Position(8, 20), new Position(7, 21), new Position(8, 22), new Position(7, 23), new Position(8, 24), new Position(7, 25), new Position(8, 26), new Position(7, 27), new Position(8, 28), new Position(7, 29), new Position(8, 30), new Position(7, 31), new Position(8, 32), new Position(7, 33), new Position(7, 34), new Position(7, 35), new Position(7, 36), new Position(7, 37), new Position(7, 38), new Position(7, 39), new Position(7, 40), new Position(7, 41), new Position(7, 42), new Position(7, 43), new Position(7, 44), new Position(7, 45), new Position(7, 46), new Position(7, 47), new Position(7, 48), new Position(7, 49), new Position(7, 50), new Position(7, 51), new Position(7, 52), new Position(7, 53), new Position(7, 54), new Position(6, 55), new Position(7, 56), new Position(6, 57), new Position(7, 58), new Position(6, 59), new Position(7, 60), new Position(6, 61), new Position(7, 62), new Position(6, 63), new Position(7, 64), new Position(6, 65), new Position(7, 66), new Position(6, 67), new Position(7, 68), new Position(6, 69), new Position(7, 70), new Position(6, 71), new Position(7, 72), new Position(6, 73), new Position(7, 74), new Position(6, 75), new Position(6, 76), new Position(6, 77), new Position(6, 78), new Position(6, 79), new Position(6, 80), new Position(6, 81), new Position(6, 82), new Position(6, 83), new Position(6, 84), new Position(6, 85), new Position(6, 86), new Position(6, 87), new Position(6, 88), new Position(6, 89), new Position(6, 90), new Position(6, 91), new Position(6, 92), new Position(6, 93), new Position(6, 94), new Position(6, 95), new Position(6, 96), new Position(6, 97), new Position(6, 98), new Position(5, 99), new Position(6, 100), new Position(5, 101), new Position(6, 102), new Position(5, 103), new Position(6, 104), new Position(5, 105), new Position(6, 106), new Position(5, 107), new Position(6, 108), new Position(5, 109), new Position(6, 110), new Position(5, 111), new Position(6, 112), new Position(5, 113), new Position(6, 114), new Position(5, 115), new Position(6, 116), new Position(5, 117), new Position(6, 118), new Position(5, 119), new Position(6, 120), new Position(5, 121), new Position(5, 122), new Position(5, 123), new Position(5, 124), new Position(5, 125), new Position(5, 126), new Position(5, 127), new Position(5, 128), new Position(5, 129), new Position(5, 130), new Position(5, 131), new Position(5, 132), new Position(5, 133), new Position(5, 134), new Position(5, 135), new Position(5, 136), new Position(5, 137), new Position(5, 138), new Position(5, 139), new Position(5, 140), new Position(5, 141), new Position(5, 142), new Position(5, 142), new Position(4, 143), new Position(5, 144), new Position(4, 145), new Position(5, 146), new Position(4, 147), new Position(4, 148), new Position(4, 149), new Position(4, 150), new Position(4, 151), new Position(4, 152), new Position(4, 153), new Position(4, 154), new Position(3, 155), new Position(4, 156), new Position(3, 157), new Position(4, 158), new Position(3, 159), new Position(3, 160), new Position(3, 161), new Position(3, 162), new Position(3, 163), new Position(3, 164), new Position(2, 165), new Position(3, 166), new Position(2, 167), new Position(3, 168), new Position(2, 169), new Position(3, 170), new Position(2, 171), new Position(2, 172), new Position(2, 173), new Position(2, 174), new Position(2, 175), new Position(2, 176), new Position(1, 177), new Position(2, 178), new Position(1, 179), new Position(2, 180), new Position(1, 181), new Position(2, 182), new Position(1, 183), new Position(1, 184), new Position(1, 185), new Position(1, 186), new Position(1, 187), new Position(1, 188), new Position(0, 189), new Position(1, 190), new Position(0, 191), new Position(1, 192), new Position(0, 193), new Position(0, 194), new Position(0, 195), new Position(0, 196), new Position(0, 197), new Position(0, 198), new Position(0, 199), new Position(0, 200), new Position(-1, 201), new Position(0, 202), new Position(-1, 203), new Position(0, 204), new Position(-1, 205), new Position(-1, 206), new Position(-1, 207), new Position(-1, 208), new Position(-1, 209), new Position(-1, 210), new Position(-2, 211), new Position(-1, 212), new Position(-2, 213), new Position(-1, 214), new Position(-2, 215), new Position(-1, 216), new Position(-2, 217), new Position(-2, 218), new Position(-2, 219), new Position(-2, 220), new Position(-2, 221), new Position(-2, 222), new Position(-3, 223), new Position(-2, 224), new Position(-3, 225), new Position(-2, 226), new Position(-3, 227), new Position(-2, 228), new Position(-3, 229), new Position(-3, 230), new Position(-3, 231), new Position(-3, 232), new Position(-3, 233), new Position(-3, 234), new Position(-4, 235), new Position(-3, 236), new Position(-4, 237), new Position(-3, 238), new Position(-4, 239), new Position(-4, 240), new Position(-4, 241), new Position(-4, 242), new Position(-4, 243), new Position(-4, 244), new Position(-4, 245), new Position(-4, 246), new Position(-5, 247), new Position(-4, 248), new Position(-5, 249), new Position(-4, 250), new Position(-5, 251), new Position(51, 119), new Position(51, 118), new Position(51, 117), new Position(51, 116), new Position(50, 115), new Position(50, 114), new Position(50, 113), new Position(50, 112), new Position(49, 111), new Position(50, 110), new Position(49, 109), new Position(49, 108), new Position(49, 107), new Position(49, 106), new Position(48, 105), new Position(48, 104), new Position(48, 103), new Position(48, 102), new Position(47, 101), new Position(48, 100), new Position(47, 99), new Position(47, 98), new Position(47, 97), new Position(47, 96), new Position(46, 95), new Position(46, 94), new Position(46, 93), new Position(46, 92), new Position(45, 91), new Position(46, 90), new Position(45, 89), new Position(45, 88), new Position(45, 87), new Position(45, 86), new Position(44, 85), new Position(44, 84), new Position(44, 83), new Position(44, 82), new Position(43, 81), new Position(44, 80), new Position(43, 79), new Position(43, 78), new Position(43, 77), new Position(43, 76), new Position(42, 75), new Position(42, 74), new Position(42, 73), new Position(42, 72), new Position(41, 71), new Position(42, 70), new Position(41, 69), new Position(41, 68), new Position(41, 67), new Position(41, 66), new Position(40, 65), new Position(40, 64), new Position(40, 63), new Position(40, 62), new Position(39, 61), new Position(40, 60), new Position(39, 59), new Position(39, 58), new Position(39, 57), new Position(39, 56), new Position(38, 55), new Position(38, 54), new Position(38, 53), new Position(38, 52), new Position(37, 51), new Position(38, 50), new Position(37, 49), new Position(37, 48), new Position(37, 47), new Position(37, 46), new Position(36, 45), new Position(36, 44), new Position(36, 43), new Position(36, 42), new Position(35, 41), new Position(36, 40), new Position(35, 39), new Position(35, 38), new Position(35, 37), new Position(35, 36), new Position(34, 35), new Position(34, 34), new Position(34, 33), new Position(34, 32), new Position(33, 31), new Position(34, 30), new Position(33, 29), new Position(33, 28), new Position(33, 27), new Position(33, 26), new Position(32, 25), new Position(32, 24), new Position(32, 23), new Position(32, 22), new Position(31, 21), new Position(32, 20), new Position(31, 19), new Position(31, 18), new Position(31, 17), new Position(31, 16), new Position(30, 15), new Position(30, 14), new Position(30, 13), new Position(30, 12), new Position(51, 119), new Position(51, 118), new Position(51, 117), new Position(51, 116), new Position(50, 115), new Position(51, 114), new Position(50, 113), new Position(50, 112), new Position(50, 111), new Position(50, 110), new Position(50, 109), new Position(50, 108), new Position(49, 107), new Position(50, 106), new Position(49, 105), new Position(49, 104), new Position(49, 103), new Position(49, 102), new Position(48, 101), new Position(49, 100), new Position(48, 99), new Position(48, 98), new Position(48, 97), new Position(48, 96), new Position(48, 95), new Position(48, 94), new Position(47, 93), new Position(48, 92), new Position(47, 91), new Position(47, 90), new Position(47, 89), new Position(47, 88), new Position(46, 87), new Position(47, 86), new Position(46, 85), new Position(46, 84), new Position(46, 83), new Position(46, 82), new Position(45, 81), new Position(46, 80), new Position(45, 79), new Position(46, 78), new Position(45, 77), new Position(45, 76), new Position(45, 75), new Position(45, 74), new Position(44, 73), new Position(45, 72), new Position(44, 71), new Position(44, 70), new Position(44, 69), new Position(44, 68), new Position(43, 67), new Position(44, 66), new Position(43, 65), new Position(44, 64), new Position(43, 63), new Position(43, 62), new Position(43, 61), new Position(43, 60), new Position(42, 59), new Position(43, 58), new Position(42, 57), new Position(42, 56), new Position(42, 55), new Position(42, 54), new Position(41, 53), new Position(42, 52), new Position(41, 51), new Position(42, 50), new Position(41, 49), new Position(41, 48), new Position(41, 47), new Position(41, 46), new Position(40, 45), new Position(41, 44), new Position(40, 43), new Position(40, 42), new Position(40, 41), new Position(40, 40), new Position(39, 39), new Position(40, 38), new Position(39, 37), new Position(39, 36), new Position(39, 35), new Position(39, 34), new Position(39, 33), new Position(39, 32), new Position(38, 31), new Position(39, 30), new Position(38, 29), new Position(38, 28), new Position(38, 27), new Position(38, 26), new Position(37, 25), new Position(38, 24), new Position(37, 23), new Position(37, 22), new Position(37, 21), new Position(37, 20), new Position(37, 19), new Position(37, 18), new Position(36, 17), new Position(37, 16), new Position(36, 15), new Position(36, 14), new Position(36, 13), new Position(36, 12), new Position(51, 119), new Position(52, 120), new Position(51, 121), new Position(52, 122), new Position(52, 123), new Position(52, 124), new Position(52, 125), new Position(53, 126), new Position(52, 127), new Position(53, 128), new Position(53, 129), new Position(53, 130), new Position(53, 131), new Position(54, 132), new Position(54, 133), new Position(54, 134), new Position(54, 135), new Position(55, 136), new Position(54, 137), new Position(55, 138), new Position(55, 139), new Position(55, 140), new Position(55, 141), new Position(56, 142), new Position(55, 143), new Position(56, 144), new Position(56, 145), new Position(56, 146), new Position(56, 147), new Position(57, 148), new Position(56, 149), new Position(57, 150), new Position(57, 151), new Position(57, 152), new Position(57, 153), new Position(58, 154), new Position(57, 155), new Position(58, 156), new Position(58, 157), new Position(58, 158), new Position(58, 159), new Position(59, 160), new Position(58, 161), new Position(59, 162), new Position(59, 163), new Position(60, 164), new Position(59, 165), new Position(60, 166), new Position(60, 167), new Position(60, 168), new Position(60, 169), new Position(61, 170), new Position(60, 171), new Position(61, 172), new Position(61, 173), new Position(61, 174), new Position(61, 175), new Position(36, 12), new Position(35, 12), new Position(34, 12), new Position(33, 11), new Position(32, 11), new Position(31, 11), new Position(36, 12), new Position(36, 11), new Position(37, 11), new Position(38, 10), new Position(5, 11), new Position(5, 12), new Position(4, 13), new Position(3, 13), new Position(3, 14), new Position(2, 15), new Position(2, 16), new Position(1, 17), new Position(1, 18), new Position(0, 19), new Position(-1, 19), new Position(-1, 20), new Position(-2, 21), new Position(-2, 22), new Position(-3, 23), new Position(-4, 23), new Position(-4, 24), new Position(-5, 25), new Position(-5, 26), new Position(-6, 27), new Position(-6, 28), new Position(-7, 29), new Position(-8, 29), new Position(-8, 30), new Position(-9, 31), new Position(20, 15), new Position(20, 16), new Position(19, 17), new Position(18, 17), new Position(18, 18), new Position(17, 19), new Position(17, 19), new Position(17, 18), new Position(16, 17), new Position(16, 16), new Position(15, 15), new Position(15, 14), new Position(14, 13), new Position(14, 12), new Position(13, 11), new Position(26, 11), new Position(26, 12), new Position(25, 12), new Position(24, 12), new Position(23, 13), new Position(23, 14), new Position(22, 14), new Position(21, 14), new Position(20, 15), new Position(20, 15), new Position(21, 14), new Position(20, 13), new Position(20, 12), new Position(20, 11), new Position(21, 10), new Position(20, 9), new Position(20, 8), new Position(20, 7), new Position(53, 11), new Position(52, 11), new Position(51, 11), new Position(50, 11), new Position(49, 11), new Position(48, 11), new Position(47, 11), new Position(46, 11), new Position(45, 11), new Position(45, 10), new Position(44, 10), new Position(43, 10), new Position(42, 10), new Position(41, 10), new Position(40, 10), new Position(39, 10), new Position(38, 10), new Position(38, 10), new Position(37, 9), new Position(36, 9), new Position(36, 8), new Position(35, 7), new Position(30, 12), new Position(29, 12), new Position(28, 12), new Position(27, 11), new Position(26, 11), new Position(26, 11), new Position(26, 10), new Position(25, 12) };
            //Position X: 17, Y: 19, Position X: 17, Y: 20, Position X: 17, Y: 21, Position X: 17, Y: 22, Position X: 17, Y: 23, Position X: 17, Y: 24, Position X: 16, Y: 25, Position X: 17, Y: 26, Position X: 16, Y: 27, Position X: 17, Y: 28, Position X: 16, Y: 29, Position X: 16, Y: 30, Position X: 16, Y: 31, Position X: 16, Y: 32, Position X: 16, Y: 33, Position X: 16, Y: 34, Position X: 15, Y: 35, Position X: 16, Y: 36, Position X: 15, Y: 37, Position X: 16, Y: 38, Position X: 15, Y: 39, Position X: 15, Y: 40, Position X: 15, Y: 41, Position X: 15, Y: 42, Position X: 15, Y: 43, Position X: 15, Y: 44, Position X: 14, Y: 45, Position X: 15, Y: 46, Position X: 14, Y: 47, Position X: 15, Y: 48, Position X: 14, Y: 49, Position X: 14, Y: 50, Position X: 14, Y: 51, Position X: 14, Y: 52, Position X: 14, Y: 53, Position X: 14, Y: 54, Position X: 13, Y: 55, Position X: 14, Y: 56, Position X: 13, Y: 57, Position X: 14, Y: 58, Position X: 13, Y: 59, Position X: 13, Y: 60, Position X: 13, Y: 61, Position X: 13, Y: 62, Position X: 13, Y: 63, Position X: 13, Y: 64, Position X: 12, Y: 65, Position X: 13, Y: 66, Position X: 12, Y: 67, Position X: 13, Y: 68, Position X: 12, Y: 69, Position X: 12, Y: 70, Position X: 12, Y: 71, Position X: 12, Y: 72, Position X: 12, Y: 73, Position X: 12, Y: 74, Position X: 11, Y: 75, Position X: 12, Y: 76, Position X: 11, Y: 77, Position X: 12, Y: 78, Position X: 11, Y: 79, Position X: 11, Y: 80, Position X: 11, Y: 81, Position X: 11, Y: 82, Position X: 10, Y: 83, Position X: 11, Y: 84, Position X: 10, Y: 85, Position X: 11, Y: 86, Position X: 10, Y: 87, Position X: 10, Y: 88, Position X: 10, Y: 89, Position X: 10, Y: 90, Position X: 10, Y: 91, Position X: 10, Y: 92, Position X: 9, Y: 93, Position X: 10, Y: 94, Position X: 9, Y: 95, Position X: 10, Y: 96, Position X: 9, Y: 97, Position X: 9, Y: 98, Position X: 9, Y: 99, Position X: 9, Y: 100, Position X: 9, Y: 101, Position X: 9, Y: 102, Position X: 8, Y: 103, Position X: 9, Y: 104, Position X: 8, Y: 105, Position X: 9, Y: 106, Position X: 8, Y: 107, Position X: 8, Y: 108, Position X: 8, Y: 109, Position X: 8, Y: 110, Position X: 8, Y: 111, Position X: 8, Y: 112, Position X: 7, Y: 113, Position X: 8, Y: 114, Position X: 7, Y: 115, Position X: 8, Y: 116, Position X: 7, Y: 117, Position X: 7, Y: 118, Position X: 7, Y: 119, Position X: 7, Y: 120, Position X: 7, Y: 121, Position X: 7, Y: 122, Position X: 6, Y: 123, Position X: 7, Y: 124, Position X: 6, Y: 125, Position X: 7, Y: 126, Position X: 6, Y: 127, Position X: 6, Y: 128, Position X: 6, Y: 129, Position X: 6, Y: 130, Position X: 6, Y: 131, Position X: 6, Y: 132, Position X: 5, Y: 133, Position X: 6, Y: 134, Position X: 5, Y: 135, Position X: 6, Y: 136, Position X: 5, Y: 137, Position X: 5, Y: 138, Position X: 5, Y: 139, Position X: 5, Y: 140, Position X: 5, Y: 141, Position X: 5, Y: 142, Position X: 8, Y: 10, Position X: 7, Y: 11, Position X: 8, Y: 12, Position X: 7, Y: 13, Position X: 8, Y: 14, Position X: 7, Y: 15, Position X: 8, Y: 16, Position X: 7, Y: 17, Position X: 8, Y: 18, Position X: 7, Y: 19, Position X: 8, Y: 20, Position X: 7, Y: 21, Position X: 8, Y: 22, Position X: 7, Y: 23, Position X: 8, Y: 24, Position X: 7, Y: 25, Position X: 8, Y: 26, Position X: 7, Y: 27, Position X: 8, Y: 28, Position X: 7, Y: 29, Position X: 8, Y: 30, Position X: 7, Y: 31, Position X: 8, Y: 32, Position X: 7, Y: 33, Position X: 7, Y: 34, Position X: 7, Y: 35, Position X: 7, Y: 36, Position X: 7, Y: 37, Position X: 7, Y: 38, Position X: 7, Y: 39, Position X: 7, Y: 40, Position X: 7, Y: 41, Position X: 7, Y: 42, Position X: 7, Y: 43, Position X: 7, Y: 44, Position X: 7, Y: 45, Position X: 7, Y: 46, Position X: 7, Y: 47, Position X: 7, Y: 48, Position X: 7, Y: 49, Position X: 7, Y: 50, Position X: 7, Y: 51, Position X: 7, Y: 52, Position X: 7, Y: 53, Position X: 7, Y: 54, Position X: 6, Y: 55, Position X: 7, Y: 56, Position X: 6, Y: 57, Position X: 7, Y: 58, Position X: 6, Y: 59, Position X: 7, Y: 60, Position X: 6, Y: 61, Position X: 7, Y: 62, Position X: 6, Y: 63, Position X: 7, Y: 64, Position X: 6, Y: 65, Position X: 7, Y: 66, Position X: 6, Y: 67, Position X: 7, Y: 68, Position X: 6, Y: 69, Position X: 7, Y: 70, Position X: 6, Y: 71, Position X: 7, Y: 72, Position X: 6, Y: 73, Position X: 7, Y: 74, Position X: 6, Y: 75, Position X: 6, Y: 76, Position X: 6, Y: 77, Position X: 6, Y: 78, Position X: 6, Y: 79, Position X: 6, Y: 80, Position X: 6, Y: 81, Position X: 6, Y: 82, Position X: 6, Y: 83, Position X: 6, Y: 84, Position X: 6, Y: 85, Position X: 6, Y: 86, Position X: 6, Y: 87, Position X: 6, Y: 88, Position X: 6, Y: 89, Position X: 6, Y: 90, Position X: 6, Y: 91, Position X: 6, Y: 92, Position X: 6, Y: 93, Position X: 6, Y: 94, Position X: 6, Y: 95, Position X: 6, Y: 96, Position X: 6, Y: 97, Position X: 6, Y: 98, Position X: 5, Y: 99, Position X: 6, Y: 100, Position X: 5, Y: 101, Position X: 6, Y: 102, Position X: 5, Y: 103, Position X: 6, Y: 104, Position X: 5, Y: 105, Position X: 6, Y: 106, Position X: 5, Y: 107, Position X: 6, Y: 108, Position X: 5, Y: 109, Position X: 6, Y: 110, Position X: 5, Y: 111, Position X: 6, Y: 112, Position X: 5, Y: 113, Position X: 6, Y: 114, Position X: 5, Y: 115, Position X: 6, Y: 116, Position X: 5, Y: 117, Position X: 6, Y: 118, Position X: 5, Y: 119, Position X: 6, Y: 120, Position X: 5, Y: 121, Position X: 5, Y: 122, Position X: 5, Y: 123, Position X: 5, Y: 124, Position X: 5, Y: 125, Position X: 5, Y: 126, Position X: 5, Y: 127, Position X: 5, Y: 128, Position X: 5, Y: 129, Position X: 5, Y: 130, Position X: 5, Y: 131, Position X: 5, Y: 132, Position X: 5, Y: 133, Position X: 5, Y: 134, Position X: 5, Y: 135, Position X: 5, Y: 136, Position X: 5, Y: 137, Position X: 5, Y: 138, Position X: 5, Y: 139, Position X: 5, Y: 140, Position X: 5, Y: 141, Position X: 5, Y: 142, Position X: 5, Y: 142, Position X: 4, Y: 143, Position X: 5, Y: 144, Position X: 4, Y: 145, Position X: 5, Y: 146, Position X: 4, Y: 147, Position X: 4, Y: 148, Position X: 4, Y: 149, Position X: 4, Y: 150, Position X: 4, Y: 151, Position X: 4, Y: 152, Position X: 4, Y: 153, Position X: 4, Y: 154, Position X: 3, Y: 155, Position X: 4, Y: 156, Position X: 3, Y: 157, Position X: 4, Y: 158, Position X: 3, Y: 159, Position X: 3, Y: 160, Position X: 3, Y: 161, Position X: 3, Y: 162, Position X: 3, Y: 163, Position X: 3, Y: 164, Position X: 2, Y: 165, Position X: 3, Y: 166, Position X: 2, Y: 167, Position X: 3, Y: 168, Position X: 2, Y: 169, Position X: 3, Y: 170, Position X: 2, Y: 171, Position X: 2, Y: 172, Position X: 2, Y: 173, Position X: 2, Y: 174, Position X: 2, Y: 175, Position X: 2, Y: 176, Position X: 1, Y: 177, Position X: 2, Y: 178, Position X: 1, Y: 179, Position X: 2, Y: 180, Position X: 1, Y: 181, Position X: 2, Y: 182, Position X: 1, Y: 183, Position X: 1, Y: 184, Position X: 1, Y: 185, Position X: 1, Y: 186, Position X: 1, Y: 187, Position X: 1, Y: 188, Position X: 0, Y: 189, Position X: 1, Y: 190, Position X: 0, Y: 191, Position X: 1, Y: 192, Position X: 0, Y: 193, Position X: 0, Y: 194, Position X: 0, Y: 195, Position X: 0, Y: 196, Position X: 0, Y: 197, Position X: 0, Y: 198, Position X: 0, Y: 199, Position X: 0, Y: 200, Position X: -1, Y: 201, Position X: 0, Y: 202, Position X: -1, Y: 203, Position X: 0, Y: 204, Position X: -1, Y: 205, Position X: -1, Y: 206, Position X: -1, Y: 207, Position X: -1, Y: 208, Position X: -1, Y: 209, Position X: -1, Y: 210, Position X: -2, Y: 211, Position X: -1, Y: 212, Position X: -2, Y: 213, Position X: -1, Y: 214, Position X: -2, Y: 215, Position X: -1, Y: 216, Position X: -2, Y: 217, Position X: -2, Y: 218, Position X: -2, Y: 219, Position X: -2, Y: 220, Position X: -2, Y: 221, Position X: -2, Y: 222, Position X: -3, Y: 223, Position X: -2, Y: 224, Position X: -3, Y: 225, Position X: -2, Y: 226, Position X: -3, Y: 227, Position X: -2, Y: 228, Position X: -3, Y: 229, Position X: -3, Y: 230, Position X: -3, Y: 231, Position X: -3, Y: 232, Position X: -3, Y: 233, Position X: -3, Y: 234, Position X: -4, Y: 235, Position X: -3, Y: 236, Position X: -4, Y: 237, Position X: -3, Y: 238, Position X: -4, Y: 239, Position X: -4, Y: 240, Position X: -4, Y: 241, Position X: -4, Y: 242, Position X: -4, Y: 243, Position X: -4, Y: 244, Position X: -4, Y: 245, Position X: -4, Y: 246, Position X: -5, Y: 247, Position X: -4, Y: 248, Position X: -5, Y: 249, Position X: -4, Y: 250, Position X: -5, Y: 251, Position X: 51, Y: 119, Position X: 51, Y: 118, Position X: 51, Y: 117, Position X: 51, Y: 116, Position X: 50, Y: 115, Position X: 50, Y: 114, Position X: 50, Y: 113, Position X: 50, Y: 112, Position X: 49, Y: 111, Position X: 50, Y: 110, Position X: 49, Y: 109, Position X: 49, Y: 108, Position X: 49, Y: 107, Position X: 49, Y: 106, Position X: 48, Y: 105, Position X: 48, Y: 104, Position X: 48, Y: 103, Position X: 48, Y: 102, Position X: 47, Y: 101, Position X: 48, Y: 100, Position X: 47, Y: 99, Position X: 47, Y: 98, Position X: 47, Y: 97, Position X: 47, Y: 96, Position X: 46, Y: 95, Position X: 46, Y: 94, Position X: 46, Y: 93, Position X: 46, Y: 92, Position X: 45, Y: 91, Position X: 46, Y: 90, Position X: 45, Y: 89, Position X: 45, Y: 88, Position X: 45, Y: 87, Position X: 45, Y: 86, Position X: 44, Y: 85, Position X: 44, Y: 84, Position X: 44, Y: 83, Position X: 44, Y: 82, Position X: 43, Y: 81, Position X: 44, Y: 80, Position X: 43, Y: 79, Position X: 43, Y: 78, Position X: 43, Y: 77, Position X: 43, Y: 76, Position X: 42, Y: 75, Position X: 42, Y: 74, Position X: 42, Y: 73, Position X: 42, Y: 72, Position X: 41, Y: 71, Position X: 42, Y: 70, Position X: 41, Y: 69, Position X: 41, Y: 68, Position X: 41, Y: 67, Position X: 41, Y: 66, Position X: 40, Y: 65, Position X: 40, Y: 64, Position X: 40, Y: 63, Position X: 40, Y: 62, Position X: 39, Y: 61, Position X: 40, Y: 60, Position X: 39, Y: 59, Position X: 39, Y: 58, Position X: 39, Y: 57, Position X: 39, Y: 56, Position X: 38, Y: 55, Position X: 38, Y: 54, Position X: 38, Y: 53, Position X: 38, Y: 52, Position X: 37, Y: 51, Position X: 38, Y: 50, Position X: 37, Y: 49, Position X: 37, Y: 48, Position X: 37, Y: 47, Position X: 37, Y: 46, Position X: 36, Y: 45, Position X: 36, Y: 44, Position X: 36, Y: 43, Position X: 36, Y: 42, Position X: 35, Y: 41, Position X: 36, Y: 40, Position X: 35, Y: 39, Position X: 35, Y: 38, Position X: 35, Y: 37, Position X: 35, Y: 36, Position X: 34, Y: 35, Position X: 34, Y: 34, Position X: 34, Y: 33, Position X: 34, Y: 32, Position X: 33, Y: 31, Position X: 34, Y: 30, Position X: 33, Y: 29, Position X: 33, Y: 28, Position X: 33, Y: 27, Position X: 33, Y: 26, Position X: 32, Y: 25, Position X: 32, Y: 24, Position X: 32, Y: 23, Position X: 32, Y: 22, Position X: 31, Y: 21, Position X: 32, Y: 20, Position X: 31, Y: 19, Position X: 31, Y: 18, Position X: 31, Y: 17, Position X: 31, Y: 16, Position X: 30, Y: 15, Position X: 30, Y: 14, Position X: 30, Y: 13, Position X: 30, Y: 12, Position X: 51, Y: 119, Position X: 51, Y: 118, Position X: 51, Y: 117, Position X: 51, Y: 116, Position X: 50, Y: 115, Position X: 51, Y: 114, Position X: 50, Y: 113, Position X: 50, Y: 112, Position X: 50, Y: 111, Position X: 50, Y: 110, Position X: 50, Y: 109, Position X: 50, Y: 108, Position X: 49, Y: 107, Position X: 50, Y: 106, Position X: 49, Y: 105, Position X: 49, Y: 104, Position X: 49, Y: 103, Position X: 49, Y: 102, Position X: 48, Y: 101, Position X: 49, Y: 100, Position X: 48, Y: 99, Position X: 48, Y: 98, Position X: 48, Y: 97, Position X: 48, Y: 96, Position X: 48, Y: 95, Position X: 48, Y: 94, Position X: 47, Y: 93, Position X: 48, Y: 92, Position X: 47, Y: 91, Position X: 47, Y: 90, Position X: 47, Y: 89, Position X: 47, Y: 88, Position X: 46, Y: 87, Position X: 47, Y: 86, Position X: 46, Y: 85, Position X: 46, Y: 84, Position X: 46, Y: 83, Position X: 46, Y: 82, Position X: 45, Y: 81, Position X: 46, Y: 80, Position X: 45, Y: 79, Position X: 46, Y: 78, Position X: 45, Y: 77, Position X: 45, Y: 76, Position X: 45, Y: 75, Position X: 45, Y: 74, Position X: 44, Y: 73, Position X: 45, Y: 72, Position X: 44, Y: 71, Position X: 44, Y: 70, Position X: 44, Y: 69, Position X: 44, Y: 68, Position X: 43, Y: 67, Position X: 44, Y: 66, Position X: 43, Y: 65, Position X: 44, Y: 64, Position X: 43, Y: 63, Position X: 43, Y: 62, Position X: 43, Y: 61, Position X: 43, Y: 60, Position X: 42, Y: 59, Position X: 43, Y: 58, Position X: 42, Y: 57, Position X: 42, Y: 56, Position X: 42, Y: 55, Position X: 42, Y: 54, Position X: 41, Y: 53, Position X: 42, Y: 52, Position X: 41, Y: 51, Position X: 42, Y: 50, Position X: 41, Y: 49, Position X: 41, Y: 48, Position X: 41, Y: 47, Position X: 41, Y: 46, Position X: 40, Y: 45, Position X: 41, Y: 44, Position X: 40, Y: 43, Position X: 40, Y: 42, Position X: 40, Y: 41, Position X: 40, Y: 40, Position X: 39, Y: 39, Position X: 40, Y: 38, Position X: 39, Y: 37, Position X: 39, Y: 36, Position X: 39, Y: 35, Position X: 39, Y: 34, Position X: 39, Y: 33, Position X: 39, Y: 32, Position X: 38, Y: 31, Position X: 39, Y: 30, Position X: 38, Y: 29, Position X: 38, Y: 28, Position X: 38, Y: 27, Position X: 38, Y: 26, Position X: 37, Y: 25, Position X: 38, Y: 24, Position X: 37, Y: 23, Position X: 37, Y: 22, Position X: 37, Y: 21, Position X: 37, Y: 20, Position X: 37, Y: 19, Position X: 37, Y: 18, Position X: 36, Y: 17, Position X: 37, Y: 16, Position X: 36, Y: 15, Position X: 36, Y: 14, Position X: 36, Y: 13, Position X: 36, Y: 12, Position X: 51, Y: 119, Position X: 52, Y: 120, Position X: 51, Y: 121, Position X: 52, Y: 122, Position X: 52, Y: 123, Position X: 52, Y: 124, Position X: 52, Y: 125, Position X: 53, Y: 126, Position X: 52, Y: 127, Position X: 53, Y: 128, Position X: 53, Y: 129, Position X: 53, Y: 130, Position X: 53, Y: 131, Position X: 54, Y: 132, Position X: 54, Y: 133, Position X: 54, Y: 134, Position X: 54, Y: 135, Position X: 55, Y: 136, Position X: 54, Y: 137, Position X: 55, Y: 138, Position X: 55, Y: 139, Position X: 55, Y: 140, Position X: 55, Y: 141, Position X: 56, Y: 142, Position X: 55, Y: 143, Position X: 56, Y: 144, Position X: 56, Y: 145, Position X: 56, Y: 146, Position X: 56, Y: 147, Position X: 57, Y: 148, Position X: 56, Y: 149, Position X: 57, Y: 150, Position X: 57, Y: 151, Position X: 57, Y: 152, Position X: 57, Y: 153, Position X: 58, Y: 154, Position X: 57, Y: 155, Position X: 58, Y: 156, Position X: 58, Y: 157, Position X: 58, Y: 158, Position X: 58, Y: 159, Position X: 59, Y: 160, Position X: 58, Y: 161, Position X: 59, Y: 162, Position X: 59, Y: 163, Position X: 60, Y: 164, Position X: 59, Y: 165, Position X: 60, Y: 166, Position X: 60, Y: 167, Position X: 60, Y: 168, Position X: 60, Y: 169, Position X: 61, Y: 170, Position X: 60, Y: 171, Position X: 61, Y: 172, Position X: 61, Y: 173, Position X: 61, Y: 174, Position X: 61, Y: 175, Position X: 36, Y: 12, Position X: 35, Y: 12, Position X: 34, Y: 12, Position X: 33, Y: 11, Position X: 32, Y: 11, Position X: 31, Y: 11, Position X: 36, Y: 12, Position X: 36, Y: 11, Position X: 37, Y: 11, Position X: 38, Y: 10, Position X: 5, Y: 11, Position X: 5, Y: 12, Position X: 4, Y: 13, Position X: 3, Y: 13, Position X: 3, Y: 14, Position X: 2, Y: 15, Position X: 2, Y: 16, Position X: 1, Y: 17, Position X: 1, Y: 18, Position X: 0, Y: 19, Position X: -1, Y: 19, Position X: -1, Y: 20, Position X: -2, Y: 21, Position X: -2, Y: 22, Position X: -3, Y: 23, Position X: -4, Y: 23, Position X: -4, Y: 24, Position X: -5, Y: 25, Position X: -5, Y: 26, Position X: -6, Y: 27, Position X: -6, Y: 28, Position X: -7, Y: 29, Position X: -8, Y: 29, Position X: -8, Y: 30, Position X: -9, Y: 31, Position X: 20, Y: 15, Position X: 20, Y: 16, Position X: 19, Y: 17, Position X: 18, Y: 17, Position X: 18, Y: 18, Position X: 17, Y: 19, Position X: 17, Y: 19, Position X: 17, Y: 18, Position X: 16, Y: 17, Position X: 16, Y: 16, Position X: 15, Y: 15, Position X: 15, Y: 14, Position X: 14, Y: 13, Position X: 14, Y: 12, Position X: 13, Y: 11, Position X: 26, Y: 11, Position X: 26, Y: 12, Position X: 25, Y: 12, Position X: 24, Y: 12, Position X: 23, Y: 13, Position X: 23, Y: 14, Position X: 22, Y: 14, Position X: 21, Y: 14, Position X: 20, Y: 15, Position X: 20, Y: 15, Position X: 21, Y: 14, Position X: 20, Y: 13, Position X: 20, Y: 12, Position X: 20, Y: 11, Position X: 21, Y: 10, Position X: 20, Y: 9, Position X: 20, Y: 8, <message truncated>

            var points = new List<Point> { new Point(3.19696375550561, 0.259064570189949), new Point(5.47673264726844, 7.35443416580299), new Point(1.46673120300599, 9.6044423550388), new Point(7.05922293293254, 13.2550710445526), new Point(9.5326367796085, 0.542006226508881), new Point(15.1557762721348, 6.70530752218576), new Point(17.3631668600082, 9.47011462294968), new Point(9.37308112968369, 13.3188468671026), new Point(18.5614522917017, 3.8653869404762), new Point(26.4218715892275, 7.7817545122382), new Point(22.6280591211412, 9.4473085372929), new Point(25.7979922908349, 14.9776541027136), new Point(34.3169988222965, 3.97211253455473), new Point(32.1128553040851, 6.21197153544611), new Point(35.6954905244966, 10.0119497878533), new Point(35.0309263314265, 13.1750887656468), new Point(40.3841408059858, 2.73020903707026), new Point(38.6681405867767, 5.37370114278686), new Point(38.0553190387112, 8.17604658388349), new Point(37.8458784920377, 12.7904092747673) };

            var map = new HexMap(20, 50);

            //new HexGrid(20, 50, tileHandle.Result);
            var lines = TestMapHelpers.GenerateMap(20, 50, points, map);
            //Debug.Log(string.Join(", ", lines));
            var grid = TestMapHelpers.CreateMap(mapStartPoint, map, tileHandle.Result);

            Assert.IsNotNull(grid);
            yield return null;

            var organizationFactory = new OrganisationFactory();
            var provinceFactory = new ProvinceFactory(map, lines, UnityEngine.Object.Instantiate, provinceHandle.Result, organizationFactory);
            var result = provinceFactory.CreateProvinces(points);
            yield return null;

            Assert.IsNotNull(result);
            Assert.AreEqual(20, result.Count);

            var provinceless = map.Where(t => t.Province == null).ToList();

            Debug.Log(provinceless.Count);
            foreach (var tile in provinceless)
            {
                Debug.Log($"{tile} is on line: {lines.Contains(tile.Position)}");
                foreach (var neighbour in map.GetNeighbours(tile))
                {
                    Debug.Log($"Neighbour: {neighbour} is on line: {lines.Contains(neighbour.Position)}");
                }
            }

            foreach (var province in result)
                Debug.Log($"Province {province.Name} has {province.HexTiles.Count()} tiles");

            Assert.False(provinceless.Any());
            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateProvinces_With250SitesAnd75x100HexMap_GeneratesProvinces()
        {
            var mapStartPoint = new GameObject("map");

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

            var points = new List<Point> { new Point(8.0491087036436, 64.2831067155502), new Point(61.3850127278757, 64.8304660547667), new Point(73.0744885942314, 20.1770658717384), new Point(9.70707227741697, 22.0379066467462), new Point(64.4973738107352, 45.9624578496266), new Point(22.3040808859766, 38.9395226710194), new Point(81.5293133591904, 26.2096487005286), new Point(59.0633596713018, 8.59913677470718), new Point(17.0122104384993, 56.7959627456944), new Point(37.6355302397327, 47.0719781224951), new Point(22.1297149495826, 65.9286294206645), new Point(10.2136158697371, 45.201881677472), new Point(35.9476793640981, 55.4121532418822), new Point(74.547266083093, 51.1524058585765), new Point(88.7445716055783, 53.8324765655363), new Point(88.318572583291, 51.7837068884558), new Point(81.197661801799, 3.58407023995373), new Point(61.6432728020676, 37.1618477092878), new Point(61.0800736253523, 43.877333995829), new Point(92.6570900658411, 31.6058754425523), new Point(84.6336970788584, 19.5848490966413), new Point(61.632070526775, 67.1982956655316), new Point(3.17062626507628, 35.9641666626391), new Point(24.9987756060431, 58.3644175940959), new Point(23.7504784426421, 23.7814884892579), new Point(68.5408094388157, 71.1747644465299), new Point(2.47216532028847, 17.6838359225932), new Point(39.0778710013618, 14.0878760749883), new Point(75.7867566280937, 0.681865210031096), new Point(50.7849711686303, 73.2091816669373), new Point(51.4463760966651, 30.1033855407049), new Point(39.7385737731767, 68.3776816364274), new Point(72.1717426479663, 38.8880303394459), new Point(69.8971050148351, 57.3570326749967), new Point(72.4209869459369, 6.98652214602871), new Point(49.4241769245938, 42.8388637056755), new Point(82.3266979317771, 70.5257330194701), new Point(77.3409763552905, 70.956553996986), new Point(26.5697445089788, 14.4361242775042), new Point(57.3021613845146, 64.9058158923433), new Point(49.1336996770155, 71.3577812962969), new Point(44.6831456113994, 42.5528377613764), new Point(3.47831461507749, 68.8411382924957), new Point(66.8060371385915, 43.0093658282465), new Point(64.7170337791168, 31.9889136962541), new Point(72.929769361359, 44.9739473988181), new Point(7.91578647443828, 70.0496533317723), new Point(89.8371599683711, 33.158165811169), new Point(85.4627783966543, 51.6907347169196), new Point(17.7286799315962, 37.4819327273788), new Point(77.2171179527497, 35.3289208083083), new Point(5.66113929481299, 10.7663607302897), new Point(81.3915982164403, 8.39366052131805), new Point(12.8935950439859, 5.32214812902834), new Point(45.3171284600706, 32.2531677616076), new Point(91.277549158911, 42.2125624950102), new Point(42.6100636937702, 47.0546676074409), new Point(48.8083136341573, 41.0970856850488), new Point(94.5903026119761, 61.1507426347354), new Point(87.9999351417646, 59.1054168181053), new Point(84.219841288505, 9.23695509193323), new Point(12.0173747995949, 22.1439452330321), new Point(35.4170643204903, 65.9693767670399), new Point(49.3956663209925, 65.4874744152126), new Point(4.51483420632539, 24.6522609594521), new Point(9.46558438309728, 42.1694783503979), new Point(1.58451335857833, 5.34998588326852), new Point(9.61633844376371, 1.397278950269), new Point(89.8040005475301, 22.6331622519685), new Point(74.5371982481038, 68.2062918172247), new Point(79.2046845635421, 44.613677610929), new Point(12.2224285263673, 16.1211807197524), new Point(50.5184299752668, 12.3005984417631), new Point(17.5577848532972, 32.8621828886039), new Point(44.906763120511, 26.0539227761579), new Point(48.7699462435068, 2.73052606579406), new Point(80.1538856281684, 28.1463237368252), new Point(64.9249065494746, 48.4808906505261), new Point(35.2469922277364, 56.0876134317776), new Point(77.9865804403027, 68.8687875563599), new Point(60.8336903223925, 18.1501622843324), new Point(2.60533693926657, 9.22114819251986), new Point(33.6839238385129, 26.058184657273), new Point(54.8890499793408, 26.4999607161153), new Point(53.3351763283532, 35.085702958091), new Point(64.1772357067919, 48.0707003064783), new Point(7.2725790605287, 71.5714374806599), new Point(98.8687784768961, 18.3385269885596), new Point(0.135119502029903, 66.8615034738842), new Point(78.1065778364924, 14.1727627674922), new Point(70.6985435242292, 4.05983032102689), new Point(42.7048580565978, 51.3208017821055), new Point(83.2107812563008, 4.34475923066249), new Point(75.2620381108774, 47.0380599857485), new Point(41.2325069970603, 32.7070794993579), new Point(65.6223404219478, 11.0386091997095), new Point(51.2929204103038, 60.8458858816167), new Point(30.4152676493001, 41.0533393086183), new Point(57.080529053733, 73.9148084837547), new Point(86.8836017189937, 11.7030660313103), new Point(60.5671560440991, 60.624040083319), new Point(89.6290953739682, 72.4809684653212), new Point(69.6786880072573, 62.383602946244), new Point(95.3804220274931, 62.3415679383751), new Point(37.7864384142619, 30.1912011393305), new Point(64.7244504223226, 33.4847331407875), new Point(95.9295536465615, 44.1202367535421), new Point(21.4367263146847, 41.4118169906604), new Point(80.5771158102793, 24.6266217886594), new Point(28.1256197039623, 20.8331232344886), new Point(58.5995873089878, 67.990118786688), new Point(93.220143150175, 7.15961795819906), new Point(80.3166405625253, 36.932253853852), new Point(13.0178813510658, 18.5901833151421), new Point(65.335709520772, 32.8485759165364), new Point(66.4534462417725, 0.186189931904054), new Point(2.56624752309464, 46.6796285131386), new Point(57.3937022529513, 66.2213358842867), new Point(14.8022992447029, 33.4398345264792), new Point(97.980222485019, 73.8143547604859), new Point(9.4321612675824, 67.9149049929878), new Point(25.1428580531584, 0.629056616047889), new Point(78.0323180970886, 15.5663091948099), new Point(17.8383154179148, 68.2199259773921), new Point(59.9652020945052, 55.322827514877), new Point(73.3455948863856, 18.0396322515046), new Point(70.7604078621419, 44.1101750201127), new Point(56.0572172464138, 51.2823008826386), new Point(86.8849915558868, 59.8358674728479), new Point(16.6511415735125, 15.1508058687443), new Point(70.4412631310715, 52.7042735538931), new Point(18.0671989373244, 69.3958325951341), new Point(72.9374850121967, 48.1937023029633), new Point(42.2308954625534, 25.0252903741902), new Point(1.63201989030094, 23.8020757044675), new Point(40.5999964590184, 26.7895247660529), new Point(33.9662039619713, 21.2088357253041), new Point(18.439179275855, 49.8868067329222), new Point(65.816895458762, 69.050646344689), new Point(30.5460897118533, 44.4683514276838), new Point(31.5710512244008, 15.510611598152), new Point(63.9054485568336, 67.5139654602455), new Point(69.6004728938455, 31.6326406968909), new Point(28.4886840737838, 31.8080301991701), new Point(6.39255603467699, 65.6180422155271), new Point(1.70841726228055, 25.2284774171321), new Point(28.0859343470474, 58.0869361907649), new Point(28.3103736430921, 28.3460288077342), new Point(72.7919307154566, 23.3978188789439), new Point(26.9040907308944, 10.1334849615272), new Point(77.5943019737463, 62.3083999381905), new Point(89.6351783767506, 21.0203985883949), new Point(33.4129754865602, 28.0344552500334), new Point(89.1678986447714, 31.6827822726605), new Point(40.5730818997012, 66.014547572478), new Point(2.79050707341661, 69.893690777893), new Point(49.5045978531729, 41.9779564151438), new Point(87.6982701405409, 37.1424537725479), new Point(6.60432281932064, 19.9908095030071), new Point(23.2398977401852, 22.886287494044), new Point(35.9866744126131, 73.7584977223345), new Point(27.0872243307937, 9.6018514398494), new Point(30.1349002342368, 5.11899468354834), new Point(7.75410554267192, 41.3019435383854), new Point(0.0636092867067127, 59.4367920651272), new Point(92.9485269407502, 25.798648433666), new Point(65.4744766761896, 12.6989457321814), new Point(80.8971411524793, 35.2901636032807), new Point(30.1155047072636, 26.7473104338848), new Point(56.909762088633, 23.9901406504168), new Point(52.1514449902584, 63.6117896612788), new Point(1.98090444783722, 12.7637266352604), new Point(84.9957926473561, 5.38330798381162), new Point(83.2470138265039, 53.0155702685078), new Point(89.0126309795364, 58.0188356172381), new Point(31.3180670721075, 34.4193205267281), new Point(8.0625896789425, 71.223840931069), new Point(76.5702851980786, 58.2412835509708), new Point(56.2713107216504, 57.3981808821662), new Point(96.986984363751, 19.8563607306482), new Point(29.7514494539944, 25.3488089839689), new Point(42.3228156456364, 44.8905620821242), new Point(94.3682326494568, 50.2871851363625), new Point(28.0319474726133, 24.3044910469579), new Point(74.2625572948077, 30.2620787239923), new Point(19.5750753397937, 52.1892540977286), new Point(68.8346992734981, 67.3810960442671), new Point(77.4666963869085, 37.2873407077451), new Point(96.6960046671778, 7.48328953119148), new Point(26.8499275221722, 17.1417689589512), new Point(22.6013709528378, 26.7804278138934), new Point(65.2426718623576, 16.02871071828), new Point(48.1987223123194, 35.0573561764589), new Point(26.4518183606918, 51.716676988507), new Point(39.418844903083, 2.2272866034076), new Point(89.9412054503063, 39.1123983939702), new Point(37.7965795950948, 22.6822493154007), new Point(2.34346844318484, 13.6330791905676), new Point(42.7794434958042, 30.5901139697945), new Point(21.7075637950132, 13.2450235091359), new Point(78.1700456408644, 37.9205495910349), new Point(95.6636786049528, 36.2726151394996), new Point(58.044718256241, 45.2202101783921), new Point(76.2127700798273, 12.6454861399929), new Point(17.819252966819, 19.9470138847581), new Point(80.2213881915535, 34.5780942405472), new Point(49.9395306850502, 55.3535526866808), new Point(3.96321975438074, 69.4580293537388), new Point(67.6699239405198, 15.6065252188624), new Point(11.8575702029548, 35.480514692832), new Point(40.8242806730905, 43.23649586981), new Point(92.0966228489283, 53.2820996573577), new Point(49.544548518278, 49.5194761024413), new Point(32.02417926538, 28.7699501415575), new Point(87.8015113998212, 44.2713201289397), new Point(7.10497177536831, 41.6877077006212), new Point(87.3038640913106, 6.82461928521498), new Point(43.7628614798947, 52.9734044545206), new Point(39.1641910379586, 12.494634535394), new Point(62.3990974344309, 64.8071176106143), new Point(69.6882875075975, 7.12658010941305), new Point(51.3692743556431, 9.51757725492007), new Point(21.7583601021945, 32.6506855909949), new Point(2.38651581499098, 46.8984361015719), new Point(26.3819516270337, 6.29365412904586), new Point(49.5689252468613, 16.3700285052741), new Point(29.0670417296081, 54.7453410731374), new Point(75.8954129404833, 15.1936392556846), new Point(57.6349232255644, 18.2240843503848), new Point(97.9823284926742, 67.8674850183853), new Point(28.4731289462527, 28.1971709282124), new Point(28.1160984361154, 43.0481214826219), new Point(19.5808981515378, 18.2757541799339), new Point(57.9560187440161, 30.5039188026003), new Point(30.2912157630973, 23.9890022305721), new Point(53.7593887731244, 38.0869247736814), new Point(57.4799025205336, 18.0561166825034), new Point(76.7788875544345, 23.3885387775435), new Point(6.47414579450812, 59.3223226523596), new Point(49.5244488988651, 4.38261330145533), new Point(63.8625130247616, 51.0388292079041), new Point(12.1075762650499, 59.3357584324366), new Point(9.65892195266621, 62.9407584783345), new Point(26.7043510050067, 10.5120926920847), new Point(32.2348209681152, 17.517994577772), new Point(13.2349016527808, 11.050159455766), new Point(17.7334663587313, 52.7742645790681), new Point(58.2282851544341, 23.8930641784766), new Point(80.4181100886399, 69.3491138943234), new Point(92.152076824639, 71.9880705429186) };
            var map = new HexMap(75, 100);

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

            var janlandObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var janland = organizationFactory.CreateCountry(janlandObject, "Janland", CountryType.Major, Color.blue);
            yield return null;

            janland.AddProvince(result.Single(p => p.Name == "Region 159"));
            janland.AddProvince(result.Single(p => p.Name == "Region 167"));
            janland.AddProvince(result.Single(p => p.Name == "Region 178"));
            janland.AddProvince(result.Single(p => p.Name == "Region 201"));
            janland.AddProvince(result.Single(p => p.Name == "Region 202"));
            janland.AddProvince(result.Single(p => p.Name == "Region 223"));
            janland.AddProvince(result.Single(p => p.Name == "Region 225"));
            janland.AddProvince(result.Single(p => p.Name == "Region 226"));

            //Add Province: Region 159
            //Add Province: Region 167
            //Add Province: Region 178
            //Add Province: Region 201
            //Add Province: Region 202
            //Add Province: Region 223
            //Add Province: Region 225
            //Add Province: Region 226

            var fantasiaObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var fantasia = organizationFactory.CreateCountry(fantasiaObject, "Fantasia", CountryType.Minor, Color.red);
            yield return null;

            fantasia.AddProvince(result.Single(p => p.Name == "Region 219"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 229"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 216"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 207"));

            //Add Province: Region 219
            //Add Province: Region 229
            //Add Province: Region 216
            //Add Province: Region 207

            //LogMap(map);

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

            terrainGenerator.GenerateTerrain(map);
            yield return null;
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

            var points = new List<Point> { new Point(3.35730692714327, 32.3552449142352), new Point(95.4750663793995, 73.4285777106083), new Point(27.0168592971828, 8.08345243338656), new Point(67.6083217759656, 41.267783086406), new Point(86.9835316277964, 37.2677802654299), new Point(56.4061107474408, 69.5699317825818), new Point(47.0097896428824, 43.7095019927758), new Point(96.6874393386242, 67.8450127960392), new Point(85.5593045696427, 16.758382222037), new Point(96.1072744522743, 26.7554546998606), new Point(13.4900394345122, 44.08613043655), new Point(13.0178907094607, 51.6382774317815), new Point(94.7174306440714, 65.8027285317903), new Point(77.6049995872215, 36.3225290795427), new Point(14.3742292511157, 62.0412385380088), new Point(78.8418427206771, 58.0722553748974), new Point(48.4305432152145, 61.4002045818605), new Point(23.3650044972845, 19.79909056602), new Point(45.4538374601183, 47.5501577032498), new Point(46.6023317215044, 7.56999076230917), new Point(39.1180363563439, 46.6268986233635), new Point(56.2564848574141, 56.5858172031985), new Point(75.0434513152779, 73.6115183716693), new Point(20.332251822265, 12.4442470606623), new Point(97.2308610920938, 36.6824725068558), new Point(89.4086942483711, 9.39535496076353), new Point(7.71851326186141, 50.04091356976), new Point(4.33636071828025, 32.4233717110117), new Point(30.2681801893135, 19.7269237002949), new Point(3.51809899579645, 28.3916915489322), new Point(32.2093489385254, 14.2129841671386), new Point(40.8353724455626, 2.97675479994004), new Point(70.0164038506413, 58.0898880120785), new Point(44.6427438993206, 47.738426060294), new Point(35.1112211416993, 52.4723287375981), new Point(45.3117877670153, 16.4032618740589), new Point(74.8176661160857, 64.2677699393908), new Point(95.6764233096859, 37.4565651963728), new Point(2.72360856445674, 27.1447273675095), new Point(93.0401901137271, 71.1873691804648), new Point(67.7011282405356, 45.563530408574), new Point(50.3627926764837, 48.061900872766), new Point(92.5924221741, 49.5369315173183), new Point(69.9727473072581, 60.1596944900973), new Point(77.807156222317, 59.0413791253424), new Point(95.2197923246863, 14.2486719769652), new Point(60.0963011747675, 6.44237458633369), new Point(76.9180711353747, 15.0267601800276), new Point(21.5438567938022, 39.0735470126725), new Point(5.68624348877289, 72.0031968094423), new Point(53.8375306789938, 41.4594187361465), new Point(80.5371634394569, 20.205284603967), new Point(3.76341193810264, 50.4274792812893), new Point(36.7517952503412, 15.5016238118995), new Point(70.270204303912, 39.7847549364831), new Point(40.6536049408156, 69.4799729760177), new Point(32.3512348366674, 5.44232097800929), new Point(69.2823509938467, 52.5121445555762), new Point(67.6518753309044, 56.4615088340181), new Point(10.3899930312252, 2.79856227002971), new Point(7.74210287106321, 47.2096292326272), new Point(85.0591164799682, 41.2033720310793), new Point(73.9796744999381, 19.6207451418139), new Point(60.8486032243113, 49.4819415004374), new Point(9.06178324299947, 56.4889501726669), new Point(28.5669534013453, 36.9622816112648), new Point(30.6289998812736, 71.5419958008183), new Point(41.3996253867632, 9.14546499920332), new Point(79.4194515666084, 17.4396311340107), new Point(60.5356176078951, 18.7830181013714), new Point(29.520586103909, 36.8012378117075), new Point(10.2137112977047, 18.3741295125215), new Point(38.3341860511965, 1.69441724647508), new Point(85.7111776730563, 39.4781465667664), new Point(38.3365014932754, 4.98217124258269), new Point(51.4514930180514, 33.6419800825613), new Point(41.8840766208638, 1.45176114582073), new Point(88.5864982658934, 67.0325654163177), new Point(69.4068626400115, 18.9961238731612), new Point(52.0517197675313, 57.1923102462628), new Point(6.61518662498108, 51.989097373555), new Point(11.6768756404877, 70.0362530443986), new Point(24.6585971217875, 67.4252614758095), new Point(62.3239369379934, 26.6397352892159), new Point(64.8813175432763, 42.6413428143791), new Point(89.8332823658517, 33.1284372951502), new Point(15.0007252916697, 62.983239315908), new Point(73.2234363924821, 42.985788576764), new Point(52.9451169999992, 45.2054120987679), new Point(16.7892440984907, 53.6035212686302), new Point(39.5381975842352, 6.00464776251682), new Point(27.8623121356882, 1.79128289864924), new Point(24.1215619426787, 61.7110961972322), new Point(7.56546229150401, 21.4426428058383), new Point(7.12509071087702, 37.9126091152023), new Point(41.8282864805443, 40.3679106311723), new Point(70.279678641483, 62.0565352384264), new Point(18.5134456998266, 44.0767952697709), new Point(37.5572127832832, 11.5982382286332), new Point(98.9230085773966, 35.2285745624586), new Point(38.942918567426, 37.4272055977151), new Point(86.9340407014517, 60.0142239583722), new Point(15.832054837063, 5.33025445571647), new Point(30.0067886025676, 42.3240502794851), new Point(27.7187845998997, 23.2303818442069), new Point(36.7335853812907, 6.67446502608921), new Point(8.62448919127904, 59.3038248202316), new Point(36.9758390965759, 72.9400371894892), new Point(41.6906694856895, 6.93688677201834), new Point(69.5755874005033, 30.7205824594575), new Point(82.6386037369439, 25.1429262185203), new Point(28.51455514902, 10.5845373312871), new Point(15.2189153927466, 26.7801994200704), new Point(73.0407982804071, 23.1561281109024), new Point(65.747915006591, 10.6558743569329), new Point(19.9508016640091, 27.9768997309622), new Point(60.5545022033874, 51.3209614303526), new Point(32.7699004270928, 66.1265986571678), new Point(20.0992042781316, 34.8121507693139), new Point(85.5643912030218, 12.7000039660838), new Point(52.5526509045403, 37.3309417363866), new Point(0.967994288992134, 56.0954435356406), new Point(13.9873376241826, 24.591122848257), new Point(45.3812290855596, 67.2284602249174), new Point(46.0457418305081, 14.8982959598761), new Point(17.2769349968419, 21.1361813690216), new Point(44.9409745540195, 43.2218683963743), new Point(63.4915162858048, 3.96592200359605), new Point(21.5569766641394, 54.39646278899), new Point(65.0703842272378, 59.0538617712696), new Point(33.0902178157541, 73.2731886223299), new Point(89.8748009078553, 10.0632171621841), new Point(11.127682154592, 73.4805277257601), new Point(47.37484965584, 29.1256264499974), new Point(46.5690003459197, 35.5119073342122), new Point(77.4827373672662, 70.350571876555), new Point(22.7160383973811, 39.3059906565146), new Point(87.5465067101393, 24.4393277235512), new Point(32.6691637601094, 39.2184663737279), new Point(0.173056913620353, 60.7846320917758), new Point(89.4463350034535, 61.3676920250839), new Point(83.9334025340776, 34.2466102066667), new Point(95.9788969131088, 67.7765390667024), new Point(91.4875804914569, 2.04109284656173), new Point(5.16760778993722, 20.5287510429177), new Point(66.9096891069364, 34.6271848532498), new Point(80.5026282884658, 4.9033599360396), new Point(82.9003280307633, 40.0085908416699), new Point(59.0679454692024, 64.6603341245373), new Point(63.9189247376839, 10.9746539327198), new Point(84.5240849654768, 4.79569630082496), new Point(43.3717775225508, 72.9061219128343), new Point(41.4487666871626, 16.563500747347), new Point(5.56074478549917, 68.2862529495201), new Point(69.2773442525777, 23.0189773789695), new Point(71.636596758215, 50.8948293146188), new Point(72.6005892500284, 61.8538369265636), new Point(88.5581557445964, 37.3664101806313), new Point(14.0942417039043, 32.9325339062757), new Point(16.4840557768401, 14.5411223417805), new Point(6.8174498443573, 33.3704109365914), new Point(33.7977573255998, 14.2804007308001), new Point(79.5994842185636, 23.2892046939997), new Point(13.6150286815199, 12.0763051286695), new Point(68.6847135232225, 25.4302121537878), new Point(72.6279118906836, 33.7590408072616), new Point(87.5490235190601, 63.1547017400873), new Point(95.795895805953, 62.0631803684231), new Point(38.7282428893858, 63.8319769510217), new Point(4.36764426499961, 55.1783009856838), new Point(85.1132736956297, 0.098403175407277), new Point(32.4533069880927, 54.8436789488623), new Point(54.8275431505533, 73.1184736542024), new Point(72.7249688644544, 72.3198448244109), new Point(17.0017447103754, 24.5994915797373), new Point(39.430764962654, 11.2192637171686), new Point(70.0209858222031, 33.2366597853772), new Point(7.86485744401107, 29.8092081285125), new Point(71.6181066444228, 18.1389077203995), new Point(17.9370842864444, 7.69270171862687), new Point(8.54424934533623, 66.080211175643), new Point(22.6712194768112, 26.3528532471288), new Point(57.167747035235, 19.7875062645355), new Point(79.5400048883352, 65.1124053760955), new Point(85.954507649855, 4.13180473359851), new Point(11.2619545144317, 20.7031128074523), new Point(39.6907456906935, 31.1431144434694), new Point(33.3403899778334, 4.99746842542545), new Point(12.1908914270768, 44.4192911733032), new Point(63.2773172865982, 60.3801307111886), new Point(57.432210388329, 11.8570453011696), new Point(98.1543932320338, 26.7405973164088), new Point(93.590735080927, 43.0682663829384), new Point(74.1431363388631, 32.204024461193), new Point(76.6258921812409, 41.7958048981595), new Point(11.4123643890081, 10.8094758534848), new Point(67.4597497426252, 69.5720020754132), new Point(65.2753695409165, 71.5398115867469), new Point(76.460428014612, 71.9051741985163), new Point(16.2042018287835, 21.1946977065851), new Point(18.2806558112058, 63.2476723609714), new Point(10.7977171828028, 8.57657010041949), new Point(21.6481761125141, 8.77038827574364), new Point(74.3188098228159, 21.1958042519148), new Point(11.1248710863874, 0.881313906461612), new Point(27.6889951232304, 9.11343500535629), new Point(59.9895997988012, 27.0273868362547), new Point(51.8593770460502, 68.5295653205968), new Point(89.2502136110562, 64.2055667369652), new Point(40.6651091280697, 73.6631808959242), new Point(51.3293382582857, 27.2501206068555), new Point(10.4839474435355, 22.4530190017321), new Point(93.1153203221575, 71.6085507532622), new Point(59.2376579322096, 34.0958280368223), new Point(75.389067349671, 27.3812879926438), new Point(29.225374338322, 11.2072092924301), new Point(43.2216066327978, 26.1034990530943), new Point(62.4981677078168, 53.6814545959614), new Point(5.06508666605925, 64.791360194232), new Point(14.1264067837626, 61.1863227948483), new Point(82.2995465543585, 34.2243178599627), new Point(31.9588913684519, 56.3946055017387), new Point(28.2270141324154, 73.4170191844073), new Point(53.4717260051853, 23.3970725673237), new Point(41.2166797845702, 54.2621250433205), new Point(6.45845324148352, 66.946672385068), new Point(55.5323536510264, 12.4490507740756), new Point(76.0257302951187, 60.4142079792983), new Point(74.1311817961425, 59.6180019227872), new Point(17.3587396770524, 18.5729142197282), new Point(51.4956993556096, 21.4556055681108), new Point(51.9674815819447, 54.9342722133427), new Point(70.953680752755, 9.48961534513608), new Point(67.9706915500437, 18.7372119076258), new Point(72.660093059605, 59.0821201657327), new Point(86.6163588304149, 1.92092068675948), new Point(71.770229796772, 43.2098193816886), new Point(16.2498441037023, 4.14306629269527), new Point(4.49735106504399, 25.4418804661566), new Point(1.81148426179378, 70.1843313594276), new Point(42.32890270526, 20.8815808365501), new Point(4.39800907550287, 2.0892990362315), new Point(30.1732698838102, 28.8985569183242), new Point(58.4610749969543, 19.8580087292278), new Point(57.896518438075, 60.3015941457365), new Point(96.6858993525085, 18.1680224538632), new Point(69.3215935557716, 65.9862181236903), new Point(30.3616784374982, 40.0612272098946), new Point(92.8191058253027, 42.9541920418638), new Point(4.49315633740889, 11.6093649154573) };
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

            var janlandObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var janland = organizationFactory.CreateCountry(janlandObject, "Janland", CountryType.Major, Color.blue);
            yield return null;

            janland.AddProvince(result.Single(p => p.Name == "Region 157"));
            janland.AddProvince(result.Single(p => p.Name == "Region 143"));
            janland.AddProvince(result.Single(p => p.Name == "Region 150"));
            janland.AddProvince(result.Single(p => p.Name == "Region 142"));
            janland.AddProvince(result.Single(p => p.Name == "Region 136"));
            janland.AddProvince(result.Single(p => p.Name == "Region 153"));
            janland.AddProvince(result.Single(p => p.Name == "Region 162"));
            janland.AddProvince(result.Single(p => p.Name == "Region 177"));

            //Try genereate provinces for country Janland attempt 0
            //Add Province: Region 157
            //Add Province: Region 143
            //Add Province: Region 150
            //Add Province: Region 142
            //Add Province: Region 136
            //Add Province: Region 153
            //Add Province: Region 162
            //Add Province: Region 177

            var fantasiaObject = UnityEngine.Object.Instantiate(countryHandle.Result);
            var fantasia = organizationFactory.CreateCountry(fantasiaObject, "Fantasia", CountryType.Minor, Color.red);
            yield return null;

            fantasia.AddProvince(result.Single(p => p.Name == "Region 201"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 183"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 168"));
            fantasia.AddProvince(result.Single(p => p.Name == "Region 190"));

            //Try genereate provinces for country Fantasia attempt 0
            //Add Province: Region 201
            //Add Province: Region 183
            //Add Province: Region 168
            //Add Province: Region 190
            yield return null;

            // LogMap(map);
            foreach (var province in fantasia.Provinces)
            {
                Debug.Log($"Neighbours of {province}: {string.Join(", ", province.GetNeighbours(map))}");
            }
            yield return null;
        }
    }
}