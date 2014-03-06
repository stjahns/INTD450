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
		TrackingPlayer,
		TrackingRight,
		TrackingLeft
	};

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
	}

	public void Update()
	{
		if (!activated)
		{
			return;
		}

		if (shouldTrackPlayer())
		{
			trackingState = TrackingState.TrackingPlayer;
		}

		switch (trackingState)
		{
			case TrackingState.TrackingPlayer:
				TrackPlayer();
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

	void TrackPlayer()
	{
		PlayerBehavior player = PlayerBehavior.Player;
		if (!player)
		{
			return;
		}

		if (!shouldTrackPlayer())
		{
			trackingState = TrackingState.TrackingRight;
			trackingTimer = 0f;
		}

		gunPivot.rotation = Quaternion.FromToRotation(Vector3.up, player.transform.position - gunPivot.position);

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

				if (player)
				{
					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, player.transform.position);
				}

				break;

			case FiringState.Firing:
				// Update laser
				firingTimer -= Time.deltaTime;

				// TODO calculate hit position with raycast

				if (player)
				{
					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, player.transform.position);
				}

				if (firingTimer < 0)
				{
					laserRenderer.enabled = false;
					firingState = FiringState.Reloading;
					firingTimer = reloadTime;

					// TEMP -- destroy player
					// TODO check if blocked by shield or whatever with raycast
					if (player)
					{
						player.Die();
					}
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

	bool shouldTrackPlayer()
	{
		// Check if player is in range
		PlayerBehavior player = PlayerBehavior.Player;

		if (!player)
		{
			return false;
		}

		if (Vector2.Distance(gunPivot.position, player.transform.position) < range)
		{
			// Check if within angle limits
			float leftAngle = Quaternion.FromToRotation(Vector3.up, leftLimit.position - gunPivot.position).eulerAngles.z;
			float rightAngle = Quaternion.FromToRotation(Vector3.up, rightLimit.position - gunPivot.position).eulerAngles.z;
			float targetAngle = Quaternion.FromToRotation(Vector3.up, player.transform.position - gunPivot.position).eulerAngles.z;

			float leftDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, targetAngle));
			float rightDelta = Mathf.Abs(Mathf.DeltaAngle(rightAngle, targetAngle));
			float rangeDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, rightAngle));

			if (!Mathf.Approximately(leftDelta + rightDelta, rangeDelta))
			{
				return false;
			}

			// Check if line of sight (nothing blocking)
			int layerMask = 0;
			blockingLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
			RaycastHit2D hit = Physics2D.Linecast(gunPivot.position, player.transform.position, layerMask);
			if (!hit)
			{
				return true;
			}
		}

		return false;
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
