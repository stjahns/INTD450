using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ConveyorBehavior : MonoBehaviour
{
	public List<HingeJoint2D> conveyorMotors;
	public float ConveyorSpeed;

	private float oldSpeed;

	void Start()
	{
		oldSpeed = ConveyorSpeed;

		conveyorMotors.ForEach(m => {
			JointMotor2D motor = m.motor;
			motor.motorSpeed = ConveyorSpeed;
			m.motor = motor;
		});
	}

	void Update()
	{
		if (oldSpeed != ConveyorSpeed)
		{
			conveyorMotors.ForEach(m => {
				JointMotor2D motor = m.motor;
				motor.motorSpeed = ConveyorSpeed;
				m.motor = motor;
			});
			oldSpeed = ConveyorSpeed;
		}
	}
}
