using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

namespace AllenamentoPersonale
{
    /// <summary>
    /// Single MonoBehaviour on the scene root. Owns the WorkoutManager and
    /// all screen controllers, wires them together, and mediates navigation.
    ///
    /// Setup in Unity:
    ///   1. Create an empty GameObject "App" in the scene.
    ///   2. Attach this script.
    ///   3. Add a UIDocument component on the same GameObject.
    ///   4. Assign your PanelSettings and the root UXML (RootDocument.uxml).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class AppController : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────
        [Header("Screen UXML Assets")]
        [SerializeField] private VisualTreeAsset homeScreenAsset;
        [SerializeField] private VisualTreeAsset workoutScreenAsset;
        [SerializeField] private VisualTreeAsset finishScreenAsset;
        [SerializeField] private VisualTreeAsset editorScreenAsset;

        // ── Runtime ────────────────────────────────────────────────
        private UIDocument   _doc;
        private VisualElement _root;
        private VisualElement _screenContainer;

        private WorkoutManager _workoutManager;

        private HomeController    _home;
        private WorkoutController _workout;
        private FinishController  _finish;
        private EditorController  _editor;

        // ── Unity lifecycle ────────────────────────────────────────
        private void Awake()
        {
            _doc  = GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            // The root UXML should contain a single container element
            // named "screen-container" that we swap screens into.
            _screenContainer = _root.Q<VisualElement>("screen-container");

            if (_screenContainer == null)
            {
                // Fallback: use root itself
                _screenContainer = _root;
            }

            _workoutManager = new WorkoutManager(this);
        }

        private void Start()
        {
            ShowHome();
        }

        // ── Navigation ─────────────────────────────────────────────
        public void ShowHome()
        {
            ClearScreen();
            _workoutManager.ReloadExercises();
            _home = new HomeController(
                homeScreenAsset.CloneTree(),
                _workoutManager.Exercises.Count,
                onStart:       ShowWorkout,
                onOpenEditor:  ShowEditor);
            _screenContainer.Add(_home.Root);
        }

        public void ShowWorkout()
        {
            ClearScreen();
            _workout = new WorkoutController(
                workoutScreenAsset.CloneTree(),
                _workoutManager,
                onFinished: ShowFinish);
            _screenContainer.Add(_workout.Root);
            _workoutManager.StartWorkout();
        }

        public void ShowFinish()
        {
            ClearScreen();
            _finish = new FinishController(
                finishScreenAsset.CloneTree(),
                onRepeat: ShowWorkout,
                onHome:   ShowHome);
            _screenContainer.Add(_finish.Root);
        }

        public void ShowEditor()
        {
            ClearScreen();
            _editor = new EditorController(
                editorScreenAsset.CloneTree(),
                onBack: ShowHome);
            _screenContainer.Add(_editor.Root);
        }

        // ── Helpers ────────────────────────────────────────────────
        private void ClearScreen()
        {
            _workoutManager.StopWorkout();
            _screenContainer.Clear();
        }
    }
}
