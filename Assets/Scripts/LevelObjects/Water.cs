using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SubmergedBody
{
	public Rigidbody2D rigidbody2D;
	public float originalDrag;

	public SubmergedBody(Rigidbody2D body)
	{
		rigidbody2D = body;
		originalDrag = body.drag;
	}
}

public class Water : MonoBehaviour {

	public List<string> affectedTags;
	public List<SubmergedBody> affectedBodies = new List<SubmergedBody>();
	
	public List<Rigidbody2D> contents = new List<Rigidbody2D>();

	public AudioClip splashClip;
	public float splashVolume = 1.0f;

	public float waterDrag;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.attachedRigidbody)
		{
			if (affectedTags.Contains(other.attachedRigidbody.tag))
			{
				bool exists = affectedBodies.Exists(b => b.rigidbody2D == other.attachedRigidbody);
				if (exists == false)
				{
					var submergedBody = new SubmergedBody(other.attachedRigidbody);
					affectedBodies.Add(submergedBody);
					submergedBody.rigidbody2D.drag = waterDrag;

					AudioSource.PlayClipAtPoint(splashClip, transform.position, splashVolume);
				}
			}
		}
		contents = affectedBodies.Select(s => s.rigidbody2D).ToList();
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.attachedRigidbody)
		{
			if (affectedTags.Contains(other.attachedRigidbody.tag))
			{
				var submergedBody = affectedBodies.Find(b => b.rigidbody2D == other.attachedRigidbody);
				if (submergedBody != null)
				{
					submergedBody.rigidbody2D.drag = submergedBody.originalDrag;
					affectedBodies.Remove(submergedBody);
					AudioSource.PlayClipAtPoint(splashClip, transform.position, splashVolume);
				}
			}
		}
		contents = affectedBodies.Select(s => s.rigidbody2D).ToList();
	}

}
