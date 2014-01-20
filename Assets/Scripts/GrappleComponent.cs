using UnityEngine;
using System.Collections;

public class GrappleComponent : LimbComponent {

	public Transform ropeStart;
	public Transform ropeEnd;
	public Transform ropeQuad;

	public Transform clampOrigin;

	public Transform clamp;

	public bool fired = false;

	public float maxDistance = 10.0f;
	public float pullForce = 10.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// Adjust rope length
		float length = Vector3.Distance(ropeStart.position, ropeEnd.position);
		Vector3 scale = ropeStart.localScale;
		ropeStart.localScale = new Vector3(scale.x, length, scale.z);

		if (fired)
		{
			// Orient arm in direction of clamp
			Animator anim = getRootComponent().GetComponentInChildren<Animator>();
			Vector3 direction = Vector3.Normalize(ropeEnd.position - ropeStart.position);
			string xVar = parentAttachmentPoint.aimX;
			string yVar = parentAttachmentPoint.aimY;

			if (anim)
			{
				anim.SetFloat(xVar, direction.x);
				anim.SetFloat(yVar, direction.y);
			}


			if (length > 0.5)
			{
				// 'pull' player to clamp
				getRootComponent().rigidbody2D.AddForce(direction * pullForce);
			}
		}
	}

	override public void FireAbility() {

		Debug.Log("GRAPPLE FIRED!");
		if (!fired)
		{
			// For now, just fire in the direction
			int groundOnly = 1 << LayerMask.NameToLayer("Ground");
			RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.rotation * Vector3.down, maxDistance, groundOnly);

			if (hit)
			{
				clamp.position = hit.point;
				fired = true;
				shouldAim = false;
				clamp.parent = null;
			}
		}
		else
		{
			// release
			clamp.parent = clampOrigin;
			clamp.localEulerAngles = Vector3.zero;
			clamp.localPosition = Vector3.zero;
			shouldAim = true;
			fired = false;
		}
	}
}
