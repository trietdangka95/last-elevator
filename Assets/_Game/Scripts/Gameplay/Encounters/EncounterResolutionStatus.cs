namespace LastElevator.Gameplay.Encounters
{
    public enum EncounterResolutionStatus
    {
        Resolved,
        InvalidEncounter,
        InvalidChoice,
        ConditionsNotMet,
        SurvivorReplacementRequired,
        InvalidSurvivorReplacement,
        UnsupportedCondition,
        InvalidEffect,
        UnsupportedEffect
    }
}
