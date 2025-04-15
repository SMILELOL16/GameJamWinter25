using UnityEngine;
using TMPro;
using System.Collections;

namespace SonicBloom.Koreo.Demos
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FeedbackDisplay : MonoBehaviour
    {
        [SerializeField, Tooltip("How long to display the feedback text (in seconds).")]
        private float displayDuration = 1.5f;

        [SerializeField, Tooltip("Reference to the LaneController to subscribe to.")]
        private LaneController laneController;

        private TextMeshProUGUI textComponent;
        private Coroutine displayRoutine;

        void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            textComponent.text = string.Empty;
        }

        void OnEnable()
        {
            if (laneController != null)
                laneController.OnFeedback += HandleFeedback;
        }

        void OnDisable()
        {
            if (laneController != null)
                laneController.OnFeedback -= HandleFeedback;
        }

        void HandleFeedback(NoteObject.Feedback feedback)
        {
            if (displayRoutine != null)
                StopCoroutine(displayRoutine);

            displayRoutine = StartCoroutine(DisplayFeedbackRoutine(feedback.ToString()));
        }

        IEnumerator DisplayFeedbackRoutine(string message)
        {
            textComponent.text = message;
            yield return new WaitForSeconds(displayDuration);
            textComponent.text = string.Empty;
        }
    }
}