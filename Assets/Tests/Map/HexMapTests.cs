using Assets.Contracts.Map;
using Assets.Scripts.Map;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace Tests
{
    public class HexMapTests
    {
        [Test]
        public void CreateHexMap_WithHeightAndWidth_CreatesValidHexMap()
        {
            var hexMap = HexMapBuilder.Create().WithHeight(10).WithWidth(12).Build();
            Assert.IsNotNull(hexMap);
            Assert.AreEqual(10, hexMap.Height);
            Assert.AreEqual(12, hexMap.Width);
        }

        [Test]
        public void GetNeighbour_WithDirection_ReturnsCorrectNeigbour()
        {
            var hexMap = HexMapBuilder.Create().WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);
            Assert.IsNotNull(tile);
            var expected = hexMap.GetTile(1, 0);

            var neigbour = hexMap.GetNeighbour(tile, Direction.Northwest);

            Assert.AreSame(expected, neigbour);
        }

        [Test]
        public void GetTile_WithValidPosition_ReturnsTile()
        {
            var hexMap = new HexMap(1, 1);
            var tile = TileBuilder.Create().Build();
            Assert.IsNotNull(tile);
            hexMap.AddTile(0, 0, tile);

            var result = hexMap.GetTile(0, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(tile, result);
        }

        [Test]
        public void ConvertPositionToCube_WithPosition_ReturnsVector3()
        {
            var hexMap = HexMapBuilder.Create().WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(0, 1);
            var expected = new Vector3(0, -1, 1);

            Debug.Log(tile.Position);

            var cube = hexMap.ConvertPositionToCube(tile.Position);

            Assert.AreEqual(expected, cube);
        }

        [Test]
        public void GetDistance_WithTwoTiles_ReturnsTwo()
        {
            var hexMap = HexMapBuilder.Create().WithHeight(3).WithWidth(3).Build();
            var a = hexMap.GetTile(0, 0);
            var b = hexMap.GetTile(1, 2);
            var distance = hexMap.GetDistance(a.Position, b.Position);

            Assert.AreEqual(2, distance);
        }

        [Test]
        public void DrawLine_WithThreeTiles_ReturnsThree()
        {
            var hexMap = HexMapBuilder.Create().WithHeight(3).WithWidth(3).Build();
            var a = hexMap.GetTile(0, 0);
            var b = hexMap.GetTile(1, 2);
            var line = hexMap.DrawLine(a.Position, b.Position).ToList();

            Assert.IsNotNull(line);
            Assert.AreEqual(3, line.Count);
            Assert.AreEqual(new Position(0, 0), line[0]);
            Assert.AreEqual(new Position(0, 1), line[1]);
            Assert.AreEqual(new Position(1, 2), line[2]);
        }
    }
}