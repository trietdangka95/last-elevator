using LastElevator.Gameplay.Survivors;
using UnityEngine;

namespace LastElevator.UI.Roster
{
    internal static class SurvivorCardPresentation
    {
        public static string GetDetails(SurvivorViewModel survivor)
        {
            return $"{survivor.Role.ToString().ToUpperInvariant()}  •  POWER {survivor.CombatPower}  •  {GetPassiveLabel(survivor.Passive)}";
        }

        public static void DrawPortrait(
            Rect portraitRect,
            SurvivorViewModel survivor,
            GUIStyle placeholderStyle)
        {
            if (survivor.Portrait == null)
            {
                GUI.Box(portraitRect, GetInitials(survivor.DisplayName), placeholderStyle);
                return;
            }

            Texture2D texture = survivor.Portrait.texture;
            Rect textureRect = survivor.Portrait.textureRect;
            var textureCoordinates = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
            GUI.DrawTextureWithTexCoords(portraitRect, texture, textureCoordinates, true);
        }

        private static string GetPassiveLabel(SimplePassive passive)
        {
            switch (passive)
            {
                case SimplePassive.ReduceTravelEnergyOne:
                    return "TRAVEL -1";
                case SimplePassive.AddCombatPowerOne:
                    return "TEAM POWER +1";
                case SimplePassive.RepairOnCheckpointFive:
                    return "CHECKPOINT REPAIR +5";
                case SimplePassive.RevealDangerSometimes:
                    return "DANGER SENSE";
                default:
                    return "NO PASSIVE";
            }
        }

        private static string GetInitials(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "?";
            }

            string[] words = displayName.Split(' ');
            return words.Length == 1
                ? words[0].Substring(0, 1).ToUpperInvariant()
                : $"{words[0][0]}{words[words.Length - 1][0]}".ToUpperInvariant();
        }
    }
}
