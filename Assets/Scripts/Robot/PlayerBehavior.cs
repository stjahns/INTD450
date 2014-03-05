using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerBehavior : MonoBehaviour {

	// Public fields
	public MonoBehaviour activeController;
	
	public HeadComponent head;

	[HideInInspector]
	public FollowCamera followCamera;

	public PlayerSkeleton skeleton;

	public RobotComponent activeArm = null;
	public RobotComponent activeLeg = null;

	public List<RobotComponent> currentArms;
	public List<RobotComponent> currentLegs;

	public List<RobotComponent> allComponents = new List<RobotComponent>();

	public List<string> jumpableLayers = new List<string>();

	public AudioClip deathSound;
	public AudioClip limbAttachSound;
	public AudioClip limbRemoveSound;

	[HideInInspector]
	public static PlayerBehavior Player;

	// True if player has any limbs attatched (more than just head)
	public bool HasLimbs { get { return allComponents.Count > 1; } }

	public bool OnGround
	{
		get
		{
			if (head)
			{
				int layerMask = 0;
				foreach (string layer in jumpableLayers)
				{
					layerMask |= 1 << LayerMask.NameToLayer(layer);
				}

				return head.checkOnGround(layerMask);
			}
			return false;
		}
	}

	public bool facingLeft = true;

	// Private fields

	private Animator anim = null;
	private bool dying = false;

	// Events
	public delegate void OnDestroyHandler(PlayerBehavior behaviour);
	public event OnDestroyHandler OnDestroy;

	// Use this for initialization
	void Start ()
	{
		// Set static reference (kinda hacky :/)
		PlayerBehavior.Player = this;

		activeController.enabled = true;

		anim = GetComponentInChildren<Animator>();
		currentArms = new List<RobotComponent>();
		currentLegs = new List<RobotComponent>();
		head.LimbAdded += OnLimbAdded;
		head.LimbRemoved += OnLimbRemoved;

		// If we are just a head, roll around
		rigidbody2D.fixedAngle = HasLimbs;

		head.PhysicsReset += c => {
			// If physics is reset, head should roll if there's no attatched components
			rigidbody2D.fixedAngle = HasLimbs;
		};

		head.OnDestroy += h => {
			Debug.Log("ON DESTROY");
			if (OnDestroy != null)
			{
				OnDestroy(this);
			}
		};
	}

	public void Die()
	{
		if (!dying)
		{
			StartCoroutine(OnDeath());
			dying = true;
		}
	}

	// Restart level after a delay
	public IEnumerator OnDeath()
	{
		// Play audio clip for death
		if (deathSound)
		{
			AudioSource.PlayClipAtPoint(deathSound, transform.position);
		}

		// stop the body from moving ...
		rigidbody2D.isKinematic = true;

		head.DestroyRobotComponent();

		yield return new WaitForSeconds(1.0f);

		Application.LoadLevel(Application.loadedLevel);
	}
	
	public void OnLimbAdded(RobotComponent limb, AttachmentType type)
	{
		if (type == AttachmentType.Arm)
		{
			currentArms.Add(limb);
			if (activeArm == null)
			{
				NextArmAbility();
			}
		}

		if (type == AttachmentType.Leg)
		{
			currentLegs.Add(limb);
			if (activeLeg == null)
			{
				NextLegAbility();
			}
		}

		allComponents.Add(limb);

		transform.eulerAngles = Vector3.zero;

		AudioSource.PlayClipAtPoint(limbRemoveSound, transform.position);

		anim.SetBool("hasTorso", allComponents.Count > 0);
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentType type)
	{
		if (type == AttachmentType.Arm)
		{
			currentArms.Remove(limb);
			if (limb == activeArm)
			{
				activeArm = null;
				NextArmAbility();
			}
		}

		if (type == AttachmentType.Leg)
		{
			currentLegs.Remove(limb);
			if (limb == activeLeg)
			{
				activeLeg = null;
				NextLegAbility();
			}
		}

		allComponents.Remove(limb);

		rigidbody2D.fixedAngle = HasLimbs;

		AudioSource.PlayClipAtPoint(limbAttachSound, transform.position);

		anim.SetBool("hasTorso", allComponents.Count > 0);
	}

	void Update ()
	{
	}

	void FixedUpdate () {

		anim = GetComponentInChildren<Animator>();
		if (anim)
		{
			if (anim.GetCurrentAnimatorStateInfo(3).nameHash
					== Animator.StringToHash("Facing.FaceRight"))
			{
				if (facingLeft)
				{
					head.ResetSpriteOrders();
					facingLeft = false;
				}
			}
			else
			{
				if (!facingLeft)
				{
					head.ResetSpriteOrders();
					facingLeft = true;
				}
			}

			if (rigidbody2D)
			{
				anim.SetFloat("lateralSpeed", Mathf.Abs(rigidbody2D.velocity.x));
				anim.SetFloat("lateralVelocity", rigidbody2D.velocity.x);
			}
		}
	}

	public void NextArmAbility()
	{
		anim = GetComponentInChildren<Animator>();
		int activeIndex = 0;
		if (activeArm != null)
		{
			if (anim && activeArm.shouldAim == true)
			{
				anim.SetBool(activeArm.parentAttachmentPoint.animatorAimFlag, false);
			}
			activeIndex = currentArms.FindIndex(arm => arm == activeArm);
			activeIndex += 1;
			if (currentArms.Count > 0)
			{
				activeIndex %= currentArms.Count;
			}
		}

		if (currentArms.Count > 0)
		{
			activeArm = currentArms[activeIndex];
			if (anim) 
			{
				anim.SetBool(activeArm.parentAttachmentPoint.animatorAimFlag, true);
			}
		}
		else
		{
			activeArm = null;
		}
	}

	public void NextLegAbility()
	{
		int activeIndex = 0;
		if (activeLeg != null)
		{
			activeIndex = currentLegs.FindIndex(leg => leg == activeLeg);
			activeIndex += 1;
			if (currentLegs.Count > 0)
			{
				activeIndex %= currentLegs.Count;
			}
		}

		if (currentLegs.Count > 0)
		{
			activeLeg = currentLegs[activeIndex];
		}
		else
		{
			activeLeg = null;
		}
	}

	public void FireArmAbility()
	{
		if (activeArm)
		{
			activeArm.FireAbility();
		}
	}

	public void FireLegAbility()
	{
		if (activeLeg)
		{
			activeLeg.FireAbility();
		}
	}

	public RobotComponent GetActiveArm()
	{
		return activeArm;
	}

	public RobotComponent GetActiveLeg()
	{
		return activeLeg;
	}

	public void SetController(MonoBehaviour controller)
	{
		if (activeController)
		{
			activeController.enabled = false;
		}

		activeController = controller;
		controller.enabled = true;
	}
}
