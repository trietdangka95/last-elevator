using LastElevator.Gameplay.Run;
using UnityEngine;

namespace LastElevator.UI.HUD
{
    public sealed class RunHudView : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.075f, 0.09f, 0.12f, 0.98f);
        private static readonly Color PrimaryTextColor = new Color(0.92f, 0.94f, 0.9f);
        private static readonly Color AccentTextColor = new Color(0.83f, 0.72f, 0.35f);

        [SerializeField] private RunController _controller;

        private GUIStyle _floorStyle;
        private GUIStyle _panelStyle;
        private GUIStyle _resourceStyle;
        private int _styledScreenHeight;

        public RunViewModel CurrentView { get; private set; }

        private void OnEnable()
        {
            if (_controller == null)
            {
                return;
            }

            _controller.RunStateChanged += HandleRunStateChanged;
            CurrentView = _controller.CurrentView;
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.RunStateChanged -= HandleRunStateChanged;
            }
        }

        private void OnGUI()
        {
            if (CurrentView == null)
            {
                return;
            }

            EnsureStyles();

            float margin = Screen.width * 0.04f;
            float panelWidth = Screen.width - (margin * 2f);
            var panelRect = new Rect(margin, Screen.height * 0.025f, panelWidth, Screen.height * 0.175f);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = PanelColor;
            GUI.Box(panelRect, GUIContent.none, _panelStyle);
            GUI.backgroundColor = previousBackground;

            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.06f), panelRect.width, panelRect.height * 0.3f),
                $"FLOOR {CurrentView.CurrentFloor:00}",
                _floorStyle);

            float cellWidth = panelRect.width * 0.46f;
            float leftX = panelRect.x + (panelRect.width * 0.035f);
            float rightX = panelRect.x + (panelRect.width * 0.505f);
            float firstRowY = panelRect.y + (panelRect.height * 0.38f);
            float secondRowY = panelRect.y + (panelRect.height * 0.67f);
            float rowHeight = panelRect.height * 0.24f;

            GUI.Label(new Rect(leftX, firstRowY, cellWidth, rowHeight),
                $"ENERGY  {CurrentView.Energy}/{CurrentView.MaxEnergy}", _resourceStyle);
            GUI.Label(new Rect(rightX, firstRowY, cellWidth, rowHeight),
                $"INTEGRITY  {CurrentView.Integrity}/{CurrentView.MaxIntegrity}", _resourceStyle);
            GUI.Label(new Rect(leftX, secondRowY, cellWidth, rowHeight),
                $"SEATS  {CurrentView.SurvivorCount}/{CurrentView.Capacity}", _resourceStyle);
            GUI.Label(new Rect(rightX, secondRowY, cellWidth, rowHeight),
                $"SCRAP  {CurrentView.Scrap}", _resourceStyle);
        }

        private void HandleRunStateChanged(RunViewModel view)
        {
            CurrentView = view;
        }

        private void EnsureStyles()
        {
            if (_styledScreenHeight == Screen.height && _panelStyle != null)
            {
                return;
            }

            _styledScreenHeight = Screen.height;
            _panelStyle = new GUIStyle(GUI.skin.box);
            _panelStyle.normal.textColor = PrimaryTextColor;
            _floorStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.032f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = AccentTextColor }
            };
            _resourceStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.RoundToInt(Screen.height * 0.018f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = PrimaryTextColor }
            };
        }
    }
}
