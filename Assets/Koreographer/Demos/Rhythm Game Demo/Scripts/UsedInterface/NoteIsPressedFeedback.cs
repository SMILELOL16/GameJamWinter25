
namespace SonicBloom.Koreo.Demos
{
    using UnityEngine;

    /// <summary>
    /// Copies the parent SpriteRenderer's width and height as localScale when enabled.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class NoteIsPressedFeedback : MonoBehaviour
    {
        private SpriteRenderer _parentSpriteRenderer;
        private SpriteRenderer _renderer;
        
        [Tooltip("Target NoteObject to read burn value from.")]
        private NoteObject note;
        [Tooltip("Minimum alpha when burn is 0.")]
        public float minAlpha = 0f;

        [Tooltip("Maximum alpha when burn is 1.")]
        public float maxAlpha = 1f;
        private void OnEnable()
        {
            Invoke(nameof(DelayedSizeMatch), 0.1f);
        }
        private void DelayedSizeMatch()
        {
            CacheParentSpriteRenderer();
            MatchSize();
        }

        private void CacheParentSpriteRenderer()
        {
            if (_parentSpriteRenderer == null && transform.parent != null)
            {
                _parentSpriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
                _renderer = GetComponent<SpriteRenderer>();

                // Defensive coding: Warn if not found
                if (_parentSpriteRenderer == null)
                {
                    Debug.LogWarning($"{nameof(NoteIsPressedFeedback)}: Parent does not have a SpriteRenderer.");
                }
                note = GetComponentInParent<NoteObject>();
            }
        }

        private void MatchSize()
        {
            if (_parentSpriteRenderer == null || _parentSpriteRenderer.sprite == null)
                return;

            _renderer.size = _parentSpriteRenderer.size;
        }
        
        private void Update()
        {
            if (note == null || _renderer == null) return;

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, note.burn);

            Color color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }
}