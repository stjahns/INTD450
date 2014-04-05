using UnityEngine;
using System.Collections;

public class SawbladeController : MonoBehaviour
{
	public bool Running
	{
		get; set;
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (Running)
		{
			//other.gameObject.SendMessage("Kill", SendMessageOptions.DontRequireReceiver);
			other.gameObject.SendMessage( "TakeDamage",
					1.0,
					SendMessageOptions.DontRequireReceiver);
		}
	}
}
