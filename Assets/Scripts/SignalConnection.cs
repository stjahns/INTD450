using UnityEngine;
using System.Collections;

public class SignalConnection : MonoBehaviour
{
	public GameObject target;
	public string message;
	public object argument;

	public void Fire()
	{
		if (target != null)
		{
			target.SendMessage(message);
		}
	}
}
