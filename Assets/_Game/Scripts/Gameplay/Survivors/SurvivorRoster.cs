using System;
using System.Collections.Generic;
using LastElevator.Core.State;
using LastElevator.Gameplay.Run;

namespace LastElevator.Gameplay.Survivors
{
    public sealed class SurvivorRoster
    {
        private const int CheckpointRepairAmount = 5;

        private readonly Dictionary<string, SurvivorDefinition> _definitions;

        public SurvivorRoster(IReadOnlyList<SurvivorDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _definitions = new Dictionary<string, SurvivorDefinition>(StringComparer.Ordinal);

            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorDefinition definition = definitions[i];

                if (definition == null || string.IsNullOrWhiteSpace(definition.id))
                {
                    throw new ArgumentException("Survivor definitions require a non-empty id.", nameof(definitions));
                }

                if (!_definitions.TryAdd(definition.id, definition))
                {
                    throw new ArgumentException(
                        $"Duplicate survivor id '{definition.id}'.",
                        nameof(definitions));
                }
            }
        }

        public SurvivorDefinition GetDefinition(string survivorId)
        {
            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return null;
            }

            _definitions.TryGetValue(survivorId, out SurvivorDefinition definition);
            return definition;
        }

        public IReadOnlyList<SurvivorDefinition> GetSurvivors(RunState state)
        {
            ValidateState(state);
            var survivors = new List<SurvivorDefinition>(state.survivorIds.Count);

            for (int i = 0; i < state.survivorIds.Count; i++)
            {
                SurvivorDefinition definition = GetRequiredDefinition(state.survivorIds[i]);
                survivors.Add(definition);
            }

            return survivors.AsReadOnly();
        }

        public SurvivorRecruitmentStatus TryRecruit(RunState state, SurvivorDefinition survivor)
        {
            ValidateState(state);
            SurvivorDefinition registered = GetRegisteredDefinition(survivor);

            if (registered == null)
            {
                return SurvivorRecruitmentStatus.InvalidSurvivor;
            }

            if (state.survivorIds.Contains(registered.id))
            {
                return SurvivorRecruitmentStatus.AlreadyInRoster;
            }

            if (RunRules.GetAvailableCapacity(state) <= 0)
            {
                return SurvivorRecruitmentStatus.ReplacementRequired;
            }

            state.survivorIds.Add(registered.id);
            return SurvivorRecruitmentStatus.Added;
        }

        public bool TryReplace(
            RunState state,
            string survivorIdToRemove,
            SurvivorDefinition survivorToAdd)
        {
            ValidateState(state);
            SurvivorDefinition registered = GetRegisteredDefinition(survivorToAdd);

            if (registered == null || string.IsNullOrWhiteSpace(survivorIdToRemove) ||
                state.capacity <= 0 || state.survivorIds.Count != state.capacity ||
                state.survivorIds.Contains(registered.id))
            {
                return false;
            }

            int replacementIndex = state.survivorIds.IndexOf(survivorIdToRemove);

            if (replacementIndex < 0)
            {
                return false;
            }

            state.survivorIds[replacementIndex] = registered.id;
            return true;
        }

        public int GetTotalPower(RunState state)
        {
            IReadOnlyList<SurvivorDefinition> survivors = GetSurvivors(state);
            int totalPower = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorDefinition survivor = survivors[i];
                totalPower += Math.Max(0, survivor.combatPower);

                if (survivor.passive == SimplePassive.AddCombatPowerOne)
                {
                    totalPower++;
                }
            }

            return totalPower;
        }

        public int GetTravelEnergyReduction(RunState state)
        {
            return CountPassive(state, SimplePassive.ReduceTravelEnergyOne);
        }

        public int GetCheckpointRepair(RunState state)
        {
            return CountPassive(state, SimplePassive.RepairOnCheckpointFive) * CheckpointRepairAmount;
        }

        public bool CanRevealDanger(RunState state)
        {
            return CountPassive(state, SimplePassive.RevealDangerSometimes) > 0;
        }

        public bool HasRole(RunState state, SurvivorRole role)
        {
            IReadOnlyList<SurvivorDefinition> survivors = GetSurvivors(state);

            for (int i = 0; i < survivors.Count; i++)
            {
                if (survivors[i].role == role)
                {
                    return true;
                }
            }

            return false;
        }

        private int CountPassive(RunState state, SimplePassive passive)
        {
            IReadOnlyList<SurvivorDefinition> survivors = GetSurvivors(state);
            int count = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                if (survivors[i].passive == passive)
                {
                    count++;
                }
            }

            return count;
        }

        private SurvivorDefinition GetRegisteredDefinition(SurvivorDefinition definition)
        {
            return definition == null ? null : GetDefinition(definition.id);
        }

        private SurvivorDefinition GetRequiredDefinition(string survivorId)
        {
            SurvivorDefinition definition = GetDefinition(survivorId);

            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"No SurvivorDefinition is registered for id '{survivorId}'.");
            }

            return definition;
        }

        private static void ValidateState(RunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.survivorIds == null)
            {
                state.survivorIds = new List<string>();
            }
        }
    }
}
