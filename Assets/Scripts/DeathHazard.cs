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
			//level.ResetLevel();
			Application.LoadLevel(Application.loadedLevel);
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (tag.Length == 0 || other.rigidbody.gameObject.tag == tag)
		{
			// TODO -- something fancier...
			//level.ResetLevel();
			Application.LoadLevel(Application.loadedLevel);
		}
	}
}
