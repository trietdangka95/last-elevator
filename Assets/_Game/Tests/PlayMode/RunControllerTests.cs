using System.Collections;
using System.Collections.Generic;
using LastElevator.Core.Bootstrap;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Run;
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
            Assert.That(view.Scrap, Is.Zero);
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
        public IEnumerator TravelCompletesAndPublishesNextCandidates()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            controller.ChooseFloor(4);
            yield return WaitForChoosingFloor(controller);

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(4));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(69));
            Assert.That(controller.CurrentView.TravelTargetFloor, Is.Zero);
            Assert.That(GetCandidateFloors(controller.CurrentView), Is.EqualTo(new[] { 5, 6, 7 }));
        }

        [UnityTest]
        public IEnumerator PlayerCanTravelFromFloorOneToFloorThirty()
        {
            RunController controller = null;
            yield return LoadGameScene(result => controller = result);

            int moves = 0;

            while (controller.CurrentView.CurrentFloor < 30)
            {
                IReadOnlyList<FloorCandidate> candidates = controller.CurrentView.FloorCandidates;
                Assert.That(candidates, Is.Not.Empty);

                controller.ChooseFloor(candidates[candidates.Count - 1].TargetFloor);
                yield return WaitForChoosingFloor(controller);

                moves++;
                Assert.That(moves, Is.LessThan(20));
            }

            Assert.That(controller.CurrentView.CurrentFloor, Is.EqualTo(30));
            Assert.That(controller.CurrentView.Energy, Is.EqualTo(17));
            Assert.That(controller.CurrentView.FloorCandidates, Is.Empty);
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

        private static IEnumerator WaitForChoosingFloor(RunController controller)
        {
            float timeout = Time.realtimeSinceStartup + PhaseChangeTimeoutSeconds;

            while (controller.CurrentView.Phase != RunPhase.ChoosingFloor &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(controller.CurrentView.Phase, Is.EqualTo(RunPhase.ChoosingFloor));
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
