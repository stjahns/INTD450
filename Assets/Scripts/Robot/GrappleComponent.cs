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
	public float legPushSpeed = 1.0f;
	public float legRetractSpeed = 1.0f;

	public float ropePullForce = 50.0f;

	public List<string> grappleableLayers = new List<string>();
	public List<string> legClampLayers = new List<string>();

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
	private DistanceJoint2D distanceJoint;
	private SliderJoint2D sliderJoint;
	
	private Rigidbody2D playerBody;


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

	virtual protected void OnGrappleHit(Collision2D hit)
	{
		ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);

		state = State.Attached;

		shouldAim = false;

		playerBody = getRootComponent().rigidbody2D;
		if (playerBody)
		{
			if (IsArm)
			{
				// Create distance joint between grapple base and hook
				distanceJoint = playerBody.gameObject.AddComponent<DistanceJoint2D>();
				distanceJoint.anchor = ropeStart.position - playerBody.transform.position;
				distanceJoint.connectedBody = projectile.rigidbody2D;
				distanceJoint.distance = Vector2.Distance(ropeStart.position, projectile.transform.position);
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

			if (isActive && Input.GetKey(KeyCode.W) && ropeLength < maxRopeLength)
			{
				// Extend
				motor.motorSpeed = legPushSpeed;
			}
			else if (isActive && Input.GetKey(KeyCode.S) && ropeLength > minLegLength)
			{
				// Retract 
				motor.motorSpeed = -legPushSpeed;
			}
			else
			{
				motor.motorSpeed = 0;
			}

			sliderJoint.motor = motor;
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


		if (state == State.Attached && IsArm)
		{
			if (ropeLength > minRopeLength) // hack that reduces some wierd oscillation issues
			{
				// Orient arm in direction of clamp
				Animator anim = getRootComponent().GetComponentInChildren<Animator>();
				Vector3 direction = Vector3.Normalize(ropeEnd.position - ropeStart.position);
				string xVar = parentAttachmentPoint.aimX;
				string yVar = parentAttachmentPoint.aimY;

				anim.SetBool(parentAttachmentPoint.animatorAimFlag, true);

				if (Skeleton.direction == PlayerSkeleton.Direction.Right)
				{
					direction.x *= -1;
				}

				if (anim)
				{
					anim.SetFloat(xVar, direction.x);
					anim.SetFloat(yVar, direction.y);
				}
			}
		}
	}

	void FixedUpdate ()
	{
		if (distanceJoint)
		{
			if (isActive && Input.GetKey(KeyCode.W) && ropeLength > minRopeLength)
			{
				var ropeToProjectile = (projectile.transform.position - ropeStart.position).normalized;
				float speedToGrapple = Vector3.Project(playerBody.velocity, ropeToProjectile).magnitude;
				if (speedToGrapple < ropePullSpeed)
				{
					// Pull on rope, by exterting a force in direction of grapple
					playerBody.AddForce(ropeToProjectile * ropePullForce);
				}
			}

			if (isActive && Input.GetKey(KeyCode.S) && ropeLength < maxRopeLength)
			{
				// Let rope extend
				distanceJoint.enabled = false;
			}
			else
			{
				distanceJoint.enabled = true;
			}

			distanceJoint.anchor = ropeStart.position - playerBody.transform.position;
			distanceJoint.distance = Vector2.Distance(ropeStart.position, projectile.transform.position);
			distanceJoint.distance = Mathf.Max(distanceJoint.distance, minRopeLength);
		}

		// TODO -- properly handle case where grapple is fired but no longer attached to player
		if (fired && parentAttachmentPoint)
		{

			ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);
			if (ropeLength > maxRopeLength + 0.5)
			{
				RetractGrapple(true);
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
			legClampLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
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
			state = State.Cocked;

			if (sliderJoint)
			{
				Destroy(sliderJoint);
				sliderJoint = null;
			}

			if (distanceJoint)
			{
				Destroy(distanceJoint);
				distanceJoint = null;
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
			// if stuck in ground, slide down to retract first...
			if (projectile.AttachedToPoint)
			{
				state = State.Retracting;
			}
			else
			{
				RetractGrapple(true);
			}
		}
	}

	override public void OnRemove()
	{
		base.OnRemove();
		RetractGrapple(true);
	}
}
