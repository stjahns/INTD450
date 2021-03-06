﻿using UnityEngine;
using System.Collections;

public class SliderController : MonoBehaviour
{
	public Transform startLimit;
	public Transform endLimit;

	public Rigidbody2D parentBody;
	public Rigidbody2D body;

	public Vector2 bodyAnchor;

	public float initialSpeed = 0f;
	public float motorForce = 10f;
	public bool useMotor;

	private GameObject rootObject;
	private SliderJoint2D sliderJoint;

	void Start()
	{
		// Reparent to root container
		transform.parent = transform.rootParent();

		// Create a slider joint from startLimit to endLimit

		if (!parentBody)
		{
			rootObject = new GameObject("SliderRoot");
			rootObject.transform.position = startLimit.transform.position;
			parentBody = rootObject.AddComponent<Rigidbody2D>();
			parentBody.isKinematic = true;
		}

		sliderJoint = parentBody.gameObject.AddComponent<SliderJoint2D>();

		JointTranslationLimits2D limits = sliderJoint.limits;

		// min is roughly distance from parentBody to startLimit...
		limits.min = Vector2.Distance(parentBody.transform.position, startLimit.position);
		limits.max = Vector2.Distance(startLimit.position, endLimit.position) + limits.min;

		sliderJoint.limits = limits;

		sliderJoint.angle = Quaternion.FromToRotation(Vector3.right, 
				endLimit.position - startLimit.position).eulerAngles.z;

		sliderJoint.useMotor = useMotor;

		JointMotor2D motor = sliderJoint.motor;
		motor.motorSpeed = initialSpeed;
		motor.maxMotorTorque = motorForce;
		sliderJoint.motor = motor;

		// Attach body to joint
		if (body)
		{
			sliderJoint.connectedBody = body;
			sliderJoint.connectedAnchor = bodyAnchor;
		}
		else
		{
			Debug.LogError("Missing body on SliderController");
		}
	}

	[InputSocket]
	public void SetSpeed(string speed)
	{
		JointMotor2D motor = sliderJoint.motor;
		motor.motorSpeed = float.Parse(speed);
		sliderJoint.motor = motor;
		sliderJoint.useMotor = true;
	}

	[InputSocket]
	public void SetForce(string force)
	{
		JointMotor2D motor = sliderJoint.motor;
		motor.maxMotorTorque = float.Parse(force);
		sliderJoint.motor = motor;
		sliderJoint.useMotor = true;
	}
}
