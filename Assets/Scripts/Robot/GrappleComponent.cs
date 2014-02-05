using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrappleComponent : LimbComponent {

	public Transform ropeStart;
	public Transform ropeEnd;
	public Transform ropeQuad;

	public Transform clampOrigin;

	public Transform clamp;

	public bool fired = false;

	public float maxDistance = 10.0f;
	public float pullForce = 10.0f;

	public List<string> grappleableLayers = new List<string>();

	public AudioClip fireClip;
	public AudioClip releaseClip;

	void Start ()
	{
		ropeStart.transform.parent = null;
	}

	void Update ()
	{
		ropeStart.transform.position = lowerLimb.transform.position;
	}

	void FixedUpdate ()
	{
		ropeStart.transform.position = lowerLimb.transform.position;

		// Adjust rope length
		float length = Vector3.Distance(ropeStart.position, ropeEnd.position);
		Vector3 scale = ropeStart.localScale;
		ropeStart.localScale = new Vector3(scale.x, length, scale.z);

		// Adjust rope angle 
		Vector3 angles = ropeStart.eulerAngles;
		angles.z = ( Mathf.Atan2(ropeEnd.position.y - ropeStart.position.y,
				ropeEnd.position.x - ropeStart.position.x) + Mathf.PI / 2f) * Mathf.Rad2Deg;
		ropeStart.eulerAngles = angles;


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

			if (length > 0.5)
			{
				// 'pull' player to clamp
				// TODO -- causes exception while player physics is resetting - null check?
				getRootComponent().rigidbody2D.AddForce(direction * pullForce);
			}
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

			// TODO use mouse coords!
			Vector3 direction = ropeStart.position - transform.position;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, mask);

			if (hit)
			{
				// TEMP: use gameObject's position to allow for larger trigger collider
				//clamp.position = hit.point;
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
			shouldAim = true;
			fired = false;

			SFXSource.PlayOneShot(releaseClip);
		}
	}
}
