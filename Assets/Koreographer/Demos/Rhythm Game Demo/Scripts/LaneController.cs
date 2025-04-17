// Top of the file
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SonicBloom.Koreo.Demos
{
    [AddComponentMenu("Koreographer/Demos/Rhythm Game/Lane Controller")]
    public class LaneController : MonoBehaviour
    {
       
        
        
        public Color color = Color.blue;
        public SpriteRenderer targetVisuals;

        [Header("Input System")]
        [Tooltip("Reference an InputAction from your Input Actions asset. Use a Value-type (e.g., Button) that returns 0 or 1.")]
        [SerializeField] public InputActionProperty inputButtonAction;

        [HideInInspector]public bool inputConsumed = false;

        [Header("Payload Filtering")]
        [Tooltip("Only events with these payloads will spawn notes in this lane.")]
        public List<string> matchedPayloads = new List<string>();

        [Header("Track Bounds")]
        public Transform TrackStart;
        public Transform TrackEnd;

        List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();
        public Queue<NoteObject> trackedNotes = new Queue<NoteObject>();

        RhythmGameController gameController;
        int pendingEventIdx = 0;

        Vector3 defaultScale;
        float scaleNormal = 1f;
        float scalePress = 1.4f;
        float scaleHold = 1.2f;

        public Vector3 SpawnPosition => TrackStart.position;
        public Vector3 TargetPosition => TrackEnd.position;
        public float DespawnDistance => Vector3.Distance(TrackStart.position, TrackEnd.position) + 2f;
        
        
        public event System.Action<bool> OnNoteStateChanged;
        public event System.Action<NoteObject.Feedback> OnFeedback;
        public void RaiseFeedback(NoteObject.Feedback feedback)
        {
            OnFeedback?.Invoke(feedback);
        }
        public void RaiseNoteStateChanged(bool isPressed)
        {
            OnNoteStateChanged?.Invoke(isPressed);
            if (!isPressed) inputConsumed = true;
        }

        public void Initialize(RhythmGameController controller)
        {
            gameController = controller;
        }

        public void Restart(int newSampleTime = 0)
        {
            for (int i = 0; i < laneEvents.Count; ++i)
            {
                if (laneEvents[i].StartSample >= newSampleTime)
                {
                    pendingEventIdx = i;
                    break;
                }
            }

            int numToClear = trackedNotes.Count;
            for (int i = 0; i < numToClear; ++i)
            {
                trackedNotes.Dequeue().OnClear();
            }
        }

        void Start()
        {
            if (TrackStart == null || TrackEnd == null)
            {
                Debug.LogError("TrackStart or TrackEnd not assigned.");
                enabled = false;
                return;
            }

            inputButtonAction.action.Enable();

            //targetVisuals.color = color;
            defaultScale = targetVisuals.transform.localScale;
        }

        void Update()
        {
            
            
            while (trackedNotes.Count > 0 && trackedNotes.Peek() == null)
            {
                trackedNotes.Dequeue();
            }

            CheckSpawnNext();

            float input = GetInputValue();

            if (input > 0.5f)
                SetScalePress();
            else if (input > 0f)
                SetScaleHold();
            else
            {
                SetScaleDefault();
                inputConsumed = false;
            }
            
        }

        public float GetInputValue()
        {
            return inputButtonAction.action?.ReadValue<float>() ?? 0f;
        }

        void CheckSpawnNext()
        {
            int samplesToTarget = GetSpawnSampleOffset();
            int currentTime = gameController.DelayedSampleTime;

            while (pendingEventIdx < laneEvents.Count &&
                   laneEvents[pendingEventIdx].StartSample < currentTime + samplesToTarget)
            {
                KoreographyEvent evt = laneEvents[pendingEventIdx];

                NoteObject newObj = gameController.GetFreshNoteObject();
                newObj.transform.position = TrackStart.position;
                newObj.Initialize(evt, color, this, gameController);

                trackedNotes.Enqueue(newObj);
                pendingEventIdx++;
            }
        }

        int GetSpawnSampleOffset()
        {
            float distanceToTravel = Vector3.Distance(TrackStart.position, TrackEnd.position);
            double secondsToTarget = distanceToTravel / gameController.noteSpeed;
            return (int)(secondsToTarget * gameController.SampleRate);
        }

        public void AddEventToLane(KoreographyEvent evt)
        {
            laneEvents.Add(evt);
        }

        public bool DoesMatchPayload(string payload)
        {
            return matchedPayloads.Contains(payload);
        }

        void AdjustScale(float multiplier)
        {
            targetVisuals.transform.localScale = defaultScale * multiplier;
        }

        public void SetScaleDefault() => AdjustScale(scaleNormal);
        public void SetScalePress() => AdjustScale(scalePress);
        public void SetScaleHold() => AdjustScale(scaleHold);
    }
}
