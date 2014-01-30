using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RobotComponent : MonoBehaviour {

	public bool attached = false;

	public RobotComponent parentComponent = null;
	public AttachmentPoint parentAttachmentPoint = null;

	public Transform groundCheck;

	public List<Collider2D> unattachedColliders = new List<Collider2D>();
	public List<Collider2D> attachedColliders = new List<Collider2D>();

	public List<RobotComponent> groundConnections = new List<RobotComponent>();

	public List<AttachmentPoint> allJoints = new List<AttachmentPoint>();

	public AudioSource limbTrack;

	public bool shouldAim = true;

	private bool resettingPhysics = false;

	public delegate void LimbChangedHandler(RobotComponent arm, AttachmentType type);
	public event LimbChangedHandler LimbAdded;
	public event LimbChangedHandler LimbRemoved;

	public delegate void PhysicsResetHandler(RobotComponent component);
	public event PhysicsResetHandler PhysicsReset;

	public GameObject explosionPrefab;
	public float explosionTime = 0.1f;

	public List<SpriteRenderer> sprites;

	void Awake()
	{
		ResetColliders();
	}

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

		// listen to childs' add/removelimb event
		unattachedPoint.owner.LimbAdded += OnLimbAdded;
		unattachedPoint.owner.LimbRemoved += OnLimbRemoved;

		foreach (RobotComponent limb in unattachedComponent.getAllChildren())
		{
			AttachmentType type = limb.parentAttachmentPoint.attachmentType;

			if (limb.limbTrack)
			{
				limb.limbTrack.mute = false;
			}

			OnLimbAdded(limb, type);
		}

		// HACK - bump up to make room
		getRootComponent().transform.Translate(0, 2.0f, 0);

		unattachedComponent.ResetColliders();
		getRootComponent().ResetPhysics();

	}

	public void OnLimbAdded(RobotComponent limb, AttachmentType type)
	{
		LimbAdded(limb, type);
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentType type)
	{
		LimbRemoved(limb, type);
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

		foreach (RobotComponent limb in child.owner.getAllChildren())
		{
			OnLimbRemoved(limb, attachmentType);

			if (limb.limbTrack)
			{
				limb.limbTrack.mute = true;
			}
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

	public bool checkOnGround(int layerMask) {

		foreach (RobotComponent component in groundConnections)
		{
			if (component.checkOnGround(layerMask))
			{
				return true;
			}
		}

		
		float checkDistance = (transform.position - groundCheck.position).magnitude;
		Vector3 checkPosition = transform.position + checkDistance * Vector3.down;

		//return Physics2D.Linecast(transform.position, groundCheck.position, layerMask);
		return Physics2D.Linecast(transform.position, checkPosition, layerMask);
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

		limbs.Add(this);

		return limbs;
	}

	public List<RobotComponent> getDirectChildren()
	{
		List<RobotComponent> limbs = new List<RobotComponent>();

		foreach (AttachmentPoint armJoint in allJoints)
		{
			if (armJoint.child && armJoint.child.owner)
			{
				limbs.Add(armJoint.child.owner);
			}
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

			if (PhysicsReset != null)
			{
				// Let things know (eg PlayerBehaviour) that rigid body was just reinitialized
				PhysicsReset(this);
			}
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
		ResetColliders();
		resettingPhysics = true;
		if (rigidbody2D)
		{
			Destroy(rigidbody2D);
		}
	}

	virtual public void ResetColliders()
	{
		unattachedColliders.ForEach(c => c.enabled = !attachedToPlayer());
		attachedColliders.ForEach(c => c.enabled = attachedToPlayer());
	}

	public void DestroyRobotComponent()
	{

		// Spawn explosion prefab
		if (explosionPrefab)
		{
			// TODO let the explosion destroy itself...
			var explosion = Instantiate(explosionPrefab, 
					transform.position, 
					Quaternion.identity) as GameObject;
			Destroy(explosion, explosionTime);
		}

		// hide all sprites for the component
		sprites.ForEach(s => s.enabled = false);

		// Destroy children
		foreach (var child in getDirectChildren())
		{
			child.DestroyRobotComponent();
		}
	}
}
