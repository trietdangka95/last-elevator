using System;
using LastElevator.Gameplay.Encounters;

namespace LastElevator.Data.Definitions
{
    [Serializable]
    public sealed class EncounterCategoryWeights
    {
        public int survivor;
        public int resource;
        public int enemy;
        public int hazard;
        public int mystery;

        public EncounterCategoryWeights()
        {
        }

        public EncounterCategoryWeights(int survivor, int resource, int enemy, int hazard, int mystery)
        {
            this.survivor = survivor;
            this.resource = resource;
            this.enemy = enemy;
            this.hazard = hazard;
            this.mystery = mystery;
        }

        internal int GetWeight(EncounterCategory category)
        {
            switch (category)
            {
                case EncounterCategory.Survivor:
                    return survivor;
                case EncounterCategory.Resource:
                    return resource;
                case EncounterCategory.Enemy:
                    return enemy;
                case EncounterCategory.Hazard:
                    return hazard;
                case EncounterCategory.Mystery:
                    return mystery;
                default:
                    return 0;
            }
        }

        internal void Validate()
        {
            ValidateNonNegative(survivor, nameof(survivor));
            ValidateNonNegative(resource, nameof(resource));
            ValidateNonNegative(enemy, nameof(enemy));
            ValidateNonNegative(hazard, nameof(hazard));
            ValidateNonNegative(mystery, nameof(mystery));

            if ((long)survivor + resource + enemy + hazard + mystery == 0)
            {
                throw new InvalidOperationException("At least one encounter category must have a positive weight.");
            }
        }

        private static void ValidateNonNegative(int weight, string fieldName)
        {
            if (weight < 0)
            {
                throw new ArgumentOutOfRangeException(fieldName, weight, "Encounter weights cannot be negative.");
            }
        }
    }
}
