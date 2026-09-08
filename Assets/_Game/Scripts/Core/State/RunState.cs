using System;
using System.Collections.Generic;

namespace LastElevator.Core.State
{
    [Serializable]
    public sealed class RunState
    {
        public int seed;
        public int currentFloor;
        public int energy;
        public int maxEnergy;
        public int integrity;
        public int maxIntegrity;
        public int capacity;
        public int scrap;
        public int travelEnergyReduction;

        public List<string> survivorIds = new List<string>();
        public List<string> ownedUpgradeIds = new List<string>();
        public List<string> resolvedEncounterIds = new List<string>();
        public List<string> flags = new List<string>();

        public bool isRunOver;
        public bool isVictory;
    }
}
