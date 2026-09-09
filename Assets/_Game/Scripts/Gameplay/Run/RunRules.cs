using System;
using LastElevator.Core.State;

namespace LastElevator.Gameplay.Run
{
    public static class RunRules
    {
        private const int StartEnergy = 75;
        private const int MaxEnergy = 100;
        private const int StartIntegrity = 100;
        private const int MaxIntegrity = 100;
        private const int StartCapacity = 4;
        private const int MaxCapacity = 6;
        private const int StartScrap = 0;
        private const int EnergyPerFloor = 2;
        private const int MinimumTravelEnergyCost = 1;

        public static RunState CreateInitialState(int seed)
        {
            return new RunState
            {
                seed = seed,
                energy = StartEnergy,
                maxEnergy = MaxEnergy,
                integrity = StartIntegrity,
                maxIntegrity = MaxIntegrity,
                capacity = StartCapacity,
                scrap = StartScrap
            };
        }

        public static int GetTravelEnergyCost(RunState state, int distance)
        {
            return GetTravelEnergyCost(state, distance, 0);
        }

        public static int GetTravelEnergyCost(
            RunState state,
            int distance,
            int additionalReduction)
        {
            ValidateState(state);

            if (distance < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(distance), distance, "Travel distance must be positive.");
            }

            long reduction = (long)Math.Max(0, state.travelEnergyReduction) +
                Math.Max(0, additionalReduction);
            long cost = ((long)distance * EnergyPerFloor) - reduction;

            return (int)Math.Min(int.MaxValue, Math.Max(MinimumTravelEnergyCost, cost));
        }

        public static bool TrySpendTravelEnergy(RunState state, int distance)
        {
            return TrySpendTravelEnergy(state, distance, 0);
        }

        public static bool TrySpendTravelEnergy(
            RunState state,
            int distance,
            int additionalReduction)
        {
            int cost = GetTravelEnergyCost(state, distance, additionalReduction);

            if (state.energy < cost)
            {
                return false;
            }

            state.energy -= cost;
            return true;
        }

        public static bool CanReachFloor(RunState state, int distance)
        {
            return CanReachFloor(state, distance, 0);
        }

        public static bool CanReachFloor(
            RunState state,
            int distance,
            int additionalReduction)
        {
            int cost = GetTravelEnergyCost(state, distance, additionalReduction);
            return state.energy >= cost;
        }

        public static void ChangeEnergy(RunState state, int amount)
        {
            ValidateState(state);
            state.energy = AddClamped(state.energy, amount, 0, state.maxEnergy);
        }

        public static void DamageIntegrity(RunState state, int amount)
        {
            ValidateNonNegativeAmount(amount);
            ValidateState(state);
            state.integrity = AddClamped(state.integrity, -amount, 0, state.maxIntegrity);
        }

        public static void RepairIntegrity(RunState state, int amount)
        {
            ValidateNonNegativeAmount(amount);
            ValidateState(state);
            state.integrity = AddClamped(state.integrity, amount, 0, state.maxIntegrity);
        }

        public static void IncreaseCapacity(RunState state, int amount)
        {
            ValidateNonNegativeAmount(amount);
            ValidateState(state);
            state.capacity = AddClamped(state.capacity, amount, 0, MaxCapacity);
        }

        public static int GetAvailableCapacity(RunState state)
        {
            ValidateState(state);
            int survivorCount = state.survivorIds == null ? 0 : state.survivorIds.Count;

            return Math.Max(0, state.capacity - survivorCount);
        }

        public static void ChangeScrap(RunState state, int amount)
        {
            ValidateState(state);
            state.scrap = AddClamped(state.scrap, amount, 0, int.MaxValue);
        }

        public static bool IsRunDead(RunState state)
        {
            ValidateState(state);
            return state.integrity <= 0 || state.energy <= 0;
        }

        private static int AddClamped(int current, int amount, int minimum, int maximum)
        {
            long result = (long)current + amount;
            return (int)Math.Min(maximum, Math.Max(minimum, result));
        }

        private static void ValidateState(RunState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
        }

        private static void ValidateNonNegativeAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount cannot be negative.");
            }
        }
    }
}
