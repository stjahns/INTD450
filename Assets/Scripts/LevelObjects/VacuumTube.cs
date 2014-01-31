using UnityEngine;
using System.Collections;

public class VacuumTube : MonoBehaviour
{
	public float maxSpeed;
	public float force;
	public Rigidbody2D content;
	public Vector2 direction;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (content == null && !other.isTrigger)
		{
			content = other.attachedRigidbody;

			Vector2 tubeDirection = transform.rotation * Vector2.up;
			direction = transform.position - other.transform.position;

			if (Vector2.Dot(tubeDirection, transform.position - other.transform.position) > 0)
			{
				direction = tubeDirection;
			}
			else
			{
				direction = -tubeDirection;
			}

			if (audio)
			{
				audio.Play();
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		content = null;

		if (audio)
		{
			audio.Stop();
		}
	}

	void FixedUpdate ()
	{
		if (content != null)
		{
			if (content.velocity.magnitude < maxSpeed)
			{
				content.AddForce(direction * force);
			}
		}
	}
}
