//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2024 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/Rhythm Game/Note Object")]
	public class NoteObject : MonoBehaviour
	{
		#region Fields

		[Tooltip("The visual to use for this Note Object.")]
		public SpriteRenderer visuals;
		public enum ScaleAxis { Height, Width }

		[Tooltip("Choose whether to scale the note's height or width based on its duration.")]
		public ScaleAxis scaleAxis = ScaleAxis.Height;
		[Tooltip("The offset is relative to the parent object.")][Range(-180f, 180f)]
		[SerializeField]private float rotationOffset = 0f;

		// If active, the KoreographyEvent that this Note Object wraps.  Contains the relevant timing information.
		KoreographyEvent trackedEvent;

		// If active, the Lane Controller that this Note Object is contained by.
		LaneController laneController;

		// If active, the Rhythm Game Controller that controls the game this Note Object is found within.
		RhythmGameController gameController;

		#endregion
		#region Static Methods
		
		// Unclamped Lerp.  Same as Vector3.Lerp without the [0.0-1.0] clamping.
		static Vector3 Lerp(Vector3 from, Vector3 to, float t)
		{
			return new Vector3 (from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
		}

		#endregion
		#region Methods

		// Prepares the Note Object for use.
		public void Initialize(KoreographyEvent evt, Color color, LaneController laneCont, RhythmGameController gameCont)
		{
			trackedEvent = evt;
			visuals.color = color;
			laneController = laneCont;
			gameController = gameCont;

			UpdatePosition();
		}

		// Resets the Note Object to its default state.
		void Reset()
		{
			trackedEvent = null;
			laneController = null;
			gameController = null;
		}

		void Update()
		{
			ScalePrefab();
			UpdatePosition();

			float distanceFromEnd = Vector3.Distance(transform.position, laneController.TargetPosition);
			if (distanceFromEnd > laneController.DespawnDistance)
			{
				gameController.ReturnNoteObjectToPool(this);
				Reset();
			}
		}

		// Updates the height of the Note Object.  This is relative to the speed at which the notes fall and 
		//  the specified Hit Window range.
		void ScalePrefab()
		{
			int durationSamples = trackedEvent.EndSample - trackedEvent.StartSample;
			float durationSeconds = durationSamples / (float)gameController.SampleRate;
			float visualLength = durationSeconds * gameController.noteSpeed;

			// Fallback for tap notes
			if (durationSamples <= 0)
			{
				visualLength = gameController.WindowSizeInUnits * 2f;
			}

			// Get base sprite size in world units
			float baseUnitWidth = visuals.sprite.rect.width / visuals.sprite.pixelsPerUnit;
			float baseUnitHeight = visuals.sprite.rect.height / visuals.sprite.pixelsPerUnit;
			

			// Set SpriteRenderer draw mode to Tiled (needed to use .size)
			if (visuals.drawMode != SpriteDrawMode.Tiled)
			{
				visuals.drawMode = SpriteDrawMode.Tiled;
			}

			// Copy current size to preserve the axis we’re not changing
			Vector2 size = visuals.size;

			if (scaleAxis == ScaleAxis.Height)
			{
				size.y = visualLength; // Height in world units
			}
			else // Width
			{
				size.x = visualLength; // Width in world units
			}
			visuals.size = size;
		}




		// Updates the position of the Note Object along the Lane based on current audio position.
		// Interpolates between TrackStart and TrackEnd based on sample time
		void UpdatePosition()
		{
			int noteSample = trackedEvent.StartSample;
			int currentSample = gameController.DelayedSampleTime;

			float sampleOffset = (noteSample - currentSample);
			float secondsToTarget = sampleOffset / (float)gameController.SampleRate;
			float distanceToTarget = secondsToTarget * gameController.noteSpeed;

			Vector3 start = laneController.SpawnPosition;
			Vector3 end = laneController.TargetPosition;
			Vector3 direction = (end - start).normalized;

			// Set position
			transform.position = end - direction * distanceToTarget;

			// Set rotation to face direction and apply offset
			if (direction != Vector3.zero)
			{
				// Default rotation facing direction
				Quaternion facingRotation = Quaternion.LookRotation(Vector3.forward, direction);
				
				Quaternion offset = Quaternion.Euler(0f, 0f, rotationOffset);

				// Final rotation
				transform.rotation = facingRotation * offset;
			}
		}


		// Checks to see if the Note Object is currently hittable or not based on current audio sample
		//  position and the configured hit window width in samples (this window used during checks for both
		//  before/after the specific sample time of the Note Object).
		public bool IsNoteHittable()
		{
			int noteTime = trackedEvent.StartSample;
			int curTime = gameController.DelayedSampleTime;
			int hitWindow = gameController.HitWindowSampleWidth;

			return (Mathf.Abs(noteTime - curTime) <= hitWindow);
		}

		// Checks to see if the note is no longer hittable based on the configured hit window width in
		//  samples.
		public bool IsNoteMissed()
		{
			bool bMissed = true;

			if (enabled)
			{
				int noteTime = trackedEvent.StartSample;
				int curTime = gameController.DelayedSampleTime;
				int hitWindow = gameController.HitWindowSampleWidth;

				bMissed = (curTime - noteTime > hitWindow);
			}
			
			return bMissed;
		}

		// Returns this Note Object to the pool which is controlled by the Rhythm Game Controller.  This
		//  helps reduce runtime allocations.
		void ReturnToPool()
		{
			gameController.ReturnNoteObjectToPool(this);
			Reset();
		}

		// Performs actions when the Note Object is hit.
		public void OnHit()
		{
			ReturnToPool();
		}

		// Performs actions when the Note Object is cleared.
		public void OnClear()
		{
			ReturnToPool();
		}

		#endregion
		void OnDrawGizmosSelected()
		{
			if (laneController == null || gameController == null || trackedEvent == null)
				return;

			Vector3 start = laneController.SpawnPosition;
			Vector3 end = laneController.TargetPosition;
			Vector3 direction = (end - start).normalized;

			// === Compute rotation with offset ===
			Quaternion baseRotation = Quaternion.LookRotation(Vector3.forward, direction);
			Quaternion offsetRotation = Quaternion.Euler(0f, 0f, rotationOffset);
			Quaternion finalRotation = baseRotation * offsetRotation;

			Vector3 rotatedForward = finalRotation * Vector3.up;

			// === StartSample ===
			int startSample = trackedEvent.StartSample;
			float startOffset = (startSample - gameController.DelayedSampleTime) / (float)gameController.SampleRate;
			float startDistance = startOffset * gameController.noteSpeed;
			Vector3 startSamplePosition = end - direction * startDistance;

			Gizmos.color = Color.green;
			Gizmos.DrawLine(startSamplePosition, startSamplePosition + rotatedForward * 0.5f);

			// === EndSample ===
			int endSample = trackedEvent.EndSample;
			float endOffset = (endSample - gameController.DelayedSampleTime) / (float)gameController.SampleRate;
			float endDistance = endOffset * gameController.noteSpeed;
			Vector3 endSamplePosition = end - direction * endDistance;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(endSamplePosition, endSamplePosition + rotatedForward * 0.5f);
		}


	}
}
