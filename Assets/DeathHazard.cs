using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeathHazard : MonoBehaviour
{
	public string tag;
	public Level level;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (tag.Length == 0 || other.attachedRigidbody.gameObject.tag == tag)
		{
			// TODO -- something fancier...
			level.Reset();
		}
	}
}
