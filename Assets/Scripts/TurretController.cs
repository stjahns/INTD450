using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TurretController : MonoBehaviour
{
	public Transform gunPivot; // use for aiming gun
	public Transform laserOrigin;

	public Transform leftLimit;
	public Transform rightLimit;

	public float fireTime = 0.1f;
	public float reloadTime = 1.0f;
	public float trackTime = 1.0f;

	public float range = 5;
	public List<string> blockingLayers;
	public List<string> targetableTags;

	public LineRenderer laserRenderer;

	public bool activated = true;

	public AudioClip firingSound;

	public GameObject explosionPrefab;

	public enum FiringState
	{
		Ready,
		Reloading,
		Firing
	};

	public enum TrackingState
	{
		TrackingTarget,
		TrackingRight,
		TrackingLeft
	};

	private GameObject currentTarget;
	private List<GameObject> targets;

	private FiringState firingState;
	private TrackingState trackingState;

	private float firingTimer;
	private float trackingTimer;

	public void Start()
	{
		firingState = FiringState.Reloading;
		firingTimer = reloadTime;

		trackingTimer = 0;

		laserRenderer.enabled = false;

		trackingState = TrackingState.TrackingRight;

		targets = new List<GameObject>();
		currentTarget = null;
	}

	public void Update()
	{
		if (!activated)
		{
			return;
		}

		if (currentTarget)
		{
			// Check if lost target / new target
			currentTarget = GetTarget();
			if (!currentTarget) 
			{
				trackingState = TrackingState.TrackingRight;
				trackingTimer = 0f;
			}
		}
		else
		{
			// Check if acquired target
			currentTarget = GetTarget();

			if (currentTarget) 
			{
				trackingState = TrackingState.TrackingTarget;
			}

		}

		switch (trackingState)
		{
			case TrackingState.TrackingTarget:
				TrackTarget();
				break;

			case TrackingState.TrackingRight:
				{
					trackingTimer += Time.deltaTime;

					Quaternion rightRotation = Quaternion.FromToRotation(Vector3.up, rightLimit.position - gunPivot.position);
					gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, rightRotation, trackingTimer / trackTime);

					if (trackingTimer > trackTime)
					{
						trackingState = TrackingState.TrackingLeft;
						trackingTimer = 0f;
					}


				}
				break;

			case TrackingState.TrackingLeft:
				{
					trackingTimer += Time.deltaTime;

					Quaternion leftRotation = Quaternion.FromToRotation(Vector3.up, leftLimit.position - gunPivot.position);
					gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, leftRotation, trackingTimer / trackTime);

					if (trackingTimer > trackTime)
					{
						trackingState = TrackingState.TrackingRight;
						trackingTimer = 0f;
					}
				}
				break;
		}
	}

	void TrackTarget()
	{
		if (!currentTarget)
		{
			return;
		}

		gunPivot.rotation = Quaternion.FromToRotation(Vector3.up, currentTarget.transform.position - gunPivot.position);

		switch (firingState)
		{
			case FiringState.Ready:
				// Fire!
				firingState = FiringState.Firing;
				firingTimer = fireTime;

				if (firingSound)
				{
					AudioSource.PlayClipAtPoint(firingSound, transform.position);
				}

				// TODO calculate hit position with raycast

				laserRenderer.enabled = true;

				if (currentTarget)
				{
					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, currentTarget.transform.position);
				}

				break;

			case FiringState.Firing:
				// Update laser
				firingTimer -= Time.deltaTime;

				// TODO calculate hit position with raycast

				if (currentTarget)
				{
					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, currentTarget.transform.position);
				}

				if (firingTimer < 0)
				{
					laserRenderer.enabled = false;
					firingState = FiringState.Reloading;
					firingTimer = reloadTime;

					// TODO check if blocked by shield or whatever with raycast
					currentTarget.SendMessage("TakeDamage", 1, SendMessageOptions.DontRequireReceiver);
				}
				break;

			case FiringState.Reloading:
				// Wait for reload
				firingTimer -= Time.deltaTime;
				if (firingTimer < 0)
				{
					firingState = FiringState.Ready;
				}
				break;
		}
	}

	// Find a valid target within range, if any
	// FIXME really inefficient if lots of targets...
	GameObject GetTarget()
	{
		// Get all targetable objects in range
		targets.Clear();
		foreach (string tag in targetableTags)
		{
			targets.AddRange(
					GameObject.FindGameObjectsWithTag(tag).Where(t =>
						Vector2.Distance(gunPivot.position, t.transform.position) < range));
		}

		// Sort targets by distance
		targets.Sort((t1, t2) => 
				Vector2.Distance(gunPivot.position, t1.transform.position).CompareTo(
				Vector2.Distance(gunPivot.position, t2.transform.position)));

		foreach (GameObject target in targets)
		{
			// Check if within angle limits
			float leftAngle = Quaternion.FromToRotation(Vector3.up, leftLimit.position - gunPivot.position).eulerAngles.z;
			float rightAngle = Quaternion.FromToRotation(Vector3.up, rightLimit.position - gunPivot.position).eulerAngles.z;
			float targetAngle = Quaternion.FromToRotation(Vector3.up, target.transform.position - gunPivot.position).eulerAngles.z;

			float leftDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, targetAngle));
			float rightDelta = Mathf.Abs(Mathf.DeltaAngle(rightAngle, targetAngle));
			float rangeDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, rightAngle));

			if (!Mathf.Approximately(leftDelta + rightDelta, rangeDelta))
			{
				// nope
				continue;
			}

			// Check if line of sight (nothing blocking)
			int layerMask = 0;
			blockingLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
			RaycastHit2D hit = Physics2D.Linecast(gunPivot.position, target.transform.position, layerMask);
			if (!hit)
			{
				return target;
			}
		}

		// No target :(
		return null;
	}

	[InputSocket]
	public void Activate()
	{
		activated = true;
	}

	[InputSocket]
	public void Deactivate()
	{
		activated = false;
	}

	[InputSocket]
	public void Kill()
	{
		activated = false;

		if (explosionPrefab)
		{
			Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}
}
