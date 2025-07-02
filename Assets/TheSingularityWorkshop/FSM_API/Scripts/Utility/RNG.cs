using System;
using System.Linq;

using UnityEngine;


/// <summary>
///     Provides a stateless, deterministic pseudo-random number generation (PRNG)
///     and hashing utility based on the principles of Squirrel Eiserloh's "Squirrel Noise" functions.
///     <br/><br/>
///     **Original Inspiration & Genius:**
///     This class is heavily inspired by and adapted from the highly influential work of
///     **Squirrel Eiserloh**, particularly his presentations at the Game Developers Conference (GDC)
///     on "Noise-Based RNG" and "Fast and Funky 1D Nonlinear Transformations."
///     His elegant bit-mangling techniques provide a remarkably fast, simple, and deterministic
///     method for generating pseudo-random values based on an integer position and an optional seed,
///     making them ideal for procedural content generation where reproducibility is key.
///     We owe a great deal of thanks to Squirrel Eiserloh for sharing his innovative approaches
///     to noise and randomness in games.
///     <br/><br/>
///     **Our Adaptation and Usage:**
///     We've specifically adapted his core hash function to suit our needs for a lightweight,
///     performant, and deterministic RNG. The aim is to have a "lookup table" of random numbers
///     that are fixed for a given position and seed, ensuring content remains consistent
///     across sessions and for networked play (if applicable).
///     This is a simplified version of a broader utility, focusing on the core hashing and
///     a few common utility functions like `RangeInt` and `WithinCircle`.
///     We have intentionally omitted or commented out more complex vector-based functions
///     to keep this core API as tiny and focused as possible for our initial deployment goals.
///     The bit constants used (`UBIT_NOISE1`, `UBIT_NOISE2`, `UBIT_NOISE3`) are directly
///     derived from his published Squirrel Noise algorithm.
/// </summary>
namespace TheSingularityWorkshop.Squirrel
{
    public static class RNG
    {
        // Bit constants from Squirrel3 (or SquirrelNoise5, as typically used in his later talks)
        // These specific values are fundamental to Squirrel Eiserloh's hashing algorithm.
        const uint UBIT_NOISE1 = 715225739;
        const uint UBIT_NOISE2 = 1073676287;
        const uint UBIT_NOISE3 = 1431655765;

        // Signed versions for compatibility where int is used.
        const int BIT_NOISE1 = (int)UBIT_NOISE1;
        const int BIT_NOISE2 = (int)UBIT_NOISE2;
        const int BIT_NOISE3 = (int)UBIT_NOISE3;

        /// <summary>
        /// Computes a deterministic 32-bit signed integer hash from an integer position and a seed.
        /// This is the core 1D hash function derived from Squirrel Eiserloh's work.
        /// </summary>
        /// <param name="pos">The input position/index for which to generate a hash.</param>
        /// <param name="seed">An optional seed to alter the hash for the same position, allowing different "random universes."</param>
        /// <returns>A deterministically hashed 32-bit signed integer.</returns>
        public static int Hash(int pos, int seed = 0)
        {
            int mangled = pos;
            mangled *= BIT_NOISE1;
            mangled += seed;
            mangled ^= (mangled >> 8);
            mangled += BIT_NOISE2;
            mangled ^= (mangled << 8);
            mangled *= BIT_NOISE3;
            mangled ^= (mangled >> 8);
            return mangled;
        }

        /// <summary>
        /// Computes a deterministic 32-bit unsigned integer hash from an unsigned integer position and a seed.
        /// This is the unsigned version of the core 1D hash function, mirroring Squirrel Eiserloh's original approach.
        /// </summary>
        /// <param name="pos">The unsigned input position/index for which to generate a hash.</param>
        /// <param name="seed">An optional unsigned seed.</param>
        /// <returns>A deterministically hashed 32-bit unsigned integer.</returns>
        public static uint UHash(uint pos, uint seed = 0)
        {
            uint mangled = pos;
            mangled *= UBIT_NOISE1;
            mangled += seed;
            mangled ^= (mangled >> 8);
            mangled += UBIT_NOISE2;
            mangled ^= (mangled << 8);
            mangled *= UBIT_NOISE3;
            mangled ^= (mangled >> 8);
            return mangled;
        }

        // Public float Range method for float values (re-enabled as it's useful for 0-1 ranges)
        /// <summary>
        /// Generates a deterministic float value within a specified range [min, max],
        /// based on an integer position and an optional seed.
        /// </summary>
        /// <param name="pos">The input position/index.</param>
        /// <param name="min">The minimum value of the range (inclusive).</param>
        /// <param name="max">The maximum value of the range (inclusive).</param>
        /// <param name="seed">An optional seed.</param>
        /// <returns>A deterministic float value within the [min, max] range.</returns>
        public static float Range(int pos, float min, float max, int seed = 0)
        {
            // Use the Value01 helper to get a normalized float, then lerp.
            // We'll add Value01 back as a public method.
            return Mathf.Lerp(min, max, Value01(pos, seed));
        }

        /// <summary>
        /// Generates a deterministic float value between 0.0 (inclusive) and 1.0 (inclusive),
        /// based on an integer position and an optional seed.
        /// </summary>
        /// <param name="pos">The input position/index.</param>
        /// <param name="seed">An optional seed.</param>
        /// <returns>A deterministic float value between 0.0 and 1.0.</returns>
        public static float Value01(int pos, int seed = 0)
        {
            // Convert the signed integer hash to an unsigned integer, then normalize to [0, 1].
            // Using int.MaxValue for normalization is common, or uint.MaxValue for unsigned.
            // Here, we convert signed int hash to a positive float, then normalize.
            // (float)Mathf.Abs(Hash(pos, seed)) / int.MaxValue is a common way for signed int.
            // Using uint.MaxValue from the unsigned hash provides a slightly better distribution across the full range.
            return (float)UHash((uint)pos, (uint)seed) / uint.MaxValue;
        }

