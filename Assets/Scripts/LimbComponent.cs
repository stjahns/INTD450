using UnityEngine;
using System.Collections;

public class LimbComponent : RobotComponent {

	public Rigidbody2D body;
	public Transform joint;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Disconnect() {
		/*
		transform.parent = null;

		body = gameObject.AddComponent<Rigidbody2D>();
*/
	}

	public void Connect(TorsoComponent torso, Transform parentJoint, bool bumpUpForLeg) {

		/*
		// TODO save body properties
		body.isKinematic = true;
		DestroyObject(body);

		Vector3 offset = joint.localPosition; 
		transform.parent = parentJoint;

		// set position + orientation
		transform.localEulerAngles = new Vector3(0,0,0);
		transform.localPosition= new Vector3(-offset.x, -offset.y);

		// HACK - move head up to make room for limb (if necessary....)
		if (bumpUpForLeg) {
			float dist = torso.groundCheck.position.y - groundCheck.position.y;
			torso.getHead().transform.Translate(0, dist, 0);
		}
*/

	}
}
