using UnityEngine;
using System.Collections.Generic;
using SonicBloom.Koreo.Demos;

namespace SonicBloom.Koreo.Demos
{
    [AddComponentMenu("Koreographer/Demos/Rhythm Game/Lane Controller")]
    public class LaneController : MonoBehaviour
    {
        #region Fields

        public Color color = Color.blue;
        public SpriteRenderer targetVisuals;
        public KeyCode keyboardButton;
        public List<string> matchedPayloads = new List<string>();

        [Header("Track Bounds")]
        public Transform TrackStart;
        public Transform TrackEnd;

        List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();
        Queue<NoteObject> trackedNotes = new Queue<NoteObject>();

        RhythmGameController gameController;
        int pendingEventIdx = 0;

        Vector3 defaultScale;
        float scaleNormal = 1f;
        float scalePress = 1.4f;
        float scaleHold = 1.2f;

        #endregion

        #region Properties

        public Vector3 SpawnPosition => TrackStart.position;
        public Vector3 TargetPosition => TrackEnd.position;

        // Despawn zone can be slightly after the end
        public float DespawnDistance => Vector3.Distance(TrackStart.position, TrackEnd.position) + 2f;

        #endregion

        #region Methods

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
                Debug.LogError("TrackStart or TrackEnd not assigned in LaneController.");
                enabled = false;
                return;
            }

            targetVisuals.color = color;
            defaultScale = targetVisuals.transform.localScale;
        }

        void Update()
        {
            while (trackedNotes.Count > 0 && trackedNotes.Peek().IsNoteMissed())
            {
                trackedNotes.Dequeue();
            }

            CheckSpawnNext();

            if (Input.GetKeyDown(keyboardButton))
            {
                CheckNoteHit();
                SetScalePress();
            }
            else if (Input.GetKey(keyboardButton))
            {
                SetScaleHold();
            }
            else if (Input.GetKeyUp(keyboardButton))
            {
                SetScaleDefault();
            }
        }

        void AdjustScale(float multiplier)
        {
            targetVisuals.transform.localScale = defaultScale * multiplier;
        }

        int GetSpawnSampleOffset()
        {
            float distanceToTravel = Vector3.Distance(TrackStart.position, TrackEnd.position);
            double secondsToTarget = distanceToTravel / gameController.noteSpeed;
            return (int)(secondsToTarget * gameController.SampleRate);
        }

        public void CheckNoteHit()
        {
            if (trackedNotes.Count > 0 && trackedNotes.Peek().IsNoteHittable())
            {
                NoteObject hitNote = trackedNotes.Dequeue();
                hitNote.OnHit();
            }
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

                // Start at TrackStart and move toward TrackEnd
                newObj.transform.position = TrackStart.position;
                newObj.Initialize(evt, color, this, gameController);

                trackedNotes.Enqueue(newObj);
                pendingEventIdx++;
            }
        }

        public void AddEventToLane(KoreographyEvent evt)
        {
            laneEvents.Add(evt);
        }

        public bool DoesMatchPayload(string payload)
        {
            foreach (var item in matchedPayloads)
            {
                if (payload == item)
                    return true;
            }
            return false;
        }

        public void SetScaleDefault() => AdjustScale(scaleNormal);
        public void SetScalePress() => AdjustScale(scalePress);
        public void SetScaleHold() => AdjustScale(scaleHold);

        #endregion
    }
}
