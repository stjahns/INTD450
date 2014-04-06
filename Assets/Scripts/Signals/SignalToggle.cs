using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SignalToggle : TriggerBase {

	public enum ToggleState
	{
		On,
		Off
	}

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onToggleOn = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onToggleOff = new List<SignalConnection>();

	public ToggleState state = ToggleState.Off;

	[InputSocket]
	public void Toggle()
	{
		if (state == ToggleState.Off)
		{
			Debug.Log("TOGGLE ON");
			onToggleOn.ForEach(s => s.Fire());
			state = ToggleState.On;
		}
		else
		{
			Debug.Log("TOGGLE OFF");
			onToggleOff.ForEach(s => s.Fire());
			state = ToggleState.Off;
		}
	}
}