        // Public float Range method for float values
        /// <summary>
        /// Generates a deterministic float value within a specified range [min, max],
        /// based on an integer position and an optional seed.
        /// </summary>
        /// <param name="pos">The input position/index.</param>
        /// <param name="min">The minimum value of the range (inclusive).</param>
        /// <param name="max">The maximum value of the range (inclusive).</param>
        /// <param name="seed">An optional seed.</param>
        /// <returns>A deterministic float value within the [min, max] range.</returns>
        public static int RangeInt(int pos, int min, int max, int seed = 0)
        {
            // Use the Value01 helper to get a normalized float, then lerp.
            // We'll add Value01 back as a public method.
            return min + (Hash(pos, (seed)) % (max - min + 1));
        }



        /// <summary>
        /// Computes a hash from a <see cref="Vector3"/> position.
        /// This allows for seeding random generation based on spatial coordinates.
        /// </summary>
        /// <param name="v">The Vector3 position.</param>
        /// <returns>A hash value derived from the Vector3 components.</returns>
        public static int HashFromVec3(Vector3 v)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + v.x.GetHashCode();
                hash = hash * 31 + v.y.GetHashCode();
                hash = hash * 31 + v.z.GetHashCode();
                return hash;
            }
        }


        /// <summary>
        /// Generates a "master" seed from an initial unsigned seed.
        /// This is useful for creating a primary randomized value from a base seed.
        /// </summary>
        /// <param name="seed">The initial unsigned seed.</param>
        /// <returns>A new, derived unsigned seed.</returns>
        public static uint Seed(uint seed)
        {
            return UHash(int.MaxValue, seed);
        }
        /// <summary>
        /// Generates a "master" seed from an initial signed integer seed.
        /// Incorporates a common hashing salt (0x9E3779B9) for better distribution.
        /// </summary>
        /// <param name="seed">The initial integer seed.</param>
        /// <returns>A new, derived integer seed.</returns>
        public static int Seed(int seed)
        {
            int salt = seed * 31 + unchecked((int)0x9E3779B9);
            return Hash(seed, salt);
            //return Hash(seed*31, seed);
        }

        /// <summary>
        /// Generates a seed from a string, useful for creating a deterministic seed from a text input (e.g., a level name).
        /// </summary>
        /// <param name="seed">The string to use as a seed.</param>
        /// <returns>A derived integer seed.</returns>
        public static int Seed(string seed)
        {
            if (string.IsNullOrEmpty(seed))
                return 0;

            unchecked
            {
                int hash = 17;// A common prime initial hash
                for (int i = 0; i < seed.Length; i++)
                {
                    // Simple hash combining. Multiplying by a prime and adding helps distribute bits.
                    hash = hash * 31 + seed[i];
                }
                return RNG.Seed(hash); // Apply the integer seeding for further scrambling
            }
        }

        /// <summary>
        /// Generates a combined seed from multiple objects, useful for creating a deterministic seed
        /// from a combination of diverse inputs.
        /// </summary>
        /// <param name="args">An array of objects to combine into a seed.</param>
        /// <returns>A derived integer seed.</returns>
        public static int Seed(params object[] args)
        {
            unchecked
            {
                int hash = 17;
                foreach (var arg in args)
                {
                    // Use GetHashCode for each object; nulls map to 0.
                    hash = hash * 31 + (arg?.GetHashCode() ?? 0);
                }
                return RNG.Seed(hash);
            }
        }

        /// <summary>
        /// Selects an index from an array of probabilities (weights) deterministically,
        /// based on an integer position and an optional seed.
        /// </summary>
        /// <param name="weights">An array of float weights representing probabilities for each index.</param>
        /// <param name="pos">The input position/index.</param>
        /// <param name="seed">An optional seed.</param>
        /// <returns>The deterministically selected index, or -1 if total weights are non-positive.</returns>
        public static int SelectIndexByProbability(float[] weights, int pos, int seed = 0)
        {
            float total = weights.Sum();
            if (total <= 0f) return -1;
            // Generate a deterministic "roll" value by hashing and scaling it by the total weights.
            // We need a float value between 0 and total.
            // Hash returns int, Value01 converts that to a float in [0,1].
            float roll = Hash(pos, seed) * total;
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return i;
            }
            return weights.Length - 1; // edge case
        }

        /// <summary>
        /// Generates a deterministic <see cref="Vector3"/> point within a 2D circle (on the XZ plane),
        /// based on an integer position and an optional seed. The Y-component is always 0.
        /// </summary>
        /// <param name="pos">The input position/index.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="seed">An optional seed.</param>
        /// <returns>A deterministic Vector3 point within the specified circle.</returns>
        public static Vector3 WithinCircle(int pos, float radius, int seed = 0)
        {
            // Generate deterministic angle and radius values using your hash function
            int angleHash = Hash(pos, seed);
            int radiusHash = Hash(pos + 1, seed); // use a different position to vary it

            // Normalize hashes to [0, 1)
            float angle01 = Mathf.Abs(angleHash) / (float)int.MaxValue;
            float radius01 = Mathf.Abs(radiusHash) / (float)int.MaxValue;

            // Convert to polar coordinates (sqrt(radius01) ensures even distribution)
            float angleRad = angle01 * Mathf.PI * 2f;
            float distance = Mathf.Sqrt(radius01) * radius;

            float x = Mathf.Cos(angleRad) * distance;
            float z = Mathf.Sin(angleRad) * distance;

            return new Vector3(x, 0f, z);
        }
    }
}
