using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RobotComponent : MonoBehaviour {

	// Public Vars
	public LimbType limbType;

	[HideInInspector]
	public bool attached = false;

	[HideInInspector]
	public RobotComponent parentComponent = null;

	[HideInInspector]
	public AttachmentPoint parentAttachmentPoint = null;

	public float partLength = 1.0f;

	public Transform groundCheck;
	public Transform wallCheck;

	public List<Collider2D> unattachedColliders = new List<Collider2D>();
	public List<Collider2D> attachedColliders = new List<Collider2D>();

	[HideInInspector]
	public List<RobotComponent> groundConnections = new List<RobotComponent>();

	public List<AttachmentPoint> allJoints = new List<AttachmentPoint>();

	public AudioSource SFXSource;

	[HideInInspector]
	public bool shouldAim = true;

	private bool _isActive = false;
	private Color inactiveColor;
	public bool isActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
			foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
			{
				renderer.color = value? new Color(1f,0.5f,0.5f) : Color.white;
			}
		}
	}


	public GameObject explosionPrefab;
	public float explosionTime = 0.1f;

	public List<SpriteRenderer> sprites;
	public List<int> spriteOrders;

	[HideInInspector]
	public PlayerSkeleton Skeleton = null;

	[HideInInspector]
	public Bone currentBone = null;

	public AttachmentPoint rootJoint;

	// Private Vars
	private bool resettingPhysics = false;

	// Events
	public delegate void LimbChangedHandler(RobotComponent arm, AttachmentSlot slot, AttachmentType type);
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

	public AttachmentSlot Slot
	{
		get
		{
			if (parentAttachmentPoint)
			{
				return parentAttachmentPoint.slot;
			}

			// Head doesn't have a parent, just assume its in neck slot
			if (limbType == LimbType.Head)
			{
				return AttachmentSlot.Neck;
			}

			return AttachmentSlot.None;
		}
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
				LevelMusic.Instance.AttachLimb(limbType, Slot);
			}
		}

		PulsingSprite pulser = gameObject.AddComponent<PulsingSprite>();
		pulser.pulseFrequency = 25;
		pulser.minAlpha = 0.5f;

		var destructable = GetComponent<DestructableBehaviour>();
		if (destructable)
		{
			destructable.Destroyed += OnDestructableDestroyed;
		}
	}

	virtual public void ResetSpriteOrders()
	{
		foreach (RobotComponent part in getDirectChildren())
		{
			part.ResetSpriteOrders();
		}

		for (int i = 0; i < sprites.Count; ++i)
		{
			sprites[i].sortingOrder = (int)currentBone.spriteOrder + spriteOrders[i];

			if (Skeleton.direction == PlayerSkeleton.Direction.Left)
			{
				sprites[i].transform.localScale = PlayerSkeleton.leftScale;
			}
			else
			{
				sprites[i].transform.localScale = PlayerSkeleton.rightScale;
			}

		}

		foreach (AttachmentPoint joint in allJoints)
		{
			joint.lightningEffect.sortingOrder = (int)joint.bone.spriteOrder;
			joint.lightningEffect.sortingLayerName = "Player";
		}
	}

	virtual public void OnAttach()
	{
		foreach (AttachmentPoint joint in allJoints)
		{
			joint.lightningEffect.sortingLayerName = "Player";
		}

		for (int i = 0; i < sprites.Count; ++i)
		{
			sprites[i].sortingLayerName = "Player";
			sprites[i].sortingOrder = (int)currentBone.spriteOrder + spriteOrders[i];
		}

		if (currentBone.spriteMirrored)
		{
			for (int i = 0; i < sprites.Count; ++i)
			{
				Vector3 scale = sprites[i].gameObject.transform.localScale;
				scale.x *= -1;
				sprites[i].gameObject.transform.localScale = scale;
			}
		}
	}

	virtual public void OnRemove()
	{
		isActive = false;
	}

	public AttachmentPoint GetJointForSlot(AttachmentSlot slot)
	{
		return allJoints.FirstOrDefault(j => j.slot == slot);
	}

	public void Attach(AttachmentPoint parentJoint, AttachmentPoint childJoint)
	{
		AttachmentSlot slot = parentJoint.slot;
		Bone bone = Skeleton.GetBoneForSlot(slot);

		RobotComponent child = childJoint.owner;

		// Parent to lower joint bone if it exists (for limbs)
		if (bone.LowerJoint)
		{
			bone = bone.LowerJoint;
		}

		child.currentBone = bone;
		child.Skeleton = Skeleton;

		// parent child to player object
		child.transform.parent = transform.rootParent();
		// attach child to bone
		bone.Attach(child.transform);

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
			joint.bone = jointBone;
			joint.transform.parent = transform.rootParent();
			jointBone.Attach(joint.transform);
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
				LevelMusic.Instance.AttachLimb(limb.limbType, limb.Slot);
			}

			OnLimbAdded(limb, slot, type);
		}

		parentJoint.childTransform = childJoint.transform;

		child.OnAttach();

		child.ResetColliders();

		getRootComponent().ResetSpriteOrders();
		getRootComponent().ResetPhysics();

	}

	public void Unattach(AttachmentPoint parentJoint, AttachmentPoint childJoint)
	{
		// First remove all children of limb from skeleton...
		foreach (RobotComponent limb in childJoint.owner.getDirectChildren())
		{
			childJoint.owner.Unattach(limb.parentAttachmentPoint, limb.parentAttachmentPoint.child);
		}

		AttachmentSlot slot = parentJoint.slot;
		Bone bone = Skeleton.GetBoneForSlot(slot);

		// Unparent from lower joint bone if it exists (for limbs)
		if (bone && bone.LowerJoint)
		{
			bone = bone.LowerJoint;
		}

		AttachmentType attachmentType = childJoint.owner.parentAttachmentPoint.attachmentType;

		RobotComponent child = childJoint.owner;
		child.Skeleton = null;

		if (LevelMusic.Instance)
		{
			LevelMusic.Instance.DetachLimb(childJoint.owner.limbType, childJoint.owner.Slot);
		}

		parentJoint.child = null;
		childJoint.parent = null;
		childJoint.owner.parentComponent = null;
		childJoint.owner.parentAttachmentPoint= null;

		if (parentJoint.connectsGround)
		{
			groundConnections.Remove(childJoint.owner);
		}

		parentJoint.childTransform = parentJoint.transform;

		// parent child to null
		bone.Detach(child.transform);
		child.transform.parent = null;
		child.parentComponent = null;
		child.Skeleton = null;

		// Unparent all child joints from bones
		foreach (AttachmentPoint joint in childJoint.owner.allJoints)
		{
			Bone jointBone = Skeleton.GetBoneForSlot(joint.slot);
			jointBone.Detach(joint.transform);
			joint.transform.parent = joint.owner.transform;
		}

		child.transform.localScale = Vector3.one;

		childJoint.owner.OnRemove();
		
		OnLimbRemoved(childJoint.owner, slot, attachmentType);

		// stop listening to childs' add/removeArm event
		childJoint.owner.LimbAdded -= OnLimbAdded;
		childJoint.owner.LimbRemoved -= OnLimbRemoved;

		// Restore rigid body to unattached childJoint part
		childJoint.owner.ResetPhysics();
	}


	public void OnLimbAdded(RobotComponent limb, AttachmentSlot slot, AttachmentType type)
	{
		if (LimbAdded != null)
		{
			LimbAdded(limb, slot, type);
		}
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentSlot slot, AttachmentType type)
	{
		if (LimbRemoved != null)
		{
			LimbRemoved(limb, slot, type);
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

	public bool checkOnGround(int layerMask)
	{

		foreach (RobotComponent component in groundConnections)
		{
			if (component.checkOnGround(layerMask))
			{
				return true;
			}
		}

		
		float checkDistance = (transform.position - groundCheck.position).magnitude;
		Vector3 checkPosition = transform.position + checkDistance * Vector3.down;
		return Physics2D.OverlapArea(new Vector2(checkPosition.x - 0.25f, checkPosition.y + 0.05f),
				new Vector2(checkPosition.x + 0.25f, checkPosition.y),
				layerMask );
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

		foreach (AttachmentPoint joint in allJoints)
		{
			if (joint.child && joint.child.owner)
			{
				limbs.Add(joint.child.owner);
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
		if (attachedToPlayer() && wallCheck)
		{
			// Normally, colliders on limbs should prevent you from sticking them through walls
			// However, a consequence of having limb collider transforms under animation control
			// (especially mouse direction) means that an arm can be quickly animated into a wall,
			// getting stuck. This is a hacky solution to pop the player out of the wall if a limb
			// ever gets jammed in

			int layerMask = 1 << LayerMask.NameToLayer("Ground");

			// Check if part is jammed into a wall
			RaycastHit2D hit = Physics2D.Linecast(PlayerBehavior.Player.transform.position, wallCheck.position, layerMask);
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
			if (rigidbody2D == null)
			{
				// Recursively reset local transforms for all child joints.
				ResetJointTransforms();

				resettingPhysics = false;
				gameObject.AddComponent<Rigidbody2D>();

				rigidbody2D.fixedAngle = getRootComponent() is HeadComponent;

				// TODO restore any other properties..?

				if (PhysicsReset != null)
				{
					// Let things know (eg PlayerBehaviour) that rigid body was just reinitialized
					PhysicsReset(this);
				}
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
				LayerMask.NameToLayer("UnattachedLimb");

			if (getRootComponent().gameObject.layer == LayerMask.NameToLayer("Boss"))
			{
				c.gameObject.layer = getRootComponent().gameObject.layer;
			}
		}

	}

	public void OnDestructableDestroyed(GameObject obj)
	{
		DestroyRobotComponent();
	}

	public void DetachChildren()
	{
		foreach (RobotComponent limb in getDirectChildren())
		{
			Unattach(limb.parentAttachmentPoint, limb.parentAttachmentPoint.child);
		}
	}

	public void DestroyRobotComponent()
	{
		//
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

		for (int i = 0; i < transform.childCount; ++i)
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
