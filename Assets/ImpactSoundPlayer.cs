using UnityEngine;
using System.Collections;

public class ImpactSoundPlayer : MonoBehaviour
{

	public AudioClip impactClip;
	[Range(0,1)]
	public float volume = 1;
	public float velocityChangeThreshold = 1;

	private Vector2 _lastVelocity;

	void Start()
	{
		_lastVelocity = rigidbody2D.velocity;
	}

	void FixedUpdate()
	{
		float change = (_lastVelocity - rigidbody2D.velocity).magnitude;
		if (change > velocityChangeThreshold)
		{
			AudioSource3D.PlayClipAtPoint(impactClip, transform.position, volume);
		}
		_lastVelocity = rigidbody2D.velocity;
	}
}
