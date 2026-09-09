using System.Collections.Generic;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Run;
using UnityEngine;

namespace LastElevator.UI.Encounter
{
    public sealed class EncounterView : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.045f, 0.05f, 0.065f, 0.99f);
        private static readonly Color IllustrationColor = new Color(0.09f, 0.105f, 0.125f, 1f);
        private static readonly Color PrimaryTextColor = new Color(0.93f, 0.94f, 0.9f);
        private static readonly Color AccentTextColor = new Color(0.83f, 0.72f, 0.35f);
        private static readonly Color DisabledTextColor = new Color(0.45f, 0.48f, 0.48f);

        [SerializeField] private RunController _controller;

        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _categoryStyle;
        private GUIStyle _illustrationStyle;
        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
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

        public void SelectOption(int optionIndex)
        {
            if (_controller != null)
            {
                _controller.ChooseEncounterOption(optionIndex);
            }
        }

        private void OnGUI()
        {
            if (CurrentView == null || CurrentView.CurrentEncounter == null ||
                (CurrentView.Phase != RunPhase.Encounter && CurrentView.Phase != RunPhase.Resolving))
            {
                return;
            }

            EnsureStyles();

            float margin = Screen.width * 0.04f;
            var panelRect = new Rect(
                margin,
                Screen.height * 0.225f,
                Screen.width - (margin * 2f),
                Screen.height * 0.75f);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = PanelColor;
            GUI.Box(panelRect, GUIContent.none, _panelStyle);

            var illustrationRect = new Rect(
                panelRect.x + (panelRect.width * 0.035f),
                panelRect.y + (panelRect.height * 0.035f),
                panelRect.width * 0.93f,
                panelRect.height * 0.25f);
            GUI.backgroundColor = IllustrationColor;
            GUI.Box(illustrationRect, "ENCOUNTER", _illustrationStyle);
            GUI.backgroundColor = previousBackground;

            EncounterViewModel encounter = CurrentView.CurrentEncounter;
            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.31f), panelRect.width, panelRect.height * 0.06f),
                encounter.Category.ToString().ToUpperInvariant(),
                _categoryStyle);
            GUI.Label(
                new Rect(panelRect.x + (panelRect.width * 0.04f), panelRect.y + (panelRect.height * 0.365f), panelRect.width * 0.92f, panelRect.height * 0.09f),
                encounter.Title,
                _titleStyle);
            GUI.Label(
                new Rect(panelRect.x + (panelRect.width * 0.06f), panelRect.y + (panelRect.height * 0.46f), panelRect.width * 0.88f, panelRect.height * 0.16f),
                encounter.Body,
                _bodyStyle);

            DrawChoices(panelRect, encounter.Choices);
        }

        private void DrawChoices(Rect panelRect, IReadOnlyList<EncounterChoiceViewModel> choices)
        {
            if (choices.Count == 0)
            {
                GUI.Label(
                    new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.7f), panelRect.width, panelRect.height * 0.12f),
                    "NO AVAILABLE RESPONSE",
                    _bodyStyle);
                return;
            }

            float gap = panelRect.height * 0.025f;
            float areaY = panelRect.y + (panelRect.height * 0.64f);
            float areaHeight = panelRect.height * 0.32f;
            float buttonHeight = (areaHeight - (gap * (choices.Count - 1))) / choices.Count;
            float buttonX = panelRect.x + (panelRect.width * 0.06f);
            float buttonWidth = panelRect.width * 0.88f;

            for (int i = 0; i < choices.Count; i++)
            {
                EncounterChoiceViewModel choice = choices[i];
                var buttonRect = new Rect(
                    buttonX,
                    areaY + (i * (buttonHeight + gap)),
                    buttonWidth,
                    buttonHeight);
                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && choice.IsAvailable && CurrentView.Phase == RunPhase.Encounter;
                _buttonStyle.normal.textColor = choice.IsAvailable ? PrimaryTextColor : DisabledTextColor;

                if (GUI.Button(buttonRect, choice.Label, _buttonStyle))
                {
                    SelectOption(choice.Index);
                }

                GUI.enabled = previousEnabled;
            }
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
            _illustrationStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.022f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = DisabledTextColor }
            };
            _categoryStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.015f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = AccentTextColor }
            };
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.026f),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = PrimaryTextColor }
            };
            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.018f),
                wordWrap = true,
                normal = { textColor = PrimaryTextColor }
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
