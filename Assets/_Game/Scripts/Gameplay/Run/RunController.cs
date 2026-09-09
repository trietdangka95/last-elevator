using System;
using System.Collections;
using System.Collections.Generic;
using LastElevator.Core.Random;
using LastElevator.Core.State;
using LastElevator.Data.Definitions;
using LastElevator.Gameplay.Encounters;
using LastElevator.Gameplay.Floor;
using LastElevator.Gameplay.Survivors;
using UnityEngine;

namespace LastElevator.Gameplay.Run
{
    public sealed class RunController : MonoBehaviour
    {
        private const int StartingFloor = 1;
        private const float DefaultTravelDurationSeconds = 0.65f;

        [SerializeField] private BalanceConfig _balanceConfig;
        [SerializeField] private List<EncounterDefinition> _encounters = new List<EncounterDefinition>();
        [SerializeField] private List<SurvivorDefinition> _survivors = new List<SurvivorDefinition>();
        [SerializeField, Min(0f)] private float _travelDurationSeconds = DefaultTravelDurationSeconds;

        private EncounterDefinition _currentEncounter;
        private EncounterResolver _encounterResolver;
        private IReadOnlyList<FloorCandidate> _floorCandidates = Array.Empty<FloorCandidate>();
        private FloorGenerator _floorGenerator;
        private RunPhase _phase;
        private RunState _state;
        private SurvivorRoster _survivorRoster;
        private EncounterResolution _pendingSurvivorResolution;
        private Coroutine _travelRoutine;
        private EncounterDefinition _travelEncounter;
        private int _travelTargetFloor;

        public event Action<RunViewModel> RunStateChanged;
        public event Action<EncounterViewModel> EncounterStarted;
        public event Action<EncounterResolution> EncounterResolved;

        public RunViewModel CurrentView { get; private set; }

        private void Start()
        {
            StartNewRun();
        }

        public void StartNewRun(int? seed = null)
        {
            if (_travelRoutine != null)
            {
                StopCoroutine(_travelRoutine);
                _travelRoutine = null;
            }

            int resolvedSeed = seed ?? CreateSeed();
            _state = RunRules.CreateInitialState(resolvedSeed);
            _state.currentFloor = StartingFloor;
            _phase = RunPhase.ChoosingFloor;
            _currentEncounter = null;
            _pendingSurvivorResolution = null;
            _travelEncounter = null;
            _travelTargetFloor = 0;

            FloorGenerationConfig floorConfig = GetFloorGenerationConfig();
            var random = new SeededRandomService(resolvedSeed);
            _survivorRoster = new SurvivorRoster(GetSurvivorDefinitions());
            _encounterResolver = new EncounterResolver(random, _survivorRoster);
            _floorGenerator = new FloorGenerator(
                random,
                floorConfig,
                GetEncounterDefinitions(),
                _survivorRoster);
            _floorCandidates = _floorGenerator.GenerateCandidates(_state);
            PublishView();
        }

        public void ChooseFloor(int floor)
        {
            if (_phase != RunPhase.ChoosingFloor)
            {
                LogIgnoredChoice($"Ignored floor choice {floor} while phase is {_phase}.");
                return;
            }

            FloorCandidate selected = FindCandidate(floor);

            if (selected == null)
            {
                LogIgnoredChoice($"Ignored unavailable floor choice {floor}.");
                return;
            }

            int passiveTravelReduction = _survivorRoster.GetTravelEnergyReduction(_state);

            if (!RunRules.TrySpendTravelEnergy(
                    _state,
                    selected.Distance,
                    passiveTravelReduction))
            {
                LogIgnoredChoice($"Ignored unaffordable floor choice {floor}.");
                return;
            }

            _phase = RunPhase.Travelling;
            _travelEncounter = selected.Encounter;
            _travelTargetFloor = selected.TargetFloor;
            _floorCandidates = Array.Empty<FloorCandidate>();
            PublishView();

            _travelRoutine = StartCoroutine(CompleteTravel());
        }

