using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastElevator.Gameplay.Encounters
{
    [Serializable]
    public sealed class EncounterChoiceData
    {
        public string label;
        public List<ConditionData> conditions = new List<ConditionData>();
        public List<EffectData> successEffects = new List<EffectData>();
        public List<EffectData> failureEffects = new List<EffectData>();

        [Range(0f, 1f)]
        public float successChance = 1f;
    }
}
