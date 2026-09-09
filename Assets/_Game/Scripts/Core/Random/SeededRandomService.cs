using System;
using System.Collections.Generic;

namespace LastElevator.Core.Random
{
    public sealed class SeededRandomService : IRandomService
    {
        private const float LargestValueBelowOne = 0.99999994f;

        private readonly System.Random _random;

        public SeededRandomService(int seed)
        {
            _random = new System.Random(seed);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        public float Value()
        {
            return Math.Min((float)_random.NextDouble(), LargestValueBelowOne);
        }

        public T PickWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (getWeight == null)
            {
                throw new ArgumentNullException(nameof(getWeight));
            }

            if (items.Count == 0)
            {
                throw new ArgumentException("At least one weighted item is required.", nameof(items));
            }

            long totalWeight = 0;

            for (int i = 0; i < items.Count; i++)
            {
                int weight = getWeight(items[i]);

                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(getWeight),
                        weight,
                        "Item weights cannot be negative.");
                }

                totalWeight += weight;
            }

            if (totalWeight == 0)
            {
                throw new InvalidOperationException("At least one item must have a positive weight.");
            }

            long selectedWeight = (long)(_random.NextDouble() * totalWeight);
            long cumulativeWeight = 0;

            for (int i = 0; i < items.Count; i++)
            {
                cumulativeWeight += getWeight(items[i]);

                if (selectedWeight < cumulativeWeight)
                {
                    return items[i];
                }
            }

            throw new InvalidOperationException("Unable to select an item from the supplied weights.");
        }
    }
}
