using UnityEngine;
using System.Collections;

public class TorsoComponent : MonoBehaviour {

	public Rigidbody2D body;
	public Collider2D collider;

	public Transform neckJoint;
	public Transform leftArmJoint;
	public Transform rightArmJoint;
	public Transform leftLegJoint;
	public Transform rightLegJoint;

	public LimbComponent leftArm;
	public LimbComponent rightArm;
	public LimbComponent leftLeg;
	public LimbComponent rightLeg;

	private LimbComponent connectedLeftArm = null;
	private LimbComponent connectedRightArm = null;
	private LimbComponent connectedLeftLeg = null;
	private LimbComponent connectedRightLeg = null;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (!connectedLeftArm && leftArm)
		{
			leftArm.Connect(this, leftArmJoint);
			connectedLeftArm = leftArm;
		}
		else if (connectedLeftArm && !leftArm)
		{
			leftArm.Disconnect();
			connectedLeftArm = null;
		}

		if (!connectedRightArm && rightArm)
		{
			rightArm.Connect(this, rightArmJoint);
			connectedRightArm = rightArm;
		}
		else if (connectedRightArm && !rightArm)
		{
			rightArm.Disconnect();
			connectedRightArm = null;
		}

		if (!connectedLeftLeg && leftLeg)
		{
			leftLeg.Connect(this, leftLegJoint);
			connectedLeftLeg = leftLeg;
		}
		else if (connectedLeftLeg && !leftLeg)
		{
			leftLeg.Disconnect();
			connectedLeftLeg = null;
		}

		if (!connectedRightLeg && rightLeg)
		{
			rightLeg.Connect(this, rightLegJoint);
			connectedRightLeg = rightLeg;
		}
		else if (connectedRightLeg && !rightLeg)
		{
			rightLeg.Disconnect();
			connectedRightLeg = null;
		}
	
	}

	public void Disconnect() {
		transform.parent = null;
		body.isKinematic = false;
		collider.enabled = true;
	}

	public void Connect(HeadComponent head) {

		Vector3 offset = neckJoint.localPosition; 
		transform.parent = head.neckConnection;

		// make body kinematic
		body.isKinematic = true;

		// TEMP - disable collider
		collider.enabled = false;

		// set position + orientation
		transform.eulerAngles = new Vector3(0,0,0);
		transform.localPosition= new Vector3(-offset.x, -offset.y);

	}
}