        private IEnumerator CompleteTravel()
        {
            yield return new WaitForSecondsRealtime(_travelDurationSeconds);

            _state.currentFloor = _travelTargetFloor;
            _currentEncounter = _travelEncounter;
            _travelEncounter = null;
            _travelTargetFloor = 0;
            _phase = RunPhase.Encounter;
            _travelRoutine = null;
            PublishView();
            EncounterStarted?.Invoke(CurrentView.CurrentEncounter);
        }

        public void ChooseEncounterOption(int optionIndex)
        {
            if (_phase != RunPhase.Encounter)
            {
                LogIgnoredChoice($"Ignored encounter choice {optionIndex} while phase is {_phase}.");
                return;
            }

            _phase = RunPhase.Resolving;
            PublishView();

            EncounterResolution resolution = _encounterResolver.Resolve(
                _state,
                _currentEncounter,
                optionIndex);

            if (resolution.RequiresSurvivorReplacement)
            {
                _pendingSurvivorResolution = resolution;
                _phase = RunPhase.ReplacingSurvivor;
                PublishView();
                return;
            }

            if (!resolution.IsResolved)
            {
                _phase = RunPhase.Encounter;
                PublishView();
                LogIgnoredChoice($"Ignored encounter choice {optionIndex}: {resolution.Status}.");
                return;
            }

            CompleteEncounter(resolution);
        }

        public void ChooseSurvivorReplacement(string survivorIdToRemove)
        {
            if (_phase != RunPhase.ReplacingSurvivor)
            {
                LogIgnoredChoice($"Ignored survivor replacement while phase is {_phase}.");
                return;
            }

            EncounterResolution resolution = _encounterResolver.CompleteSurvivorReplacement(
                _state,
                _pendingSurvivorResolution,
                survivorIdToRemove);

            if (!resolution.IsResolved)
            {
                LogIgnoredChoice($"Ignored survivor replacement: {resolution.Status}.");
                return;
            }

            _pendingSurvivorResolution = null;
            CompleteEncounter(resolution);
        }

        private FloorCandidate FindCandidate(int floor)
        {
            for (int i = 0; i < _floorCandidates.Count; i++)
            {
                FloorCandidate candidate = _floorCandidates[i];

                if (candidate.TargetFloor == floor)
                {
                    return candidate;
                }
            }

            return null;
        }

        private FloorGenerationConfig GetFloorGenerationConfig()
        {
            if (_balanceConfig == null)
            {
                throw new InvalidOperationException("RunController requires a BalanceConfig reference.");
            }

            if (_balanceConfig.floorGeneration == null)
            {
                throw new InvalidOperationException("BalanceConfig requires floor generation settings.");
            }

            return _balanceConfig.floorGeneration;
        }

        private IReadOnlyList<EncounterDefinition> GetEncounterDefinitions()
        {
            if (_encounters == null || _encounters.Count == 0)
            {
                throw new InvalidOperationException("RunController requires at least one EncounterDefinition.");
            }

            return _encounters;
        }

        private IReadOnlyList<SurvivorDefinition> GetSurvivorDefinitions()
        {
            if (_survivors == null || _survivors.Count == 0)
            {
                throw new InvalidOperationException("RunController requires SurvivorDefinition references.");
            }

            return _survivors;
        }

        private void CompleteEncounter(EncounterResolution resolution)
        {
            _currentEncounter = null;
            _phase = RunPhase.ChoosingFloor;
            _floorCandidates = _floorGenerator.GenerateCandidates(_state);
            PublishView();
            EncounterResolved?.Invoke(resolution);
        }

        private void PublishView()
        {
            EncounterViewModel encounterView = _currentEncounter == null
                ? null
                : new EncounterViewModel(_currentEncounter, _state, _encounterResolver);
            CurrentView = new RunViewModel(
                _state,
                _phase,
                _travelTargetFloor,
                _floorCandidates,
                encounterView,
                _survivorRoster,
                _pendingSurvivorResolution == null
                    ? null
                    : _pendingSurvivorResolution.PendingSurvivor);
            RunStateChanged?.Invoke(CurrentView);
        }

        private int CreateSeed()
        {
            return unchecked((Environment.TickCount * 397) ^ GetInstanceID());
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private static void LogIgnoredChoice(string message)
        {
            Debug.LogWarning(message);
        }
    }
}
