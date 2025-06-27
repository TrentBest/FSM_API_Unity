using NUnit.Framework;

using TheSingularityWorkshop.Squirrel;

using System.Linq;

using UnityEngine;

public class RNGTests
{
    [Test]
    public void Hash_Deterministic()
    {
        int a = RNG.Hash(42, 123);
        int b = RNG.Hash(42, 123);
        int c = RNG.Hash(43, 123);
        int d = RNG.Hash(42, 124);

        Assert.AreEqual(a, b, "Hash should be deterministic with same inputs.");
        Assert.AreNotEqual(a, c, "Hash should change with different position.");
        Assert.AreNotEqual(a, d, "Hash should change with different seed.");
    }

    [Test]
    public void UHash_Deterministic()
    {
        uint a = RNG.UHash(42u, 123u);
        uint b = RNG.UHash(42u, 123u);
        uint c = RNG.UHash(43u, 123u);
        uint d = RNG.UHash(42u, 124u);

        Assert.AreEqual(a, b, "UHash should be deterministic with same inputs.");
        Assert.AreNotEqual(a, c, "UHash should change with different position.");
        Assert.AreNotEqual(a, d, "UHash should change with different seed.");
    }

    [Test]
    public void RangeInt_WithinBounds()
    {
        for (int i = 0; i < 1000; i++)
        {
            int value = RNG.RangeInt(i, 10, 20, 123);
            Assert.IsTrue(value >= 10 && value <= 20, $"Value {value} out of bounds!");
        }
    }

    [Test]
    public void RangeInt_DistributionCheck()
    {
        const int min = 0, max = 4;
        const int sampleCount = 10000;
        int[] bins = new int[max - min + 1];

        for (int i = 0; i < sampleCount; i++)
        {
            int val = RNG.RangeInt(i, min, max, 99);
            bins[val - min]++;
        }

        // Each bin should have at least 80% of the expected avg
        int expected = sampleCount / bins.Length;
        for (int i = 0; i < bins.Length; i++)
        {
            Assert.IsTrue(bins[i] > expected * 0.8, $"Bin {i} too low: {bins[i]}");
        }
    }

    [Test]
    public void SelectIndexByProbability_ReturnsValidIndex()
    {
        float[] weights = { 0.1f, 0.2f, 0.3f, 0.4f };
        for (int i = 0; i < 1000; i++)
        {
            int index = RNG.SelectIndexByProbability(weights, i, 7);
            Assert.IsTrue(index >= 0 && index < weights.Length, $"Index out of bounds: {index}");
        }
    }

    [Test]
    public void HashFromVec3_IsDeterministic()
    {
        Vector3 v = new Vector3(1f, 2f, 3f);
        int h1 = RNG.HashFromVec3(v);
        int h2 = RNG.HashFromVec3(v);
        Assert.AreEqual(h1, h2);
    }

    [Test]
    public void Seed_Int_IsDeterministic()
    {
        int input = 12345;
        int first = RNG.Seed(input);
        int second = RNG.Seed(input);
        Assert.AreEqual(first, second, "Seed(int) should be deterministic.");
    }

    [Test]
    public void Seed_UInt_IsDeterministic()
    {
        uint input = 67890;
        uint first = RNG.Seed(input);
        uint second = RNG.Seed(input);
        Assert.AreEqual(first, second, "Seed(uint) should be deterministic.");
    }

    [Test]
    public void Seed_Int_IsDifferentFromInput()
    {
        int input = 12345;
        int output = RNG.Seed(input);
        Assert.AreNotEqual(input, output, "Seed(int) should not return the input value unchanged.");
    }

    [Test]
    public void Seed_UInt_IsDifferentFromInput()
    {
        uint input = 67890;
        uint output = RNG.Seed(input);
        Assert.AreNotEqual(input, output, "Seed(uint) should not return the input value unchanged.");
    }

    [Test]
    public void Seed_Int_And_Seed_UInt_DoNotCollide()
    {
        int intInput = 123456789;
        uint uintInput = (uint)intInput;
        int intSeed = RNG.Seed(intInput);
        uint uintSeed = RNG.Seed(uintInput);
        Assert.AreNotEqual(intSeed, (int)uintSeed, "Seed(int) and Seed(uint) for the same value should not collide.");
    }

    [Test]
    public void PrintSomeSeeds()
    {
        for (int i = 0; i < 100; i++)
        {
            int seed = RNG.Seed(i);
            TestContext.WriteLine($"Seed({i}) = {seed}");
        }
    }

    [Test]
    public void RangeInt_Should_Have_Uniform_Stats()
    {
        int sampleCount = 100000;
        int min = -1000;
        int max = 1000;

        var values = new int[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            values[i] = RNG.RangeInt(i, min, max);
        }

        float mean = (float)values.Average();
        float variance = values.Select(v => (v - mean) * (v - mean)).Sum() / sampleCount;
        float stdDev = Mathf.Sqrt(variance);

        int realMin = values.Min();
        int realMax = values.Max();
        int unique = values.Distinct().Count();

        int zeroCrosses = values.Count(v => v == 0);

        Debug.Log($"Sample Count: {sampleCount}");
        Debug.Log($"Range: [{min}, {max}]");
        Debug.Log($"Observed Min: {realMin}, Max: {realMax}");
        Debug.Log($"Mean: {mean:F3}, StdDev: {stdDev:F3}");
        Debug.Log($"Unique Values: {unique}");
        Debug.Log($"Zeroes: {zeroCrosses}");
    }
}
