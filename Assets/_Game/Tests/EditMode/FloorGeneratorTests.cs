using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Data.Definitions;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Run;
using NUnit.Framework;

namespace LastElevator.Tests.EditMode
{
    public sealed class FloorGeneratorTests
    {
        [Test]
        public void GeneratesIncreasingOneTwoAndThreeFloorCandidates()
        {
            RunState state = RunRules.CreateInitialState(101);
            state.currentFloor = 4;
            FloorGenerator generator = CreateGenerator(state.seed);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            Assert.That(candidates.Count, Is.EqualTo(3));
            AssertCandidate(candidates[0], 5, 1, 2, FloorBand.Early);
            AssertCandidate(candidates[1], 6, 2, 4, FloorBand.Early);
            AssertCandidate(candidates[2], 7, 3, 6, FloorBand.Early);
        }

        [TestCase(8, new[] { 9, 10 })]
        [TestCase(9, new[] { 10 })]
        [TestCase(18, new[] { 19, 20 })]
        [TestCase(19, new[] { 20 })]
        [TestCase(28, new[] { 29, 30 })]
        [TestCase(29, new[] { 30 })]
        public void MandatoryFloorsCannotBeSkipped(int currentFloor, int[] expectedFloors)
        {
            RunState state = RunRules.CreateInitialState(202);
            state.currentFloor = currentFloor;
            FloorGenerator generator = CreateGenerator(state.seed);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            Assert.That(GetFloorNumbers(candidates), Is.EqualTo(expectedFloors));
        }

        [Test]
        public void CheckpointsAndBossUseMandatoryCategories()
        {
            RunState checkpointAState = RunRules.CreateInitialState(303);
            checkpointAState.currentFloor = 9;
            RunState checkpointBState = RunRules.CreateInitialState(303);
            checkpointBState.currentFloor = 19;
            RunState bossState = RunRules.CreateInitialState(303);
            bossState.currentFloor = 29;

            FloorCandidate checkpointA = CreateGenerator(303).GenerateCandidates(checkpointAState)[0];
            FloorCandidate checkpointB = CreateGenerator(303).GenerateCandidates(checkpointBState)[0];
            FloorCandidate boss = CreateGenerator(303).GenerateCandidates(bossState)[0];

            Assert.That(checkpointA.Band, Is.EqualTo(FloorBand.CheckpointA));
            Assert.That(checkpointA.SignalCategory, Is.EqualTo(EncounterCategory.Checkpoint));
            Assert.That(checkpointA.IsMandatory, Is.True);
            Assert.That(checkpointB.Band, Is.EqualTo(FloorBand.CheckpointB));
            Assert.That(checkpointB.SignalCategory, Is.EqualTo(EncounterCategory.Checkpoint));
            Assert.That(checkpointB.IsMandatory, Is.True);
            Assert.That(boss.Band, Is.EqualTo(FloorBand.Boss));
            Assert.That(boss.SignalCategory, Is.EqualTo(EncounterCategory.Boss));
            Assert.That(boss.IsMandatory, Is.True);
        }

        [TestCase(1, FloorBand.Early)]
        [TestCase(9, FloorBand.Early)]
        [TestCase(10, FloorBand.CheckpointA)]
        [TestCase(11, FloorBand.Mid)]
        [TestCase(19, FloorBand.Mid)]
        [TestCase(20, FloorBand.CheckpointB)]
        [TestCase(21, FloorBand.Late)]
        [TestCase(29, FloorBand.Late)]
        [TestCase(30, FloorBand.Boss)]
        public void UsesLockedFloorBands(int floor, FloorBand expectedBand)
        {
            Assert.That(FloorGenerator.GetBand(floor), Is.EqualTo(expectedBand));
        }

        [Test]
        public void FiltersCandidatesThatCannotBeReachedWithCurrentEnergy()
        {
            RunState state = RunRules.CreateInitialState(404);
            state.currentFloor = 4;
            state.energy = 4;
            FloorGenerator generator = CreateGenerator(state.seed);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            Assert.That(GetFloorNumbers(candidates), Is.EqualTo(new[] { 5, 6 }));
        }

        [Test]
        public void CandidateEnergyCostIncludesTravelReduction()
        {
            RunState state = RunRules.CreateInitialState(505);
            state.currentFloor = 4;
            state.travelEnergyReduction = 1;
            FloorGenerator generator = CreateGenerator(state.seed);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            Assert.That(GetEnergyCosts(candidates), Is.EqualTo(new[] { 1, 3, 5 }));
        }

        [Test]
        public void SameSeedAndRouteProduceSameFloorAndEncounterCategorySequence()
        {
            RunState firstState = RunRules.CreateInitialState(18492301);
            RunState secondState = RunRules.CreateInitialState(18492301);
            FloorGenerator firstGenerator = CreateGenerator(firstState.seed);
            FloorGenerator secondGenerator = CreateGenerator(secondState.seed);
            int[] route = { 0, 3, 6, 9, 10, 13, 16, 19, 20, 23, 26, 29 };

            for (int i = 0; i < route.Length; i++)
            {
                firstState.currentFloor = route[i];
                secondState.currentFloor = route[i];

                IReadOnlyList<FloorCandidate> first = firstGenerator.GenerateCandidates(firstState);
                IReadOnlyList<FloorCandidate> second = secondGenerator.GenerateCandidates(secondState);

                AssertCandidateSequencesMatch(first, second);
            }
        }

