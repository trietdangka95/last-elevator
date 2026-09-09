using System;
using System.Collections.Generic;

namespace LastElevator.Core.Random
{
    public interface IRandomService
    {
        int Range(int minInclusive, int maxExclusive);

        float Value();

        T PickWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight);
    }
}
