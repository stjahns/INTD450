using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleComponent : LimbComponent {

	public Transform ropeStart;
	public Transform ropeEnd;

	public Transform clampOrigin;

	public Transform clamp;

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

			// 'pull' player to clamp
			// TODO -- causes exception while player physics is resetting - null check?
			getRootComponent().rigidbody2D.AddForce(direction * pullForce);
		}
	}

	override public void FireAbility()
	{
		if (!fired)
		{
			int mask = 0;

			foreach (string layer in grappleableLayers)
			{
				mask |= 1 << LayerMask.NameToLayer(layer);
			}

			Vector3 direction = forward.position - lowerLimb.transform.position;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, mask);

			if (hit)
			{
				clamp.position = hit.collider.gameObject.transform.position;
				fired = true;
				shouldAim = false;
				clamp.parent = null;

				SFXSource.PlayOneShot(fireClip);
			}
			
			// TODO - fire grapple at pullable objects

			// TODO - fire grapple at ground layer, retract immediately
		}
		else
		{
			// release
			clamp.parent = clampOrigin;
			clamp.localEulerAngles = Vector3.zero;
			clamp.localPosition = Vector3.zero;
			clamp.localScale = Vector3.one;
			shouldAim = true;
			fired = false;

			SFXSource.PlayOneShot(releaseClip);
		}
	}
}
