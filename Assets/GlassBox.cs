using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlassBox : MonoBehaviour
{
	public List<GameObject> parts;
	public float shatterForce = 100;
	public float shatterTorque = 10;

	[Range(0,1)]
	public float shatterVolume = 1;
	public AudioClip shatterClip;

	void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log(other);
		if (other.tag == "BoxShatter")
		{
			// shatter!
			Destroy(rigidbody2D);

			foreach (var part in parts)
			{
				Rigidbody2D body = part.AddComponent<Rigidbody2D>();
				var direction = body.transform.position - transform.position + Vector3.up;
				body.AddForce(direction * shatterForce);
				body.AddTorque(Random.RandomRange(-shatterTorque, shatterTorque));

				foreach (var collider in part.GetComponentsInChildren<Collider2D>())
				{
					Destroy(collider);
				}
			}

			AudioSource3D.PlayClipAtPoint(shatterClip, transform.position, shatterVolume);

			Destroy(gameObject, 5);
		}
	}
}
