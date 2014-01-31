using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CannonballBehaviour : MonoBehaviour
{
	public List<string> collisionLayers;

	public SpriteRenderer explosion;

	public float explosionTime = 0.1f;
	public float explosionRadius = 1.0f;
	public float explosionForce = 100.0f;
	public float fuseTimer = 1.0f;
	public List<string> affectedTags = new List<string>();

	private float elapsedTime = 0.0f;

	public AudioClip explosionClip;

	void Update ()
	{
		elapsedTime += Time.deltaTime;
		if (elapsedTime > fuseTimer)
		{
			Explode();
		}
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (collisionLayers.Contains(LayerMask.LayerToName(coll.gameObject.layer)))
		{
			Explode();
		}
	}

	void Explode()
	{
		explosion.enabled = true;

		Destroy(gameObject.rigidbody2D);
		Destroy(gameObject, explosionTime);

		HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>();

		// Find all objects in an area...
		foreach (var other in Physics2D.OverlapCircleAll(transform.position, explosionRadius))
		{
			if (other.attachedRigidbody && affectedTags.Contains(other.attachedRigidbody.tag))
			{
				affectedBodies.Add(other.attachedRigidbody);
			}

			var destructable = other.gameObject.GetComponent<DestructableBehaviour>();
			if (destructable && destructable.enabled)
			{
				destructable.Explode();
			}
		}

		// apply force on all affected bodies
		foreach (var body in affectedBodies)
		{
			var direction = body.transform.position - transform.position;
			direction.Normalize();
			body.AddForce(direction * explosionForce);
		}

		AudioSource.PlayClipAtPoint(explosionClip, transform.position);
	}
}
