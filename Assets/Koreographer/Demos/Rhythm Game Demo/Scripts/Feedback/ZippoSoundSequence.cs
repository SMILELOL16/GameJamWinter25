using UnityEngine;
using UnityEngine.Events;
using Ami.BroAudio;
using Ami.BroAudio.Runtime;
using UnityEngine.Serialization;

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

        private Animator _animator;
        [SerializeField]private string _animatorStateBool = "IsZippoOpen";

        private void OnEnable()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null) Debug.Log("Animator not found");
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
            _animator.SetBool(_animatorStateBool, active);
        }
    }
}