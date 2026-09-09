using System;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Gameplay.Run;
using LastElevator.Gameplay.Survivors;

namespace LastElevator.Gameplay.Encounters
{
    public sealed class EncounterResolver
    {
        private readonly IRandomService _random;
        private readonly SurvivorRoster _roster;

        public EncounterResolver(IRandomService random)
            : this(random, null)
        {
        }

        public EncounterResolver(IRandomService random, SurvivorRoster roster)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _roster = roster;
        }

        public bool CanChoose(RunState state, EncounterChoiceData choice)
        {
            return EvaluateConditions(state, choice) == EncounterResolutionStatus.Resolved;
        }

        public EncounterResolution Resolve(
            RunState state,
            EncounterDefinition encounter,
            int choiceIndex)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (encounter == null || encounter.choices == null)
            {
                return CreateFailure(EncounterResolutionStatus.InvalidEncounter, encounter, choiceIndex);
            }

            if (choiceIndex < 0 || choiceIndex >= encounter.choices.Count ||
                encounter.choices[choiceIndex] == null)
            {
                return CreateFailure(EncounterResolutionStatus.InvalidChoice, encounter, choiceIndex);
            }

            EncounterChoiceData choice = encounter.choices[choiceIndex];
            EncounterResolutionStatus conditionStatus = EvaluateConditions(state, choice);

            if (conditionStatus != EncounterResolutionStatus.Resolved)
            {
                return CreateFailure(conditionStatus, encounter, choiceIndex);
            }

            EncounterResolutionStatus effectStatus = ValidateEffects(choice.successEffects);

            if (effectStatus == EncounterResolutionStatus.Resolved)
            {
                effectStatus = ValidateEffects(choice.failureEffects);
            }

            if (effectStatus != EncounterResolutionStatus.Resolved)
            {
                return CreateFailure(effectStatus, encounter, choiceIndex);
            }

            bool succeeded = RollSuccess(choice.successChance);
            IReadOnlyList<EffectData> selectedEffects = succeeded
                ? choice.successEffects
                : choice.failureEffects;
            SurvivorDefinition survivor = FindSurvivorEffect(selectedEffects);

            if (survivor != null)
            {
                SurvivorRecruitmentStatus recruitment = _roster.TryRecruit(state, survivor);

                if (recruitment == SurvivorRecruitmentStatus.ReplacementRequired)
                {
                    return new EncounterResolution(
                        EncounterResolutionStatus.SurvivorReplacementRequired,
                        encounter.id,
                        choiceIndex,
                        succeeded,
                        _roster.GetDefinition(survivor.id),
                        selectedEffects);
                }

                if (recruitment == SurvivorRecruitmentStatus.InvalidSurvivor)
                {
                    return CreateFailure(EncounterResolutionStatus.InvalidEffect, encounter, choiceIndex);
                }
            }

            ApplyNonSurvivorEffects(state, selectedEffects);
            RecordResolvedEncounter(state, encounter.id);

