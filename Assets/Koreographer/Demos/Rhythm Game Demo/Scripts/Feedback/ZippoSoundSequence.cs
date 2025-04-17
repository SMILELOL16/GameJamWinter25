using UnityEngine;
using UnityEngine.Events;
using Ami.BroAudio;
using Ami.BroAudio.Runtime;

namespace SonicBloom.Koreo.Demos
{
    public class ZippoSoundSequence : MonoBehaviour
    {
        [Header("Lane Controller")]
        public LaneController laneController;
        private bool inputConsumed;

        [Header("BroAudio Sound IDs")]
        public SoundID zippoOpen;
        public SoundID zippoStrike;
        public SoundID zippoClose;
        
        [Header("Unity Events")]
        [Header("Hook up additional actions or effects ")]
        [Header("to be triggered when a note starts or ends.")]
        public UnityEvent noteStart;
        public UnityEvent noteEnd;

        private void OnEnable()
        {
            if (laneController != null)
            {
                laneController.OnNoteStateChanged += HandleNoteStateChanged;
            }
        }

        private void OnDisable()
        {
            if (laneController != null)
            {
                laneController.OnNoteStateChanged -= HandleNoteStateChanged;
            }
        }

        private void HandleNoteStateChanged(bool active)
        {
            if (active)
            {
                inputConsumed = true;
                noteStart?.Invoke();
                SoundManager.Instance.Play(zippoOpen);
                SoundManager.Instance.Play(zippoStrike);
            }
            else
            {
                if (!inputConsumed) return;
                inputConsumed = false;
                noteEnd?.Invoke();
                SoundManager.Instance.Play(zippoClose);
            }
        }
    }
}