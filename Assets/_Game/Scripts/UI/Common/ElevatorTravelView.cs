using LastElevator.Gameplay.Run;
using UnityEngine;

namespace LastElevator.UI.Common
{
    public sealed class ElevatorTravelView : MonoBehaviour
    {
        private static readonly Color DoorColor = new Color(0.13f, 0.15f, 0.18f, 1f);
        private static readonly Color DoorEdgeColor = new Color(0.38f, 0.35f, 0.27f, 1f);
        private static readonly Color PrimaryTextColor = new Color(0.9f, 0.91f, 0.86f);
        private static readonly Color TravelTextColor = new Color(0.93f, 0.67f, 0.24f);

        [SerializeField] private RunController _controller;

        private GUIStyle _doorStyle;
        private GUIStyle _statusStyle;
        private GUIStyle _trimStyle;
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

            if (CurrentView.Phase == RunPhase.Encounter ||
                CurrentView.Phase == RunPhase.Resolving)
            {
                return;
            }

            EnsureStyles();

            float elevatorWidth = Screen.width * 0.72f;
            float elevatorHeight = Screen.height * 0.32f;
            float elevatorX = (Screen.width - elevatorWidth) * 0.5f;
            float elevatorY = Screen.height * 0.235f;
            var elevatorRect = new Rect(elevatorX, elevatorY, elevatorWidth, elevatorHeight);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = DoorColor;
            GUI.Box(elevatorRect, GUIContent.none, _doorStyle);

            float trimWidth = Screen.width * 0.012f;
            GUI.backgroundColor = DoorEdgeColor;
            GUI.Box(
                new Rect((Screen.width - trimWidth) * 0.5f, elevatorY, trimWidth, elevatorHeight),
                GUIContent.none,
                _trimStyle);
            GUI.backgroundColor = previousBackground;

            string status = GetStatusText(CurrentView);
            _statusStyle.normal.textColor = CurrentView.Phase == RunPhase.Travelling
                ? TravelTextColor
                : PrimaryTextColor;
            GUI.Label(
                new Rect(elevatorX, elevatorY + (elevatorHeight * 0.36f), elevatorWidth, elevatorHeight * 0.28f),
                status,
                _statusStyle);
        }

        private void HandleRunStateChanged(RunViewModel view)
        {
            CurrentView = view;
        }

        private static string GetStatusText(RunViewModel view)
        {
            if (view.Phase == RunPhase.Travelling)
            {
                return $"ASCENDING\nFLOOR {view.TravelTargetFloor:00}";
            }

            if (view.CurrentFloor == 30)
            {
                return "FINAL FLOOR\nREACHED";
            }

            return "SIGNALS\nACQUIRED";
        }

        private void EnsureStyles()
        {
            if (_styledScreenHeight == Screen.height && _doorStyle != null)
            {
                return;
            }

            _styledScreenHeight = Screen.height;
            _doorStyle = new GUIStyle(GUI.skin.box);
            _trimStyle = new GUIStyle(GUI.skin.box);
            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.03f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = PrimaryTextColor }
            };
        }
    }
}
