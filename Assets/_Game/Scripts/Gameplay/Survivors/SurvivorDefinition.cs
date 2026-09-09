using UnityEngine;

namespace LastElevator.Gameplay.Survivors
{
    [CreateAssetMenu(fileName = "Survivor", menuName = "LastElevator/Survivor")]
    public sealed class SurvivorDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite portrait;
        public SurvivorRole role;

        [Range(0, 5)]
        public int combatPower;

        public SimplePassive passive;
    }
}
