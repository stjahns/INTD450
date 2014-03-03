using UnityEngine;
using System.Collections;

public enum AttachmentSlot
{
	None,
	Neck,
	Spine,
	LeftShoulder,
	RightShoulder,
	LeftHip,
	RightHip,
}

public class PlayerSkeleton : MonoBehaviour
{

	public Bone Neck;
	public Bone Spine;
	public Bone LeftShoulder;
	public Bone RightShoulder;
	public Bone LeftHip;
	public Bone RightHip;

	public Bone GetBoneForSlot(AttachmentSlot slot)
	{
		switch (slot)
		{
			case AttachmentSlot.Neck:
				return Neck;
			case AttachmentSlot.Spine:
				return Spine;
			case AttachmentSlot.LeftShoulder:
				return LeftShoulder;
			case AttachmentSlot.RightShoulder:
				return RightShoulder;
			case AttachmentSlot.LeftHip:
				return LeftHip;
			case AttachmentSlot.RightHip:
				return RightHip;
		}

		return null;
	}

}