            return new EncounterResolution(
                EncounterResolutionStatus.Resolved,
                encounter.id,
                choiceIndex,
                succeeded);
        }

        public EncounterResolution CompleteSurvivorReplacement(
            RunState state,
            EncounterResolution pendingResolution,
            string survivorIdToRemove)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (_roster == null || pendingResolution == null ||
                !pendingResolution.RequiresSurvivorReplacement ||
                pendingResolution.PendingSurvivor == null ||
                pendingResolution.DeferredEffects == null)
            {
                return CreateReplacementFailure(pendingResolution);
            }

            bool isRefusal = string.IsNullOrWhiteSpace(survivorIdToRemove);

            if (!isRefusal && !_roster.TryReplace(
                    state,
                    survivorIdToRemove,
                    pendingResolution.PendingSurvivor))
            {
                return CreateReplacementFailure(pendingResolution);
            }

            ApplyNonSurvivorEffects(state, pendingResolution.DeferredEffects);
            RecordResolvedEncounter(state, pendingResolution.EncounterId);

            return new EncounterResolution(
                EncounterResolutionStatus.Resolved,
                pendingResolution.EncounterId,
                pendingResolution.ChoiceIndex,
                pendingResolution.Succeeded);
        }

        private EncounterResolutionStatus EvaluateConditions(
            RunState state,
            EncounterChoiceData choice)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (choice == null)
            {
                return EncounterResolutionStatus.InvalidChoice;
            }

            IReadOnlyList<ConditionData> conditions = choice.conditions;

            if (conditions == null)
            {
                return EncounterResolutionStatus.Resolved;
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                ConditionData condition = conditions[i];

                if (condition == null)
                {
                    return EncounterResolutionStatus.UnsupportedCondition;
                }

                switch (condition.type)
                {
                    case ConditionType.None:
                        break;
                    case ConditionType.MinEnergy:
                        if (state.energy < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinIntegrity:
                        if (state.integrity < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinScrap:
                        if (state.scrap < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.HasFreeCapacity:
                        if (RunRules.GetAvailableCapacity(state) <= 0)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.MinTeamPower:
                        if (_roster == null)
                        {
                            return EncounterResolutionStatus.UnsupportedCondition;
                        }

                        if (_roster.GetTotalPower(state) < condition.intValue)
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    case ConditionType.HasRole:
                        if (_roster == null ||
                            !Enum.TryParse(condition.stringValue, true, out SurvivorRole role) ||
                            !Enum.IsDefined(typeof(SurvivorRole), role))
                        {
                            return EncounterResolutionStatus.UnsupportedCondition;
                        }

                        if (!_roster.HasRole(state, role))
                        {
                            return EncounterResolutionStatus.ConditionsNotMet;
                        }

                        break;
                    default:
                        return EncounterResolutionStatus.UnsupportedCondition;
                }
            }

            return EncounterResolutionStatus.Resolved;
        }

        private EncounterResolutionStatus ValidateEffects(IReadOnlyList<EffectData> effects)
        {
            if (effects == null)
            {
                return EncounterResolutionStatus.Resolved;
            }

            bool hasSurvivorEffect = false;

            for (int i = 0; i < effects.Count; i++)
            {
                EffectData effect = effects[i];

                if (effect == null)
                {
                    return EncounterResolutionStatus.InvalidEffect;
                }

                switch (effect.type)
                {
                    case EffectType.None:
                    case EffectType.AddEnergy:
                    case EffectType.AddScrap:
                        break;
                    case EffectType.AddIntegrity:
                    case EffectType.DamageIntegrity:
                        if (effect.intValue < 0)
                        {
                            return EncounterResolutionStatus.InvalidEffect;
                        }

                        break;
                    case EffectType.SetRunFlag:
                        if (string.IsNullOrWhiteSpace(effect.stringValue))
                        {
                            return EncounterResolutionStatus.InvalidEffect;
                        }

                        break;
                    case EffectType.AddSurvivor:
                        if (hasSurvivorEffect || _roster == null || effect.survivor == null ||
                            _roster.GetDefinition(effect.survivor.id) == null)
                        {
                            return EncounterResolutionStatus.InvalidEffect;
                        }

                        hasSurvivorEffect = true;
                        break;
                    case EffectType.StartCombat:
                    default:
                        return EncounterResolutionStatus.UnsupportedEffect;
                }
            }

            return EncounterResolutionStatus.Resolved;
        }

        private bool RollSuccess(float successChance)
        {
            if (successChance <= 0f)
            {
                return false;
            }

            if (successChance >= 1f)
            {
                return true;
            }

            return _random.Value() < successChance;
        }

        private static SurvivorDefinition FindSurvivorEffect(IReadOnlyList<EffectData> effects)
        {
            if (effects == null)
            {
                return null;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].type == EffectType.AddSurvivor)
                {
                    return effects[i].survivor;
                }
            }

            return null;
        }

        private static void ApplyNonSurvivorEffects(
            RunState state,
            IReadOnlyList<EffectData> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                EffectData effect = effects[i];

                switch (effect.type)
                {
                    case EffectType.None:
                    case EffectType.AddSurvivor:
                        break;
                    case EffectType.AddEnergy:
                        RunRules.ChangeEnergy(state, effect.intValue);
                        break;
                    case EffectType.AddIntegrity:
                        RunRules.RepairIntegrity(state, effect.intValue);
                        break;
                    case EffectType.AddScrap:
                        RunRules.ChangeScrap(state, effect.intValue);
                        break;
                    case EffectType.DamageIntegrity:
                        RunRules.DamageIntegrity(state, effect.intValue);
                        break;
                    case EffectType.SetRunFlag:
                        if (!state.flags.Contains(effect.stringValue))
                        {
                            state.flags.Add(effect.stringValue);
                        }

                        break;
                }
            }
        }

        private static void RecordResolvedEncounter(RunState state, string encounterId)
        {
            if (!string.IsNullOrWhiteSpace(encounterId) &&
                !state.resolvedEncounterIds.Contains(encounterId))
            {
                state.resolvedEncounterIds.Add(encounterId);
            }
        }

        private static EncounterResolution CreateFailure(
            EncounterResolutionStatus status,
            EncounterDefinition encounter,
            int choiceIndex)
        {
            return new EncounterResolution(
                status,
                encounter == null ? null : encounter.id,
                choiceIndex,
                false);
        }

        private static EncounterResolution CreateReplacementFailure(
            EncounterResolution pendingResolution)
        {
            return new EncounterResolution(
                EncounterResolutionStatus.InvalidSurvivorReplacement,
                pendingResolution == null ? null : pendingResolution.EncounterId,
                pendingResolution == null ? -1 : pendingResolution.ChoiceIndex,
                pendingResolution != null && pendingResolution.Succeeded);
        }
    }
}
