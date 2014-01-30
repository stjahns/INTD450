using UnityEngine;
using System.Collections;

public class AttachmentSystem {

	public static AttachmentPoint AttachedPoint = null;
	public static AttachmentPoint UnattachedPoint = null;

	public static void UnselectPoint(AttachmentPoint point) {

		if (point == AttachedPoint)
		{
			AttachedPoint = null;
		}

		if (point == UnattachedPoint)
		{
			UnattachedPoint = null;
		}

		point.selected = false;
	}

	public static void SelectPoint(AttachmentPoint point) {

		if (point.owner.attachedToPlayer())
		{
			if (AttachedPoint)
			{
				AttachedPoint.selected = false;
			}
			AttachedPoint = point;
			AttachedPoint.selected = true;
		}
		else
		{
			if (UnattachedPoint)
			{
				UnattachedPoint.selected = false;
			}
			UnattachedPoint = point;
			UnattachedPoint.selected = true;
		}

		if (UnattachedPoint && AttachedPoint)
		{
			// If two points selected, stick em together!
			AttachedPoint.owner.Attach(AttachedPoint, UnattachedPoint);
			UnattachedPoint.selected = false;
			AttachedPoint.selected = false;
			UnattachedPoint = null;
			AttachedPoint = null;
		}
	}
}

public enum AttachmentType {
	Default,
	Arm,
	Leg
}

public class AttachmentPoint : MonoBehaviour {

	public AttachmentPoint parent = null;
	public AttachmentPoint child = null;

	public int animatorAimLayer;
	public string aimX;
	public string aimY;

	public RobotComponent owner;
	public ParticleSystem emitter;

	public bool connectsGround = false;

	public AttachmentType attachmentType = AttachmentType.Default;

	bool m_selected;
	public bool selected
	{
		set
		{
			if (value)
			{
				emitter.startColor = Color.white;
			}
			else
			{
				emitter.startColor = Color.blue;
			}
			m_selected = value;
		}
		get
		{
			return m_selected;
		}
	}

	private bool mouseOver;

	public GameObject triggerPrefab;
	private GameObject trigger;

	// Use this for initialization
	void Start () {
		selected = false;
		mouseOver = false;
		emitter.enableEmission = false;

		// Create a new, unparented GameObject for the trigger so it doesn't affect 
		// center of mass
		trigger = Instantiate(triggerPrefab, transform.position, Quaternion.identity)
			as GameObject;

		if (trigger == null)
		{
			Debug.LogError("Trigger prefab not set on attachment point.", this);
		}

		trigger.transform.parent = null;
	}
	
	void FixedUpdate () {

		// move collider
		trigger.transform.position = transform.position;

		Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 tp = new Vector2(wp.x, wp.y);

		int attachments = 1 << LayerMask.NameToLayer("Attachments");

		if (trigger.collider2D == Physics2D.OverlapPoint(tp, attachments))
		{
			if (Input.GetKeyDown(KeyCode.F))
			{
				mouseClick();
			}

			mouseOver = true;
		}
		else
		{
			mouseOver = false;
		}

		if (mouseOver && selected)
		{
			emitter.emissionRate = 200;
		}
		else
		{
			emitter.emissionRate = 50;
		}

		emitter.enableEmission = (mouseOver || selected);
	}

	void mouseClick() {
		if (parent)
		{
			parent.owner.Unattach(parent, this);
		}
		else if (child)
		{
			owner.Unattach(this, child);
		}
		else if (!selected)
		{
			AttachmentSystem.SelectPoint(this);
		}
		else
		{
			AttachmentSystem.UnselectPoint(this);
		}
	}
}
