using UnityEngine;

namespace LastElevator.Data.Definitions
{
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "LastElevator/Balance Config")]
    public sealed class BalanceConfig : ScriptableObject
    {
        public FloorGenerationConfig floorGeneration = new FloorGenerationConfig();
    }
}
