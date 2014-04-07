using UnityEngine;
using System.Collections;

public class SharkAttackTrigger : MonoBehaviour
{
	[SerializeField]
	private SharkController _shark;

	private Transform _currentTarget;

	void OnTriggerEnter2D(Collider2D collider)
	{
		if (_currentTarget == null)
		{
			var destructable = collider.GetComponent<DestructableBehaviour>();
			if (destructable)
			{
				_shark.SetTarget(destructable.transform);
				_currentTarget = destructable.transform;
			}
			else if (collider.attachedRigidbody.GetComponent<DestructableBehaviour>() || collider.attachedRigidbody.GetComponent<PlayerBehavior>())
			{
				_shark.SetTarget(collider.attachedRigidbody.transform);
				_currentTarget = collider.attachedRigidbody.transform;
			}
		}

		// If it's a steak, overrides everything!
		if (collider.tag == "robo-steak")
		{
			_shark.SetTarget(collider.transform, true);
			_currentTarget = collider.transform;
		}
	}

	void OnTriggerStay2D(Collider2D collider)
	{
		if (_currentTarget == null)
		{
			var destructable = collider.GetComponent<DestructableBehaviour>();
			if (destructable)
			{
				_shark.SetTarget(destructable.transform);
				_currentTarget = destructable.transform;
			}
		}

		// If it's a steak, overrides everything!
		if (collider.tag == "robo-steak")
		{
			_shark.SetTarget(collider.transform, true);
			_currentTarget = collider.transform;
		}
	}

	void OnTriggerExit2D(Collider2D collider)
	{
		if (collider.transform == _currentTarget)
		{
			Debug.Log("TARGET LOST");
			_currentTarget = null;
			_shark.SetTarget(_currentTarget);
		}
	}
}
