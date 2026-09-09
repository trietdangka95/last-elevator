using System.Collections.Generic;
using UnityEngine;

namespace LastElevator.Gameplay.Encounters
{
    [CreateAssetMenu(fileName = "Encounter", menuName = "LastElevator/Encounter")]
    public sealed class EncounterDefinition : ScriptableObject
    {
        public string id;
        public string title;

        [TextArea]
        public string body;

        public Sprite illustration;
        public EncounterCategory category;
        public string clueText;
        public int weight = 10;
        public int minFloor = 1;
        public int maxFloor = 29;
        public List<EncounterChoiceData> choices = new List<EncounterChoiceData>();
    }
}
