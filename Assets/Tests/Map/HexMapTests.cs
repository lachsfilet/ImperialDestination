using Assets.Contracts.Map;
using Assets.Scripts.Map;
using Helpers;
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
            var hexMap = HexMapBuilder.New.WithHeight(10).WithWidth(12).Build();
            Assert.IsNotNull(hexMap);
            Assert.AreEqual(10, hexMap.Height);
            Assert.AreEqual(12, hexMap.Width);
        }

        [Test]
        [TestCase(Direction.Northwest, 1, 0)]
        [TestCase(Direction.Northeast, 2, 0)]
        [TestCase(Direction.East, 2, 1)]
        [TestCase(Direction.Southeast, 2, 2)]
        [TestCase(Direction.Southwest, 1, 2)]
        [TestCase(Direction.West, 0, 1)]
        public void GetNeighbours_WithOddTile_ReturnsCorrectNeigbour(Direction direction, int x, int y)
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);

            var expected = hexMap.GetTile(x, y);
            Assert.IsNotNull(expected);

            var neigbour = hexMap.GetNeighbour(tile, direction);

            Assert.AreSame(expected, neigbour);
        }

        [Test]
        [TestCase(Direction.Northwest, 0, 1)]
        [TestCase(Direction.Northeast, 1, 1)]
        [TestCase(Direction.East, 2, 2)]
        [TestCase(Direction.Southeast, 1, 3)]
        [TestCase(Direction.Southwest, 0, 3)]
        [TestCase(Direction.West, 0, 2)]
        public void GetNeighbours_WithEvenTile_ReturnsCorrectNeigbour(Direction direction, int x, int y)
        {
            var hexMap = HexMapBuilder.New.WithHeight(4).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 2);

            var expected = hexMap.GetTile(x, y);
            Assert.IsNotNull(expected);

            var neigbour = hexMap.GetNeighbour(tile, direction);

            Assert.AreSame(expected, neigbour);
        }

        [Test]
        public void GetNeighbours_WithOddTile_ReturnsAllNeighbours()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);

            var neighbours = hexMap.GetNeighbours(tile).ToList();

            Assert.AreEqual(6, neighbours.Count);

            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(1, 0))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 0))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 1))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 2))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(1, 2))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(0, 1))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
        }

        [Test]
        public void GetNeighbours_WithEvenTile_ReturnsAllNeighbours()
        {
            var hexMap = HexMapBuilder.New.WithHeight(8).WithWidth(8).Build();
            var tile = hexMap.GetTile(3, 4);

            var neighbours = hexMap.GetNeighbours(tile).ToList();

            Assert.AreEqual(6, neighbours.Count);

            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 3))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(3, 3))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 4))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(4, 4))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(2, 5))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
            Assert.IsTrue(neighbours.Any(n => n.Position.Equals(new Position(3, 5))), $"Neighbours are {string.Join(", ", neighbours.Select(n => n.Position))}");
        }

        [Test]
        public void GetTile_WithValidPosition_ReturnsTile()
        {
            var hexMap = new HexMap(1, 1);
            var tile = TileBuilder.New.Build();
            Assert.IsNotNull(tile);
            hexMap.AddTile(0, 0, tile);

            var result = hexMap.GetTile(0, 0);

            Assert.IsNotNull(result);
            Assert.AreEqual(tile, result);
        }

        [Test]
        public void ConvertPositionToCube_WithPosition_ReturnsVector3()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(0, 1);
            var expected = new Vector3(0, -1, 1);

            Debug.Log(tile.Position);

            var cube = hexMap.ConvertPositionToCube(tile.Position);

            Assert.AreEqual(expected, cube);
        }

        [Test]
        public void GetDistance_WithTwoTiles_ReturnsTwo()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var a = hexMap.GetTile(0, 0);
            var b = hexMap.GetTile(1, 2);
            var distance = hexMap.GetDistance(a.Position, b.Position);

            Assert.AreEqual(2, distance);
        }

        [Test]
        public void DrawLine_WithThreeTiles_ReturnsThree()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var a = hexMap.GetTile(0, 0);
            var b = hexMap.GetTile(1, 2);
            var line = hexMap.DrawLine(a.Position, b.Position).ToList();

            Assert.IsNotNull(line);
            Assert.AreEqual(3, line.Count);
            Assert.AreEqual(new Position(0, 0), line[0]);
            Assert.AreEqual(new Position(0, 1), line[1]);
            Assert.AreEqual(new Position(1, 2), line[2]);
        }

        [Test]
        public void GetNeighboursWithDirection_WithSixNeighbours_ReturnsNeighboursInRightOrder()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var center = hexMap.GetTile(1, 1);

            var neighbours = hexMap.GetNeighboursWithDirection(center).ToList();

            Assert.AreEqual(6, neighbours.Count);

            Assert.AreEqual(Direction.Northeast, neighbours[0].Direction);
            Assert.AreEqual(Direction.East, neighbours[1].Direction);
            Assert.AreEqual(Direction.Southeast, neighbours[2].Direction);
            Assert.AreEqual(Direction.Southwest, neighbours[3].Direction);
            Assert.AreEqual(Direction.West, neighbours[4].Direction);
            Assert.AreEqual(Direction.Northwest, neighbours[5].Direction);

            Assert.AreSame(hexMap.GetTile(2, 0), neighbours[0].Neighbour);
            Assert.AreSame(hexMap.GetTile(2, 1), neighbours[1].Neighbour);
            Assert.AreSame(hexMap.GetTile(2, 2), neighbours[2].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 2), neighbours[3].Neighbour);
            Assert.AreSame(hexMap.GetTile(0, 1), neighbours[4].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 0), neighbours[5].Neighbour);

            Assert.AreSame(center, neighbours[0].HexTile);
            Assert.AreSame(center, neighbours[1].HexTile);
            Assert.AreSame(center, neighbours[2].HexTile);
            Assert.AreSame(center, neighbours[3].HexTile);
            Assert.AreSame(center, neighbours[4].HexTile);
            Assert.AreSame(center, neighbours[5].HexTile);
        }

        [Test]
        public void GetNeighboursWithDirection_WithFiveNeighbours_ReturnsNeighboursInRightOrder()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(0, 1);

            var neighbours = hexMap.GetNeighboursWithDirection(tile).ToList();

            Assert.AreEqual(5, neighbours.Count);

            Assert.AreEqual(Direction.Northeast, neighbours[0].Direction);
            Assert.AreEqual(Direction.East, neighbours[1].Direction);
            Assert.AreEqual(Direction.Southeast, neighbours[2].Direction);
            Assert.AreEqual(Direction.Southwest, neighbours[3].Direction);
            Assert.AreEqual(Direction.Northwest, neighbours[4].Direction);

            Assert.AreSame(hexMap.GetTile(1, 0), neighbours[0].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 1), neighbours[1].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 2), neighbours[2].Neighbour);
            Assert.AreSame(hexMap.GetTile(0, 2), neighbours[3].Neighbour);
            Assert.AreSame(hexMap.GetTile(0, 0), neighbours[4].Neighbour);

            Assert.AreSame(tile, neighbours[0].HexTile);
            Assert.AreSame(tile, neighbours[1].HexTile);
            Assert.AreSame(tile, neighbours[2].HexTile);
            Assert.AreSame(tile, neighbours[3].HexTile);
            Assert.AreSame(tile, neighbours[4].HexTile);
        }

        [Test]
        public void GetNeighboursWithDirection_WithSixNeighboursAndreverseOrder_ReturnsNeighboursInLeftOrder()
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);

            var neighbours = hexMap.GetNeighboursWithDirection(tile, true).ToList();

            Assert.AreEqual(6, neighbours.Count);

            Assert.AreEqual(Direction.Northwest, neighbours[0].Direction);
            Assert.AreEqual(Direction.West, neighbours[1].Direction);
            Assert.AreEqual(Direction.Southwest, neighbours[2].Direction);
            Assert.AreEqual(Direction.Southeast, neighbours[3].Direction);
            Assert.AreEqual(Direction.East, neighbours[4].Direction);
            Assert.AreEqual(Direction.Northeast, neighbours[5].Direction);

            Assert.AreSame(hexMap.GetTile(2, 0), neighbours[5].Neighbour);
            Assert.AreSame(hexMap.GetTile(2, 1), neighbours[4].Neighbour);
            Assert.AreSame(hexMap.GetTile(2, 2), neighbours[3].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 2), neighbours[2].Neighbour);
            Assert.AreSame(hexMap.GetTile(0, 1), neighbours[1].Neighbour);
            Assert.AreSame(hexMap.GetTile(1, 0), neighbours[0].Neighbour);

            Assert.AreSame(tile, neighbours[0].HexTile);
            Assert.AreSame(tile, neighbours[1].HexTile);
            Assert.AreSame(tile, neighbours[2].HexTile);
            Assert.AreSame(tile, neighbours[3].HexTile);
            Assert.AreSame(tile, neighbours[4].HexTile);
            Assert.AreSame(tile, neighbours[5].HexTile);
        }

        [Test]
        [TestCase(2, 0, 2, 1)]
        [TestCase(2, 1, 2, 2)]
        [TestCase(2, 2, 1, 2)]
        [TestCase(1, 2, 0, 1)]
        [TestCase(0, 1, 1, 0)]
        [TestCase(1, 0, 2, 0)]
        public void GetNextNeighbourWithDirection_WithNeighbour_ReturnsNextNeighbour(int ax, int ay, int bx, int by)
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);
            var neighbour = hexMap.GetTile(ax, ay);
            var expected = hexMap.GetTile(bx, by);

            var result = hexMap.GetNextNeighbourWithDirection(tile, neighbour);

            Assert.AreSame(tile, result.HexTile);
            Assert.AreSame(expected, result.Neighbour);
        }

        [Test]
        [TestCase(2, 0, 1, 0)]
        [TestCase(1, 0, 0, 1)]
        [TestCase(0, 1, 1, 2)]
        [TestCase(1, 2, 2, 2)]
        [TestCase(2, 2, 2, 1)]
        [TestCase(2, 1, 2, 0)]
        public void GetNextNeighbourWithDirection_WithNeighbourAndReverseOrder_ReturnsNextNeighbour(int ax, int ay, int bx, int by)
        {
            var hexMap = HexMapBuilder.New.WithHeight(3).WithWidth(3).Build();
            var tile = hexMap.GetTile(1, 1);
            var neighbour = hexMap.GetTile(ax, ay);
            var expected = hexMap.GetTile(bx, by);

            var result = hexMap.GetNextNeighbourWithDirection(tile, neighbour, true);

            Assert.AreSame(tile, result.HexTile);
            Assert.AreSame(expected, result.Neighbour);
        }
    }
}