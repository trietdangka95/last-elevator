using System;

namespace LastElevator.Core.State
{
    [Serializable]
    public sealed class PlayerSaveData
    {
        public int schemaVersion = 1;
        public int bestFloor;
        public int runsStarted;
        public int runsWon;
        public bool tutorialCompleted;

        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool hapticsEnabled = true;
    }
}
