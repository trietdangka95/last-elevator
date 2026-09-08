using LastElevator.Core.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastElevator.UI.MainMenu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        private AsyncOperation _gameLoad;

        public void Play()
        {
            if (_gameLoad != null)
            {
                return;
            }

            _gameLoad = SceneManager.LoadSceneAsync(GameScenes.Game, LoadSceneMode.Single);
        }

        private void OnGUI()
        {
            float buttonWidth = Screen.width * 0.8f;
            float buttonHeight = Mathf.Max(56f, Screen.height * 0.08f);
            float buttonX = (Screen.width - buttonWidth) * 0.5f;
            float buttonY = Screen.height * 0.78f;

            bool previousEnabled = GUI.enabled;
            GUI.enabled = _gameLoad == null;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "PLAY"))
            {
                Play();
            }

            GUI.enabled = previousEnabled;
        }
    }
}
