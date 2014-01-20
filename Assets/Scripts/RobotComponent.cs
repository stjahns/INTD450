using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RobotComponent : MonoBehaviour {

	public bool attached = false;

	public RobotComponent parentComponent = null;
	public AttachmentPoint parentAttachmentPoint = null;

	public Transform groundCheck;

	public List<RobotComponent> groundConnections = new List<RobotComponent>();

	public List<AttachmentPoint> armAbilityJoints = new List<AttachmentPoint>();
	public List<AttachmentPoint> legAbilityJoints = new List<AttachmentPoint>();

	public bool shouldAim = true;

	public void Attach(AttachmentPoint attachedPoint, AttachmentPoint unattachedPoint)
	{
		RobotComponent unattachedComponent = unattachedPoint.owner;
		unattachedComponent.transform.parent = attachedPoint.transform;

		// set position + orientation
		Vector3 offset = unattachedPoint.transform.localPosition;
		unattachedPoint.owner.transform.localEulerAngles = new Vector3(0,0,0);
		unattachedPoint.owner.transform.localPosition= new Vector3(-offset.x, -offset.y);

		// If we don't set body to kinematic, it will get will physics update that will move component after attaching...
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

		// HACK - bump up to make room
		float bump = attachedPoint.owner.transform.position.y - unattachedPoint.owner.transform.position.y;
		getRootComponent().transform.Translate(0, bump, 0);
		//head.transform.Translate(0, bump, 0);

	}

	public void Unattach(AttachmentPoint parent, AttachmentPoint child)
	{
		child.owner.transform.parent = null;

		parent.child = null;
		child.parent = null;
		child.owner.parentComponent = null;
		child.owner.parentAttachmentPoint= null;

		if (parent.connectsGround)
		{
			groundConnections.Remove(child.owner);
		}

		// Restore rigid body to unattached child part
		child.owner.gameObject.AddComponent<Rigidbody2D>();
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

	public List<RobotComponent> getArmLimbs()
	{
		List<RobotComponent> limbs = new List<RobotComponent>();

		foreach (AttachmentPoint armJoint in armAbilityJoints)
		{
			if (armJoint.child && armJoint.child.owner)
			{
				limbs.AddRange(armJoint.child.owner.getArmLimbs());
			}
		}

		if (armAbilityJoints.Count == 0)
		{
			limbs.Add(this);
		}

		return limbs;
	}

	virtual public void FireAbility()
	{
		Debug.Log("Fire Dummy Ability");
	
	}

}
