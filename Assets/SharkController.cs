using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SharkController : StateMachineBase
{
	public enum State
	{
		Waiting,
		Pacing,
		Attack,
		NomSteak
	}

	public State initialState;
	public float thrustDelta;
	public float hoverBounceHeight;

	private float _targetAltitude;
	private bool _maintainAltitude = true;

	private bool _hasSteak = false;

	void Start ()
	{
		currentState = initialState;

		int layerMask = 1 << LayerMask.NameToLayer("Ground");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, Mathf.Infinity, layerMask);
		if (hit)
		{
			_targetAltitude = Vector2.Distance(transform.position, hit.point) + hoverBounceHeight;
		}
	}

	override protected void FixedUpdate ()
	{
		base.FixedUpdate();

		// Maintain altitude 
		int layerMask = 1 << LayerMask.NameToLayer("Ground");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, Mathf.Infinity, layerMask);
		if (hit && rigidbody2D)
		{
			float altitude = Vector2.Distance(transform.position, hit.point);

			// get equalization thrust force
			Vector2 gravity = Physics2D.gravity * rigidbody2D.mass;

			Vector2 thrust = -gravity;

			if (_maintainAltitude)
			{
				if (altitude < _targetAltitude)
				{
					thrust += Vector2.up * thrustDelta;
				}
				else
				{
					thrust -= Vector2.up * thrustDelta;
				}

				// if get bounced up or down, need to return to target height
				// without increasing the bounce height
				if (altitude > _targetAltitude + hoverBounceHeight ||
					altitude < _targetAltitude - hoverBounceHeight)
				{
					rigidbody2D.drag = 0.5f;
				}
				else
				{
					rigidbody2D.drag = 0;
				}
			}

			rigidbody2D.AddForce(thrust);
		}
	}

	void FacePoint(Vector3 point)
	{
		Vector3 scale = transform.localScale;
		if (transform.position.x < point.x)
		{
			// face right
			scale.x = 1;
		}
		else
		{
			// face left
			scale.x = -1;
		}
		transform.localScale = scale;
	}

	public void SetTarget(Transform newTarget, bool force = false)
	{
		if (_hasSteak)
		{
			return;
		}

		if ((State)currentState != State.Attack || force)
		{
			if (target != newTarget)
			{
				target = newTarget;
				Debug.Log("NEW TARGET");
				currentState = State.Attack;
			}
		}
		else if (newTarget == null)
		{
			Debug.Log("NULL TARGET");
			currentState = State.Pacing;
		}
	}

	//--------------------------------------------------------------------------------
	// Waiting
	//--------------------------------------------------------------------------------

	[System.Serializable]
	public class WaitingParameters
	{
		public float stopForceGain = 1;
	}

	public WaitingParameters waitParams;

	void Waiting_FixedUpdate()
	{
		// Make sure we aren't moving laterally..
		if (rigidbody2D)
		{
			float targetSpeed = 0;
			float currentSpeed = rigidbody2D.velocity.x;
			float error = targetSpeed - currentSpeed;
			float force = waitParams.stopForceGain * error;
			rigidbody2D.AddForce(Vector2.right * force);
		}
	}

	IEnumerator Waiting_ExitState()
	{
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// Pacing
	//--------------------------------------------------------------------------------


	[System.Serializable]
	public class PacingParameters
	{
		public float paceTime = 10f;
		public float paceSpeed = 1f;
		public float paceForce = 1f;

		public List<Transform> waypoints;
	}

	public PacingParameters pacingParams;

	private int currentWaypointIndex;
	private Transform currentWaypoint;

	IEnumerator Pacing_EnterState()
	{
		Debug.Log("Entered Pacing");

		currentWaypointIndex = 0;
		currentWaypoint = pacingParams.waypoints[currentWaypointIndex];

		// Return to waiting after paceTime
		yield return new WaitForSeconds(pacingParams.paceTime);
		//currentState = State.Waiting;
	}

	void Pacing_Update()
	{
		FacePoint(currentWaypoint.position);
	}

	void Pacing_FixedUpdate()
	{
		float distanceToWaypoint = Mathf.Abs(transform.position.x - currentWaypoint.position.x);

		if (distanceToWaypoint < 0.1)
		{
			// reached waypoint, switch to next waypoint
			currentWaypointIndex += 1;
			currentWaypointIndex %= pacingParams.waypoints.Count;
			currentWaypoint = pacingParams.waypoints[currentWaypointIndex];
		}

		// Move to waypoint
		Vector2 toWaypoint = currentWaypoint.position - transform.position;

		// If speed in direction of waypoint is less than pacing speed,
		// apply force in waypoint's direction

		if (toWaypoint.x > 0 && rigidbody2D.velocity.x < pacingParams.paceSpeed)
		{
			rigidbody2D.AddForce(Vector3.right * pacingParams.paceForce);
		}

		// TODO what if going too fast in right direction?

		if (toWaypoint.x < 0 && rigidbody2D.velocity.x > -pacingParams.paceSpeed)
		{
			rigidbody2D.AddForce(Vector3.left * pacingParams.paceForce);
		}
	}

	IEnumerator Pacing_ExitState()
	{
		Debug.Log("Exited Pacing");
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// Attack
	//--------------------------------------------------------------------------------

	IEnumerator Attack_EnterState()
	{
		Debug.Log("Entered Attack");
		_maintainAltitude = false;
		yield return 0;
	}

	public Transform target;

	public float moveSpeedModifier = 1;
	public float maxMoveSpeed = 1;
	public float moveAccelGain = 1;
	public float maxMoveForce = 1;
	public float attackDistance = 1;

	void Attack_Update()
	{
		if (_hasSteak)
		{
			return;
		}

		if (target)
		{
			// face target
			FacePoint(target.position);

			if (Vector2.Distance(target.position, transform.position) < attackDistance * 2)
			{
				if (target.tag == "robo-steak")
				{
					/*
					foreach (var collider in target.GetComponentsInChildren<Collider2D>())
					{
						Destroy(collider);
					}
					*/
				}
			}
			
			// when reach target, destroy it, return to pacing
			if (Vector2.Distance(target.position, transform.position) < attackDistance)
			{
				if (target.tag != "robo-steak")
				{
					var destructable = target.GetComponent<DestructableBehaviour>();
					if (destructable)
					{
						destructable.Explode();
					}

					var player = target.GetComponent<PlayerBehavior>();
					if (player)
					{
						player.Die();
					}
					Debug.Log("EXPLODE?");
					currentState = State.Pacing;
				}
				else
				{
					// keep gnawing on that robo steak
					Debug.Log("MMM STEAK");
					_hasSteak = true;
					currentState = State.NomSteak;
				}
			}
		}
		else
		{
			// lost target...
			Debug.Log("LOST TARGET");
			currentState = State.Pacing;
		}
	}

	void Attack_FixedUpdate()
	{
		if (_hasSteak)
		{
			return;
		}

		if (target)
		{
			Vector3 moveTarget = target.position;
			Vector2 toMoveTarget = moveTarget - transform.position;

			// Calculate target velocity, proportional to distance, but capped at max speed
			Vector2 targetVelocity = Vector2.ClampMagnitude(moveSpeedModifier * toMoveTarget, maxMoveSpeed);
			Vector2 velocityError = targetVelocity - rigidbody2D.velocity;

			// Force proportional to gain and velocity error, capped at max force
			Vector2 force = Vector2.ClampMagnitude(moveAccelGain * velocityError, maxMoveForce);

			// Apply the force
			rigidbody2D.AddForce(force);
		}
	}
	
	void Attack_OnCollisionEnter2D(Collision2D collision)
	{
		if (_hasSteak)
		{
			return;
		}

		Debug.Log("NOM");
		var destructable = collision.collider.GetComponent<DestructableBehaviour>();
		if (destructable)
		{
			destructable.Explode();
			//currentState = State.Pacing;
		}
	}


	IEnumerator Attack_ExitState()
	{
		_maintainAltitude = true;
		yield return new WaitForSeconds(0.5f);
		Debug.Log("Exited Attack");
	}
	
	// NOM STEAK

	public Transform nomWaypoint;

	IEnumerator NomSteak_EnterState()
	{
		_maintainAltitude = false;
		yield return 0;
	}

	void NomSteak_FixedUpdate()
	{
		Vector3 moveTarget = nomWaypoint.position;
		Vector2 toMoveTarget = moveTarget - transform.position;

		// Calculate target velocity, proportional to distance, but capped at max speed
		Vector2 targetVelocity = Vector2.ClampMagnitude(moveSpeedModifier * toMoveTarget, maxMoveSpeed);
		Vector2 velocityError = targetVelocity - rigidbody2D.velocity;

		// Force proportional to gain and velocity error, capped at max force
		Vector2 force = Vector2.ClampMagnitude(moveAccelGain * velocityError, maxMoveForce);

		// Apply the force
		rigidbody2D.AddForce(force);
	}
}
