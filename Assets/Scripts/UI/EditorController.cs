using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

namespace AllenamentoPersonale
{
    /// <summary>
    /// Full CRUD editor for the exercise list.
    /// Renders a dynamic list of exercise rows and an inline form for
    /// creating / editing a single exercise.
    /// </summary>
    public class EditorController
    {
        public VisualElement Root { get; }

        // ── Working copy ───────────────────────────────────────────
        private List<ExerciseData> _exercises;
        private int _editingIndex = -1;   // -1 = new exercise

        // ── Cached top-level refs ──────────────────────────────────
        private VisualElement _listContainer;
        private VisualElement _formPanel;

        // Form fields
        private TextField    _fieldName;
        private DropdownField _dropType;
        private SliderInt    _sliderDuration;
        private Label        _lblDurationValue;
        private SliderInt    _sliderReps;
        private Label        _lblRepsValue;
        private VisualElement _rowDuration;
        private VisualElement _rowReps;

        // ── Constructor ────────────────────────────────────────────
        public EditorController(VisualElement root, Action onBack)
        {
            Root       = root;
            _exercises = ExerciseRepository.Load();

            // Navigation
            root.Q<Button>("btn-back")
                ?.RegisterCallback<ClickEvent>(_ =>
                {
                    ExerciseRepository.Save(_exercises);
                    onBack?.Invoke();
                });

            root.Q<Button>("btn-reset")
                ?.RegisterCallback<ClickEvent>(_ => ConfirmReset());

            root.Q<Button>("btn-add-new")
                ?.RegisterCallback<ClickEvent>(_ => OpenForm(-1));

            // List container
            _listContainer = root.Q<VisualElement>("list-container");

            // Form panel
            _formPanel = root.Q<VisualElement>("form-panel");

            // Form fields
            _fieldName        = root.Q<TextField>("field-name");
            _dropType         = root.Q<DropdownField>("drop-type");
            _sliderDuration   = root.Q<SliderInt>("slider-duration");
            _lblDurationValue = root.Q<Label>("lbl-duration-value");
            _sliderReps       = root.Q<SliderInt>("slider-reps");
            _lblRepsValue     = root.Q<Label>("lbl-reps-value");
            _rowDuration      = root.Q<VisualElement>("row-duration");
            _rowReps          = root.Q<VisualElement>("row-reps");

            // Wire type dropdown → show/hide rows
            _dropType?.RegisterValueChangedCallback(evt => RefreshFormRows(evt.newValue));

            // Slider labels
            _sliderDuration?.RegisterValueChangedCallback(evt =>
            {
                if (_lblDurationValue != null)
                    _lblDurationValue.text = $"{evt.newValue}s";
            });
            _sliderReps?.RegisterValueChangedCallback(evt =>
            {
                if (_lblRepsValue != null)
                    _lblRepsValue.text = evt.newValue.ToString();
            });

            // Form save / cancel
            root.Q<Button>("btn-form-save")
                ?.RegisterCallback<ClickEvent>(_ => SaveForm());
            root.Q<Button>("btn-form-cancel")
                ?.RegisterCallback<ClickEvent>(_ => CloseForm());

            // Init
            CloseForm();
            RebuildList();
        }

        // ── List rendering ─────────────────────────────────────────

        private void RebuildList()
        {
            _listContainer?.Clear();
            for (int i = 0; i < _exercises.Count; i++)
                _listContainer?.Add(MakeRow(i));
        }

