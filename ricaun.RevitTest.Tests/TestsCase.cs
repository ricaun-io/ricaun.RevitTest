﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NUnit.Framework;
using System;

namespace ricaun.RevitTest.Tests
{
    public class TestsCase
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void TestStringCase(string value)
        {
            Assert.IsTrue(string.IsNullOrWhiteSpace(value));
        }

        [TestCase("a")]
        [TestCase("1")]
        [TestCase("_")]
        [TestCase("/")]
        [TestCase("\\")]
        [TestCase("\"")]
        //[TestCase("\n")]
        [TestCase(".")]
        [TestCase(",")]
        public void TestStringCase_IsNot(string value)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(value));
        }
    }
}