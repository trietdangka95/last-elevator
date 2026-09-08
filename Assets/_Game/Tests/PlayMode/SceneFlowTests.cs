using System.Collections;
using LastElevator.Core.Bootstrap;
using LastElevator.UI.MainMenu;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LastElevator.Tests.PlayMode
{
    public sealed class SceneFlowTests
    {
        [UnityTest]
        public IEnumerator BootstrapLoadsMainMenu()
        {
            yield return SceneManager.LoadSceneAsync(GameScenes.Bootstrap, LoadSceneMode.Single);
            yield return WaitForActiveScene(GameScenes.MainMenu);
        }

        [UnityTest]
        public IEnumerator PlayLoadsGameScene()
        {
            yield return SceneManager.LoadSceneAsync(GameScenes.MainMenu, LoadSceneMode.Single);

            MainMenuView mainMenu = Object.FindAnyObjectByType<MainMenuView>();
            Assert.That(mainMenu, Is.Not.Null);

            mainMenu.Play();
            yield return WaitForActiveScene(GameScenes.Game);
        }

        private static IEnumerator WaitForActiveScene(string sceneName)
        {
            float timeout = Time.realtimeSinceStartup + 5f;

            while (SceneManager.GetActiveScene().name != sceneName && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
        }
    }
}
