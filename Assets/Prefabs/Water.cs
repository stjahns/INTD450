using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public float waterDrag;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.attachedRigidbody)
		{
			if (affectedTags.Contains(other.attachedRigidbody.tag))
			{
				if (!affectedBodies.Exists(b => b.rigidbody2D == other.attachedRigidbody));
				{
					var submergedBody = new SubmergedBody(other.attachedRigidbody);
					affectedBodies.Add(submergedBody);
					submergedBody.rigidbody2D.drag = waterDrag;
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.attachedRigidbody)
		{
			if (affectedTags.Contains(other.attachedRigidbody.tag))
			{
				var submergedBody = affectedBodies.Find(b => b.rigidbody2D == other.attachedRigidbody);
				submergedBody.rigidbody2D.drag = submergedBody.originalDrag;
				affectedBodies.Remove(submergedBody);
			}
		}
	}

}
