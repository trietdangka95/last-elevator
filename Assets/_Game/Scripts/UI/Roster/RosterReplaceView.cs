using System.Collections.Generic;
using LastElevator.Gameplay.Run;
using LastElevator.Gameplay.Survivors;
using UnityEngine;

namespace LastElevator.UI.Roster
{
    public sealed class RosterReplaceView : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.045f, 0.05f, 0.065f, 0.99f);
        private static readonly Color NewSurvivorColor = new Color(0.18f, 0.16f, 0.08f, 1f);
        [SerializeField] private RunController _controller;

        private readonly RosterReplaceStyles _styles = new RosterReplaceStyles();

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

        public void SelectReplacement(string survivorIdToRemove)
        {
            if (_controller != null)
            {
                _controller.ChooseSurvivorReplacement(survivorIdToRemove);
            }
        }

        public void RefuseNewSurvivor()
        {
            SelectReplacement(null);
        }

        private void OnGUI()
        {
            if (CurrentView == null || CurrentView.Phase != RunPhase.ReplacingSurvivor ||
                CurrentView.PendingSurvivor == null)
            {
                return;
            }

            _styles.Ensure();

            float margin = Screen.width * 0.04f;
            var panelRect = new Rect(
                margin,
                Screen.height * 0.225f,
                Screen.width - (margin * 2f),
                Screen.height * 0.75f);

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = PanelColor;
            GUI.Box(panelRect, GUIContent.none, _styles.Panel);
            GUI.backgroundColor = previousBackground;

            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.025f), panelRect.width, panelRect.height * 0.07f),
                "ELEVATOR FULL",
                _styles.Title);

            DrawNewSurvivor(panelRect, CurrentView.PendingSurvivor);
            DrawRoster(panelRect, CurrentView.Survivors);

            var refuseRect = new Rect(
                panelRect.x + (panelRect.width * 0.08f),
                panelRect.y + (panelRect.height * 0.89f),
                panelRect.width * 0.84f,
                panelRect.height * 0.08f);

            if (GUI.Button(refuseRect, "LEAVE NEW SURVIVOR", _styles.Button))
            {
                RefuseNewSurvivor();
            }
        }

        private void DrawNewSurvivor(Rect panelRect, SurvivorViewModel survivor)
        {
            var cardRect = new Rect(
                panelRect.x + (panelRect.width * 0.06f),
                panelRect.y + (panelRect.height * 0.11f),
                panelRect.width * 0.88f,
                panelRect.height * 0.2f);
            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = NewSurvivorColor;
            GUI.Box(cardRect, GUIContent.none, _styles.Panel);
            GUI.backgroundColor = previousBackground;

            GUI.Label(
                new Rect(cardRect.x + (cardRect.width * 0.22f), cardRect.y + (cardRect.height * 0.08f), cardRect.width * 0.74f, cardRect.height * 0.22f),
                "NEW SURVIVOR",
                _styles.Section);
            SurvivorCardPresentation.DrawPortrait(
                new Rect(
                    cardRect.x + (cardRect.width * 0.04f),
                    cardRect.y + (cardRect.height * 0.18f),
                    cardRect.width * 0.18f,
                    cardRect.height * 0.65f),
                survivor,
                _styles.Portrait);
            GUI.Label(
                new Rect(cardRect.x + (cardRect.width * 0.22f), cardRect.y + (cardRect.height * 0.3f), cardRect.width * 0.74f, cardRect.height * 0.3f),
                survivor.DisplayName,
                _styles.Name);
            GUI.Label(
                new Rect(cardRect.x + (cardRect.width * 0.22f), cardRect.y + (cardRect.height * 0.62f), cardRect.width * 0.74f, cardRect.height * 0.24f),
                SurvivorCardPresentation.GetDetails(survivor),
                _styles.Detail);
        }

        private void DrawRoster(Rect panelRect, IReadOnlyList<SurvivorViewModel> survivors)
        {
            GUI.Label(
                new Rect(panelRect.x, panelRect.y + (panelRect.height * 0.325f), panelRect.width, panelRect.height * 0.06f),
                "CHOOSE WHO LEAVES",
                _styles.Section);

            if (survivors.Count == 0)
            {
                return;
            }

            float areaY = panelRect.y + (panelRect.height * 0.39f);
            float areaHeight = panelRect.height * 0.46f;
            float gap = panelRect.height * 0.015f;
            float buttonHeight = (areaHeight - (gap * (survivors.Count - 1))) / survivors.Count;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorViewModel survivor = survivors[i];
                var buttonRect = new Rect(
                    panelRect.x + (panelRect.width * 0.06f),
                    areaY + (i * (buttonHeight + gap)),
                    panelRect.width * 0.88f,
                    buttonHeight);

                if (GUI.Button(buttonRect, GUIContent.none, _styles.Button))
                {
                    SelectReplacement(survivor.Id);
                }

                SurvivorCardPresentation.DrawPortrait(
                    new Rect(
                        buttonRect.x + (buttonRect.width * 0.025f),
                        buttonRect.y + (buttonRect.height * 0.12f),
                        buttonRect.height * 0.76f,
                        buttonRect.height * 0.76f),
                    survivor,
                    _styles.Portrait);
                GUI.Label(
                    new Rect(
                        buttonRect.x + (buttonRect.width * 0.18f),
                        buttonRect.y,
                        buttonRect.width * 0.78f,
                        buttonRect.height * 0.48f),
                    survivor.DisplayName.ToUpperInvariant(),
                    _styles.Name);
                GUI.Label(
                    new Rect(
                        buttonRect.x + (buttonRect.width * 0.18f),
                        buttonRect.y + (buttonRect.height * 0.43f),
                        buttonRect.width * 0.78f,
                        buttonRect.height * 0.45f),
                    SurvivorCardPresentation.GetDetails(survivor),
                    _styles.Detail);
            }
        }

        private void HandleRunStateChanged(RunViewModel view)
        {
            CurrentView = view;
        }
    }
}
