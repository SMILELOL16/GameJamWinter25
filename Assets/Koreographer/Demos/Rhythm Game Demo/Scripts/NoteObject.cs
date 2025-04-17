using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
    [AddComponentMenu("Koreographer/Demos/Rhythm Game/Note Object")]
    public class NoteObject : MonoBehaviour
    {
        [Tooltip("The visual to use for this Note Object.")]
        public SpriteRenderer visuals;

        public enum ScaleAxis { Height, Width }
        public enum Feedback { Miss, Good, Perfect }

        [Tooltip("Choose whether to scale the note's height or width based on its duration.")]
        public ScaleAxis scaleAxis = ScaleAxis.Height;

        [Tooltip("The offset is relative to the parent object.")]
        [SerializeField, Range(-180f, 180f)] private float rotationOffset = 0f;

        KoreographyEvent trackedEvent;
        LaneController laneController;
        RhythmGameController gameController;

        private bool isPressed = false;
        private bool pressedOnTime = false;
        private bool releasedOnTime = false;
        private bool hasBeenEvaluated = false;
        
        private int noteStart; // int noteStart
        private int noteEnd; // int noteEnd
        
        public float burn = 0f; // === Burn value (0 to 1 normalized time between noteStart and noteEnd) ===

        public void Initialize(KoreographyEvent evt, Color color, LaneController laneCont, RhythmGameController gameCont)
        {
            Reset();
            trackedEvent = evt;
            visuals.color = color;
            laneController = laneCont;
            gameController = gameCont;

            isPressed = false;
            pressedOnTime = false;
            releasedOnTime = false;
            hasBeenEvaluated = false;

            UpdatePosition();
        }

        void Update()
        {
            if (trackedEvent == null || laneController == null || gameController == null || hasBeenEvaluated)
                return;

            float inputValue = laneController.GetInputValue();
            int currentSample = gameController.DelayedSampleTime;

            noteStart = trackedEvent.StartSample;
            noteEnd = trackedEvent.EndSample;
            
            int hitWindow = gameController.HitWindowSampleWidth;
            if (noteEnd - noteStart <= 0) noteEnd = noteStart + hitWindow;
            
            // === Burn calculation ===
            if (isPressed) burn = Mathf.InverseLerp(noteStart, noteEnd, currentSample);
            
            // === Input handling ===

            if (!isPressed && inputValue > 0.5f &&!laneController.inputConsumed)
            {
                if (currentSample >= noteStart && currentSample <= noteEnd)
                {
                    isPressed = true;
                    pressedOnTime = Mathf.Abs(currentSample - noteStart) <= hitWindow;
                    laneController.RaiseNoteStateChanged(true);
                }
                else if (currentSample > noteEnd)
                {
                    Evaluate(forceMiss: true);
                    return;
                }
            }

            if (isPressed && inputValue <= 0.1f && !hasBeenEvaluated)
            {
                releasedOnTime = Mathf.Abs(currentSample - noteEnd) <= hitWindow;
                laneController.RaiseNoteStateChanged(true);
                Evaluate();
            }

            if (currentSample > noteEnd + hitWindow)
            {
                Evaluate(!isPressed ? true : false);
            }

            ScalePrefab();
            UpdatePosition();
        }

        void Evaluate(bool forceMiss = false)
        {
            if (hasBeenEvaluated) return;
            
            laneController.RaiseNoteStateChanged(false);
            
            Feedback result = Feedback.Miss;
            
            if (forceMiss || (!pressedOnTime && !releasedOnTime))
                result = Feedback.Miss;
            else if (pressedOnTime && releasedOnTime)
                result = Feedback.Perfect;
            else
                result = Feedback.Good;

            laneController.RaiseFeedback(result);
            Debug.Log($"Note Judged: {result}");

            hasBeenEvaluated = true;
            OnClear();
        }

        void ScalePrefab()
        {
            if (visuals == null) return;

            int durationSamples = trackedEvent.EndSample - trackedEvent.StartSample;
            float durationSeconds = durationSamples / (float)gameController.SampleRate;
            float visualLength = durationSeconds * gameController.noteSpeed;

            if (durationSamples <= 0)
                visualLength = gameController.WindowSizeInUnits *2f;

            if (visuals.drawMode != SpriteDrawMode.Sliced)
                visuals.drawMode = SpriteDrawMode.Sliced;

            Vector2 size = visuals.size;
            if (scaleAxis == ScaleAxis.Height)
                size.y = visualLength;
            else
                size.x = visualLength;

            visuals.size = size;
        }

        void UpdatePosition()
        {
            int currentSample = gameController.DelayedSampleTime;
            float secondsToTarget = (trackedEvent.StartSample - currentSample) / (float)gameController.SampleRate;
            float distanceToTarget = secondsToTarget * gameController.noteSpeed;

            Vector3 start = laneController.SpawnPosition;
            Vector3 end = laneController.TargetPosition;
            Vector3 direction = (end - start).normalized;

            transform.position = end - direction * distanceToTarget;

            if (direction != Vector3.zero)
            {
                Quaternion facingRotation = Quaternion.LookRotation(Vector3.forward, direction);
                Quaternion offset = Quaternion.Euler(0f, 0f, rotationOffset);
                transform.rotation = facingRotation * offset;
            }
        }

        public void OnClear()
        {
            gameController.ReturnNoteObjectToPool(this);
        }

        void Reset()
        {
            trackedEvent = null;
            laneController = null;
            gameController = null;
            burn = 0f;
        }
        
        void OnDrawGizmosSelected()
        {
            if (laneController == null || gameController == null || trackedEvent == null)
                return;

            Vector3 start = laneController.SpawnPosition;
            Vector3 end = laneController.TargetPosition;
            Vector3 direction = (end - start).normalized;

            // === StartSample ===
            int startSample = noteStart;
            float startOffset = (startSample - gameController.DelayedSampleTime) / (float)gameController.SampleRate;
            float startDistance = startOffset * gameController.noteSpeed;
            Vector3 startSamplePosition = end - direction * startDistance;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(startSamplePosition, startSamplePosition + Vector3.up * 0.5f);

            // === EndSample ===
            int endSample = noteEnd;
            float endOffset = (endSample - gameController.DelayedSampleTime) / (float)gameController.SampleRate;
            float endDistance = endOffset * gameController.noteSpeed;
            Vector3 endSamplePosition = end - direction * endDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(endSamplePosition, endSamplePosition + Vector3.up * 0.5f);
        }
    }
}
