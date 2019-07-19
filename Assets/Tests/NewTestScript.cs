using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class NewTestScript
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

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
