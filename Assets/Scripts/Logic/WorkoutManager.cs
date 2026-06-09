using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AllenamentoPersonale
{
    public enum WorkoutPhase { Idle, Prep, Work, WaitReps, Done }

    /// <summary>
    /// Pure-logic workout runner. No MonoBehaviour – driven by AppController.
    /// All timing is handled via Unity coroutines started from AppController.
    /// </summary>
    public class WorkoutManager
    {
        // ── Config ─────────────────────────────────────────────────
        public const int PrepSeconds = 3;

        // ── State ──────────────────────────────────────────────────
        public List<ExerciseData> Exercises { get; private set; }
        public int  CurrentIndex  { get; private set; }
        public WorkoutPhase Phase { get; private set; } = WorkoutPhase.Idle;
        public int  Countdown     { get; private set; }   // seconds remaining

        // ── Events (subscribed by WorkoutController) ────────────────
        public event Action<int> OnExerciseChanged;   // index
        public event Action<WorkoutPhase> OnPhaseChanged;
        public event Action<int> OnTick;              // countdown value
        public event Action OnWorkoutFinished;

        // ── Internal ───────────────────────────────────────────────
        private Coroutine _coroutine;
        private MonoBehaviour _runner;

        // ── Constructor ────────────────────────────────────────────
        public WorkoutManager(MonoBehaviour coroutineRunner)
        {
            _runner   = coroutineRunner;
            Exercises = ExerciseRepository.Load();
        }

        // ── Public API ─────────────────────────────────────────────
        public void ReloadExercises() => Exercises = ExerciseRepository.Load();

        public void StartWorkout()
        {
            StopWorkout();
            CurrentIndex = 0;
            BeginExercise(0);
        }

        public void StopWorkout()
        {
            if (_coroutine != null)
                _runner.StopCoroutine(_coroutine);
            _coroutine = null;
            Phase = WorkoutPhase.Idle;
        }

        /// <summary>Call this when the user confirms reps are done.</summary>
        public void ConfirmRepsDone()
        {
            if (Phase != WorkoutPhase.WaitReps) return;
            Advance();
        }

        // ── Internal helpers ───────────────────────────────────────
        private void BeginExercise(int index)
        {
            CurrentIndex = index;
            OnExerciseChanged?.Invoke(index);
            _coroutine = _runner.StartCoroutine(RunPrep());
        }

        private IEnumerator RunPrep()
        {
            SetPhase(WorkoutPhase.Prep);
            Countdown = PrepSeconds;
            OnTick?.Invoke(Countdown);

            while (Countdown > 0)
            {
                yield return new WaitForSeconds(1f);
                Countdown--;
                OnTick?.Invoke(Countdown);
            }

            StartWork();
        }

        private void StartWork()
        {
            var ex = Exercises[CurrentIndex];

            if (ex.type == ExerciseType.Timer)
            {
                _coroutine = _runner.StartCoroutine(RunTimer(ex.duration));
            }
            else
            {
                SetPhase(WorkoutPhase.WaitReps);
            }
        }

        private IEnumerator RunTimer(int seconds)
        {
            SetPhase(WorkoutPhase.Work);
            Countdown = seconds;
            OnTick?.Invoke(Countdown);

            while (Countdown > 0)
            {
                yield return new WaitForSeconds(1f);
                Countdown--;
                OnTick?.Invoke(Countdown);
            }

            Advance();
        }

        private void Advance()
        {
            int next = CurrentIndex + 1;
            if (next < Exercises.Count)
            {
                BeginExercise(next);
            }
            else
            {
                SetPhase(WorkoutPhase.Done);
                OnWorkoutFinished?.Invoke();
            }
        }

        private void SetPhase(WorkoutPhase p)
        {
            Phase = p;
            OnPhaseChanged?.Invoke(p);
        }
    }
}
