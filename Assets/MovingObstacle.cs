using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DestructableBehaviour))]
public class MovingObstacle : MonoBehaviour
{
	[SerializeField]
	private bool _destroyOtherObstacles;

	[SerializeField]
	private bool _limbsFallImmediately;

	[SerializeField]
	private float _destroyDelay;

	[SerializeField]
	private float _speedThreshold;

	void OnCollisionEnter2D(Collision2D collision)
	{
		MovingObstacle otherObstacle = collision.gameObject.GetComponent<MovingObstacle>();
		if (otherObstacle && _destroyOtherObstacles)
		{
			// Obstacles 'destroy' each other if they hit each other
			StartCoroutine(fallRoutine(gameObject, _destroyDelay));
			StartCoroutine(fallRoutine(otherObstacle.gameObject, _destroyDelay));
		}

		RobotComponent limb = collision.gameObject.GetComponent<RobotComponent>();
		if (limb && rigidbody2D.velocity.magnitude > _speedThreshold)
		{
			// If hit free limb, blow it away
			if (!limb.attachedToPlayer())
			{
				StartCoroutine(fallRoutine(limb.gameObject, _destroyDelay));
			}
			else
			{
				// if hit player, detach all limbs
				var rootComponent = limb.getRootComponent();
				var limbs = rootComponent.getAllChildren();
				rootComponent.DetachChildren();
				if (_limbsFallImmediately)
				{
					foreach (var childLimb in limbs)
					{
						if (childLimb != rootComponent)
						{
							StartCoroutine(fallRoutine(childLimb.gameObject, _destroyDelay));
						}
					}
				}
			}
		}
	}

	IEnumerator fallRoutine(GameObject obj, float delay)
	{
		// Remove colliders so object falls from view
		foreach (Collider2D collider in obj.GetComponentsInChildren<Collider2D>())
		{
			collider.enabled = false;
		}

		yield return new WaitForSeconds(delay);

		DestructableBehaviour destructable = obj.GetComponent<DestructableBehaviour>();
		if (destructable)
		{
			destructable.Destroy();
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
