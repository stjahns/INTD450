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
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			// Abort to regular mode
			player.SetController(movementController);
			return;
		}

		switch (state)
		{
			case AttachmentState.SelectParent:
				SelectParent();
				break;
			case AttachmentState.SelectChild:
				SelectChild();
				break;
			case AttachmentState.AttachingPart:
				break;
		}
	}

	void SelectParent()
	{
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

	void SelectChild()
	{
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
				selectedParentJoint.owner.Attach(selectedParentJoint, selectedChildJoint);
				player.SetController(movementController);
				state = AttachmentState.AttachingPart;
			}
			else
			{
				// Nothing selected, abort
				state = AttachmentState.AttachingPart;
				player.SetController(movementController);
			}
		}
	}
}
