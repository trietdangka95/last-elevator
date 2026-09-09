using System;
using System.Collections.Generic;
using LastElevator.Core.State;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Survivors;

namespace LastElevator.Gameplay.Run
{
    public sealed class RunViewModel
    {
        internal RunViewModel(
            RunState state,
            RunPhase phase,
            int travelTargetFloor,
            IReadOnlyList<FloorCandidate> floorCandidates,
            EncounterViewModel currentEncounter,
            SurvivorRoster roster,
            SurvivorDefinition pendingSurvivor)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            CurrentFloor = state.currentFloor;
            Energy = state.energy;
            MaxEnergy = state.maxEnergy;
            Integrity = state.integrity;
            MaxIntegrity = state.maxIntegrity;
            Capacity = state.capacity;
            SurvivorCount = state.survivorIds == null ? 0 : state.survivorIds.Count;
            Survivors = CreateSurvivorViews(roster.GetSurvivors(state));
            PendingSurvivor = pendingSurvivor == null
                ? null
                : new SurvivorViewModel(pendingSurvivor);
            TeamPower = roster.GetTotalPower(state);
            TravelEnergyReduction = roster.GetTravelEnergyReduction(state);
            Scrap = state.scrap;
            Phase = phase;
            TravelTargetFloor = travelTargetFloor;
            FloorCandidates = floorCandidates ?? Array.Empty<FloorCandidate>();
            CurrentEncounter = currentEncounter;
        }

        public int CurrentFloor { get; }

        public int Energy { get; }

        public int MaxEnergy { get; }

        public int Integrity { get; }

        public int MaxIntegrity { get; }

        public int Capacity { get; }

        public int SurvivorCount { get; }

        public IReadOnlyList<SurvivorViewModel> Survivors { get; }

        public SurvivorViewModel PendingSurvivor { get; }

        public int TeamPower { get; }

        public int TravelEnergyReduction { get; }

        public int Scrap { get; }

        public RunPhase Phase { get; }

        public int TravelTargetFloor { get; }

        public IReadOnlyList<FloorCandidate> FloorCandidates { get; }

        public EncounterViewModel CurrentEncounter { get; }

        private static IReadOnlyList<SurvivorViewModel> CreateSurvivorViews(
            IReadOnlyList<SurvivorDefinition> definitions)
        {
            var survivors = new List<SurvivorViewModel>(definitions.Count);

            for (int i = 0; i < definitions.Count; i++)
            {
                survivors.Add(new SurvivorViewModel(definitions[i]));
            }

            return survivors.AsReadOnly();
        }
    }
}
