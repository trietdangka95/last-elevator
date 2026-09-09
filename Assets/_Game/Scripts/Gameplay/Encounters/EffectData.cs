using System;
using LastElevator.Gameplay.Survivors;

namespace LastElevator.Gameplay.Encounters
{
    [Serializable]
    public sealed class EffectData
    {
        public EffectType type;
        public int intValue;
        public string stringValue;
        public SurvivorDefinition survivor;
    }
}
