using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
    public class ScoreManager : MonoBehaviour
    {
        [Tooltip("Reference to the LaneController that this tracker will listen to.")]
        [SerializeField] private LaneController lane;

        private int totalNotes = 0;
        private int perfectCount = 0;
        private int goodCount = 0;
        private int missCount = 0;

        [Tooltip("If Misses exceed this percentage of total notes, the player loses (1-100).")]
        [Range(1, 100)]
        [SerializeField] private int lossThresholdPercent = 40;

        void OnEnable()
        {
            if (lane != null)
            {
                lane.OnFeedback += HandleFeedback;
                totalNotes = lane.trackedNotes.Count;
                Debug.Log("Total notes: " + totalNotes);
            }
        }

        void OnDisable()
        {
            if (lane != null)
            {
                lane.OnFeedback -= HandleFeedback;
            }
        }

        private void HandleFeedback(NoteObject.Feedback feedback)
        {
            switch (feedback)
            {
                case NoteObject.Feedback.Perfect:
                    perfectCount++;
                    break;
                case NoteObject.Feedback.Good:
                    goodCount++;
                    break;
                case NoteObject.Feedback.Miss:
                    missCount++;
                    break;
            }

           if (totalNotes > 0)
            {
                float missRatio = (missCount / (float)totalNotes) * 100f;

                if (missRatio > lossThresholdPercent)
                {
                    Debug.Log($"Player lost! Missed {missRatio:F0}% of notes.");
                    // Optional: Trigger game over logic here
                }
            }
        }
    }
}