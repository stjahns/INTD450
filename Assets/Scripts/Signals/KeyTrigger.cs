using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

public class KeyTrigger : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onKeyDown = new List<SignalConnection>();
	
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onKeyUp = new List<SignalConnection>();

	public string key;

	public bool keyEnabled = false;

	[InputSocket]
	public void EnableKey()
	{
		keyEnabled = true;
	}

	[InputSocket]
	public void DisableKey()
	{
		keyEnabled = false;
	}

	void Update()
	{
		if (keyEnabled)
		{
			if (Input.GetKeyDown(key))
			{
				onKeyDown.ForEach(s => s.Fire());
			}

			if (Input.GetKeyUp(key))
			{
				onKeyUp.ForEach(s => s.Fire());
			}
		}
	}
}
