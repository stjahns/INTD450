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

	public SpriteRenderer spriteRenderer;
	public Sprite cockedSprite;
	public Sprite firedSprite;

	private Transform forward;

	private float ropeLength;
	private LineRenderer ropeLine;
	private DistanceJoint2D ropeJoint;
	private SliderJoint2D legJoint;
	

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
			if (IsArm)
			{
				ropeJoint = playerBody.gameObject.AddComponent<DistanceJoint2D>();
				ropeJoint.connectedBody = hit.rigidbody;
				ropeJoint.anchor = ropeStart.position - playerBody.transform.position;
				ropeJoint.distance = ropeLength;
			}
			else
			{
				Rigidbody2D anchorBody = projectile.rigidbody2D;
				projectile.transform.rotation = Quaternion.identity;

				legJoint = playerBody.gameObject.AddComponent<SliderJoint2D>();
				legJoint.anchor = ropeStart.position - playerBody.transform.position;

				legJoint.connectedBody = anchorBody;
				legJoint.connectedAnchor = ropeEnd.transform.position.XY()
					- anchorBody.transform.position.XY();

				legJoint.useLimits = true;

				JointTranslationLimits2D limits = new JointTranslationLimits2D();
				limits.max = ropeLength + 0.1f;
				limits.min = ropeLength;
				legJoint.limits = limits;

			}
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

		if (legJoint && playerBody)
		{
			legJoint.anchor = ropeStart.position - playerBody.transform.position;

			JointTranslationLimits2D limits = legJoint.limits;

			if (Input.GetKey(KeyCode.W))
			{
				// Extend leg
				if (limits.min < maxRopeLength)
				{
					limits.min += ropePullSpeed * Time.deltaTime;
					limits.max += ropePullSpeed * Time.deltaTime;
				}
			}

			if (Input.GetKey(KeyCode.S))
			{
				// Retract leg
				if (limits.min > 0.0f)
				{
					limits.min -= ropePullSpeed * Time.deltaTime;
					limits.max -= ropePullSpeed * Time.deltaTime;
				}
			}

			legJoint.limits = limits;
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
			direction = forward.position - lowerLimb.transform.position;
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
			// Retract grappling hook back to base
			projectile.ResetProjectile();
			shouldAim = true;
			fired = false;

			if (ropeJoint)
			{
				Destroy(ropeJoint);
				ropeJoint = null;
			}

			if (legJoint)
			{
				Destroy(legJoint);
				legJoint = null;
			}

			SFXSource.PlayOneShot(releaseClip);
			spriteRenderer.sprite = cockedSprite;
		}
	}
}
