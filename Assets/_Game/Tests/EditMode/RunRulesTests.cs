using LastElevator.Core.State;
using LastElevator.Gameplay.Run;
using NUnit.Framework;

namespace LastElevator.Tests.EditMode
{
    public sealed class RunRulesTests
    {
        [Test]
        public void CreateInitialStateUsesLockedResourceValues()
        {
            RunState state = RunRules.CreateInitialState(12345);

            Assert.That(state.seed, Is.EqualTo(12345));
            Assert.That(state.energy, Is.EqualTo(75));
            Assert.That(state.maxEnergy, Is.EqualTo(100));
            Assert.That(state.integrity, Is.EqualTo(100));
            Assert.That(state.maxIntegrity, Is.EqualTo(100));
            Assert.That(state.capacity, Is.EqualTo(4));
            Assert.That(state.scrap, Is.Zero);
        }

        [TestCase(1, 2)]
        [TestCase(3, 6)]
        public void GetTravelEnergyCostUsesFloorDistance(int distance, int expectedCost)
        {
            RunState state = RunRules.CreateInitialState(1);

            int cost = RunRules.GetTravelEnergyCost(state, distance);

            Assert.That(cost, Is.EqualTo(expectedCost));
        }

        [Test]
        public void GetTravelEnergyCostAppliesEfficientMotorReduction()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.travelEnergyReduction = 1;

            int cost = RunRules.GetTravelEnergyCost(state, 3);

            Assert.That(cost, Is.EqualTo(5));
        }

        [Test]
        public void GetTravelEnergyCostNeverDropsBelowOne()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.travelEnergyReduction = 20;

            int cost = RunRules.GetTravelEnergyCost(state, 1);

            Assert.That(cost, Is.EqualTo(1));
        }

        [Test]
        public void TrySpendTravelEnergyReducesEnergyWhenAffordable()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.energy = 10;

            bool didSpend = RunRules.TrySpendTravelEnergy(state, 3);

            Assert.That(didSpend, Is.True);
            Assert.That(state.energy, Is.EqualTo(4));
        }

        [Test]
        public void TrySpendTravelEnergyPreservesEnergyWhenUnaffordable()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.energy = 5;

            bool didSpend = RunRules.TrySpendTravelEnergy(state, 3);

            Assert.That(didSpend, Is.False);
            Assert.That(state.energy, Is.EqualTo(5));
        }

        [TestCase(6, true)]
        [TestCase(5, false)]
        public void CanReachFloorUsesTravelEnergyCost(int energy, bool expectedCanReach)
        {
            RunState state = RunRules.CreateInitialState(1);
            state.energy = energy;

            bool canReach = RunRules.CanReachFloor(state, 3);

            Assert.That(canReach, Is.EqualTo(expectedCanReach));
        }

        [Test]
        public void ChangeEnergyClampsAtMaximum()
        {
            RunState state = RunRules.CreateInitialState(1);

            RunRules.ChangeEnergy(state, 50);

            Assert.That(state.energy, Is.EqualTo(state.maxEnergy));
        }

        [Test]
        public void DamageIntegrityClampsAtZeroAndEndsRun()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.integrity = 10;

            RunRules.DamageIntegrity(state, 20);

            Assert.That(state.integrity, Is.Zero);
            Assert.That(RunRules.IsRunDead(state), Is.True);
        }

        [Test]
        public void RepairIntegrityClampsAtMaximum()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.integrity = 80;

            RunRules.RepairIntegrity(state, 30);

            Assert.That(state.integrity, Is.EqualTo(state.maxIntegrity));
        }

        [Test]
        public void EnergyLossClampsAtZeroAndEndsRun()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.energy = 5;

            RunRules.ChangeEnergy(state, -10);

            Assert.That(state.energy, Is.Zero);
            Assert.That(RunRules.IsRunDead(state), Is.True);
        }

        [Test]
        public void PositiveResourcesKeepRunAlive()
        {
            RunState state = RunRules.CreateInitialState(1);

            Assert.That(RunRules.IsRunDead(state), Is.False);
        }

        [Test]
        public void IncreaseCapacityDoesNotExceedMvpOneMaximum()
        {
            RunState state = RunRules.CreateInitialState(1);

            RunRules.IncreaseCapacity(state, 10);

            Assert.That(state.capacity, Is.EqualTo(6));
        }

        [Test]
        public void GetAvailableCapacityAccountsForCurrentSurvivors()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.survivorIds.Add("survivor_one");
            state.survivorIds.Add("survivor_two");

            int availableCapacity = RunRules.GetAvailableCapacity(state);

            Assert.That(availableCapacity, Is.EqualTo(2));
        }

        [Test]
        public void ChangeScrapNeverProducesNegativeScrap()
        {
            RunState state = RunRules.CreateInitialState(1);
            state.scrap = 5;

            RunRules.ChangeScrap(state, -10);

            Assert.That(state.scrap, Is.Zero);
        }
    }
}
