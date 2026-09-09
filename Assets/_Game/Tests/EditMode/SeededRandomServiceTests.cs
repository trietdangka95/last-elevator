using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using NUnit.Framework;

namespace LastElevator.Tests.EditMode
{
    public sealed class SeededRandomServiceTests
    {
        [Test]
        public void SameSeedProducesSameSequence()
        {
            IRandomService first = new SeededRandomService(18492301);
            IRandomService second = new SeededRandomService(18492301);

            for (int i = 0; i < 20; i++)
            {
                Assert.That(first.Range(-10, 50), Is.EqualTo(second.Range(-10, 50)));
                Assert.That(first.Value(), Is.EqualTo(second.Value()));
            }
        }

        [Test]
        public void RangeUsesInclusiveMinimumAndExclusiveMaximum()
        {
            IRandomService random = new SeededRandomService(12);

            for (int i = 0; i < 100; i++)
            {
                int value = random.Range(3, 7);

                Assert.That(value, Is.GreaterThanOrEqualTo(3));
                Assert.That(value, Is.LessThan(7));
            }
        }

        [Test]
        public void ValueStaysWithinUnitInterval()
        {
            IRandomService random = new SeededRandomService(34);

            for (int i = 0; i < 100; i++)
            {
                float value = random.Value();

                Assert.That(value, Is.GreaterThanOrEqualTo(0f));
                Assert.That(value, Is.LessThan(1f));
            }
        }

        [Test]
        public void PickWeightedUsesOnlyPositiveWeightItems()
        {
            IRandomService random = new SeededRandomService(56);
            IReadOnlyList<WeightedItem> items = new[]
            {
                new WeightedItem("never", 0),
                new WeightedItem("always", 10)
            };

            for (int i = 0; i < 20; i++)
            {
                WeightedItem selected = random.PickWeighted(items, item => item.Weight);

                Assert.That(selected.Id, Is.EqualTo("always"));
            }
        }

        [Test]
        public void PickWeightedIsDeterministicForTheSameSeed()
        {
            IRandomService first = new SeededRandomService(78);
            IRandomService second = new SeededRandomService(78);
            IReadOnlyList<WeightedItem> items = new[]
            {
                new WeightedItem("low", 1),
                new WeightedItem("medium", 3),
                new WeightedItem("high", 6)
            };

            for (int i = 0; i < 20; i++)
            {
                WeightedItem firstSelection = first.PickWeighted(items, item => item.Weight);
                WeightedItem secondSelection = second.PickWeighted(items, item => item.Weight);

                Assert.That(firstSelection.Id, Is.EqualTo(secondSelection.Id));
            }
        }

        [Test]
        public void PickWeightedRejectsAnEmptyCollection()
        {
            IRandomService random = new SeededRandomService(90);

            Assert.Throws<ArgumentException>(() =>
                random.PickWeighted(Array.Empty<WeightedItem>(), item => item.Weight));
        }

        [Test]
        public void PickWeightedRejectsNegativeWeights()
        {
            IRandomService random = new SeededRandomService(91);
            IReadOnlyList<WeightedItem> items = new[]
            {
                new WeightedItem("invalid", -1),
                new WeightedItem("valid", 1)
            };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                random.PickWeighted(items, item => item.Weight));
        }

        [Test]
        public void PickWeightedRejectsAllZeroWeights()
        {
            IRandomService random = new SeededRandomService(92);
            IReadOnlyList<WeightedItem> items = new[]
            {
                new WeightedItem("zero-a", 0),
                new WeightedItem("zero-b", 0)
            };

            Assert.Throws<InvalidOperationException>(() =>
                random.PickWeighted(items, item => item.Weight));
        }

        private sealed class WeightedItem
        {
            public WeightedItem(string id, int weight)
            {
                Id = id;
                Weight = weight;
            }

            public string Id { get; }

            public int Weight { get; }
        }
    }
}
