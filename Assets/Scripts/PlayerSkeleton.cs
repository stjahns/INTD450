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

	public enum Direction
	{
		Left,
		Right
	};

	public Direction direction;

	[HideInInspector]
	public static Vector3 leftScale = new Vector3(1, 1, 1);

	[HideInInspector]
	public static Vector3 rightScale = new Vector3(-1, 1, 1);

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

	public void LateUpdate()
	{
		Bone spine = GetBoneForSlot(AttachmentSlot.Spine);

		if (direction == Direction.Left)
		{
			spine.transform.localScale = leftScale;
		}
		else
		{
			spine.transform.localScale = rightScale;
		}
	}
}
