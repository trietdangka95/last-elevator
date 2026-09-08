using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastElevator.Core.Bootstrap
{
    public sealed class Bootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        private IEnumerator Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            Screen.orientation = ScreenOrientation.Portrait;

            yield return SceneManager.LoadSceneAsync(GameScenes.MainMenu, LoadSceneMode.Single);
        }
    }
}
