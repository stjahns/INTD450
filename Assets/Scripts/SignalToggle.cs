using UnityEngine;
using UnityEditor;
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

	// Color of editor scene representation (rectangle)
	public Color color;

	override public void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = color;
		Gizmos.DrawCube(transform.position, new Vector3(3, 1, 1));
		Gizmos.DrawCube(transform.position, new Vector3(3, 1, 1));

		Handles.color = Color.white;
		Vector3 position = transform.position;
		position.y += 0.5f;
		Handles.Label(position, "TOGGLE");
	}

	[InputSocket]
	public void Toggle()
	{
		if (state == ToggleState.On)
		{
			Debug.Log("TOGGLE ON");
			onToggleOn.ForEach(s => s.Fire());
			state = ToggleState.Off;
		}
		else
		{
			Debug.Log("TOGGLE OFF");
			onToggleOff.ForEach(s => s.Fire());
			state = ToggleState.On;
		}
	}
}
