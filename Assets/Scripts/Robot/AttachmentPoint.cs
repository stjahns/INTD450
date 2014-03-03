using UnityEngine;
using System.Collections;

public enum AttachmentType {
	Default,
	Arm,
	Leg
}

public class AttachmentPoint : MonoBehaviour {

	public AttachmentPoint parent = null;
	public AttachmentPoint child = null;

	public string animatorAimFlag;
	public string aimX;
	public string aimY;

	public RobotComponent owner;

	public bool connectsGround = false;

	public AttachmentType attachmentType = AttachmentType.Default;

	public AttachmentSlot slot;

	public LightningRenderer lightningEffect;

	public Bone bone;

	public Transform childTransform
	{
		set
		{
			if (value)
			{
				lightningEffect.end = value;
				lightningEffect.enabled = true;
			}
			else
			{
				lightningEffect.enabled = false;
			}
		}
	}

	bool m_selected;
	public bool selected
	{
		set
		{
			if (value)
			{
				lightningEffect.color = Color.white; 
			}
			else
			{
				lightningEffect.color = Color.blue; 
			}
			m_selected = value;
		}
		get
		{
			return m_selected;
		}
	}

	// Use this for initialization
	void Start ()
	{
		selected = false;
	}
}
