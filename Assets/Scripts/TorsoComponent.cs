using UnityEngine;
using System.Collections;

public class TorsoComponent : RobotComponent {

	public Rigidbody2D body;

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

	private HeadComponent m_head = null;

	// Use this for initialization
	void Start () {
		//CheckLimbConnections();
	}

	
	// Update is called once per frame
	void FixedUpdate () {
		//CheckLimbConnections();
	}


	public HeadComponent getHead() {
		return m_head;
	}

	void CheckLimbConnections() {

		if (!connectedLeftArm && leftArm)
		{
			leftArm.Connect(this, leftArmJoint, false);
			connectedLeftArm = leftArm;
		}
		else if (connectedLeftArm && !leftArm)

		{
			connectedLeftArm.Disconnect();
			connectedLeftArm = null;
		}

		if (!connectedRightArm && rightArm)
		{
			rightArm.Connect(this, rightArmJoint, false);
			connectedRightArm = rightArm;
		}
		else if (connectedRightArm && !rightArm)
		{
			connectedRightArm.Disconnect();
			connectedRightArm = null;
		}

		bool noLegs = !connectedLeftLeg && !connectedRightLeg;

		if (!connectedLeftLeg && leftLeg)
		{
			leftLeg.Connect(this, leftLegJoint, noLegs);
			connectedLeftLeg = leftLeg;
		}
		else if (connectedLeftLeg && !leftLeg)
		{
			connectedLeftLeg.Disconnect();
			connectedLeftLeg = null;
		}

		if (!connectedRightLeg && rightLeg)
		{
			rightLeg.Connect(this, rightLegJoint, noLegs);
			connectedRightLeg = rightLeg;
		}
		else if (connectedRightLeg && !rightLeg)
		{
			connectedRightLeg.Disconnect();
			connectedRightLeg = null;
		}
	
	}

	public void Disconnect() {
		transform.parent = null;

		// Restore rigid body to unattached torso
		body = gameObject.AddComponent<Rigidbody2D>();

		m_head = null;
	}

	public void Connect(HeadComponent head) {

		Vector3 offset = neckJoint.localPosition; 
		transform.parent = head.neckConnection;

		// If we don't set body to kinematic, it will get will physics update that will move component after attaching...
		body.isKinematic = true;


		// TODO Save rigidbody properties..

		Destroy(body);
		body = null;

		// set position + orientation
		transform.localEulerAngles = new Vector3(0,0,0);
		transform.localPosition= new Vector3(-offset.x, -offset.y);

		// HACK - move head up to make room for torso...
		float torsoSize = head.groundCheck.position.y - groundCheck.position.y;
		head.transform.Translate(0, torsoSize, 0);

		m_head = head;

	}
}
