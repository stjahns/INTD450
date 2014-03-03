using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
	public Transform closedPosition;
	public Transform openPosition;
	public GameObject doorCollider;

	public bool debug = false;

	public AudioClip openClip;
	public AudioClip closeClip;

	public SliderJoint2D doorSlider;

	public float doorSpeed = 100f;
	public float doorForce = 10000f;

	public enum State
	{
		Opening,
		Closing,
		Opened,
		Closed,
	};

	public State state;

	[InputSocket]
	public void Open()
	{
		if (debug)
		{
			Debug.Log("OPENING DOOR");
		}

		if (state != State.Opening)
		{

			JointMotor2D motor = new JointMotor2D();
			motor.motorSpeed = doorSpeed;
			motor.maxMotorTorque = doorForce;
			doorSlider.motor = motor;
			doorSlider.useMotor = true;

			AudioSource.PlayClipAtPoint(openClip, transform.position);

			state = State.Opening;
		}
	}

	[InputSocket]
	public void Close()
	{
		if (debug)
		{
			Debug.Log("CLOSING DOOR");
		}

		if (state != State.Closing)
		{
			JointMotor2D motor = new JointMotor2D();
			motor.motorSpeed = -doorSpeed;
			motor.maxMotorTorque = doorForce;
			doorSlider.motor = motor;
			doorSlider.useMotor = true;

			AudioSource.PlayClipAtPoint(closeClip, transform.position);
			state = State.Closing;
		}
	}

	public void Start()
	{
	}

	public void Update()
	{
		switch (state)
		{
			case State.Opening:
				// TODO play clip when fully open
				// Maybe use a trigger collider....
				//AudioSource.PlayClipAtPoint(openClip, transform.position);
				break;
			case State.Closing:
				break;
		}
	}
}
