using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace SonicBloom.Koreo.Demos
{
    /// <summary>
    /// Handles pause input, toggles the pause UI, triggers events,
    /// and communicates with the RhythmGameController to pause/resume game logic.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("Action used to toggle pause. Should be a button action like 'Start' or 'Escape'.")]
        [SerializeField] public InputActionProperty pauseAction;

        [Header("UI")]
        [Tooltip("UI panel that will be shown when the game is paused.")]
        [SerializeField] private GameObject pausePanel;

        [Header("Events")]
        public UnityEvent OnPauseUnity;
        public UnityEvent OnResumeUnity;

        // Internal state
        private bool isPaused;

        // Cached reference to RhythmGameController
        private RhythmGameController rhythmGameController;

        private void Awake()
        {
            // Find and cache the RhythmGameController in the scene
            rhythmGameController = FindObjectOfType<RhythmGameController>();

            if (rhythmGameController == null)
            {
                Debug.LogWarning("PauseManager could not find RhythmGameController in the scene.");
            }
        }

        private void OnEnable()
        {
            if (pauseAction.action != null)
            {
                pauseAction.action.Enable();
                pauseAction.action.performed += OnPausePerformed;
            }
            else
            {
                Debug.LogWarning("Pause action is not assigned or has no valid InputAction.");
            }
        }

        private void OnDisable()
        {
            if (pauseAction.action != null)
            {
                pauseAction.action.performed -= OnPausePerformed;
                pauseAction.action.Disable();
            }
        }

        /// <summary>
        /// Input callback for toggling pause.
        /// </summary>
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        /// <summary>
        /// Call to resume the game (e.g. from UI).
        /// </summary>
        public void Resume()
        {
            SetPause(false);
        }

        /// <summary>
        /// Toggles pause state.
        /// </summary>
        private void TogglePause()
        {
            SetPause(!isPaused);
        }

        /// <summary>
        /// Applies pause state, triggers events, and informs RhythmGameController.
        /// </summary>
        private void SetPause(bool pause)
        {
            if (isPaused == pause) return;

            isPaused = pause;

            if (pausePanel != null)
                pausePanel.SetActive(pause);

            // Call RhythmGameController's pause function
            if (rhythmGameController != null)
            {
                rhythmGameController.Pause(pause);
            }

            if (pause)
            {
                OnPauseUnity?.Invoke();
            }
            else
            {
                OnResumeUnity?.Invoke();
            }
        }
    }
}
