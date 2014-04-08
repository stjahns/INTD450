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

	public List<string> blockingLayers;

	public CameraTarget selectParentCamera;
	public CameraTarget selectChildCamera;

	public AudioClip onEnableClip;
	public AudioClip onDisableClip;
	public AudioClip jointSelectedClip;

	public AudioClip jointHoverClip;

	public MeshRenderer attachmentRangeVisual;
	public MeshRenderer attachmentShadowVisual;

	public GameObject textPrefab;

	public Rect headBounds;
	public Rect headBodyBounds;
	public Rect headBodyLegBounds;


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

	private AttachmentPoint selectedParentJoint = null;
	private AttachmentPoint selectedChildJoint = null;

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

	void Start ()
	{
		GameObject textObject = Instantiate(textPrefab, new Vector3(0.5f, 0.5f, 0), Quaternion.identity)
			as GameObject;
		attachmentText = textObject.GetComponent<GUIText>();
		attachmentText.enabled = false;

		attachmentRangeVisual.sortingLayerName = "UI";
		attachmentShadowVisual.sortingLayerName = "UI";
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
				enabled = false;
				return;
			}
		}

		// Transition camera to zoom on player
		selectParentCamera.AcquireCamera();

		AudioSource.PlayClipAtPoint(onEnableClip, transform.position);

		attachmentShadowVisual.enabled = true;
		movementController.MouseAim = false;

	}

	void GetUnattachedChildren()
	{
		// Get all unattached parts in range
		int attachmentLayer = 1 << LayerMask.NameToLayer("Attachments");
		var attachments = Physics2D.OverlapCircleAll(transform.position,
				attachmentRange,
				attachmentLayer);

		childJoints = new List<AttachmentPoint>();

		int layerMask = 0;
		blockingLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));

		foreach (var collider in attachments)
		{
			// Check for line-of-sight between parent joint and attachment point
			var hit = Physics2D.Linecast(selectedParentJoint.transform.position, collider.transform.position, layerMask);
			if (hit)
			{
				continue;
			}
			
			bool isHead = parentJoints.Count == 1;

			AttachmentPoint joint = collider.gameObject.GetComponent<AttachmentPoint>();
			if (joint && joint.slot == AttachmentSlot.None && joint.parent == null)
			{
				if (!isHead && joint.attachmentType == AttachmentType.LevelAttachment)
				{
					// Can only attach to sockets, etc. when just a head
					continue;
				}

				childJoints.Add(joint);
			}
		}
	}

	void OnDisable()
	{
		SetSelectedParent(null);
		SetSelectedChild(null);

		// Restore camera
		selectChildCamera.ReleaseCamera();
		selectParentCamera.ReleaseCamera();

		AudioSource.PlayClipAtPoint(onDisableClip, transform.position);

		attachmentRangeVisual.enabled = false;
		attachmentShadowVisual.enabled = false;

		attachmentText.enabled = false;

		movementController.enabled = true;
		movementController.MouseAim = true;
	}

	void Abort()
	{
		if (selectedParentJoint)
		{
			selectedParentJoint.childTransform = selectedParentJoint.transform;
			selectedParentJoint.owner.getRootComponent().ResetPhysics();
		}

		SetSelectedParent(null);
		SetSelectedChild(null);

		enabled = false;
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

			SetSelectedParent(closestJoint);

			if (selectedParentJoint.AttachedToLevelObject)
			{
				attachmentText.text = "DISCONNECT FROM ";
				attachmentText.text += selectedParentJoint.child.AttachmentName;
				attachmentText.color = Color.red;
			}
			else if (selectedParentJoint.child != null )
			{
				attachmentText.text = "DETACH ";
				attachmentText.text += selectedParentJoint.child.AttachmentName;
				attachmentText.text += " FROM ";
				attachmentText.text += selectedParentJoint.AttachmentName;
				attachmentText.color = Color.red;
			}
			else
			{
				attachmentText.text = "ATTACH TO ";
				attachmentText.text += selectedParentJoint.AttachmentName;
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
				enabled = false;
			}
			else if (selectedParentJoint.child != null )
			{
				// If selected parent joint already has a child, detach it
				selectedParentJoint.child.OnDetach();
				selectedParentJoint.owner.Unattach(selectedParentJoint, selectedParentJoint.child);
				selectedParentJoint.childTransform = selectedParentJoint.transform;
				state = AttachmentState.AttachingPart;
				enabled = false;
			}
			else
			{
				// Select a child to attach now
				state = AttachmentState.SelectChild;

				// Transition camera ...
				selectChildCamera.AcquireCamera();

				attachmentShadowVisual.enabled = false;
				attachmentRangeVisual.enabled = true;
				attachmentRangeVisual.transform.localScale = Vector3.one * (2 * attachmentRange + 0.5f);

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
		GetUnattachedChildren();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Abort to regular mode
			Abort();
			return;
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
			SetSelectedChild(closestJoint);
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
				attachmentText.text = selectedChildJoint.AttachmentName;
			}
		}
		else
		{
			// remove lightning bolt thing
			selectedParentJoint.childTransform = selectedParentJoint.transform;
			SetSelectedChild(null);
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

		int layerMask = 0;
		blockingLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
		
		if (selectedChildJoint.owner != null)
		{
			float partLength = selectedChildJoint.owner.partLength;

			Vector2[] directions = new Vector2[] {
				Vector2.up * -1,
					Vector2.right,
					Vector2.right * -1
			};

			foreach (var direction in directions)
			{
				var hit = Physics2D.Raycast(selectedParentJoint.transform.position, direction, partLength, layerMask);
				if (hit)
				{
					float distance = Vector2.Distance(hit.point, selectedParentJoint.transform.position);
					targetPosition -= (direction * (partLength - distance)).XY0();
				}
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
		if (selectedChildJoint.attachmentType == AttachmentType.LevelAttachment)
		{
			// don't worry about bounds?
		}
		else if (selectedParentJoint.slot == AttachmentSlot.Spine)
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

		int layerMask = 0;
		blockingLayers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));
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

			// Disable movement during actual attachment...
			movementController.enabled = false;

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

		float parameter = Mathfx.Hermite(0, 1, attachmentTime / attachmentEndTime);

		selectedChildJoint.owner.transform.position = 
			Vector3.Lerp(childStartPosition,
					childTargetPosition,
					parameter);

		selectedChildJoint.owner.transform.rotation = 
			Quaternion.Slerp(childStartRotation,
					childTargetRotation,
					parameter);

		selectedParentJoint.owner.getRootComponent().transform.position = 
			Vector3.Lerp(parentStartPosition,
					parentTargetPosition,
					parameter);

		selectedParentJoint.owner.getRootComponent().transform.rotation = 
			Quaternion.Lerp(parentStartRotation,
					parentTargetRotation,
					parameter);

		if (attachmentTime > attachmentEndTime)
		{
			selectedChildJoint.OnAttach();
			selectedParentJoint.owner.Attach(selectedParentJoint, selectedChildJoint);
			enabled = false;
		}
	}

	// Suck down on distance joint
	void AttachingToLevelObject()
	{
		attachmentTime += Time.deltaTime;

		float parameter = Mathfx.Hermite(0, 1, attachmentTime / attachmentEndTime);
		float distance = Mathf.Lerp(initialDistance, 0.0f, parameter);
		selectedParentJoint.SetAttachmentDistance(distance);

		if (attachmentTime > attachmentEndTime)
		{
			selectedChildJoint.OnAttach();
			enabled = false;
		}
	}

	void SetSelectedParent(AttachmentPoint point)
	{
		if (selectedParentJoint != point)
		{
			if (selectedParentJoint != null)
			{
				selectedParentJoint.selected = false;

				if (selectedParentJoint.owner != null)
				{
					selectedParentJoint.owner.OnDestroy -= OnParentDestroyed;
				}
			}

			if (point != null)
			{
				point.selected = true;
				if (point.owner != null)
				{
					point.owner.OnDestroy += OnParentDestroyed;
				}
				AudioSource.PlayClipAtPoint(jointHoverClip, transform.position);
			}
		}

		selectedParentJoint = point;
	}

	void OnParentDestroyed(RobotComponent component)
	{
		SetSelectedParent(null);

		if (state == AttachmentState.SelectChild 
			|| state == AttachmentState.AttachingPart 
			|| state == AttachmentState.AttachingToLevelObject)
		{
			Abort();
		}
	}

	void SetSelectedChild(AttachmentPoint point)
	{
		if (selectedChildJoint != point)
		{
			if (selectedChildJoint != null)
			{
				selectedChildJoint.selected = false;

				if (selectedChildJoint.owner != null)
				{
					selectedChildJoint.owner.OnDestroy -= OnChildDestroyed;
					selectedChildJoint.owner.GetComponent<PulsingSprite>().Disable();
				}
			}

			if (point != null)
			{
				point.selected = true;
				if (point.owner != null)
				{
					point.owner.OnDestroy += OnChildDestroyed;
					point.owner.GetComponent<PulsingSprite>().Enable();
				}
				AudioSource.PlayClipAtPoint(jointHoverClip, transform.position);
			}
		}

		selectedChildJoint = point;
	}

	void OnChildDestroyed(RobotComponent component)
	{
		SetSelectedChild(null);

		if (state == AttachmentState.AttachingPart 
			|| state == AttachmentState.AttachingToLevelObject)
		{
			Abort();
		}
	}
}
