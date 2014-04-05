using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class ObjectDestroyer : MonoBehaviour
{
	public List<string> objectTags;
	public bool isEnabled = true;
	public bool explode = false;

	//
	// lets us set an icon...
	//
	void OnDrawGizmos()
	{
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		DestroyObject(other.attachedRigidbody.gameObject);
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		DestroyObject(other.collider.attachedRigidbody.gameObject);
	}

	void DestroyObject(GameObject other)
	{
		if (isEnabled && objectTags.Count == 0 || 
				objectTags.Contains(other.tag))
		{
			other.SendMessage("OnDestroy", SendMessageOptions.DontRequireReceiver);

			DestructableBehaviour destructable = other.GetComponent<DestructableBehaviour>();
			if (destructable)
			{
				if (explode)
				{
					destructable.Explode();
				}
				else
				{
					destructable.Destroy();
				}
			}
			else
			{
				// Destroy(other);
				// To prevent wierd bugs (eg grapple projectile being destroyed), 
				// only allow destroying DestructableBehaviours
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
