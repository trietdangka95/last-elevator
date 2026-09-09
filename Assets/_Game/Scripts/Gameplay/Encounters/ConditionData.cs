using System;

namespace LastElevator.Gameplay.Encounters
{
    [Serializable]
    public sealed class ConditionData
    {
        public ConditionType type;
        public int intValue;
        public string stringValue;
    }
}
