using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SignalConnection : MonoBehaviour
{
	public GameObject target;
	public string message;
	public string argument = null;

	public void Fire()
	{
		if (target != null)
		{
			if (argument != null)
			{
				target.SendMessage(message, argument);
			}
			else
			{
				target.SendMessage(message);
			}
		}
	}
}
