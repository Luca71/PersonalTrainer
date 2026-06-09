using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AllenamentoPersonale
{
    /// <summary>
    /// Drives the workout screen.
    /// Subscribes to WorkoutManager events and updates UIToolkit elements.
    /// </summary>
    public class WorkoutController
    {
        public VisualElement Root { get; }

        private readonly WorkoutManager _manager;

        // Cached UI refs
        private Label         _lblIndex;
        private Label         _lblName;
        private Label         _lblPhase;
        private Label         _lblTimer;
        private Label         _lblReps;
        private ProgressBar   _progress;
        private Button        _btnRepsDone;
        private VisualElement _timerBlock;
        private VisualElement _repsBlock;

        public WorkoutController(
            VisualElement root,
            WorkoutManager manager,
            Action onFinished)
        {
            Root     = root;
            _manager = manager;

            // Cache elements
            _lblIndex    = root.Q<Label>("lbl-index");
            _lblName     = root.Q<Label>("lbl-name");
            _lblPhase    = root.Q<Label>("lbl-phase");
            _lblTimer    = root.Q<Label>("lbl-timer");
            _lblReps     = root.Q<Label>("lbl-reps");
            _progress    = root.Q<ProgressBar>("progress-bar");
            _btnRepsDone = root.Q<Button>("btn-reps-done");
            _timerBlock  = root.Q<VisualElement>("timer-block");
            _repsBlock   = root.Q<VisualElement>("reps-block");

            // Wire button
            _btnRepsDone?.RegisterCallback<ClickEvent>(_ => _manager.ConfirmRepsDone());

            // Subscribe to manager events
            _manager.OnExerciseChanged  += HandleExerciseChanged;
            _manager.OnPhaseChanged     += HandlePhaseChanged;
            _manager.OnTick             += HandleTick;
            _manager.OnWorkoutFinished  += () => onFinished?.Invoke();

            // Initial state: hide interactive reps button
            SetRepsBlockVisible(false);
        }

        // ── Event handlers ─────────────────────────────────────────

        private void HandleExerciseChanged(int index)
        {
            var ex    = _manager.Exercises[index];
            var total = _manager.Exercises.Count;

            if (_lblIndex != null) _lblIndex.text = $"Esercizio {index + 1} / {total}";
            if (_lblName  != null) _lblName.text  = ex.name;

            // Reset sub-widgets
            SetRepsBlockVisible(false);
            if (_timerBlock  != null) _timerBlock.style.display  = DisplayStyle.Flex;
            if (_lblReps     != null) _lblReps.text = "";

            // Configure progress bar range
            if (_progress != null)
            {
                _progress.lowValue  = 0;
                _progress.highValue = ex.type == ExerciseType.Timer
                    ? ex.duration
                    : WorkoutManager.PrepSeconds;
            }
        }

        private void HandlePhaseChanged(WorkoutPhase phase)
        {
            switch (phase)
            {
                case WorkoutPhase.Prep:
                    SetPhaseLabel("⏱ Preparati…", "phase-prep");
                    if (_progress != null)
                    {
                        _progress.highValue = WorkoutManager.PrepSeconds;
                        _progress.value     = WorkoutManager.PrepSeconds;
                    }
                    SetRepsBlockVisible(false);
                    if (_timerBlock != null) _timerBlock.style.display = DisplayStyle.Flex;
                    break;

                case WorkoutPhase.Work:
                    SetPhaseLabel("🔥 In corso…", "phase-work");
                    var ex = _manager.Exercises[_manager.CurrentIndex];
                    if (_progress != null)
                    {
                        _progress.highValue = ex.duration;
                        _progress.value     = ex.duration;
                    }
                    SetRepsBlockVisible(false);
                    if (_timerBlock != null) _timerBlock.style.display = DisplayStyle.Flex;
                    break;

                case WorkoutPhase.WaitReps:
                    SetPhaseLabel("Esegui le ripetizioni", "phase-work");
                    if (_timerBlock != null) _timerBlock.style.display = DisplayStyle.None;
                    var repEx = _manager.Exercises[_manager.CurrentIndex];
                    if (_lblReps != null) _lblReps.text = $"{repEx.reps} rip.";
                    SetRepsBlockVisible(true);
                    break;
            }
        }

        private void HandleTick(int countdown)
        {
            var phase = _manager.Phase;

            if (phase == WorkoutPhase.Prep)
            {
                if (_lblTimer != null)
                    _lblTimer.text = countdown > 0 ? countdown.ToString() : "VAI!";
                if (_progress != null) _progress.value = countdown;
            }
            else if (phase == WorkoutPhase.Work)
            {
                int mins = countdown / 60;
                int secs = countdown % 60;
                if (_lblTimer != null)
                    _lblTimer.text = $"{mins}:{secs:D2}";
                if (_progress != null) _progress.value = countdown;
            }
        }

        // ── Helpers ────────────────────────────────────────────────

        private void SetPhaseLabel(string text, string ussClass)
        {
            if (_lblPhase == null) return;
            _lblPhase.text = text;
            _lblPhase.RemoveFromClassList("phase-prep");
            _lblPhase.RemoveFromClassList("phase-work");
            _lblPhase.AddToClassList(ussClass);
        }

        private void SetRepsBlockVisible(bool visible)
        {
            if (_repsBlock   != null)
                _repsBlock.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_btnRepsDone != null)
            {
                _btnRepsDone.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                _btnRepsDone.SetEnabled(visible);
            }
        }
    }
}
