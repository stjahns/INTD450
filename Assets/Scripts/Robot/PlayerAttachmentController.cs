using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttachmentController : MonoBehaviour
{

	// Public fields

	public PlayerBehavior player;
	public PlayerMovementController movementController;
	public float attachmentRange;
	public float attachmentEndTime = 1.0f;
	public float selectParentViewportHeight;
	public float selectChildViewportHeight;

	public AudioClip onEnableClip;
	public AudioClip onDisableClip;
	public AudioClip jointSelectedClip;

	public MeshRenderer attachmentRangeVisual;
	public MeshRenderer attachmentShadowVisual;

	public Rect headBounds;
	public Rect headBodyBounds;
	public Rect headBodyLegBounds;

	public GameObject textPrefab;

	// Private fields

	private enum AttachmentState
	{
		SelectParent,
		SelectChild,
		AttachingPart,
		AttachingToLevelObject
	}

	private AttachmentState state;

	private List<RobotComponent> attachedParts;
	private List<AttachmentPoint> parentJoints;
	private List<AttachmentPoint> childJoints;

	private AttachmentPoint selectedParentJoint;
	private AttachmentPoint selectedChildJoint;

	private Vector3 parentStartPosition;
	private Quaternion parentStartRotation;
	
	private Vector3 parentTargetPosition;
	private Quaternion parentTargetRotation;

	private Vector3 childStartPosition;
	private Quaternion childStartRotation;

	private Vector3 childTargetPosition;
	private Quaternion childTargetRotation;

	private float attachmentTime;
	private float viewportHeightOriginal;

	private float initialDistance;

	private GUIText attachmentText;

	// --------------------------------------------------------------------------------
	// Attachment Controller
	// TODO - handle case where limbs are destroyed while being attached
	// TODO - check for objects in range on update, not on initialize
	// --------------------------------------------------------------------------------

	void Start ()
	{
		GameObject textObject = Instantiate(textPrefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity)
			as GameObject;
		attachmentText = textObject.GetComponent<GUIText>();
		attachmentText.enabled = false;
	}

	void OnEnable ()
	{
		state = AttachmentState.SelectParent;

		attachedParts = player.allComponents;

		parentJoints = new List<AttachmentPoint>();

		foreach (var part in attachedParts)
		{
			parentJoints.AddRange(part.allJoints);
		}

		// Special case: we are hooked into a terminal, as just a head
		if (parentJoints.Count == 1)
		{
			AttachmentPoint parentJoint = parentJoints[0];
			if (parentJoint && parentJoint.AttachedToLevelObject)
			{
				parentJoint.child.OnDetach();
				parentJoint.DetachFromLevelObject();
				parentJoint.childTransform = parentJoint.transform;
				player.SetController(movementController);
				return;
			}
		}

		// Get all unattached parts in range
		int attachmentLayer = 1 << LayerMask.NameToLayer("Attachments");
		var attachments = Physics2D.OverlapCircleAll(transform.position,
				attachmentRange,
				attachmentLayer);

		childJoints = new List<AttachmentPoint>();

		foreach (var collider in attachments)
		{
			AttachmentPoint joint = collider.gameObject.GetComponent<AttachmentPoint>();
			if (joint && joint.slot == AttachmentSlot.None && joint.parent == null)
			{
				childJoints.Add(joint);
			}
		}

		// Set up camera zoom
		viewportHeightOriginal = player.followCamera.viewportHeight;
		player.followCamera.viewportHeight = selectParentViewportHeight;

		AudioSource.PlayClipAtPoint(onEnableClip, transform.position);

		attachmentShadowVisual.enabled = true;

	}

	void OnDisable()
	{
		if (selectedParentJoint)
		{
			selectedParentJoint.selected = false;
			selectedParentJoint = null;
		}

		if (selectedChildJoint)
		{
			selectedChildJoint.selected = false;
			selectedChildJoint = null;
		}

		// Restore original zoom
		player.followCamera.viewportHeight = viewportHeightOriginal;

		AudioSource.PlayClipAtPoint(onDisableClip, transform.position);

		attachmentRangeVisual.enabled = false;
		attachmentShadowVisual.enabled = false;

		attachmentText.enabled = false;
	}

	void Abort()
	{
		if (selectedParentJoint)
		{
			if (selectedParentJoint.child == null)
			{
				selectedParentJoint.childTransform = selectedParentJoint.transform;
			}

			selectedParentJoint.selected = false;
			selectedParentJoint = null;
		}

		if (selectedChildJoint)
		{
			selectedChildJoint.selected = false;
			selectedChildJoint = null;
		}

		player.SetController(movementController);
	}

	// Update is called once per frame
	void Update ()
	{
		switch (state)
		{
			case AttachmentState.SelectParent:
				SelectParent();
				break;
			case AttachmentState.SelectChild:
				SelectChild();
				break;
			case AttachmentState.AttachingPart:
				AttachingPart();
				break;
			case AttachmentState.AttachingToLevelObject:
				AttachingToLevelObject();
				break;
		}
	}

	//
	// Select joint on body to attach new part to / detach existing part
	//
	void SelectParent()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Abort to regular mode
			Abort();
			return;
		}

		if (selectedParentJoint)
		{
			selectedParentJoint.selected = false;
		}

		// Select the joint closest to the mouse pointer / direction
		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float closestDistance = float.MaxValue;
		AttachmentPoint closestJoint = null;
		foreach(var parentJoint in parentJoints)
		{
			float distance = Vector2.Distance(mousePosition.XY(), parentJoint.transform.position.XY());
			if (distance < closestDistance)
			{
				closestJoint = parentJoint;
				closestDistance = distance;
			}
		}

		if (closestJoint)
		{
			attachmentText.enabled = true;
			attachmentText.color = Color.white;

			selectedParentJoint = closestJoint;
			selectedParentJoint.selected = true;

			if (selectedParentJoint.AttachedToLevelObject)
			{
				attachmentText.text = "DISCONNECT FROM ";
				attachmentText.text += selectedParentJoint.child.name;
			}
			else if (selectedParentJoint.child != null )
			{
				attachmentText.text = "DETACH ";
				attachmentText.text += selectedParentJoint.child.owner.name;
				attachmentText.text += " FROM ";
				attachmentText.text += selectedParentJoint.slot.ToString();
			}
			else
			{
				attachmentText.text = "ATTACH TO ";
				attachmentText.text += selectedParentJoint.slot.ToString();
				attachmentText.text += "...";
			}
		}
		else
		{
			attachmentText.enabled = false;
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			if (selectedParentJoint.AttachedToLevelObject)
			{
				selectedParentJoint.child.OnDetach();
				selectedParentJoint.DetachFromLevelObject();
				selectedParentJoint.childTransform = selectedParentJoint.transform;
				player.SetController(movementController);
			}
			else if (selectedParentJoint.child != null )
			{
				// If selected parent joint already has a child, detach it
				selectedParentJoint.child.OnDetach();
				selectedParentJoint.owner.Unattach(selectedParentJoint, selectedParentJoint.child);
				selectedParentJoint.childTransform = selectedParentJoint.transform;
				state = AttachmentState.AttachingPart;
				player.SetController(movementController);
			}
			else
			{
				// Select a child to attach now
				state = AttachmentState.SelectChild;
				player.followCamera.viewportHeight = selectChildViewportHeight;

				attachmentShadowVisual.enabled = false;
				attachmentRangeVisual.enabled = true;
				attachmentRangeVisual.transform.localScale = Vector3.one * (2 * attachmentRange);

				// Show the range...
				AudioSource.PlayClipAtPoint(jointSelectedClip, transform.position);
			}
		}
	}

	//
	// Select unattached child part to attach to body
	//
	void SelectChild()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Abort to regular mode
			Abort();
			return;
		}

		if (selectedChildJoint)
		{
			selectedChildJoint.selected = false;
		}

		// Select the joint closest to the mouse pointer / direction

		Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float closestDistance = float.MaxValue;

		AttachmentPoint closestJoint = null;
		foreach(var child in childJoints)
		{
			float distance = Vector2.Distance(mousePosition.XY(), child.transform.position.XY());
			if (distance < closestDistance)
			{
				closestJoint = child;
				closestDistance = distance;
			}
		}

		attachmentText.enabled = false;

		if (closestJoint)
		{
			selectedChildJoint = closestJoint;
			selectedChildJoint.selected = true;
			selectedParentJoint.childTransform = selectedChildJoint.transform;

			attachmentText.enabled = true;
			if (!CheckRoomToAttach())
			{
				attachmentText.color = Color.red;
				attachmentText.text = "NO ROOM";
			}
			else
			{
				attachmentText.color = Color.white;
				attachmentText.text = selectedChildJoint.owner.name;
			}
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			if (selectedChildJoint != null)
			{
				AudioSource.PlayClipAtPoint(jointSelectedClip, transform.position);
				StartAttachingPart();
			}
			else
			{
				// Nothing selected, abort
				Abort();
				state = AttachmentState.AttachingPart;
			}
		}
	}

	// Gets player target position at end of attachment
	Vector3 FindParentTargetPosition()
	{
		Vector3 startPosition = selectedParentJoint.owner.getRootComponent().transform.position;
		Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y, 0);

		int ground = 1 << LayerMask.NameToLayer("Ground");
		float partLength = selectedChildJoint.owner.partLength;

		Vector2[] directions = new Vector2[] {
			Vector2.up * -1,
				Vector2.right,
				Vector2.right * -1
		};

		foreach (var direction in directions)
		{
			var hit = Physics2D.Raycast(selectedParentJoint.transform.position, direction, partLength, ground);
			if (hit)
			{
				float distance = Vector2.Distance(hit.point, selectedParentJoint.transform.position);
				targetPosition -= (direction * (partLength - distance)).XY0();
			}
		}

		return targetPosition;
	}

	// Returns true if able to attach without level stuff getting in the way
	bool CheckRoomToAttach()
	{
		Rect bounds = new Rect();

		Vector3 targetPosition = FindParentTargetPosition();

		// Determine target configuration
		if (selectedParentJoint.slot == AttachmentSlot.Spine)
		{
			// head + torso
			bounds = headBodyBounds;
		}
		else if (selectedParentJoint.slot == AttachmentSlot.LeftHip
				|| selectedParentJoint.slot == AttachmentSlot.RightHip)
		{
			// head + torso + leg
			bounds = headBodyLegBounds;
		}

		//	Check if any interfering colliders within bounds offset from targetPosition
		Vector2 pointA = new Vector2(bounds.xMin + targetPosition.x,
				bounds.yMax + targetPosition.y);
		Vector2 pointB = new Vector2(bounds.xMax + targetPosition.x,
				bounds.yMin + targetPosition.y);

		// TODO configurable
		int layerMask = 1 << LayerMask.NameToLayer("Ground");
		if (Physics2D.OverlapArea(pointA, pointB, layerMask))
		{
			return false;
		}

		return true;
	}

	//
	// Determine target positions + rotations for body and attached part
	//
	void StartAttachingPart()
	{
		if (selectedChildJoint.attachmentType != AttachmentType.LevelAttachment)
		{
			parentStartRotation = selectedParentJoint.owner.getRootComponent().transform.rotation;
			parentTargetRotation = Quaternion.identity;

			parentStartPosition = selectedParentJoint.owner.getRootComponent().transform.position;
			parentTargetPosition = FindParentTargetPosition();

			if (!CheckRoomToAttach())
			{
				Abort();
				return;
			}

			childStartPosition = selectedChildJoint.owner.transform.position;
			childStartRotation = selectedChildJoint.owner.transform.rotation;

			Vector3 parentChange = parentTargetPosition - parentStartPosition;

			Bone jointBone = player.skeleton.GetBoneForSlot(selectedParentJoint.slot);
			if (jointBone.LowerJoint)
			{
				jointBone = jointBone.LowerJoint;
			}

			childTargetPosition = Quaternion.Inverse(parentStartRotation) * (jointBone.transform.position - player.transform.position) + player.transform.position + parentChange;
			childTargetRotation = Quaternion.Inverse(parentStartRotation) * jointBone.GetBoneRotation();

			state = AttachmentState.AttachingPart;

			if (selectedChildJoint.owner.rigidbody2D)
			{
				Destroy(selectedChildJoint.owner.rigidbody2D);
			}

			if (selectedParentJoint.owner.getRootComponent().rigidbody2D)
			{
				Destroy(selectedParentJoint.owner.getRootComponent().rigidbody2D);
			}

			// TODO attachment speed proportional to distance / rotation?

			attachmentTime = 0.0f;
		}
		else
		{
			// it's a 'static' level object we are attaching to, it won't move (to us, at least)
			// so just attach player to it using a hinge joint..
			if (selectedChildJoint.collider2D.attachedRigidbody)
			{
				// First, make a distance joint to pull body to attachment
				selectedParentJoint.AttachToLevelObject(selectedChildJoint);

				// TODO attachment speed proportional to distance / rotation?
				initialDistance = Vector2.Distance(selectedParentJoint.transform.position,
						selectedChildJoint.transform.position);

				attachmentTime = 0.0f;
				selectedParentJoint.SetAttachmentDistance(initialDistance);

				attachmentTime = 0.0f;
				state = AttachmentState.AttachingToLevelObject;
			}
		}
	}

	//
	// Interpolate positions / rotations until part is attached to body
	//
	void AttachingPart()
	{
		attachmentTime += Time.deltaTime;


		selectedChildJoint.owner.transform.position = 
			Vector3.Lerp(childStartPosition,
					childTargetPosition,
					attachmentTime / attachmentEndTime);

		selectedChildJoint.owner.transform.rotation = 
			Quaternion.Slerp(childStartRotation,
					childTargetRotation,
					attachmentTime / attachmentEndTime);

		selectedParentJoint.owner.getRootComponent().transform.position = 
			Vector3.Lerp(parentStartPosition,
					parentTargetPosition,
					attachmentTime / attachmentEndTime);

		selectedParentJoint.owner.getRootComponent().transform.rotation = 
			Quaternion.Lerp(parentStartRotation,
					parentTargetRotation,
					attachmentTime / attachmentEndTime);

		if (attachmentTime > attachmentEndTime)
		{
			selectedChildJoint.OnAttach();
			selectedParentJoint.owner.Attach(selectedParentJoint, selectedChildJoint);
			player.SetController(movementController);
		}
	}

	// Suck down on distance joint
	void AttachingToLevelObject()
	{
		attachmentTime += Time.deltaTime;
		float distance = Mathf.Lerp(initialDistance, 0.0f, attachmentTime / attachmentEndTime);
		selectedParentJoint.SetAttachmentDistance(distance);

		if (attachmentTime > attachmentEndTime)
		{
			selectedChildJoint.OnAttach();
			player.SetController(movementController);
		}
	}
}
