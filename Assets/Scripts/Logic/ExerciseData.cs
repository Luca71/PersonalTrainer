using System;
using System.Collections.Generic;
using UnityEngine;

namespace AllenamentoPersonale
{
    public enum ExerciseType { Timer, Reps }

    [Serializable]
    public class ExerciseData
    {
        public string name;
        public ExerciseType type;
        public int duration;   // seconds – used when type == Timer
        public int reps;       // count   – used when type == Reps

        public ExerciseData() { }

        public ExerciseData(ExerciseData src)
        {
            name     = src.name;
            type     = src.type;
            duration = src.duration;
            reps     = src.reps;
        }
    }

    // ---------------------------------------------------------------
    // Persistent save / load (PlayerPrefs JSON)
    // ---------------------------------------------------------------
    [Serializable]
    internal class ExerciseListWrapper
    {
        public List<ExerciseData> items;
    }

    public static class ExerciseRepository
    {
        private const string PREF_KEY = "workout_exercises_v1";

        public static List<ExerciseData> Load()
        {
            if (PlayerPrefs.HasKey(PREF_KEY))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<ExerciseListWrapper>(
                        PlayerPrefs.GetString(PREF_KEY));
                    if (wrapper?.items != null && wrapper.items.Count > 0)
                        return wrapper.items;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ExerciseRepository] Load failed: {e.Message}");
                }
            }
            return GetDefaults();
        }

        public static void Save(List<ExerciseData> exercises)
        {
            var wrapper = new ExerciseListWrapper { items = exercises };
            PlayerPrefs.SetString(PREF_KEY, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(PREF_KEY);
            PlayerPrefs.Save();
        }

        public static List<ExerciseData> GetDefaults() => new List<ExerciseData>
        {
            new() { name = "Camminata veloce sul posto",  type = ExerciseType.Timer, duration = 60 },
            new() { name = "Rotazioni delle spalle",      type = ExerciseType.Timer, duration = 30 },
            new() { name = "Circonduzioni delle braccia", type = ExerciseType.Timer, duration = 30 },
            new() { name = "Slanci leggeri delle gambe",  type = ExerciseType.Timer, duration = 60 },
            new() { name = "Squat leggeri",               type = ExerciseType.Timer, duration = 60 },
            new() { name = "Mobilità collo e anche",      type = ExerciseType.Timer, duration = 60 },
            new() { name = "Squat a corpo libero",        type = ExerciseType.Reps,  reps = 15 },
            new() { name = "Piegamenti al muro",          type = ExerciseType.Reps,  reps = 12 },
            new() { name = "Ponte glutei",                type = ExerciseType.Reps,  reps = 15 },
            new() { name = "Rematore caricato",           type = ExerciseType.Reps,  reps = 12 },
            new() { name = "Plank",                       type = ExerciseType.Timer, duration = 30 },
        };
    }
}
