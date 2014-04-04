using UnityEngine;
using System.Collections;
using SimpleJSON;

public class DoorBehaviour : MonoBehaviour, SaveableComponent
{
	public Transform closedPosition;
	public Transform openPosition;
	public Rigidbody2D doorBody;

	public bool debug = false;

	public AudioClip openClip;
	public AudioClip closeClip;

	public float doorSpeed = 100f;
	public float doorForce = 10000f;

	private SliderJoint2D doorSlider;
    public bool saveState = false;
    public bool severed = false;

    private Vector2 severPoint = Vector2.zero;
	public enum State
	{
		Opening,
		Closing,
		Opened,
		Closed,
	};
    

    public void SaveState(JSONNode data)
    {
        if (saveState)
        {
            data[gameObject.name]["Severed"].AsBool = severed;
            data[gameObject.name]["Opening"] = State.Opening.ToString();
            data[gameObject.name]["Closing"] = State.Closing.ToString();
            data[gameObject.name]["Opened"] = State.Opened.ToString();
            data[gameObject.name]["Closed"] = State.Closed.ToString();
        }
    }
    public void LoadState(JSONNode data)
    {
        if (saveState)
        {
            if (data[gameObject.name] != null && data[gameObject.name]["Severed"].AsBool)
            {
                // sever the chain!
              
            }
        }
    }

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
		// Create slider joint if necessary, connect to door body
		doorSlider = GetComponent<SliderJoint2D>();
		if (doorSlider == null)
		{
			doorSlider = gameObject.AddComponent<SliderJoint2D>();
		}
		doorSlider.connectedBody = doorBody;

		doorSlider.angle = Vector2.Angle(Vector3.down, openPosition.position - closedPosition.position);

		var limits = doorSlider.limits;
		limits.min = 0;
		limits.max = Vector2.Distance(openPosition.position, closedPosition.position);
		doorSlider.limits = limits;
		doorSlider.useLimits = true;

		var motor = doorSlider.motor;
		motor.motorSpeed = 0f;
		doorSlider.motor = motor;
		doorSlider.useMotor = true;

		switch (state)
		{
			case State.Opened:
				doorBody.transform.position = openPosition.position;
				break;
			case State.Closed:
				doorBody.transform.position = closedPosition.position;
				break;
			case State.Closing:
				doorBody.transform.position = openPosition.position;
				state = State.Opened;
				Close();
				break;
			case State.Opening:
				doorBody.transform.position = closedPosition.position;
				state = State.Closed;
				Open();
				break;
		}
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
