using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Floor;
using UnityEngine;

namespace LastElevator.UI.FloorChoice
{
    internal static class FloorSignalPresentation
    {
        private static readonly Color SurvivorColor = new Color(0.25f, 0.47f, 0.43f);
        private static readonly Color ResourceColor = new Color(0.43f, 0.39f, 0.2f);
        private static readonly Color EnemyColor = new Color(0.48f, 0.2f, 0.2f);
        private static readonly Color HazardColor = new Color(0.5f, 0.31f, 0.18f);
        private static readonly Color MysteryColor = new Color(0.28f, 0.27f, 0.42f);
        private static readonly Color MandatoryColor = new Color(0.48f, 0.36f, 0.12f);

        internal static string GetCardText(FloorCandidate candidate)
        {
            return $"FLOOR {candidate.TargetFloor:00}\n{GetSignalText(candidate.SignalCategory)}\n" +
                   $"{GetDangerText(candidate.SignalCategory)}\n-{candidate.EnergyCost} ENERGY\nGO";
        }

        internal static Color GetColor(EncounterCategory category)
        {
            switch (category)
            {
                case EncounterCategory.Survivor:
                    return SurvivorColor;
                case EncounterCategory.Resource:
                    return ResourceColor;
                case EncounterCategory.Enemy:
                    return EnemyColor;
                case EncounterCategory.Hazard:
                    return HazardColor;
                case EncounterCategory.Mystery:
                    return MysteryColor;
                default:
                    return MandatoryColor;
            }
        }

        private static string GetSignalText(EncounterCategory category)
        {
            switch (category)
            {
                case EncounterCategory.Survivor:
                    return "DISTRESS SIGNAL";
                case EncounterCategory.Resource:
                    return "POWER SURGE";
                case EncounterCategory.Enemy:
                    return "SCRATCHING SOUNDS";
                case EncounterCategory.Hazard:
                    return "STRUCTURAL ALERT";
                case EncounterCategory.Mystery:
                    return "UNKNOWN BROADCAST";
                case EncounterCategory.Checkpoint:
                    return "CHECKPOINT SIGNAL";
                case EncounterCategory.Boss:
                    return "FINAL SIGNAL";
                default:
                    return "NO SIGNAL";
            }
        }

        private static string GetDangerText(EncounterCategory category)
        {
            switch (category)
            {
                case EncounterCategory.Boss:
                    return "DANGER: EXTREME";
                case EncounterCategory.Enemy:
                    return "DANGER: HIGH";
                case EncounterCategory.Hazard:
                    return "DANGER: MEDIUM";
                case EncounterCategory.Mystery:
                    return "DANGER: UNKNOWN";
                default:
                    return "DANGER: LOW";
            }
        }
    }
}
