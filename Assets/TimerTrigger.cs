using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TimerTrigger : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onFire = new List<SignalConnection>();

	public float time;
	public bool startOnLoad;
	public bool repeat;

	private float elapsed;
	private bool running;

	void Start ()
	{
		running = false;
		elapsed = 0f;

		if (startOnLoad)
		{
			StartTimer();
		}
	}
	
	void Update ()
	{
		if (running)
		{
			elapsed += Time.deltaTime;

			if (elapsed > time)
			{
				onFire.ForEach(s => s.Fire());

				if (repeat)
				{
					elapsed = 0;
				}
				else
				{
					running = false;
					elapsed = 0;
				}
			}
		}
	}

	[InputSocket]
	public void StartTimer()
	{
		running = true;
		elapsed = 0f;
	}

	[InputSocket]
	public void StopTimer()
	{
		running = false;
	}

	[InputSocket]
	public void ResumeTimer()
	{
		running = true;
	}
}
