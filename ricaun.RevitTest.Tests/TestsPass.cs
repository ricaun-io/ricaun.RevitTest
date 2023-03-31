﻿using NUnit.Framework;
using System.Threading;

namespace ricaun.RevitTest.Tests
{
    public class TestsPass
    {
        [Explicit]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void TestsPass1(int i)
        {
            Thread.Sleep(10 * i);
            Assert.Pass($"{i}");
        }
    }
}