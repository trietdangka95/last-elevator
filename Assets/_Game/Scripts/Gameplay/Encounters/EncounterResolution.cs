namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterResolution
    {
        internal EncounterResolution(
            EncounterResolutionStatus status,
            string encounterId,
            int choiceIndex,
            bool succeeded)
        {
            Status = status;
            EncounterId = encounterId;
            ChoiceIndex = choiceIndex;
            Succeeded = succeeded;
        }

        public EncounterResolutionStatus Status { get; }

        public string EncounterId { get; }

        public int ChoiceIndex { get; }

        public bool Succeeded { get; }

        public bool IsResolved => Status == EncounterResolutionStatus.Resolved;
    }
}
