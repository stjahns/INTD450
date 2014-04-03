using UnityEngine;
using System.Collections;

public class ShieldEffectBehvaviour : MonoBehaviour {

	public AudioClip deflectClip;

	[InputSocket]
	public void TakeDamage(int damage)
	{
		AudioSource3D.PlayClipAtPoint(deflectClip, transform.position);

		// TODO take hit position as a param, show an effect?
	}
}
