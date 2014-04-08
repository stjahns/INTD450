using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

public class DeathHazard : MonoBehaviour, SaveableComponent
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

	public bool saveState = false;

	public void SaveState(JSONNode data)
	{
		if (saveState)
		{
			data[gameObject.name]["enabled"].AsBool = isEnabled;
		}
	}

	public void LoadState(JSONNode data)
	{
		if (saveState)
		{
			if (data[gameObject.name] != null)
			{
				isEnabled = data[gameObject.name]["enabled"].AsBool;
			}
		}
	}
}
