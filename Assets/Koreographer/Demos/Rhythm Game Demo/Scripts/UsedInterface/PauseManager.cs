
namespace SonicBloom.Koreo.Demos
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Manages game pause and resume using the new Input System.
    /// Toggles a pause UI panel and freezes time via Time.timeScale.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("Action used to toggle pause. Should be a button action like 'Start' or 'Escape'.")]
        [SerializeField]
        public InputActionProperty pauseAction;

        [Header("UI")] [Tooltip("UI panel that will be shown when the game is paused.")] [SerializeField]
        private GameObject pausePanel;

        // Tracks whether the game is currently paused
        private bool isPaused;

        private void OnEnable()
        {
            // Validate and bind input callback
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
            // Unbind input callback
            if (pauseAction.action != null)
            {
                pauseAction.action.performed -= OnPausePerformed;
                pauseAction.action.Disable();
            }
        }

        /// <summary>
        /// Called when the assigned pause input is triggered.
        /// Toggles the current pause state.
        /// </summary>
        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        /// <summary>
        /// Public method to resume the game. Can be linked to a UI button.
        /// </summary>
        public void Resume()
        {
            SetPause(false);
        }

        /// <summary>
        /// Toggles between paused and unpaused states.
        /// </summary>
        private void TogglePause()
        {
            SetPause(!isPaused);
        }

        /// <summary>
        /// Applies pause state: activates panel and freezes/unfreezes game time.
        /// </summary>
        /// <param name="pause">True to pause the game, false to resume.</param>
        private void SetPause(bool pause)
        {
            isPaused = pause;

            // Show/hide pause panel
            if (pausePanel != null)
                pausePanel.SetActive(pause);

            // Freeze or unfreeze time
            Time.timeScale = pause ? 0f : 1f;
        }
    }
}
