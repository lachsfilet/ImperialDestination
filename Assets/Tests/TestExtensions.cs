using Assets.Contracts.Utilities;
using NUnit.Framework;
using System.Linq;

namespace Tests
{
    public class TestExtensions
    {
        // A Test behaves as an ordinary method
        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(8, 1)]
        [TestCase(1887, 4)]
        [TestCase(12345, 5)]
        [TestCase(-10, 2)]
        public void TestCountDigits(int n, int expected)
        {
            var result = n.CountDigits();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TestShuffle()
        {
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var result = numbers.Shuffle().ToList();

            foreach (var number in numbers)
                Assert.Contains(number, result);

            // Shuffle does not mean, that every number has to be in another position
            //Debug.Log(string.Join(", ", numbers.Select(n => n)));
            //Debug.Log(string.Join(", ", result.Select(n => n)));
            //for (var i = 0; i < numbers.Length; i++)
            //    Assert.AreNotEqual(numbers[i], result[i]);
        }

        [Test]
        public void TestAll()
        {
            Assert.IsTrue(new[] { false, false }.All(f => !f));
            Assert.IsTrue(Enumerable.Empty<bool>().All(f => !f));
        }
    }
}