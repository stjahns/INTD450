using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossController : StateMachineBase {

	public enum State
	{
		Waiting,
		Pacing,
		UsingCannon,
		UsingGrapple,
		KickObstacle
	}

	public State initialState;

	// Public configuration variables
	public Animator animator;
	public PlayerSkeleton skeleton;

	public HeadComponent head;
	public TorsoComponent torso;
	public BossCannon cannonArm;
	public BossGrapple grappleArm;
	public ShieldComponent shieldArm;
	public SpringComponent springArm;

	public float thrustDelta;
	public float hoverBounceHeight;

	private float targetAltitude;

	void Start ()
	{
		currentState = initialState;

		int layerMask = 1 << LayerMask.NameToLayer("Ground");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, Mathf.Infinity, layerMask);
		if (hit)
		{
			targetAltitude = Vector2.Distance(transform.position, hit.point) + hoverBounceHeight;
		}

		// manually attach all robot components together...
		head.Attach(head.GetJointForSlot(AttachmentSlot.Spine), torso.rootJoint);
		torso.Attach(torso.GetJointForSlot(AttachmentSlot.RightShoulder), cannonArm.rootJoint);
		torso.Attach(torso.GetJointForSlot(AttachmentSlot.LeftShoulder), grappleArm.rootJoint);
		torso.Attach(torso.GetJointForSlot(AttachmentSlot.LeftHip), shieldArm.rootJoint);
		torso.Attach(torso.GetJointForSlot(AttachmentSlot.RightHip), springArm.rootJoint);

		// Also consider alternative of spawning the prefabs and attaching them..?
	}

	private bool facingLeft = true;
	private bool usingShield = false;

	override protected void Update ()
	{
		base.Update();

		// State-independant update
		if (animator && rigidbody2D)
		{
			if (rigidbody2D.velocity.x > 0.1f)
			{
				animator.SetBool("facingLeft", false);
				skeleton.direction = PlayerSkeleton.Direction.Right;
			}
			else if (rigidbody2D.velocity.x < -0.1f)
			{
				animator.SetBool("facingLeft", true);
				skeleton.direction = PlayerSkeleton.Direction.Left;
			}

			if (animator.GetCurrentAnimatorStateInfo(3).nameHash
					== Animator.StringToHash("Facing.FaceRight"))
			{
				if (facingLeft)
				{
					head.ResetSpriteOrders();
					facingLeft = false;
				}
			}
			else
			{
				if (!facingLeft)
				{
					head.ResetSpriteOrders();
					facingLeft = true;
				}
			}
		}

		// Check if we got a gun pointed at us (potentially)
		// TODO only if player has line of sight?
		var player = PlayerBehavior.Player;
		if (player.activeArm is CannonComponent)
		{
			if (!usingShield)
			{
				shieldArm.FireAbility();
				animator.SetBool("aimLowerRight", true);
				usingShield = true;
			}

			// Also consider gameplay benefits of making it its own state...
			// boss is frozen when you point a gun at him.. what a wuss!

			// Aim the shield
			Transform target = PlayerBehavior.Player.transform;
			Vector2 toTarget = target.position - shieldArm.transform.position;
			toTarget.Normalize();

			if (skeleton.direction == PlayerSkeleton.Direction.Right)
			{
				toTarget.x *= -1;
			}
			animator.SetFloat("lowerRightArmX", toTarget.x);
			animator.SetFloat("lowerRightArmY", toTarget.y);
		}
		else
		{
			if (usingShield)
			{
				shieldArm.FireAbility();
				animator.SetBool("aimLowerRight", false);
				usingShield = false;
			}
		}
	}

	private bool maintainAltitude = true;

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

			if (maintainAltitude)
			{
				if (altitude < targetAltitude)
				{
					thrust += Vector2.up * thrustDelta;
				}
				else
				{
					thrust -= Vector2.up * thrustDelta;
				}

				// if get bounced up or down, need to return to target height
				// without increasing the bounce height
				if (altitude > targetAltitude + hoverBounceHeight ||
					altitude < targetAltitude - hoverBounceHeight)
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


	//================================================================================
	// States
	//================================================================================

	//--------------------------------------------------------------------------------
	// Waiting
	//--------------------------------------------------------------------------------

	public float waitTime = 3;
	public List<State> behaviours;
	private int behaviourIndex = -1;
	
	IEnumerator Waiting_EnterState()
	{
		Debug.Log("Entered Waiting");
		yield return new WaitForSeconds(waitTime);

		// pick next behaviour state from list of states
		behaviourIndex += 1;
		behaviourIndex %= behaviours.Count;
		currentState = behaviours[behaviourIndex];
	}

	void Waiting_Update()
	{
	}

	public float stopForceGain = 1;

	void Waiting_FixedUpdate()
	{
		// Make sure we aren't moving laterally..
		if (rigidbody2D)
		{
			float targetSpeed = 0;
			float currentSpeed = rigidbody2D.velocity.x;
			float error = targetSpeed - currentSpeed;
			float force = stopForceGain * error;
			rigidbody2D.AddForce(Vector2.right * force);
		}
	}

	IEnumerator Waiting_ExitState()
	{
		Debug.Log("Exited Waiting");
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// Pacing
	//--------------------------------------------------------------------------------

	public float paceTime = 10f;
	public float paceSpeed = 1f;
	public float paceForce = 1f;

	public List<Transform> waypoints;

	private int currentWaypointIndex;
	private Transform currentWaypoint;

	IEnumerator Pacing_EnterState()
	{
		Debug.Log("Entered Pacing");

		currentWaypointIndex = 0;
		currentWaypoint = waypoints[currentWaypointIndex];

		// Return to waiting after paceTime
		yield return new WaitForSeconds(paceTime);
		currentState = State.Waiting;
	}

	void Pacing_FixedUpdate()
	{
		float distanceToWaypoint = Mathf.Abs(transform.position.x - currentWaypoint.position.x);

		if (distanceToWaypoint < 0.1)
		{
			// reached waypoint, switch to next waypoint
			currentWaypointIndex += 1;
			currentWaypointIndex %= waypoints.Count;
			currentWaypoint = waypoints[currentWaypointIndex];
		}

		// Move to waypoint
		Vector2 toWaypoint = currentWaypoint.position - transform.position;

		// If speed in direction of waypoint is less than pacing speed,
		// apply force in waypoint's direction

		if (toWaypoint.x > 0 && rigidbody2D.velocity.x < paceSpeed)
		{
			rigidbody2D.AddForce(Vector3.right * paceForce);
		}

		// TODO what if going too fast in right direction?

		if (toWaypoint.x < 0 && rigidbody2D.velocity.x > -paceSpeed)
		{
			rigidbody2D.AddForce(Vector3.left * paceForce);
		}
	}

	IEnumerator Pacing_ExitState()
	{
		Debug.Log("Exited Pacing");
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// UsingCannon
	//--------------------------------------------------------------------------------

	public float cannonAimTime = 3f;
	public float initialAimError = 3f;
	public GameObject targetPipPrefab;

	private TargetPip _pipInstance;

	IEnumerator UsingCannon_EnterState()
	{
		animator.SetBool("aimLeft", true);

		Transform target = PlayerBehavior.Player.transform;

		// Instantiate the target pip randomly somewhere within initialAimError
		Vector2 pipPosition = target.position.XY() + Random.insideUnitCircle * initialAimError;
		GameObject pip = Instantiate(targetPipPrefab, pipPosition, Quaternion.identity)
			as GameObject;
		_pipInstance = pip.GetComponent<TargetPip>();
		_pipInstance.target = target;

		cannonArm.pip = _pipInstance;

		// shoot 
		yield return new WaitForSeconds(cannonAimTime);
		cannonArm.FireAbility();
		currentState = State.Waiting;
	}

	void UsingCannon_Update()
	{
		// Aim cannon arm in direction of pip
		Vector2 toPip = _pipInstance.transform.position - cannonArm.transform.position;
		toPip.Normalize();

		if (skeleton.direction == PlayerSkeleton.Direction.Right)
		{
			toPip.x *= -1;
		}

		animator.SetFloat("leftArmX", toPip.x);
		animator.SetFloat("leftArmY", toPip.y);
	}

	IEnumerator UsingCannon_ExitState()
	{
		animator.SetBool("aimLeft", false);

		// disable aim pip
		_pipInstance.RemovePip();
		cannonArm.pip = null;

		Debug.Log("Exited UsingCannon");
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// UsingGrapple
	//--------------------------------------------------------------------------------

	private Transform grappleTarget;
	private GameObject obstacle;

	IEnumerator UsingGrapple_EnterState()
	{
		Debug.Log("Entered UsingGrapple");
		animator.SetBool("aimRight", true);

		grappleArm.GrabbedObstacle += OnGrabObstacle;

		obstacle = null;
		grappleTarget = null;

		// Find a grapple target, (right most?)
		var obstacles = GameObject.FindGameObjectsWithTag("BossGrappleTarget");
		foreach (var obst in obstacles)
		{
			if (obstacle == null)
			{
				obstacle = obst;
			}
			else if (obstacle.transform.position.x < obst.transform.position.x)
			{
				obstacle = obst;
			}
		}

		if (obstacle)
		{
			grappleTarget = obstacle.transform;
			yield return new WaitForSeconds(1);

			grappleArm.FireAbility();
			
			yield return new WaitForSeconds(1);

			grappleArm.FireAbility();

			currentState = State.KickObstacle;
		}
		else
		{
			currentState = State.Waiting;
			yield return 0;
		}

	}

	void UsingGrapple_Update()
	{
		if (grappleTarget)
		{
			Vector2 toTarget = grappleTarget.position - grappleArm.transform.position;
			toTarget.Normalize();

			if (skeleton.direction == PlayerSkeleton.Direction.Right)
			{
				toTarget.x *= -1;
			}

			animator.SetFloat("rightArmX", toTarget.x);
			animator.SetFloat("rightArmY", toTarget.y);
		}
	}

	void OnGrabObstacle(GameObject o)
	{
		obstacle = o;
	}

	IEnumerator UsingGrapple_ExitState()
	{
		animator.SetBool("aimRight", false);
		grappleTarget = null;

		grappleArm.GrabbedObstacle -= OnGrabObstacle;

		Debug.Log("Exited UsingGrapple");
		yield return 0;
	}

	//--------------------------------------------------------------------------------
	// KickObstacle
	//--------------------------------------------------------------------------------

	IEnumerator KickObstacle_EnterState()
	{
		Debug.Log("Entered KickObstacle");

		animator.SetBool("aimLowerLeft", true);

		maintainAltitude = false;

		yield return 0;
	}

	public float moveSpeedModifier = 1;
	public float maxMoveSpeed = 1;
	public float moveAccelGain = 1;
	public float maxMoveForce = 1;
	public float kickDistance = 1.5f;

	public PhysicsMaterial2D bouncyMaterial;

	void KickObstacle_Update()
	{
		if (obstacle)
		{
			// Aim spring leg at it
			Vector2 toTarget = obstacle.transform.position - springArm.transform.position;
			toTarget.Normalize();

			if (skeleton.direction == PlayerSkeleton.Direction.Right)
			{
				toTarget.x *= -1;
			}

			animator.SetFloat("lowerLeftArmX", toTarget.x);
			animator.SetFloat("lowerLeftArmY", toTarget.y);

			// if close enough, kick it!
			if (Vector2.Distance(springArm.transform.position, obstacle.transform.position) < kickDistance + 0.1
					&& Mathf.Abs(springArm.transform.position.y - obstacle.transform.position.y) < 0.5f)
			{
				springArm.FireAbility();
				currentState = State.Waiting;
				foreach (var c in obstacle.GetComponents<Collider2D>())
				{
					c.enabled = false;
					c.sharedMaterial = bouncyMaterial;
					c.enabled = true;
				}
			}
		}
	}


	void KickObstacle_FixedUpdate()
	{
		if (obstacle)
		{
			// Move so that spring leg is in line with obstacle, N units to left or right

			Vector3 moveTarget = obstacle.transform.position;
			if (obstacle.transform.position.x > transform.position.x)
			{
				moveTarget += new Vector3(-kickDistance, 0, 0);
			}
			else
			{
				moveTarget += new Vector3(kickDistance, 0, 0);
			}

			Vector2 toMoveTarget = moveTarget - springArm.transform.position;

			// Calculate target velocity, proportional to distance, but capped at max speed
			Vector2 targetVelocity = Vector2.ClampMagnitude(moveSpeedModifier * toMoveTarget, maxMoveSpeed);
			Vector2 velocityError = targetVelocity - rigidbody2D.velocity;

			// Force proportional to gain and velocity error, capped at max force
			Vector2 force = Vector2.ClampMagnitude(moveAccelGain * velocityError, maxMoveForce);

			// Apply the force
			rigidbody2D.AddForce(force);
		}
	}

	IEnumerator KickObstacle_ExitState()
	{
		maintainAltitude = true;

		yield return new WaitForSeconds(0.5f);
		animator.SetBool("aimLowerLeft", false);

		Debug.Log("Exited KickObstacle");
	}
}
