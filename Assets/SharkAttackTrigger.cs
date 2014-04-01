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
