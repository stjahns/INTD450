using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	public AudioClip explosionClip;

	public float time = 1f;

	void Start ()
	{
		if (explosionClip)
		{
			AudioSource.PlayClipAtPoint(explosionClip, transform.position);
		}

		Destroy(gameObject, 1f);
	}
}

