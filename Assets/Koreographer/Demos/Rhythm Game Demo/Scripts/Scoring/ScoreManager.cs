using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SonicBloom.Koreo.Demos
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Lane Tracking")]
        [Tooltip("Reference to the LaneController that this tracker will listen to.")]
        [SerializeField] private LaneController lane;

        [Header("Note Tracking")]
        [Tooltip("Total notes to expect. Set this in the Inspector.")]
        [Min(1)]
        [SerializeField] private int totalNotes = 1;
        [SerializeField]private int _perfectCount;
        [SerializeField]private int _goodCount;
        [SerializeField]private int _missCount;

        [Header("Loss Condition")]
        [Tooltip("If Misses exceed this percentage of total notes, the player loses (1-100).")]
        [Range(1, 100)]
        [SerializeField] private int lossThresholdPercent = 40;

        [Header("Events")]
        [Tooltip("Triggered when Miss count exceeds loss threshold.")]
        public UnityEvent MissThresholdExceededUnity;
        [Tooltip("Triggered when sum of all the hits and misses is equal to total notes.")]
        public UnityEvent SongCompletedUnity;
        public event Action MissThresholdExceeded;
        public event Action SongCompleted;

        

        private void OnEnable()
        {
            if (lane != null)
            {
                lane.OnFeedback += HandleFeedback;
            }
            _perfectCount = 0;
            _goodCount = 0;
            _missCount = 0;
        }

        private void OnDisable()
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
                    _perfectCount++;
                    break;
                case NoteObject.Feedback.Good:
                    _goodCount++;
                    break;
                case NoteObject.Feedback.Miss:
                    _missCount++;
                    break;
            }

            EvaluateScore();
        }

        private void EvaluateScore()
        {
            int totalHits = _perfectCount + _goodCount + _missCount;

            if (totalHits >= totalNotes)
            {
                TriggerGameWon();
                return;
            }

            float missRatio = (_missCount / (float)totalNotes) * 100f;
            if (missRatio > lossThresholdPercent)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            Debug.Log($"Player lost! Missed {(_missCount / (float)totalNotes) * 100f:F0}% of notes.");
            MissThresholdExceeded?.Invoke();
            MissThresholdExceededUnity?.Invoke();
        }

        private void TriggerGameWon()
        {
            Debug.Log("Player won! All notes evaluated.");
            SongCompleted?.Invoke();
            SongCompletedUnity?.Invoke();
        }
    }
}
