using System.Collections.Generic;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Run;
using UnityEngine;

namespace LastElevator.UI.FloorChoice
{
    public sealed class FloorChoiceView : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.055f, 0.065f, 0.085f, 0.98f);
        private static readonly Color PrimaryTextColor = new Color(0.93f, 0.94f, 0.9f);
        private static readonly Color MutedTextColor = new Color(0.64f, 0.68f, 0.68f);

        [SerializeField] private RunController _controller;

        private GUIStyle _buttonStyle;
        private GUIStyle _emptyStyle;
        private GUIStyle _headingStyle;
        private GUIStyle _panelStyle;
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

        public void SelectFloor(int floor)
        {
            if (_controller != null)
            {
                _controller.ChooseFloor(floor);
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
            var panelRect = new Rect(
                margin,
                Screen.height * 0.59f,
                Screen.width - (margin * 2f),
                Screen.height * 0.385f);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = PanelColor;
            GUI.Box(panelRect, GUIContent.none, _panelStyle);
            GUI.backgroundColor = previousBackground;

            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.025f), panelRect.width, panelRect.height * 0.14f),
                GetHeading(CurrentView),
                _headingStyle);

            if (CurrentView.Phase == RunPhase.Travelling)
            {
                DrawEmptyState(panelRect, "ELEVATOR IN MOTION");
                return;
            }

            IReadOnlyList<FloorCandidate> candidates = CurrentView.FloorCandidates;

            if (candidates.Count == 0)
            {
                DrawEmptyState(
                    panelRect,
                    CurrentView.CurrentFloor == 30 ? "THE FINAL DOORS ARE SEALED" : "NO REACHABLE FLOORS");
                return;
            }

            DrawCandidateCards(panelRect, candidates);
        }

        private void DrawCandidateCards(Rect panelRect, IReadOnlyList<FloorCandidate> candidates)
        {
            float horizontalPadding = panelRect.width * 0.035f;
            float gap = panelRect.width * 0.02f;
            float availableWidth = panelRect.width - (horizontalPadding * 2f) - (gap * (candidates.Count - 1));
            float cardWidth = availableWidth / candidates.Count;
            float cardY = panelRect.y + (panelRect.height * 0.18f);
            float cardHeight = panelRect.height * 0.76f;

            for (int i = 0; i < candidates.Count; i++)
            {
                FloorCandidate candidate = candidates[i];
                float cardX = panelRect.x + horizontalPadding + (i * (cardWidth + gap));
                var cardRect = new Rect(cardX, cardY, cardWidth, cardHeight);

                Color previousBackground = GUI.backgroundColor;
                GUI.backgroundColor = FloorSignalPresentation.GetColor(candidate.SignalCategory);

                if (GUI.Button(cardRect, FloorSignalPresentation.GetCardText(candidate), _buttonStyle))
                {
                    SelectFloor(candidate.TargetFloor);
                }

                GUI.backgroundColor = previousBackground;
            }
        }

        private void DrawEmptyState(Rect panelRect, string message)
        {
            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.3f), panelRect.width, panelRect.height * 0.45f),
                message,
                _emptyStyle);
        }

        private void HandleRunStateChanged(RunViewModel view)
        {
            CurrentView = view;
        }

        private static string GetHeading(RunViewModel view)
        {
            if (view.Phase == RunPhase.Travelling)
            {
                return $"TRAVELLING TO FLOOR {view.TravelTargetFloor:00}";
            }

            return view.CurrentFloor == 30 ? "DESTINATION REACHED" : "CHOOSE NEXT STOP";
        }

        private void EnsureStyles()
        {
            if (_styledScreenHeight == Screen.height && _panelStyle != null)
            {
                return;
            }

            _styledScreenHeight = Screen.height;
            _panelStyle = new GUIStyle(GUI.skin.box);
            _headingStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.023f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = PrimaryTextColor }
            };
            _emptyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.022f),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = MutedTextColor }
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.018f),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = PrimaryTextColor },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };
        }
    }
}
