using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum LimbType
{
	Head,
	Torso,
	Cannon,
	Spring,
	Grapple,
	Saw,
	Shield
}

public class LevelMusic : MonoBehaviour
{

	public List<AttachmentSlot> limbSlots;
	public List<SlotTracks> slotTracks;

	public static LevelMusic Instance;


	[Range(0,1)]
	public float volume = 1;

	//
	// Set static instance to this, mute all limb tracks
	// Limb tracks will be umnuted in their limb's Start() if they are attached,
	// and will be unmuted/muted when limbs are attached/detached
	//
	void Awake ()
	{
		LevelMusic.Instance = this;
	}

	//
	// Start playing all limb tracks at the same time
	//
	void Start ()
	{
		slotTracks.ForEach(s => s.PlayTracks());

		foreach (var source in GetComponentsInChildren<AudioSource>())
		{
			source.volume = volume;
		}
	}

	//
	// Allows us to set an editor icon
	//
	void OnDrawGizmos()
	{
	}

	public void AttachLimb(LimbType limb, AttachmentSlot slot)
	{
		int slotIndex = limbSlots.FindIndex(s => s == slot);
		if (slotIndex != -1 && slotIndex < slotTracks.Count)
		{
			SlotTracks tracks = slotTracks[slotIndex];
			tracks.UnmuteTrack(limb);
		}
	}

	public void DetachLimb(LimbType limb, AttachmentSlot slot)
	{
		int slotIndex = limbSlots.FindIndex(s => s == slot);
		if (slotIndex != -1 && slotIndex < slotTracks.Count)
		{
			SlotTracks tracks = slotTracks[slotIndex];
			tracks.MuteTrack(limb);
		}
	}
}
