using System;
using System.Collections.Generic;
using LastElevator.Core.State;
using LastElevator.Gameplay.Floor;

namespace LastElevator.Gameplay.Run
{
    public sealed class RunViewModel
    {
        internal RunViewModel(
            RunState state,
            RunPhase phase,
            int travelTargetFloor,
            IReadOnlyList<FloorCandidate> floorCandidates)
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
            Scrap = state.scrap;
            Phase = phase;
            TravelTargetFloor = travelTargetFloor;
            FloorCandidates = floorCandidates ?? Array.Empty<FloorCandidate>();
        }

        public int CurrentFloor { get; }

        public int Energy { get; }

        public int MaxEnergy { get; }

        public int Integrity { get; }

        public int MaxIntegrity { get; }

        public int Capacity { get; }

        public int SurvivorCount { get; }

        public int Scrap { get; }

        public RunPhase Phase { get; }

        public int TravelTargetFloor { get; }

        public IReadOnlyList<FloorCandidate> FloorCandidates { get; }
    }
}