        [Test]
        public void RegularSignalsDoNotRepeatWithinOneCandidateSetWhenAlternativesExist()
        {
            RunState state = RunRules.CreateInitialState(606);
            state.currentFloor = 4;
            FloorGenerator generator = CreateGenerator(state.seed);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            var categories = new HashSet<EncounterCategory>();
            for (int i = 0; i < candidates.Count; i++)
            {
                categories.Add(candidates[i].SignalCategory);
            }

            Assert.That(categories.Count, Is.EqualTo(candidates.Count));
        }

        [Test]
        public void UsesInjectedBandWeightsInsteadOfHardcodedPercentages()
        {
            var onlyMystery = new EncounterCategoryWeights(0, 0, 0, 0, 1);
            var config = new FloorGenerationConfig
            {
                early = onlyMystery,
                mid = onlyMystery,
                late = onlyMystery
            };
            RunState state = RunRules.CreateInitialState(707);
            state.currentFloor = 4;
            var generator = new FloorGenerator(new SeededRandomService(state.seed), config);

            IReadOnlyList<FloorCandidate> candidates = generator.GenerateCandidates(state);

            Assert.That(candidates[0].SignalCategory, Is.EqualTo(EncounterCategory.Mystery));
            Assert.That(candidates[1].SignalCategory, Is.EqualTo(EncounterCategory.Mystery));
            Assert.That(candidates[2].SignalCategory, Is.EqualTo(EncounterCategory.Mystery));
        }

        [Test]
        public void DefaultConfigMatchesPlannedEarlyMidLateWeights()
        {
            var config = new FloorGenerationConfig();

            AssertWeights(config.early, 30, 30, 15, 10, 15);
            AssertWeights(config.mid, 20, 20, 30, 15, 15);
            AssertWeights(config.late, 15, 15, 40, 20, 10);
        }

        [Test]
        public void GeneratingCandidatesDoesNotMutateRunState()
        {
            RunState state = RunRules.CreateInitialState(808);
            state.currentFloor = 4;
            int energyBefore = state.energy;
            FloorGenerator generator = CreateGenerator(state.seed);

            generator.GenerateCandidates(state);

            Assert.That(state.currentFloor, Is.EqualTo(4));
            Assert.That(state.energy, Is.EqualTo(energyBefore));
        }

        [Test]
        public void FinalFloorHasNoFurtherCandidates()
        {
            RunState state = RunRules.CreateInitialState(909);
            state.currentFloor = 30;

            IReadOnlyList<FloorCandidate> candidates = CreateGenerator(state.seed).GenerateCandidates(state);

            Assert.That(candidates, Is.Empty);
        }

        [TestCase(-1)]
        [TestCase(31)]
        public void RejectsCurrentFloorOutsideRunBounds(int currentFloor)
        {
            RunState state = RunRules.CreateInitialState(1001);
            state.currentFloor = currentFloor;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CreateGenerator(state.seed).GenerateCandidates(state));
        }

        private static FloorGenerator CreateGenerator(int seed)
        {
            return new FloorGenerator(new SeededRandomService(seed), new FloorGenerationConfig());
        }

        private static int[] GetFloorNumbers(IReadOnlyList<FloorCandidate> candidates)
        {
            var floors = new int[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                floors[i] = candidates[i].TargetFloor;
            }

            return floors;
        }

        private static int[] GetEnergyCosts(IReadOnlyList<FloorCandidate> candidates)
        {
            var costs = new int[candidates.Count];

            for (int i = 0; i < candidates.Count; i++)
            {
                costs[i] = candidates[i].EnergyCost;
            }

            return costs;
        }

        private static void AssertCandidate(
            FloorCandidate candidate,
            int targetFloor,
            int distance,
            int energyCost,
            FloorBand band)
        {
            Assert.That(candidate.TargetFloor, Is.EqualTo(targetFloor));
            Assert.That(candidate.Distance, Is.EqualTo(distance));
            Assert.That(candidate.EnergyCost, Is.EqualTo(energyCost));
            Assert.That(candidate.Band, Is.EqualTo(band));
        }

        private static void AssertCandidateSequencesMatch(
            IReadOnlyList<FloorCandidate> first,
            IReadOnlyList<FloorCandidate> second)
        {
            Assert.That(first.Count, Is.EqualTo(second.Count));

            for (int i = 0; i < first.Count; i++)
            {
                Assert.That(first[i].TargetFloor, Is.EqualTo(second[i].TargetFloor));
                Assert.That(first[i].SignalCategory, Is.EqualTo(second[i].SignalCategory));
            }
        }

        private static void AssertWeights(
            EncounterCategoryWeights weights,
            int survivor,
            int resource,
            int enemy,
            int hazard,
            int mystery)
        {
            Assert.That(weights.survivor, Is.EqualTo(survivor));
            Assert.That(weights.resource, Is.EqualTo(resource));
            Assert.That(weights.enemy, Is.EqualTo(enemy));
            Assert.That(weights.hazard, Is.EqualTo(hazard));
            Assert.That(weights.mystery, Is.EqualTo(mystery));
        }
    }
}
