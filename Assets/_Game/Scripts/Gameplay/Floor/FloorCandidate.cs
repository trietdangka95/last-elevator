using LastElevator.Gameplay.Encounters;

namespace LastElevator.Gameplay.Floor
{
    public sealed class FloorCandidate
    {
        internal FloorCandidate(
            int targetFloor,
            int distance,
            int energyCost,
            FloorBand band,
            EncounterCategory signalCategory)
        {
            TargetFloor = targetFloor;
            Distance = distance;
            EnergyCost = energyCost;
            Band = band;
            SignalCategory = signalCategory;
        }

        public int TargetFloor { get; }

        public int Distance { get; }

        public int EnergyCost { get; }

        public FloorBand Band { get; }

        public EncounterCategory SignalCategory { get; }

        public bool IsMandatory =>
            Band == FloorBand.CheckpointA ||
            Band == FloorBand.CheckpointB ||
            Band == FloorBand.Boss;
    }
}
