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

	// Tracks numbers of attached limb types
	private static Dictionary<LimbType, int> activeLimbs;

	//
	// Set static instance to this, mute all limb tracks
	// Limb tracks will be umnuted in their limb's Start() if they are attached,
	// and will be unmuted/muted when limbs are attached/detached
	//
	void Awake ()
	{
		LevelMusic.Instance = this;

		activeLimbs = new Dictionary<LimbType, int>();
		limbTypes.ForEach(l => activeLimbs.Add(l, 0));

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

	//
	// Allows us to set an editor icon
	//
	void OnDrawGizmos()
	{
	}

	//
	// Tell music system a limb was added, unmuting track if necessary
	//
	public void AttachLimb(LimbType limb)
	{
		if (activeLimbs.ContainsKey(limb))
		{
			activeLimbs[limb]++;
		}

		CheckTrackMuted(limb);
	}

	//
	// Tell music system a limb was removed, muting track if necessary
	//
	public void DetachLimb(LimbType limb)
	{
		if (activeLimbs.ContainsKey(limb))
		{
			activeLimbs[limb]--;
		}
		CheckTrackMuted(limb);
	}

	private void CheckTrackMuted(LimbType limb)
	{
		int trackIndex = limbTypes.FindIndex(t => t == limb);
		if (trackIndex != -1 && trackIndex < limbTracks.Count)
		{
			// Mute the track if less than one active limb of its type
			limbTracks[trackIndex].mute = activeLimbs[limb] < 1;
		}
	}
}
