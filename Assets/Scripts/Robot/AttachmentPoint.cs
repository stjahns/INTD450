﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AttachmentType
{
	Default,
	Arm,
	Leg,
	LevelAttachment
}

public class AttachmentPoint : TriggerBase
{
	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onAttach = new List<SignalConnection>();

	[OutputEventConnections]
	[HideInInspector]
	public List<SignalConnection> onDetach = new List<SignalConnection>();

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

	private DistanceJoint2D joint;

	public bool AttachedToLevelObject
	{
		get { return joint != null; }
	}

	public void AttachToLevelObject(AttachmentPoint levelAttachment)
	{
		joint = PlayerBehavior.Player.gameObject.AddComponent<DistanceJoint2D>();
		joint.anchor = transform.position - PlayerBehavior.Player.transform.position;
		joint.connectedBody = levelAttachment.collider2D.attachedRigidbody;
		joint.connectedAnchor = levelAttachment.transform.position
			- levelAttachment.collider2D.attachedRigidbody.transform.position;

		child = levelAttachment;
	}

	public void DetachFromLevelObject()
	{
		child = null;

		if (AttachedToLevelObject)
		{
			Destroy(joint);
			joint = null;
		}
	}

	public void OnAttach()
	{
		onAttach.ForEach(s => s.Fire());
	}

	public void OnDetach()
	{
		onDetach.ForEach(s => s.Fire());
	}

	public void SetAttachmentDistance(float distance)
	{
		joint.distance = distance;
	}

	// Use this for initialization
	void Start ()
	{
		selected = false;
	}
}
