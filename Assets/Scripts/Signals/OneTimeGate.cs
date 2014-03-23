using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OneTimeGate : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> outputs = new List<SignalConnection>();

	public int fireCount = 1;

	[InputSocket]
	public void FireOutput()
	{
		if (fireCount > 0)
		{
			outputs.ForEach(s => s.Fire());
			--fireCount;
		}
	}
}
