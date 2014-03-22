using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : StateMachineBase {

	public enum State
	{
		Waiting,
		Pacing,
		UsingCannon,
		UsingGrapple
	}

	public State initialState;

	// Public configuration variables
	public Animator animator;
	public PlayerSkeleton skeleton;

	public HeadComponent head;
	public TorsoComponent torso;
	public BossCannon cannonArm;
	public GrappleComponent grappleArm;

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

		// Also consider alternative of spawning the prefabs and attaching them..?
	}

	private bool facingLeft = true;

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

	public Transform grappleTarget;
	public GameObject obstaclePrefab;

	IEnumerator UsingGrapple_EnterState()
	{
		Debug.Log("Entered UsingGrapple");
		animator.SetBool("aimRight", true);

		Transform target = PlayerBehavior.Player.transform;

		yield return new WaitForSeconds(1);

		grappleArm.FireAbility();
		
		yield return new WaitForSeconds(1);

		grappleArm.FireAbility();
		Instantiate(obstaclePrefab, grappleTarget.position, Quaternion.identity);

		currentState = State.Waiting;
	}

	void UsingGrapple_Update()
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

	IEnumerator UsingGrapple_ExitState()
	{
		animator.SetBool("aimRight", false);

		Debug.Log("Exited UsingGrapple");
		yield return 0;
	}
}
