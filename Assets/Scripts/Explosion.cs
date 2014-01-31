using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	public AudioClip explosionClip;

	void Start ()
	{
		if (explosionClip)
		{
			AudioSource.PlayClipAtPoint(explosionClip, transform.position);
		}
	}
}
