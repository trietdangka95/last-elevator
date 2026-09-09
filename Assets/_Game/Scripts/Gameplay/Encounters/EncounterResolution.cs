using System.Collections.Generic;
using LastElevator.Gameplay.Survivors;

namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterResolution
    {
        internal EncounterResolution(
            EncounterResolutionStatus status,
            string encounterId,
            int choiceIndex,
            bool succeeded,
            SurvivorDefinition pendingSurvivor = null,
            IReadOnlyList<EffectData> deferredEffects = null)
        {
            Status = status;
            EncounterId = encounterId;
            ChoiceIndex = choiceIndex;
            Succeeded = succeeded;
            PendingSurvivor = pendingSurvivor;
            DeferredEffects = deferredEffects;
        }

        public EncounterResolutionStatus Status { get; }

        public string EncounterId { get; }

        public int ChoiceIndex { get; }

        public bool Succeeded { get; }

        public SurvivorDefinition PendingSurvivor { get; }

        public bool IsResolved => Status == EncounterResolutionStatus.Resolved;

        public bool RequiresSurvivorReplacement =>
            Status == EncounterResolutionStatus.SurvivorReplacementRequired;

        internal IReadOnlyList<EffectData> DeferredEffects { get; }
    }
}
