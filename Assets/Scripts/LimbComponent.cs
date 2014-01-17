using UnityEngine;
using System.Collections;

public class LimbComponent : MonoBehaviour {

	public Rigidbody2D body;
	public Collider2D collider;
	public Transform joint;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Disconnect() {
		transform.parent = null;
		body.isKinematic = false;
		collider.enabled = true;
	}

	public void Connect(TorsoComponent torso, Transform parentJoint) {

		Vector3 offset = joint.localPosition; 
		transform.parent = parentJoint;

		// make body kinematic
		body.isKinematic = true;

		// TEMP - disable collider
		collider.enabled = false;

		// set position + orientation
		transform.eulerAngles = new Vector3(0,0,0);
		transform.localPosition= new Vector3(-offset.x, -offset.y);

	}
}
