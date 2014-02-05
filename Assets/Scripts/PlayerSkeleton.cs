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
				break;
			case AttachmentSlot.Spine:
				return Spine;
				break;
			case AttachmentSlot.LeftShoulder:
				return LeftShoulder;
				break;
			case AttachmentSlot.RightShoulder:
				return RightShoulder;
				break;
			case AttachmentSlot.LeftHip:
				return LeftHip;
				break;
			case AttachmentSlot.RightHip:
				return RightHip;
				break;
		}

		return null;
	}

}
