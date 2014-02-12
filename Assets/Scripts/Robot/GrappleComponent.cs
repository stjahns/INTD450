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
	public float maxRopeLength = 5.0f;
	public float ropePullSpeed = 1.0f; // units per second

	public List<string> grappleableLayers = new List<string>();

	public AudioClip fireClip;
	public AudioClip releaseClip;

	public float ropeWidth = 0.1f;
	public Material ropeMaterial;

	private Transform forward;

	private float ropeLength;
	private LineRenderer ropeLine;
	private DistanceJoint2D ropeJoint;

	private Rigidbody2D playerBody;

	void Start ()
	{
		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = lowerLimb.transform;
		forwardObject.transform.localPosition = new Vector3(0, -1, 0);
		forward = forwardObject.transform;
		
		ropeLine = gameObject.AddComponent<LineRenderer>();
		ropeLine.SetWidth(ropeWidth, ropeWidth);
		ropeLine.material = ropeMaterial;
		ropeLine.SetPosition(0, ropeStart.position);
		ropeLine.SetPosition(1, ropeEnd.position);

		projectile.GrappleHit += OnGrappleHit;
	}

	void OnGrappleHit(Collision2D hit)
	{
		ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);

		playerBody = getRootComponent().rigidbody2D;
		if (playerBody)
		{
			ropeJoint = playerBody.gameObject.AddComponent<DistanceJoint2D>();
			ropeJoint.connectedBody = hit.rigidbody;
			ropeJoint.anchor = ropeStart.position - playerBody.transform.position;
			ropeJoint.distance = ropeLength;
		}
	}

	void Update ()
	{
		ropeLine.SetPosition(0, ropeStart.position);
		ropeLine.SetPosition(1, ropeEnd.position);

		if (ropeJoint && playerBody)
		{
			ropeJoint.anchor = ropeStart.position - playerBody.transform.position;

			if (Input.GetKey(KeyCode.W))
			{
				// Pull in rope
				if (ropeJoint.distance > minRopeLength)
				{
					ropeJoint.distance -= ropePullSpeed * Time.deltaTime;
				}
			}

			if (Input.GetKey(KeyCode.S))
			{
				// Let rope out
				if (ropeJoint.distance < maxRopeLength)
				{
					ropeJoint.distance += ropePullSpeed * Time.deltaTime;
				}
			}
		}
	}

	void FixedUpdate ()
	{

		// TODO -- properly handle case where grapple is fired but no longer attached to player
		if (fired && parentAttachmentPoint)
		{

			ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);
			if (ropeLength > maxRopeLength)
			{
				projectile.ResetProjectile();
				fired = false;
			}

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

	override public void FireAbility()
	{
		// TODO only if arm!
		if (!fired)
		{
			// Fire it like a projectile, until it hits something...
			Vector3 direction = forward.position - lowerLimb.transform.position;
			direction.Normalize();
			projectile.FireProjectile(direction * projectileVelocity);
			SFXSource.PlayOneShot(fireClip);

			// TODO - what about grabbing objects?

			fired = true;
		}
		else
		{
			// Retract grappling hook back to base
			projectile.ResetProjectile();
			shouldAim = true;
			fired = false;

			if (ropeJoint)
			{
				Destroy(ropeJoint);
				ropeJoint = null;
			}

			SFXSource.PlayOneShot(releaseClip);
		}
	}
}
