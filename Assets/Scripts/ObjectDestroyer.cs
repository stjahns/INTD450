using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ObjectDestroyer : MonoBehaviour
{
	public List<string> objectTags;
	public bool isEnabled = true;

	//
	// lets us set an icon...
	//
	void OnDrawGizmos()
	{
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (isEnabled && objectTags.Count == 0 || 
				objectTags.Contains(other.attachedRigidbody.gameObject.tag))
		{
			GameObject obj = other.attachedRigidbody.gameObject;
			obj.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);

			DestructableBehaviour destructable = obj.GetComponent<DestructableBehaviour>();
			if (destructable)
			{
				destructable.Destroy();
			}
			else
			{
				Destroy(obj);
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (isEnabled && objectTags.Count == 0 || 
				objectTags.Contains(other.rigidbody.gameObject.tag))
		{
			GameObject obj = other.rigidbody.gameObject;
			obj.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);

			DestructableBehaviour destructable = obj.GetComponent<DestructableBehaviour>();
			if (destructable)
			{
				destructable.Destroy();
			}
			else
			{
				Destroy(obj);
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
