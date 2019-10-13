using Assets.Contracts.Map;
using Assets.Contracts.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class PositionComparerTests
    {
        [Test]
        public void Compare_WithCoordinates_ComparesPositions()
        {
            var comparer = new PositionComparer(100);
            // 100010
            var a = new Position(10, 100);
            //  10100
            var b = new Position(100, 10);

            var result = comparer.Compare(a, b);

            Assert.AreEqual(1, result);

            result = comparer.Compare(b, a);

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void OrderBy_WithTwoPositions_ComparesPositions()
        {
            var comparer = new PositionComparer(100);
            var list = new List<Position>
            {
                // 100010
                new Position(10, 100),
                // 10100
                new Position(100, 10),
            };

            var result = list.OrderBy(p => p, comparer).ToList();

            Assert.IsTrue(list.Last() == result.First());
            Assert.IsTrue(list.First() == result.Last());
        }

        [Test]
        public void OrderBy_WithFourPositions_ComparesPositions()
        {
            var comparer = new PositionComparer(100);
            var list = new List<Position>
            {
                // 100002
                new Position(2, 100),
                //  10001
                new Position(1, 10),
                // 100001
                new Position(1, 100),
                //  10010
                new Position(10, 10)
            };

            var result = list.OrderBy(p => p, comparer).ToList();

            Assert.IsTrue(list[0] == result[3], $"{list[0]} is not {result[2]}");
            Assert.IsTrue(list[1] == result[0], $"{list[1]} is not {result[0]}");
            Assert.IsTrue(list[2] == result[2], $"{list[2]} is not {result[1]}");
            Assert.IsTrue(list[3] == result[1], $"{list[3]} is not {result[2]}");
        }

        [Test]
        public void OrderBy_WithZeroValuePositions_ComparesPositions()
        {
            var comparer = new PositionComparer(100);
            var list = new List<Position>
            {
                // 0
                new Position(0, 0),
                // 1
                new Position(1, 0),
                // 1000
                new Position(0, 1),
                // 10000
                new Position(0, 10),
                // 10
                new Position(10, 0)
            };

            var result = list.OrderBy(p => p, comparer).ToList();

            Assert.IsTrue(list[0] == result[0], $"{list[0]} is not {result[2]}");
            Assert.IsTrue(list[1] == result[1], $"{list[1]} is not {result[0]}");
            Assert.IsTrue(list[2] == result[3], $"{list[2]} is not {result[1]}");
            Assert.IsTrue(list[3] == result[4], $"{list[3]} is not {result[2]}");
            Assert.IsTrue(list[4] == result[2], $"{list[3]} is not {result[2]}");
        }
    }
}