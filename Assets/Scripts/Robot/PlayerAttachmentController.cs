using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttachmentController : MonoBehaviour
{

	public PlayerBehavior player;
	public PlayerMovementController movementController;
	public float attachmentRange;

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
		// For parent component (robot), determine space necessary to attach child part

		// Set parent target position
		// Set parent target rotation if necessary
		parentStartPosition = selectedParentJoint.owner.getRootComponent().transform.position;
		parentStartRotation = selectedParentJoint.owner.getRootComponent().transform.rotation;

		// Figure out size of new child part
		// Given current parent joint transform, is there room for part? otherwise move 
		// TODO might need to abort under certain circumstances (too cramped)

		parentTargetPosition = new Vector3(parentStartPosition.x, parentStartPosition.y + 1, 0);
		parentTargetRotation = Quaternion.identity;

		childStartPosition = selectedChildJoint.owner.transform.position;
		childStartRotation = selectedChildJoint.owner.transform.rotation;

		childTargetPosition = selectedParentJoint.transform.position;
		childTargetPosition = new Vector3(childTargetPosition.x, childTargetPosition.y + 1, 0);
		childTargetRotation = Quaternion.identity;

		state = AttachmentState.AttachingPart;

		if (selectedChildJoint.owner.rigidbody2D)
		{
			Destroy(selectedChildJoint.owner.rigidbody2D);
		}

		if (selectedParentJoint.owner.getRootComponent().rigidbody2D)
		{
			Destroy(selectedParentJoint.owner.getRootComponent().rigidbody2D);
		}

		// TODO attachment speed proportional to distance?

		attachmentTime = 0.0f;
	}

	private float attachmentTime;
	public float attachmentEndTime = 1.0f;

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
