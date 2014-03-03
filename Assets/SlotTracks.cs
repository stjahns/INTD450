using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlotTracks : MonoBehaviour
{
	public List<LimbType> limbTypes;

	private List<AudioSource> limbTracks;

	void Awake ()
	{
		limbTracks = GetComponents<AudioSource>().ToList();

		foreach (AudioSource track in limbTracks)
		{
			track.mute = true;
		}
	}

	public void PlayTracks()
	{
		limbTracks.ForEach(t => t.Play());
	}

	public void MuteTrack(LimbType type)
	{
		int trackIndex = limbTypes.FindIndex(t => t == type);
		if (trackIndex != -1 && trackIndex < limbTracks.Count)
		{
			limbTracks[trackIndex].mute = true;
		}
	}

	public void UnmuteTrack(LimbType type)
	{
		int trackIndex = limbTypes.FindIndex(t => t == type);
		if (trackIndex != -1 && trackIndex < limbTracks.Count)
		{
			limbTracks[trackIndex].mute = false;
		}
	}

	//
	// Allows us to set an editor icon
	//
	void OnDrawGizmos()
	{
	}
}