        private VisualElement MakeRow(int index)
        {
            var ex  = _exercises[index];
            var row = new VisualElement();
            row.AddToClassList("exercise-row");

            // Info block
            var info = new VisualElement();
            info.AddToClassList("row-info");

            var name = new Label(ex.name);
            name.AddToClassList("row-name");

            var detail = new Label(ex.type == ExerciseType.Timer
                ? $"⏱ {ex.duration}s"
                : $"✕ {ex.reps} rip.");
            detail.AddToClassList("row-detail");

            info.Add(name);
            info.Add(detail);

            // Buttons block
            var buttons = new VisualElement();
            buttons.AddToClassList("row-buttons");

            var btnUp = new Button(() => MoveExercise(index, -1)) { text = "▲" };
            btnUp.AddToClassList("btn-icon");

            var btnDown = new Button(() => MoveExercise(index, +1)) { text = "▼" };
            btnDown.AddToClassList("btn-icon");

            var btnEdit = new Button(() => OpenForm(index)) { text = "✎" };
            btnEdit.AddToClassList("btn-icon");
            btnEdit.AddToClassList("btn-icon--accent");

            var btnDel = new Button(() => DeleteExercise(index)) { text = "✕" };
            btnDel.AddToClassList("btn-icon");
            btnDel.AddToClassList("btn-icon--danger");

            if (index == 0) btnUp.SetEnabled(false);
            if (index == _exercises.Count - 1) btnDown.SetEnabled(false);

            buttons.Add(btnUp);
            buttons.Add(btnDown);
            buttons.Add(btnEdit);
            buttons.Add(btnDel);

            row.Add(info);
            row.Add(buttons);

            return row;
        }

        // ── CRUD ───────────────────────────────────────────────────

        private void MoveExercise(int index, int direction)
        {
            int target = index + direction;
            if (target < 0 || target >= _exercises.Count) return;
            (_exercises[index], _exercises[target]) = (_exercises[target], _exercises[index]);
            RebuildList();
        }

        private void DeleteExercise(int index)
        {
            if (_exercises.Count <= 1) return;   // keep at least one
            _exercises.RemoveAt(index);
            RebuildList();
        }

        private void ConfirmReset()
        {
            _exercises = ExerciseRepository.GetDefaults();
            RebuildList();
            CloseForm();
        }

        // ── Form ───────────────────────────────────────────────────

        private void OpenForm(int index)
        {
            _editingIndex = index;

            ExerciseData src = index >= 0
                ? new ExerciseData(_exercises[index])
                : new ExerciseData { name = "", type = ExerciseType.Timer, duration = 30, reps = 10 };

            if (_fieldName  != null) _fieldName.value  = src.name;
            if (_dropType   != null) _dropType.value   = src.type == ExerciseType.Timer ? "Timer" : "Ripetizioni";

            if (_sliderDuration != null)
            {
                _sliderDuration.lowValue  = 5;
                _sliderDuration.highValue = 300;
                _sliderDuration.value     = src.duration > 0 ? src.duration : 30;
            }
            if (_lblDurationValue != null) _lblDurationValue.text = $"{src.duration}s";

            if (_sliderReps != null)
            {
                _sliderReps.lowValue  = 1;
                _sliderReps.highValue = 50;
                _sliderReps.value     = src.reps > 0 ? src.reps : 10;
            }
            if (_lblRepsValue != null) _lblRepsValue.text = src.reps.ToString();

            RefreshFormRows(_dropType?.value ?? "Timer");

            if (_formPanel != null) _formPanel.style.display = DisplayStyle.Flex;
        }

        private void CloseForm()
        {
            if (_formPanel != null) _formPanel.style.display = DisplayStyle.None;
            _editingIndex = -1;
        }

        private void SaveForm()
        {
            if (_fieldName == null) return;

            string rawName = _fieldName.value.Trim();
            if (string.IsNullOrEmpty(rawName)) return;

            bool isTimer  = _dropType?.value == "Timer";
            var saved = new ExerciseData
            {
                name     = rawName,
                type     = isTimer ? ExerciseType.Timer : ExerciseType.Reps,
                duration = _sliderDuration?.value ?? 30,
                reps     = _sliderReps?.value ?? 10,
            };

            if (_editingIndex >= 0 && _editingIndex < _exercises.Count)
                _exercises[_editingIndex] = saved;
            else
                _exercises.Add(saved);

            CloseForm();
            RebuildList();
        }

        private void RefreshFormRows(string typeValue)
        {
            bool isTimer = typeValue == "Timer";
            if (_rowDuration != null)
                _rowDuration.style.display = isTimer ? DisplayStyle.Flex : DisplayStyle.None;
            if (_rowReps != null)
                _rowReps.style.display = isTimer ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
