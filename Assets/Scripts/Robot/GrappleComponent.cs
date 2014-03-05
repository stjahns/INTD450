using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleComponent : LimbComponent {

	public Transform ropeStart;
	public Transform ropeEnd;

	public Transform clampOrigin;

	public GrappleProjectile projectile;
	public float projectileVelocity;

	public bool fired = false;

	public float minRopeLength = 1.0f;
	public float minLegLength = 0.2f;
	public float maxRopeLength = 5.0f;
	public float ropePullSpeed = 1.0f; // units per second
	public float legRetractSpeed = 1.0f; // units per second

	public List<string> grappleableLayers = new List<string>();

	public AudioClip fireClip;
	public AudioClip releaseClip;

	public SpriteRenderer spriteRenderer;
	public Sprite cockedSprite;
	public Sprite firedSprite;

	public enum State
	{
		Cocked,
		Fired,
		Attached,
		Retracting
	}

	public State state;

	public ChainRenderer chainRenderer;

	private Transform forward;

	private float ropeLength;
	private DistanceJoint2D ropeJoint;
	private SliderJoint2D sliderJoint;
	
	private Rigidbody2D playerBody;

	private GameObject grappleRope;
	private GameObject playerHingeObject;

	private HingeJoint2D playerHinge;
	private HingeJoint2D grappleHinge;

	override public void Start ()
	{
		base.Start();

		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = transform;
		forwardObject.transform.localPosition = new Vector3(0, -1, 0);
		forward = forwardObject.transform;

		projectile.GrappleHit += OnGrappleHit;

		chainRenderer = GetComponent<ChainRenderer>();
	}

	void OnGrappleHit(Collision2D hit)
	{
		ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);

		state = State.Attached;

		playerBody = getRootComponent().rigidbody2D;
		if (playerBody)
		{
			if (IsArm)
			{
				// Create hinge object on player
				playerHingeObject = new GameObject("playerHinge");
				playerHingeObject.transform.position = ropeStart.position;
				playerHingeObject.AddComponent<Rigidbody2D>();

				playerHinge = playerBody.gameObject.AddComponent<HingeJoint2D>();
				playerHinge.anchor = ropeStart.position - playerBody.transform.position;
				playerHinge.connectedBody = playerHingeObject.rigidbody2D;

				// Create grapple rope with a slider joint, connect to player hinge
				grappleRope = new GameObject("grappleRope");
				grappleRope.transform.position = projectile.transform.position;
				grappleRope.transform.eulerAngles = Vector3.zero;

				sliderJoint = grappleRope.AddComponent<SliderJoint2D>();
				sliderJoint.connectedBody = playerHingeObject.rigidbody2D;

				Quaternion sliderRotation = Quaternion.FromToRotation(Vector3.up,
						ropeStart.position - ropeEnd.position);
				sliderJoint.angle = sliderRotation.eulerAngles.z;

				// Connect grapple hook to rope with hinge joint
				grappleHinge = projectile.gameObject.AddComponent<HingeJoint2D>();
				grappleHinge.connectedBody = grappleRope.rigidbody2D;

				JointMotor2D motor = sliderJoint.motor;
				motor.motorSpeed = 0.0f;
				sliderJoint.motor = motor;
				sliderJoint.useMotor = true;
			}
			else
			{
				Rigidbody2D anchorBody = projectile.rigidbody2D;
				projectile.transform.rotation = Quaternion.identity;

				sliderJoint = playerBody.gameObject.AddComponent<SliderJoint2D>();
				sliderJoint.anchor = ropeStart.position - playerBody.transform.position;

				sliderJoint.connectedBody = anchorBody;
				sliderJoint.connectedAnchor = ropeEnd.transform.position.XY()
					- anchorBody.transform.position.XY();

				JointMotor2D motor = sliderJoint.motor;
				motor.motorSpeed = 0.0f;
				sliderJoint.motor = motor;
				sliderJoint.useMotor = true;
			}
		}
	}

	override public void Update ()
	{
		base.Update();

		if (chainRenderer != null)
		{
			chainRenderer.sortingLayer = spriteRenderer.sortingLayerName;
		}
		
		if (sliderJoint)
		{
			JointMotor2D motor = sliderJoint.motor;

			if (IsArm)
			{
				if (Input.GetKey(KeyCode.W) && ropeLength > minRopeLength)
				{
					// Retract
					motor.motorSpeed = ropePullSpeed;
				}
				else if (Input.GetKey(KeyCode.S) && ropeLength < maxRopeLength)
				{
					// Extend 
					motor.motorSpeed = -ropePullSpeed;
				}
				else
				{
					motor.motorSpeed = 0;
				}
			}
			else
			{
				if (Input.GetKey(KeyCode.W) && ropeLength < maxRopeLength)
				{
					// Extend
					motor.motorSpeed = ropePullSpeed;
				}
				else if (Input.GetKey(KeyCode.S) && ropeLength > minLegLength)
				{
					// Retract 
					motor.motorSpeed = -ropePullSpeed;
				}
				else
				{
					motor.motorSpeed = 0;
				}
			}

			sliderJoint.motor = motor;
		}

		if (IsArm && playerHinge)
		{
			playerHinge.anchor = ropeStart.position - playerBody.transform.position;
		}
		
		if (!IsArm && sliderJoint && playerBody)
		{
			sliderJoint.anchor = ropeStart.position - playerBody.transform.position;
		}

		if (state == State.Retracting)
		{
			if (!IsArm && ropeLength > minLegLength)
			{
				// Retract 
				JointMotor2D motor = sliderJoint.motor;
				motor.motorSpeed = -legRetractSpeed;
				sliderJoint.motor = motor;
			}
			else
			{
				// Detach
				projectile.ResetProjectile();
				shouldAim = true;
				fired = false;

				if (sliderJoint)
				{
					Destroy(sliderJoint);
					sliderJoint = null;
				}

				state = State.Cocked;
			}
		}
	}

	void FixedUpdate ()
	{
		// TODO -- properly handle case where grapple is fired but no longer attached to player
		if (fired && parentAttachmentPoint)
		{

			ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);
			if (ropeLength > maxRopeLength + 0.5)
			{
				RetractGrapple(true);
			}

			if (IsArm)
			{
				// Orient arm in direction of clamp
				Animator anim = getRootComponent().GetComponentInChildren<Animator>();
				Vector3 direction = Vector3.Normalize(ropeEnd.position - ropeStart.position);
				string xVar = parentAttachmentPoint.aimX;
				string yVar = parentAttachmentPoint.aimY;

				// HACK!
				var player = getRootComponent().gameObject.GetComponentInChildren<PlayerBehavior>();
				if (!player.facingLeft)
				{
					direction.x *= -1;
				}

				if (anim)
				{
					anim.SetFloat(xVar, direction.x);
					anim.SetFloat(yVar, direction.y);
				}

				// MOAR HACK
				if (!player.facingLeft)
				{
					direction.x *= -1;
				}
			}
		}
	}

	override public void FireAbility()
	{
		playerBody = getRootComponent().rigidbody2D;

		int layerMask = 0;
		Vector3 direction = Vector3.down;

		if (IsArm)
		{
			grappleableLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
			direction = forward.position - transform.position;
			direction.Normalize();
		}
		else // leg
		{
			layerMask = 1 << LayerMask.NameToLayer("Ground");
		}

		if (!fired)
		{
			// Fire it like a projectile, until it hits something...
			projectile.FireProjectile(direction * projectileVelocity + playerBody.velocity.XY0(), layerMask);
			
			SFXSource.PlayOneShot(fireClip);

			// TODO - what about grabbing objects?

			fired = true;
			spriteRenderer.sprite = firedSprite;
		}
		else
		{
			RetractGrapple();
		}
	}

	void RetractGrapple(bool immediate = false)
	{
		if (immediate || IsArm)
		{
			if (grappleHinge)
			{
				Destroy(grappleHinge);
				grappleHinge = null;
			}

			if (playerHinge)
			{
				Destroy(playerHinge);
				playerHinge = null;
			}

			if (grappleRope)
			{
				Destroy(grappleRope);
				grappleRope = null;
			}

			if (playerHingeObject)
			{
				Destroy(playerHingeObject);
				playerHingeObject = null;
			}

			if (sliderJoint)
			{
				Destroy(sliderJoint);
				sliderJoint = null;
			}

			// Retract grappling hook back to base
			projectile.ResetProjectile();
			shouldAim = true;
			fired = false;

			SFXSource.PlayOneShot(releaseClip);
			spriteRenderer.sprite = cockedSprite;
		}
		else
		{
			// slide down to retract first...
			state = State.Retracting;
		}
	}

	override public void OnRemove()
	{
		base.OnRemove();
		RetractGrapple(true);
	}
}
