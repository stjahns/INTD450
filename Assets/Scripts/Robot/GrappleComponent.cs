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

	public float maxDistance = 10.0f;
	public float pullForce = 10.0f;

	public List<string> grappleableLayers = new List<string>();

	public AudioClip fireClip;
	public AudioClip releaseClip;

	public float ropeWidth = 0.1f;
	public Material ropeMaterial;

	private Transform forward;

	private LineRenderer ropeLine;

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
	}

	void Update ()
	{
		ropeLine.SetPosition(0, ropeStart.position);
		ropeLine.SetPosition(1, ropeEnd.position);
	}

	void FixedUpdate ()
	{


		// TODO -- properly handle case where grapple is fired but no longer attached to player
		if (fired && parentAttachmentPoint)
		{

			float ropeLength = Vector2.Distance(ropeStart.position, ropeEnd.position);
			if (ropeLength > maxDistance)
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

			if (projectile.AttachedToPoint)
			{
				// 'pull' player to clamp
				Rigidbody2D playerBody = getRootComponent().rigidbody2D;
				if (playerBody)
				{
					playerBody.AddForce(direction * pullForce);
				}
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

			SFXSource.PlayOneShot(releaseClip);
		}
	}
}
