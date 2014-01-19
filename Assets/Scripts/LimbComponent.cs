using UnityEngine;
using System.Collections;

public class LimbComponent : MonoBehaviour {

	public Rigidbody2D body;
	public Collider2D collider;
	public Transform joint;
	public Transform groundCheck;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool checkOnGround() {

		int groundOnly = 1 << LayerMask.NameToLayer("Ground");
		return Physics2D.Linecast(transform.position, groundCheck.position, groundOnly);

	}

	public void Disconnect() {
		transform.parent = null;
		collider.enabled = true;

		body = gameObject.AddComponent<Rigidbody2D>();
	}

	public void Connect(TorsoComponent torso, Transform parentJoint, bool bumpUpForLeg) {

		// TODO save body properties
		body.isKinematic = true;
		DestroyObject(body);

		Vector3 offset = joint.localPosition; 
		transform.parent = parentJoint;

		// TEMP - disable collider
		//collider.enabled = false;

		// set position + orientation
		transform.localEulerAngles = new Vector3(0,0,0);
		transform.localPosition= new Vector3(-offset.x, -offset.y);

		// HACK - move head up to make room for limb (if necessary....)
		if (bumpUpForLeg) {
			float dist = torso.groundCheck.position.y - groundCheck.position.y;
			torso.getHead().transform.Translate(0, dist, 0);
		}

	}
}
