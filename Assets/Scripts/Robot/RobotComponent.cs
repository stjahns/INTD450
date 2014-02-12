using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RobotComponent : MonoBehaviour {

	// Public Vars
	public LimbType limbType;
	public bool attached = false;

	public RobotComponent parentComponent = null;
	public AttachmentPoint parentAttachmentPoint = null;

	public float partLength = 1.0f;

	public Transform groundCheck;
	public Transform wallCheck;

	public List<Collider2D> unattachedColliders = new List<Collider2D>();
	public List<Collider2D> attachedColliders = new List<Collider2D>();

	public List<RobotComponent> groundConnections = new List<RobotComponent>();

	public List<AttachmentPoint> allJoints = new List<AttachmentPoint>();

	public AudioSource SFXSource;

	public bool shouldAim = true;

	public GameObject explosionPrefab;
	public float explosionTime = 0.1f;

	public List<SpriteRenderer> sprites;

	public PlayerSkeleton Skeleton = null;

	public Transform lowerLimbJoint;
	public GameObject upperLimb;
	public SpriteRenderer upperLimbSprite;
	public GameObject lowerLimb;
	public List<SpriteRenderer> lowerLimbSprites;
	public List<int> lowerLimbSpriteOrders;

	public Bone currentBone = null;

	// Private Vars
	private bool resettingPhysics = false;

	// Events
	public delegate void LimbChangedHandler(RobotComponent arm, AttachmentType type);
	public delegate void PhysicsResetHandler(RobotComponent component);
	public delegate void OnDestroyHandler(RobotComponent component);

	public event LimbChangedHandler LimbAdded;
	public event LimbChangedHandler LimbRemoved;
	public event PhysicsResetHandler PhysicsReset;
	public event OnDestroyHandler OnDestroy;

	void Awake()
	{
		ResetColliders();
	}

	public bool IsArm
	{
		get
		{
			if (parentAttachmentPoint)
			{
				return parentAttachmentPoint.slot == AttachmentSlot.LeftShoulder
					|| parentAttachmentPoint.slot == AttachmentSlot.RightShoulder;
			}

			return false;
		}
	}

	public bool IsLeg
	{
		get
		{
			if (parentAttachmentPoint)
			{
				return parentAttachmentPoint.slot == AttachmentSlot.LeftHip
					|| parentAttachmentPoint.slot == AttachmentSlot.RightHip;
			}

			return false;
		}
	}


	virtual public void Start()
	{
		if (attachedToPlayer())
		{
			if (LevelMusic.Instance)
			{
				LevelMusic.Instance.AttachLimb(limbType);
			}
		}
	}

	public void ResetSpriteOrders()
	{
		foreach (RobotComponent part in getDirectChildren())
		{
			part.ResetSpriteOrders();
		}

		upperLimbSprite.sortingOrder = (int)currentBone.spriteOrder;

		if (currentBone.LowerJoint)
		{
			for (int i = 0; i < lowerLimbSprites.Count; ++i)
			{
				lowerLimbSprites[i].sortingOrder = (int)currentBone.LowerJoint.spriteOrder + lowerLimbSpriteOrders[i];
			}
		}
	}

	public void Attach(AttachmentPoint parentJoint, AttachmentPoint childJoint)
	{
		AttachmentSlot slot = parentJoint.slot;
		Bone bone = Skeleton.GetBoneForSlot(slot);

		RobotComponent child = childJoint.owner;

		child.currentBone = bone;

		child.Skeleton = Skeleton;

		// parent child to bone
		child.transform.parent = bone.gameObject.transform;
		child.transform.localScale = Vector3.one;
		child.transform.localPosition = Vector3.zero;
		child.transform.localEulerAngles = Vector3.zero;

		if (child.upperLimbSprite)
		{
			child.upperLimbSprite.sortingLayerName = "Player";
			child.upperLimbSprite.sortingOrder = (int)bone.spriteOrder;
		}

		// if child has lowerlimb, parent lowerlimb to bone.lowerlimb
		if (child.lowerLimb && bone.LowerJoint)
		{
			child.lowerLimb.transform.parent = bone.LowerJoint.gameObject.transform;
			child.lowerLimb.transform.localPosition = Vector3.zero;
			child.lowerLimb.transform.localEulerAngles = Vector3.zero;

			for (int i = 0; i < child.lowerLimbSprites.Count; ++i)
			{
				child.lowerLimbSprites[i].sortingLayerName = "Player";
				child.lowerLimbSprites[i].sortingOrder = (int)bone.LowerJoint.spriteOrder + child.lowerLimbSpriteOrders[i];
			}

			if (bone.LowerJoint.spriteMirrored)
			{
				for (int i = 0; i < child.lowerLimbSprites.Count; ++i)
				{
					Vector3 scale = child.lowerLimbSprites[i].gameObject.transform.localScale;
					scale.x *= -1;
					child.lowerLimbSprites[i].gameObject.transform.localScale = scale;
				}
			}
		}

		if (childJoint.rigidbody2D)
		{
			// If we don't set body to kinematic, it will get will physics update that will 
			// move component after attaching...
			child.rigidbody2D.isKinematic = true;
		}

		if (childJoint.owner.rigidbody2D)
		{
			Destroy(childJoint.owner.rigidbody2D);
		}


		// Parent all child joints to the bones themselves (for torso)
		foreach (AttachmentPoint joint in childJoint.owner.allJoints)
		{
			Bone jointBone = Skeleton.GetBoneForSlot(joint.slot);
			joint.transform.parent = jointBone.transform;
			joint.transform.localPosition = Vector3.zero;
			joint.transform.localRotation = Quaternion.identity;
		}

		parentJoint.child = childJoint;
		childJoint.parent = parentJoint;

		childJoint.owner.parentComponent = parentJoint.owner;
		childJoint.owner.parentAttachmentPoint = parentJoint;

		if (parentJoint.connectsGround)
		{
			groundConnections.Add(childJoint.owner);
		}

		// listen to childs' add/removelimb event
		childJoint.owner.LimbAdded += OnLimbAdded;
		childJoint.owner.LimbRemoved += OnLimbRemoved;

		foreach (RobotComponent limb in child.getAllChildren())
		{
			AttachmentType type = limb.parentAttachmentPoint.attachmentType;

			if (LevelMusic.Instance)
			{
				LevelMusic.Instance.AttachLimb(limb.limbType);
			}

			OnLimbAdded(limb, type);
		}

		child.ResetColliders();
		getRootComponent().ResetPhysics();

	}

	public void Unattach(AttachmentPoint parentJoint, AttachmentPoint childJoint)
	{
		AttachmentType attachmentType = childJoint.owner.parentAttachmentPoint.attachmentType;

		RobotComponent child = childJoint.owner;
		child.Skeleton = null;

		parentJoint.child = null;
		childJoint.parent = null;
		childJoint.owner.parentComponent = null;
		childJoint.owner.parentAttachmentPoint= null;

		if (parentJoint.connectsGround)
		{
			groundConnections.Remove(childJoint.owner);
		}

		// parent child to null
		child.transform.parent = null;
		child.parentComponent = null;
		child.Skeleton = null;

		// if child has lowerlimb, parent lowerlimb to child.join
		if (child.lowerLimb)
		{
			child.lowerLimb.transform.parent = child.lowerLimbJoint;
			child.lowerLimb.transform.localPosition = Vector3.zero;
			child.lowerLimb.transform.localEulerAngles = Vector3.zero;
			child.lowerLimb.transform.localScale = Vector3.one;
		}

		// Unparent all child joints from bones
		foreach (AttachmentPoint joint in childJoint.owner.allJoints)
		{
			joint.transform.parent = joint.owner.transform;
		}

		child.transform.localScale = Vector3.one;

		// Also remove all children of limb from skeleton...
		foreach (RobotComponent limb in childJoint.owner.getDirectChildren())
		{
			childJoint.owner.Unattach(limb.parentAttachmentPoint, limb.parentAttachmentPoint.child);
		}

		foreach (RobotComponent limb in childJoint.owner.getAllChildren())
		{
			OnLimbRemoved(limb, attachmentType);

			if (LevelMusic.Instance)
			{
				LevelMusic.Instance.DetachLimb(limb.limbType);
			}
		}

		// stop listening to childs' add/removeArm event
		childJoint.owner.LimbAdded -= OnLimbAdded;
		childJoint.owner.LimbRemoved -= OnLimbRemoved;

		// Restore rigid body to unattached childJoint part
		childJoint.owner.ResetPhysics();
	}


	public void OnLimbAdded(RobotComponent limb, AttachmentType type)
	{
		if (LimbAdded != null)
		{
			LimbAdded(limb, type);
		}
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentType type)
	{
		if (LimbRemoved != null)
		{
			LimbRemoved(limb, type);
		}
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

	virtual public void Update() 
	{
		if (wallCheck)
		{
			// Normally, colliders on limbs should prevent you from sticking them through walls
			// However, a consequence of having limb collider transforms under animation control
			// (especially mouse direction) means that an arm can be quickly animated into a wall,
			// getting stuck. This is a hacky solution to pop the player out of the wall if a limb
			// ever gets jammed in

			int layerMask = 1 << LayerMask.NameToLayer("Ground");

			// Check if part is jammed into a wall
			RaycastHit2D hit = Physics2D.Linecast(transform.position, wallCheck.position, layerMask);
			if (hit)
			{
				// Bump player out of wall 
				// (Get projection of overlapped linecast onto wall normal, and bump player by that vector)
				Vector2 cast = transform.position - wallCheck.position;
				cast *= 1 - hit.fraction;
				Vector2 bump = (Vector2.Dot(cast, hit.normal) / Vector2.Dot(hit.normal, hit.normal)) * hit.normal;
				PlayerBehavior.Player.transform.position += bump.XY0();

				// TODO handle edge case where bumping off right wall causes opposite limb to enter
				// left wall (eg spreading arms in cramped space) -- better to just leave arm in
				// wall, but try to avoid this situation in level design
			}
		}
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
		if (attachedToPlayer())
		{
			unattachedColliders.ForEach(c => c.enabled = false);
			attachedColliders.ForEach(c => c.enabled = true);
		}
		else
		{
			attachedColliders.ForEach(c => c.enabled = false);
			unattachedColliders.ForEach(c => c.enabled = true);
		}


		foreach (var c in unattachedColliders.Concat(attachedColliders))
		{
			c.gameObject.layer = attachedToPlayer() ?
				LayerMask.NameToLayer("Player") :
				LayerMask.NameToLayer("LevelObjects");
		}

	}

	public void DestroyRobotComponent()
	{
		// Detatch from parent...
		if (parentComponent && parentAttachmentPoint && parentAttachmentPoint.child)
		{
			parentComponent.Unattach(parentAttachmentPoint, parentAttachmentPoint.child);
		}

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

		// any child robot components should be unattached now...

		for (int i = 0; i < transform.GetChildCount(); ++i)
		{
			//Destroy(transform.GetChild(i).gameObject, explosionTime);
		}

		Destroy(gameObject, explosionTime);

		if (OnDestroy != null)
		{
			OnDestroy(this);
		}
	}
}
