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

	public int animatorAimLayer;
	public string aimX;
	public string aimY;

	public RobotComponent owner;
	public ParticleSystem emitter;

	public bool connectsGround = false;

	public AttachmentType attachmentType = AttachmentType.Default;

	public AttachmentSlot slot;

	bool m_selected;
	public bool selected
	{
		set
		{
			if (value)
			{
				emitter.startColor = Color.white;
			}
			else
			{
				emitter.startColor = Color.blue;
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
		emitter.startLifetime = 0.05f;
	}
}
