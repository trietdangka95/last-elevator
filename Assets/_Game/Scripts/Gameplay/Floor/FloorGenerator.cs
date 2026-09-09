using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Data.Definitions;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Run;

namespace LastElevator.Gameplay.Floor
{
    public sealed class FloorGenerator
    {
        private const int FirstFloor = 1;
        private const int CheckpointAFloor = 10;
        private const int CheckpointBFloor = 20;
        private const int BossFloor = 30;
        private const int MaximumCandidateDistance = 3;

        private readonly FloorGenerationConfig _config;
        private readonly IReadOnlyList<EncounterDefinition> _encounters;
        private readonly IRandomService _random;
        private readonly IReadOnlyList<EncounterCategory> _regularCategories;

        public FloorGenerator(IRandomService random, FloorGenerationConfig config)
            : this(random, config, null)
        {
        }

        public FloorGenerator(
            IRandomService random,
            FloorGenerationConfig config,
            IReadOnlyList<EncounterDefinition> encounters)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _encounters = encounters;
            _regularCategories = new[]
            {
                EncounterCategory.Survivor,
                EncounterCategory.Resource,
                EncounterCategory.Enemy,
                EncounterCategory.Hazard,
                EncounterCategory.Mystery
            };
        }

        public IReadOnlyList<FloorCandidate> GenerateCandidates(RunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.currentFloor < 0 || state.currentFloor > BossFloor)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state.currentFloor,
                    $"Current floor must be between 0 and {BossFloor}.");
            }

            if (state.currentFloor == BossFloor)
            {
                return Array.Empty<FloorCandidate>();
            }

            int lastCandidateFloor = Math.Min(
                state.currentFloor + MaximumCandidateDistance,
                GetNextMandatoryFloor(state.currentFloor));
            var candidates = new List<FloorCandidate>(MaximumCandidateDistance);
            var unusedCategories = new List<EncounterCategory>(_regularCategories);
            var unusedEncounters = _encounters == null
                ? null
                : new List<EncounterDefinition>(_encounters);

            for (int targetFloor = state.currentFloor + 1; targetFloor <= lastCandidateFloor; targetFloor++)
            {
                int distance = targetFloor - state.currentFloor;

                if (!RunRules.CanReachFloor(state, distance))
                {
                    break;
                }

                int energyCost = RunRules.GetTravelEnergyCost(state, distance);
                FloorBand band = GetBand(targetFloor);
                EncounterCategory category = GetSignalCategory(band, unusedCategories);
                EncounterDefinition encounter = GetEncounter(category, targetFloor, unusedEncounters);

                candidates.Add(new FloorCandidate(
                    targetFloor,
                    distance,
                    energyCost,
                    band,
                    category,
                    encounter));
            }

            return candidates.AsReadOnly();
        }

        private EncounterDefinition GetEncounter(
            EncounterCategory category,
            int floor,
            List<EncounterDefinition> unusedEncounters)
        {
            if (_encounters == null)
            {
                return null;
            }

            List<EncounterDefinition> eligible = GetEligibleEncounters(
                unusedEncounters,
                floor,
                category,
                true);

            if (eligible.Count == 0)
            {
                eligible = GetEligibleEncounters(unusedEncounters, floor, category, false);
            }

            if (eligible.Count == 0)
            {
                eligible = GetEligibleEncounters(_encounters, floor, category, true);
            }

            if (eligible.Count == 0)
            {
                eligible = GetEligibleEncounters(_encounters, floor, category, false);
            }

            if (eligible.Count == 0)
            {
                throw new InvalidOperationException($"No encounter is available for floor {floor}.");
            }

            EncounterDefinition selected = _random.PickWeighted(eligible, definition => definition.weight);
            unusedEncounters.Remove(selected);
            return selected;
        }

        private static List<EncounterDefinition> GetEligibleEncounters(
            IReadOnlyList<EncounterDefinition> encounters,
            int floor,
            EncounterCategory category,
            bool requireCategoryMatch)
        {
            var eligible = new List<EncounterDefinition>();

            if (encounters == null)
            {
                return eligible;
            }

            for (int i = 0; i < encounters.Count; i++)
            {
                EncounterDefinition encounter = encounters[i];

                if (encounter == null || encounter.weight <= 0 ||
                    floor < encounter.minFloor || floor > encounter.maxFloor)
                {
                    continue;
                }

                if (!requireCategoryMatch || encounter.category == category)
                {
                    eligible.Add(encounter);
                }
            }

            return eligible;
        }

        public static FloorBand GetBand(int floor)
        {
            if (floor < FirstFloor || floor > BossFloor)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(floor),
                    floor,
                    $"Floor must be between {FirstFloor} and {BossFloor}.");
            }

            if (floor < CheckpointAFloor)
            {
                return FloorBand.Early;
            }

            if (floor == CheckpointAFloor)
            {
                return FloorBand.CheckpointA;
            }

            if (floor < CheckpointBFloor)
            {
                return FloorBand.Mid;
            }

            if (floor == CheckpointBFloor)
            {
                return FloorBand.CheckpointB;
            }

            if (floor < BossFloor)
            {
                return FloorBand.Late;
            }

            return FloorBand.Boss;
        }

        private EncounterCategory GetSignalCategory(
            FloorBand band,
            List<EncounterCategory> unusedCategories)
        {
            if (band == FloorBand.CheckpointA || band == FloorBand.CheckpointB)
            {
                return EncounterCategory.Checkpoint;
            }

            if (band == FloorBand.Boss)
            {
                return EncounterCategory.Boss;
            }

            EncounterCategoryWeights weights = _config.GetWeights(band);
            List<EncounterCategory> eligibleCategories = GetEligibleCategories(unusedCategories, weights);

            if (eligibleCategories.Count == 0)
            {
                eligibleCategories = GetEligibleCategories(_regularCategories, weights);
            }

            EncounterCategory selected = _random.PickWeighted(
                eligibleCategories,
                weights.GetWeight);
            unusedCategories.Remove(selected);
            return selected;
        }

        private static List<EncounterCategory> GetEligibleCategories(
            IReadOnlyList<EncounterCategory> categories,
            EncounterCategoryWeights weights)
        {
            var eligible = new List<EncounterCategory>(categories.Count);

            for (int i = 0; i < categories.Count; i++)
            {
                EncounterCategory category = categories[i];

                if (weights.GetWeight(category) > 0)
                {
                    eligible.Add(category);
                }
            }

            return eligible;
        }

        private static int GetNextMandatoryFloor(int currentFloor)
        {
            if (currentFloor < CheckpointAFloor)
            {
                return CheckpointAFloor;
            }

            if (currentFloor < CheckpointBFloor)
            {
                return CheckpointBFloor;
            }

            return BossFloor;
        }
    }
}
