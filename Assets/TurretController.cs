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

	public float range = 5;
	public List<string> blockingLayers;

	public LineRenderer laserRenderer;

	public bool activated = true;

	public enum FiringState
	{
		Ready,
		Reloading,
		Firing
	};

	private FiringState firingState;
	private float timer;

	public void Start()
	{
		firingState = FiringState.Reloading;
		timer = reloadTime;

		laserRenderer.enabled = false;
	}

	public void Update()
	{
		PlayerBehavior player = PlayerBehavior.Player;

		if (activated && shouldTrackPlayer())
		{
			gunPivot.rotation = Quaternion.FromToRotation(Vector3.up, player.transform.position - gunPivot.position);

			switch (firingState)
			{
				case FiringState.Ready:
					// Fire!
					firingState = FiringState.Firing;
					timer = fireTime;

					laserRenderer.enabled = true;
					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, player.transform.position);

					break;

				case FiringState.Firing:
					// Update laser
					timer -= Time.deltaTime;

					laserRenderer.SetPosition(0, laserOrigin.position);
					laserRenderer.SetPosition(1, player.transform.position);

					if (timer < 0)
					{
						laserRenderer.enabled = false;
						firingState = FiringState.Reloading;
						timer = reloadTime;
					}
					break;

				case FiringState.Reloading:
					// Wait for reload
					timer -= Time.deltaTime;
					if (timer < 0)
					{
						firingState = FiringState.Ready;
					}
					break;
			}
		}
	}

	bool shouldTrackPlayer()
	{
		// Check if player is in range
		PlayerBehavior player = PlayerBehavior.Player;

		if (Vector2.Distance(gunPivot.position, player.transform.position) < range)
		{
			// Check if within angle limits
			float leftAngle = Quaternion.FromToRotation(Vector3.up, leftLimit.position - gunPivot.position).eulerAngles.z;
			float rightAngle = Quaternion.FromToRotation(Vector3.up, rightLimit.position - gunPivot.position).eulerAngles.z;
			float targetAngle = Quaternion.FromToRotation(Vector3.up, player.transform.position - gunPivot.position).eulerAngles.z;

			float leftDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, targetAngle));
			float rightDelta = Mathf.Abs(Mathf.DeltaAngle(rightAngle, targetAngle));
			float rangeDelta = Mathf.Abs(Mathf.DeltaAngle(leftAngle, rightAngle));

			Debug.Log(string.Format("{0} {1} {2}", leftDelta, rightDelta, rangeDelta));

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
}
