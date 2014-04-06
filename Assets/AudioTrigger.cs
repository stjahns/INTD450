using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioTrigger : MonoBehaviour
{
	[InputSocket]
	public void PlayAudio()
	{
		audio.Play();
	}

	[InputSocket]
	public void StopAudio()
	{
		audio.Stop();
	}

	public void OnDrawGizmos()
	{
	}
}
