using UnityEngine;

namespace LastElevator.UI.Roster
{
    internal sealed class RosterReplaceStyles
    {
        private static readonly Color PrimaryTextColor = new Color(0.93f, 0.94f, 0.9f);
        private static readonly Color AccentTextColor = new Color(0.83f, 0.72f, 0.35f);

        private int _screenHeight;

        public GUIStyle Button { get; private set; }

        public GUIStyle Detail { get; private set; }

        public GUIStyle Name { get; private set; }

        public GUIStyle Panel { get; private set; }

        public GUIStyle Portrait { get; private set; }

        public GUIStyle Section { get; private set; }

        public GUIStyle Title { get; private set; }

        public void Ensure()
        {
            if (_screenHeight == Screen.height && Panel != null)
            {
                return;
            }

            _screenHeight = Screen.height;
            Panel = new GUIStyle(GUI.skin.box);
            Title = CreateLabelStyle(0.026f, AccentTextColor, FontStyle.Bold);
            Section = CreateLabelStyle(0.014f, AccentTextColor, FontStyle.Bold);
            Name = CreateLabelStyle(0.023f, PrimaryTextColor, FontStyle.Bold);
            Detail = CreateLabelStyle(0.014f, PrimaryTextColor, FontStyle.Normal);
            Portrait = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.018f),
                fontStyle = FontStyle.Bold,
                normal = { textColor = AccentTextColor }
            };
            Button = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * 0.014f),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = PrimaryTextColor },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };
        }

        private static GUIStyle CreateLabelStyle(float size, Color color, FontStyle fontStyle)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Screen.height * size),
                fontStyle = fontStyle,
                wordWrap = true,
                normal = { textColor = color }
            };
        }
    }
}
