using System;
using UnityEngine;

namespace LastElevator.Gameplay.Survivors
{
    public sealed class SurvivorViewModel
    {
        internal SurvivorViewModel(SurvivorDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Id = definition.id;
            DisplayName = definition.displayName;
            Portrait = definition.portrait;
            Role = definition.role;
            CombatPower = definition.combatPower;
            Passive = definition.passive;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public Sprite Portrait { get; }

        public SurvivorRole Role { get; }

        public int CombatPower { get; }

        public SimplePassive Passive { get; }
    }
}
