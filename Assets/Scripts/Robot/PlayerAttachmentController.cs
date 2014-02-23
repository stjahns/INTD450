﻿using UnityEngine;
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
	public LineRenderer attachmentLineVisual;

	// Private fields

	private enum AttachmentState
	{
		SelectParent,
		SelectChild,
		AttachingPart
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

	void OnEnable ()
	{
		state = AttachmentState.SelectParent;

		attachedParts = player.allComponents;

		parentJoints = new List<AttachmentPoint>();

		foreach (var part in attachedParts)
		{
			parentJoints.AddRange(part.allJoints);
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
		viewportHeightOriginal = player.camera.viewportHeight;
		player.camera.viewportHeight = selectParentViewportHeight;

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
		player.camera.viewportHeight = viewportHeightOriginal;

		AudioSource.PlayClipAtPoint(onDisableClip, transform.position);

		attachmentRangeVisual.enabled = false;
		attachmentLineVisual.enabled = false;
		attachmentShadowVisual.enabled = false;
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
		}

		if (selectedChildJoint != null && selectedParentJoint != null)
		{
			attachmentLineVisual.enabled = true;
			attachmentLineVisual.SetPosition(0, selectedParentJoint.transform.position);
			attachmentLineVisual.SetPosition(1, selectedChildJoint.transform.position);
		}
		else
		{
			attachmentLineVisual.enabled = false;
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
			player.SetController(movementController);
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
			selectedParentJoint = closestJoint;
			selectedParentJoint.selected = true;
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			if (selectedParentJoint.child != null)
			{
				// If selected parent joint already has a child, detach it
				selectedParentJoint.owner.Unattach(selectedParentJoint, selectedParentJoint.child);
				state = AttachmentState.AttachingPart;
				player.SetController(movementController);
			}
			else
			{
				// Select a child to attach now
				state = AttachmentState.SelectChild;
				player.camera.viewportHeight = selectChildViewportHeight;

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
			player.SetController(movementController);
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

		if (closestJoint)
		{
			selectedChildJoint = closestJoint;
			selectedChildJoint.selected = true;
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
				state = AttachmentState.AttachingPart;
				player.SetController(movementController);
			}
		}
	}

	//
	// Determine target positions + rotations for body and attached part
	//
	void StartAttachingPart()
	{
		parentStartPosition = selectedParentJoint.owner.getRootComponent().transform.position;
		parentStartRotation = selectedParentJoint.owner.getRootComponent().transform.rotation;

		parentTargetPosition = new Vector3(parentStartPosition.x, parentStartPosition.y, 0);
		parentTargetRotation = Quaternion.identity;

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
				parentTargetPosition -= (direction * (partLength - distance)).XY0();
			}

			// TODO might need to abort under certain circumstances if too cramped
		}

		childStartPosition = selectedChildJoint.owner.transform.position;
		childStartRotation = selectedChildJoint.owner.transform.rotation;

		Vector3 parentChange = parentTargetPosition - parentStartPosition;

		Bone jointBone = player.skeleton.GetBoneForSlot(selectedParentJoint.slot);

		childTargetPosition = selectedParentJoint.transform.position + parentChange;
		childTargetRotation = jointBone.GetBoneRotation() * parentTargetRotation;

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
			selectedParentJoint.owner.Attach(selectedParentJoint, selectedChildJoint);
			player.SetController(movementController);
		}
	}
}