using System;
using System.Collections.Generic;

namespace ProjectBase.Common.Utils
{
    public static class MathUtils
    {
        private static readonly Random _rng = new();

        public static int WeightedRandom(IList<float> weights)
        {
            return WeightedRandom(weights, _rng);
        }

        public static int WeightedRandom(IList<float> weights, Random rng)
        {
            if (weights == null || weights.Count == 0)
                throw new ArgumentException("Weights list cannot be empty.");

            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
                total += weights[i];

            float roll = (float)(rng.NextDouble() * total);
            float cumulative = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return i;
            }
            return weights.Count - 1;
        }

        public static int WeightedRandom(IList<int> weights)
        {
            return WeightedRandom(weights, _rng);
        }

        public static int WeightedRandom(IList<int> weights, Random rng)
        {
            if (weights == null || weights.Count == 0)
                throw new ArgumentException("Weights list cannot be empty.");

            int total = 0;
            for (int i = 0; i < weights.Count; i++)
                total += weights[i];

            int roll = rng.Next(total);
            int cumulative = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return i;
            }
            return weights.Count - 1;
        }

        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = (value - fromMin) / (fromMax - fromMin);
            return toMin + t * (toMax - toMin);
        }

        public static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }

        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
