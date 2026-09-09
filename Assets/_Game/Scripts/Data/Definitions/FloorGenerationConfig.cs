using System;
using LastElevator.Gameplay.Floor;

namespace LastElevator.Data.Definitions
{
    [Serializable]
    public sealed class FloorGenerationConfig
    {
        public EncounterCategoryWeights early = new EncounterCategoryWeights(30, 30, 15, 10, 15);
        public EncounterCategoryWeights mid = new EncounterCategoryWeights(20, 20, 30, 15, 15);
        public EncounterCategoryWeights late = new EncounterCategoryWeights(15, 15, 40, 20, 10);

        internal EncounterCategoryWeights GetWeights(FloorBand band)
        {
            switch (band)
            {
                case FloorBand.Early:
                    return RequireWeights(early, nameof(early));
                case FloorBand.Mid:
                    return RequireWeights(mid, nameof(mid));
                case FloorBand.Late:
                    return RequireWeights(late, nameof(late));
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(band),
                        band,
                        "Mandatory floors do not use regular encounter weights.");
            }
        }

        private static EncounterCategoryWeights RequireWeights(
            EncounterCategoryWeights weights,
            string fieldName)
        {
            if (weights == null)
            {
                throw new InvalidOperationException($"Floor generation weights '{fieldName}' are missing.");
            }

            weights.Validate();
            return weights;
        }
    }
}
