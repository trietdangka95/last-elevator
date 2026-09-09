using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LastElevator.Core.State;
using LastElevator.Core.Bootstrap;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Run;
using LastElevator.UI.Common;
using LastElevator.UI.Encounter;
using LastElevator.UI.FloorChoice;
using LastElevator.UI.HUD;
using LastElevator.UI.Roster;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LastElevator.Tests.PlayMode
{
    public sealed class RunControllerTests
    {
        private const float PhaseChangeTimeoutSeconds = 2f;

        [UnityTest]
        public IEnumerator GameSceneStartsAtFloorOneWithInitialResourcesAndCandidates()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            RunViewModel view = controller.CurrentView;

            Assert.That(view, Is.Not.Null);
            Assert.That(view.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
            Assert.That(view.CurrentFloor, Is.EqualTo(1));
            Assert.That(view.Energy, Is.EqualTo(75));
            Assert.That(view.MaxEnergy, Is.EqualTo(100));
            Assert.That(view.Integrity, Is.EqualTo(100));
            Assert.That(view.MaxIntegrity, Is.EqualTo(100));
            Assert.That(view.Capacity, Is.EqualTo(4));
            Assert.That(view.SurvivorCount, Is.Zero);
            Assert.That(view.Survivors, Is.Empty);
            Assert.That(view.PendingSurvivor, Is.Null);
            Assert.That(view.Scrap, Is.Zero);
            Assert.That(view.CurrentEncounter, Is.Null);
            Assert.That(GetCandidateFloors(view), Is.EqualTo(new[] { 2, 3, 4 }));
        }

        [UnityTest]
        public IEnumerator ChoosingReachableFloorSpendsEnergyAndStartsTravel()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            controller.ChooseFloor(4);

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(1));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(69));
            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.Travelling));
            Assert.That(controller.CurrentView.TravelTargetFloor, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator InvalidFloorChoiceIsIgnored()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            LogAssert.Expect(LogType.Warning, "Ignored unavailable floor choice 10.");
            controller.ChooseFloor(10);

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(1));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(75));
            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
        }

        [UnityTest]
        public IEnumerator SecondChoiceDuringTravelIsIgnored()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            controller.ChooseFloor(4);
            LogAssert.Expect(LogType.Warning, "Ignored floor choice 3 while phase is Travelling.");
            controller.ChooseFloor(3);

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(1));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(69));
            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.Travelling));
            Assert.That(controller.CurrentView.TravelTargetFloor, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator TravelCompletesAndOpensEncounter()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            controller.ChooseFloor(4);
            yield return WaitForPhase(controller, RunPhase.Encounter);

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(4));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(69));
            Assert.That(controller.CurrentView.TravelTargetFloor, Is.Zero);
            Assert.That(controller.CurrentView.FloorCandidates, Is.Empty);
            Assert.That(controller.CurrentView.CurrentEncounter, Is.Not.Null);
            Assert.That(controller.CurrentView.CurrentEncounter.Choices.Count, Is.GreaterThanOrEqualTo(2));
        }

        [UnityTest]
        public IEnumerator PlayerCanTravelFromFloorOneToFloorThirty()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);
            FloorChoiceView floorChoice = Object.FindAnyObjectByType<FloorChoiceView>();

            int moves = 0;

            while (controller.CurrentView.CurrentFloor < 30)
            {
                IReadOnlyList<FloorCandidate> candidates = controller.CurrentView.FloorCandidates;
                Assert.That(candidates, Is.Not.Empty);

                floorChoice.SelectFloor(candidates[candidates.Count - 1].TargetFloor);
                yield return WaitForPhase(controller, RunPhase.Encounter);
                controller.ChooseEncounterOption(1);
                yield return WaitForPhase(controller, RunPhase.ChoosingFloor);

                moves++;
                Assert.That(moves, Is.LessThan(20));
            }

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(30));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(17));
            Assert.That(controller.CurrentView.CurrentEncounter, Is.Null);
            Assert.That(controller.CurrentView.FloorCandidates, Is.Empty);
        }

        [UnityTest]
        public IEnumerator GameSceneContainsConnectedGameplayViews()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            RunHudView hud = Object.FindAnyObjectByType<RunHudView>();
            FloorChoiceView floorChoice = Object.FindAnyObjectByType<FloorChoiceView>();
            ElevatorTravelView travel = Object.FindAnyObjectByType<ElevatorTravelView>();
            EncounterView encounter = Object.FindAnyObjectByType<EncounterView>();
            RosterReplaceView roster = Object.FindAnyObjectByType<RosterReplaceView>();

            Assert.That(hud, Is.Not.Null);
            Assert.That(floorChoice, Is.Not.Null);
            Assert.That(travel, Is.Not.Null);
            Assert.That(encounter, Is.Not.Null);
            Assert.That(roster, Is.Not.Null);
            Assert.That(hud.CurrentView, Is.SameAs(controller.CurrentView));
            Assert.That(floorChoice.CurrentView, Is.SameAs(controller.CurrentView));
            Assert.That(travel.CurrentView, Is.SameAs(controller.CurrentView));
            Assert.That(encounter.CurrentView, Is.SameAs(controller.CurrentView));
            Assert.That(roster.CurrentView, Is.SameAs(controller.CurrentView));
        }

        [UnityTest]
        public IEnumerator SurvivorEncounterRecruitsWhenThereIsFreeCapacity()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);
            FloorCandidate candidate = StartRunWithKnockingDoorCandidate(controller);

            controller.ChooseFloor(candidate.TargetFloor);
            yield return WaitForPhase(controller, RunPhase.Encounter);
            controller.ChooseEncounterOption(0);

            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
            Assert.That(controller.CurrentView.SurvivorCount, Is.EqualTo(1));
            Assert.That(
                controller.CurrentView.Survivors.Select(survivor => survivor.Id),
                Is.EqualTo(new[] { "survivor_maya_medic" }));
        }

        [UnityTest]
        public IEnumerator FullRosterRequiresReplaceOrRefuseBeforeContinuing()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);
            FloorCandidate candidate = StartRunWithKnockingDoorCandidate(controller);
            RunState state = GetRunState(controller);
            state.capacity = 3;
            state.survivorIds.Add("survivor_ken_engineer");
            state.survivorIds.Add("survivor_rin_guard");
            state.survivorIds.Add("survivor_tom_civilian");

            controller.ChooseFloor(candidate.TargetFloor);
            yield return WaitForPhase(controller, RunPhase.Encounter);
            controller.ChooseEncounterOption(0);

            RosterReplaceView rosterView = Object.FindAnyObjectByType<RosterReplaceView>();
            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ReplacingSurvivor));
            Assert.That(controller.CurrentView.PendingSurvivor.Id, Is.EqualTo("survivor_maya_medic"));
            Assert.That(controller.CurrentView.SurvivorCount, Is.EqualTo(3));
            Assert.That(rosterView.CurrentView, Is.SameAs(controller.CurrentView));

            rosterView.SelectReplacement("survivor_ken_engineer");

            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
            Assert.That(controller.CurrentView.SurvivorCount, Is.EqualTo(3));
            Assert.That(
                controller.CurrentView.Survivors.Select(survivor => survivor.Id),
                Does.Contain("survivor_maya_medic"));
            Assert.That(
                controller.CurrentView.Survivors.Select(survivor => survivor.Id),
                Does.Not.Contain("survivor_ken_engineer"));
        }

        [UnityTest]
        public IEnumerator FloorChoiceViewSendsSelectionThroughRunController()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);
            FloorChoiceView floorChoice = Object.FindAnyObjectByType<FloorChoiceView>();

            floorChoice.SelectFloor(4);

            Assert.That(controller.CurrentView.Energy, Is.EqualTo(69));
            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.Travelling));
            Assert.That(floorChoice.CurrentView, Is.SameAs(controller.CurrentView));
        }

        [UnityTest]
        public IEnumerator EncounterViewSendsChoiceThroughRunControllerAndReturnsToFloorChoice()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);
            EncounterView encounter = Object.FindAnyObjectByType<EncounterView>();

            controller.ChooseFloor(4);
            yield return WaitForPhase(controller, RunPhase.Encounter);

            encounter.SelectOption(1);

            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
            Assert.That(controller.CurrentView.CurrentEncounter, Is.Null);
            Assert.That(GetCandidateFloors(controller.CurrentView), Is.EqualTo(new[] { 5, 6, 7 }));
        }

        private static IEnumerator LoadGameScene(System.Action<RunController> setController)
        {
            yield return SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Single);
            yield return null;

            RunController controller = Object.FindAnyObjectByType<RunController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.CurrentView, Is.Not.Null);
            setController(controller);
        }

        private static IEnumerator WaitForPhase(RunController controller, RunPhase expectedPhase)
        {
            float timeout = Time.realtimeSinceStartup + PhaseChangeTimeoutSeconds;

            while (controller.CurrentView.Phase != expectedPhase &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(controller.CurrentView.Phase, Is.EqualTo(expectedPhase));
        }

        private static FloorCandidate StartRunWithKnockingDoorCandidate(RunController controller)
        {
            for (int seed = 1; seed <= 200; seed++)
            {
                controller.StartNewRun(seed);

                for (int i = 0; i < controller.CurrentView.FloorCandidates.Count; i++)
                {
                    FloorCandidate candidate = controller.CurrentView.FloorCandidates[i];

                    if (candidate.Encounter != null && candidate.Encounter.id == "event_knocking_door")
                    {
                        return candidate;
                    }
                }
            }

            Assert.Fail("Expected a seeded run with event_knocking_door in the first floor choices.");
            return null;
        }

        private static RunState GetRunState(RunController controller)
        {
            FieldInfo stateField = typeof(RunController).GetField(
                "_state",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(stateField, Is.Not.Null);
            return (RunState)stateField.GetValue(controller);
        }

        private static int[] GetCandidateFloors(RunViewModel view)
        {
            var floors = new int[view.FloorCandidates.Count];

            for (int i = 0; i < view.FloorCandidates.Count; i++)
            {
                floors[i] = view.FloorCandidates[i].TargetFloor;
            }

            return floors;
        }
    }
}
