﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotComponent : MonoBehaviour {

	public bool attached = false;

	public RobotComponent parentComponent = null;
	public AttachmentPoint parentAttachmentPoint = null;

	public Transform groundCheck;

	public List<RobotComponent> groundConnections = new List<RobotComponent>();

	public List<AttachmentPoint> allJoints = new List<AttachmentPoint>();

	public bool shouldAim = true;

	private bool resettingPhysics = false;

	public delegate void LimbChangedHandler(RobotComponent arm, AttachmentType type);
	public event LimbChangedHandler LimbAdded;
	public event LimbChangedHandler LimbRemoved;

	public void Attach(AttachmentPoint attachedPoint, AttachmentPoint unattachedPoint)
	{
		RobotComponent unattachedComponent = unattachedPoint.owner;
		unattachedComponent.transform.parent = attachedPoint.transform;

		// If we don't set body to kinematic, it will get will physics update that will 
		// move component after attaching...
		unattachedComponent.rigidbody2D.isKinematic = true;
		Destroy(unattachedPoint.owner.rigidbody2D);

		attachedPoint.child = unattachedPoint;
		unattachedPoint.parent = attachedPoint;

		unattachedPoint.owner.parentComponent = attachedPoint.owner;
		unattachedPoint.owner.parentAttachmentPoint = attachedPoint;

		if (attachedPoint.connectsGround)
		{
			groundConnections.Add(unattachedPoint.owner);
		}

		// set position + orientation
		//Vector3 offset = unattachedPoint.transform.localPosition;
		//unattachedPoint.owner.transform.localEulerAngles = new Vector3(0,0,0);
		//unattachedPoint.owner.transform.localPosition= new Vector3(-offset.x, -offset.y);

		// listen to childs' add/removeArm event
		unattachedPoint.owner.LimbAdded += OnLimbAdded;
		unattachedPoint.owner.LimbRemoved += OnLimbRemoved;

		foreach (RobotComponent arm in unattachedComponent.getAllChildren())
		{
			AttachmentType type = arm.parentAttachmentPoint.attachmentType;
			OnLimbAdded(arm, type);
		}

		// HACK - bump up to make room
		getRootComponent().transform.Translate(0, 2.0f, 0);

		getRootComponent().ResetPhysics();

	}

	public void OnLimbAdded(RobotComponent arm, AttachmentType type)
	{
		LimbAdded(arm, type);
	}

	public void OnLimbRemoved(RobotComponent arm, AttachmentType type)
	{
		LimbRemoved(arm, type);
	}

	public void Unattach(AttachmentPoint parent, AttachmentPoint child)
	{
		AttachmentType attachmentType = child.owner.parentAttachmentPoint.attachmentType;
		child.owner.transform.parent = null;

		parent.child = null;
		child.parent = null;
		child.owner.parentComponent = null;
		child.owner.parentAttachmentPoint= null;

		if (parent.connectsGround)
		{
			groundConnections.Remove(child.owner);
		}

		foreach (RobotComponent arm in child.owner.getAllChildren())
		{
			OnLimbRemoved(arm, attachmentType);
		}

		// stop listening to childs' add/removeArm event
		child.owner.LimbAdded -= OnLimbAdded;
		child.owner.LimbRemoved -= OnLimbRemoved;

		// Restore rigid body to unattached child part
		child.owner.ResetPhysics();
	}

	public bool attachedToPlayer()
	{
		if (parentComponent)
		{
			return parentComponent.attachedToPlayer();
		}
		return attached;
	}

	public RobotComponent getRootComponent()
	{
		if (parentComponent)
		{
			return parentComponent.getRootComponent();
		}
		else
		{
			return this;
		}
	}

	public bool checkOnGround() {

		foreach (RobotComponent component in groundConnections)
		{
			if (component.checkOnGround())
			{
				return true;
			}
		}

		int groundOnly = 1 << LayerMask.NameToLayer("Ground");
		return Physics2D.Linecast(transform.position, groundCheck.position, groundOnly);
	}

	public List<RobotComponent> getAllChildren()
	{
		List<RobotComponent> limbs = new List<RobotComponent>();

		foreach (AttachmentPoint armJoint in allJoints)
		{
			if (armJoint.child && armJoint.child.owner)
			{
				limbs.AddRange(armJoint.child.owner.getAllChildren());
			}
		}

		if (allJoints.Count == 0)
		{
			limbs.Add(this);
		}

		return limbs;
	}

	virtual public void FireAbility()
	{
		Debug.Log("Fire Dummy Ability");
	}

	void LateUpdate() 
	{
		if (resettingPhysics)
		{

			// Recursively reset local transforms for all child joints.
			ResetJointTransforms();

			resettingPhysics = false;
			var body = gameObject.AddComponent<Rigidbody2D>();
			body.fixedAngle = attachedToPlayer();

			// TODO restore any other properties..?
		}
	}

	virtual public void ResetJointTransforms()
	{
		foreach (AttachmentPoint point in allJoints)
		{
			if (point.child && point.child.owner)
			{

				Vector3 offset = point.child.transform.localPosition;
				point.child.owner.transform.localPosition = new Vector3(-offset.x, -offset.y);
				point.child.owner.transform.localEulerAngles = Vector3.zero;
				point.child.owner.ResetJointTransforms();
			}
		}
	}

	virtual public void ResetPhysics()
	{
		resettingPhysics = true;
		if (rigidbody2D)
		{
			Destroy(rigidbody2D);
		}
	}
}
