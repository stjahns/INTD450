using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LimbType
{
	Head,
	Torso,
	Cannon,
	Spring,
	Grapple
}

public class LevelMusic : MonoBehaviour
{
	public List<LimbType> limbTypes;
	public List<AudioSource> limbTracks;

	public static LevelMusic Instance;

	//
	// Mute or unmuted the autio track associated with the given LimbType
	//
	public void SetLimbTrackMuted(LimbType limb, bool muted)
	{
		int trackIndex = limbTypes.FindIndex(t => t == limb);
		if (trackIndex != -1 && trackIndex < limbTracks.Count)
		{
			limbTracks[trackIndex].mute = muted;
		}
	}

	//
	// Set static instance to this, mute all limb tracks
	// Limb tracks will be umnuted in their limb's Start() if they are attached,
	// and will be unmuted/muted when limbs are attached/detached
	//
	void Awake ()
	{
		LevelMusic.Instance = this;

		foreach (AudioSource track in limbTracks)
		{
			track.mute = true;
		}

	}

	//
	// Start playing all limb tracks at the same time
	//
	void Start ()
	{
		foreach (AudioSource track in limbTracks)
		{
			track.Play();
		}
	}
}
