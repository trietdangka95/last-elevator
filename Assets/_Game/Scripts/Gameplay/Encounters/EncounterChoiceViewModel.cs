namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterChoiceViewModel
    {
        internal EncounterChoiceViewModel(int index, string label, bool isAvailable)
        {
            Index = index;
            Label = label;
            IsAvailable = isAvailable;
        }

        public int Index { get; }

        public string Label { get; }

        public bool IsAvailable { get; }
    }
}
