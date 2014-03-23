using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeathHazard : MonoBehaviour
{
	public List<string> vulnerableTags;
	public bool isEnabled = true;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (isEnabled && vulnerableTags.Count == 0 || 
				vulnerableTags.Contains(other.attachedRigidbody.gameObject.tag))
		{
			GameObject obj = other.attachedRigidbody.gameObject;
			PlayerBehavior player = obj.GetComponent<PlayerBehavior>();
			if (player)
			{
				player.Die();
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (isEnabled && vulnerableTags.Count == 0 || 
				vulnerableTags.Contains(other.rigidbody.gameObject.tag))
		{
			GameObject obj = other.rigidbody.gameObject;
			PlayerBehavior player = obj.GetComponent<PlayerBehavior>();
			if (player)
			{
				player.Die();
			}
		}
	}

	[InputSocket]
	public void setEnabled()
	{
		isEnabled = true;
	}
	[InputSocket]
	public void setDisabled()
	{
		isEnabled = false;
	}
}
