using System;
using NUnit.Framework;
using TheSingularityWorkshop.Squirrel;
using UnityEngine;

namespace TheSingularityWorkshop.FSM.Tests
{
    [TestFixture]
    public class RNGTests
    {
        [Test]
        public void Hash_IsDeterministic_AndDifferentForDifferentInputs()
        {
            int a = RNG.Hash(42, 123);
            int b = RNG.Hash(42, 123);
            int c = RNG.Hash(43, 123);
            int d = RNG.Hash(42, 124);

            Assert.AreEqual(a, b, "Hash should be deterministic for same input/seed");
            Assert.AreNotEqual(a, c, "Hash should differ for different input");
            Assert.AreNotEqual(a, d, "Hash should differ for different seed");
        }

        [Test]
        public void UHash_IsDeterministic_AndDifferentForDifferentInputs()
        {
            uint a = RNG.UHash(42, 123);
            uint b = RNG.UHash(42, 123);
            uint c = RNG.UHash(43, 123);
            uint d = RNG.UHash(42, 124);

            Assert.AreEqual(a, b, "UHash should be deterministic for same input/seed");
            Assert.AreNotEqual(a, c, "UHash should differ for different input");
            Assert.AreNotEqual(a, d, "UHash should differ for different seed");
        }

        [Test]
        public void Range_ReturnsWithinBounds_AndIsDeterministic()
        {
            float min = 5.0f, max = 10.0f;
            float a = RNG.Range(100, min, max, 42);
            float b = RNG.Range(100, min, max, 42);
            Assert.GreaterOrEqual(a, min);
            Assert.LessOrEqual(a, max);
            Assert.AreEqual(a, b, 1e-6f, "Range should be deterministic");
        }

        [Test]
        public void Value01_ReturnsBetweenZeroAndOne()
        {
            float v = RNG.Value01(12345, 6789);
            Assert.GreaterOrEqual(v, 0.0f);
            Assert.LessOrEqual(v, 1.0f);
        }

        [Test]
        public void RangeInt_ReturnsWithinBounds_AndIsDeterministic()
        {
            int min = 1, max = 5;
            int a = RNG.RangeInt(100, min, max, 42);
            int b = RNG.RangeInt(100, min, max, 42);
            Assert.GreaterOrEqual(a, min);
            Assert.LessOrEqual(a, max);
            Assert.AreEqual(a, b, "RangeInt should be deterministic");
        }

        [Test]
        public void HashFromVec3_IsDeterministic_AndSensitiveToInput()
        {
            Vector3 v1 = new Vector3(1, 2, 3);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(3, 2, 1);

            int h1 = RNG.HashFromVec3(v1);
            int h2 = RNG.HashFromVec3(v2);
            int h3 = RNG.HashFromVec3(v3);

            Assert.AreEqual(h1, h2, "HashFromVec3 should be deterministic");
            Assert.AreNotEqual(h1, h3, "HashFromVec3 should be sensitive to input");
        }

        [Test]
        public void Seed_Uint_And_Int_AreDeterministic()
        {
            uint su1 = RNG.Seed(123u);
            uint su2 = RNG.Seed(123u);
            Assert.AreEqual(su1, su2);

            int si1 = RNG.Seed(123);
            int si2 = RNG.Seed(123);
            Assert.AreEqual(si1, si2);
        }

        [Test]
        public void Seed_String_IsDeterministic_AndHandlesNullOrEmpty()
        {
            int s1 = RNG.Seed("hello");
            int s2 = RNG.Seed("hello");
            int s3 = RNG.Seed("");
            int s4 = RNG.Seed((string)null);

            Assert.AreEqual(s1, s2, "Seed(string) should be deterministic");
            Assert.AreEqual(0, s3, "Seed(\"\") should return 0");
            Assert.AreEqual(0, s4, "Seed(null) should return 0");
        }

        [Test]
        public void Seed_ParamsObject_IsDeterministic_AndHandlesNulls()
        {
            int s1 = RNG.Seed(1, "a", null);
            int s2 = RNG.Seed(1, "a", null);
            int s3 = RNG.Seed(1, "b", null);

            Assert.AreEqual(s1, s2, "Seed(params) should be deterministic");
            Assert.AreNotEqual(s1, s3, "Seed(params) should be sensitive to input");
        }

        [Test]
        public void SelectIndexByProbability_ReturnsValidIndex_AndHandlesEdgeCases()
        {
            float[] weights = { 0.1f, 0.2f, 0.7f };
            int idx = RNG.SelectIndexByProbability(weights, 42, 7);
            Assert.True(idx > 0);
            Assert.Less(idx, weights.Length);

            float[] zeroWeights = { 0f, 0f, 0f };
            int idx2 = RNG.SelectIndexByProbability(zeroWeights, 42, 7);
            Assert.AreEqual(-1, idx2, "Should return -1 if total weights are non-positive");
        }

        [Test]
        public void WithinCircle_ReturnsPointWithinRadius_AndIsDeterministic()
        {
            float radius = 5f;
            Vector3 v1 = RNG.WithinCircle(123, radius, 42);
            Vector3 v2 = RNG.WithinCircle(123, radius, 42);
            Assert.AreEqual(v1, v2, "WithinCircle should be deterministic");
            Assert.AreEqual(0f, v1.y, 1e-6f, "Y should always be 0");
            Assert.LessOrEqual(new Vector2(v1.x, v1.z).magnitude, radius + 1e-5f, "Point should be within radius");
        }
    }
}