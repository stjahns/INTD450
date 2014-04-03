using UnityEngine;
using System.Collections;

public static class AudioSource3D
{
	public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f)
	{
        GameObject prefab = Resources.Load("3DAudioSource") as GameObject;
		GameObject sourceObject = Object.Instantiate(prefab, position, Quaternion.identity)
			as GameObject;
		AudioSource source = sourceObject.GetComponent<AudioSource>();
		source.PlayOneShot(clip, volume);
		Object.Destroy(sourceObject, clip.length + 1.0f);
	}

	public static void PlayClipOmnipresent(AudioClip clip, float volume = 1.0f)
	{
        GameObject prefab = Resources.Load("3DAudioSource") as GameObject;
		GameObject sourceObject = Object.Instantiate(prefab, Vector3.one, Quaternion.identity)
			as GameObject;
		AudioSource source = sourceObject.GetComponent<AudioSource>();
		source.spread = 180;
		source.pan = 0;
		source.dopplerLevel = 0;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.maxDistance = 1000000;
		source.volume = volume;
		source.clip = clip;
		source.Play();
		Object.Destroy(sourceObject, clip.length + 1.0f);
	}
}
