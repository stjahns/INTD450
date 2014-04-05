using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Text.RegularExpressions;

public class PlayerBehavior : MonoBehaviour, SaveableComponent
{
	// Public fields
	public MonoBehaviour activeController;

	public HeadComponent head;

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

	public CameraTarget cameraTarget;

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

	public void SaveState(JSONNode data)
	{
		data["positionX"].AsFloat = transform.position.x;
		data["positionY"].AsFloat = transform.position.y;

		AttachmentSlot[] slots = {
			AttachmentSlot.Spine,
			AttachmentSlot.LeftShoulder,
			AttachmentSlot.RightShoulder,
			AttachmentSlot.LeftHip,
			AttachmentSlot.RightHip
		};

		foreach (var slot in slots)
		{
			// save attached limbs
			RobotComponent slotComponent = allComponents.Find(c => c.Slot == slot);
			if (slotComponent)
			{
				data[slot.ToString()]["prefab"] = slotComponent.prefabName;

				// check if spawned by object spawner
				string spawnerName = Regex.Match(slotComponent.name, @"\(([^)]*)\)").Groups[1].Value;
				if (spawnerName.Length > 0)
				{
					data[slot.ToString()]["spawner"] = spawnerName;
				}
			}
		}
	}

	public void LoadState(JSONNode data)
	{
		Debug.Log(data);
		if (data["positionX"] != null  && data["positionY"] != null)
		{
			transform.position = new Vector3(data["positionX"].AsFloat, data["positionY"].AsFloat, 0);
		}

		StartCoroutine(LoadLimbs(data));
	}

	IEnumerator LoadLimbs(JSONNode data)
	{
		yield return 0;

		RobotComponent torsoComponent = null;

		string spineSlot = AttachmentSlot.Spine.ToString();
		if (data[spineSlot] != null)
		{
			GameObject prefab = Resources.Load(data[spineSlot]["prefab"]) as GameObject;

			if (prefab != null)
			{
				var torsoObject = GameObject.Instantiate(prefab, 
						transform.position, Quaternion.identity) as GameObject;
				torsoObject.name = data[spineSlot]["prefab"];
				torsoComponent = torsoObject.GetComponent<RobotComponent>();

				head.Attach(head.GetJointForSlot(AttachmentSlot.Spine),
						torsoComponent.rootJoint);

				string spawnerName = data[spineSlot]["spawner"];
				if (spawnerName != null)
				{
					torsoObject.name += string.Format("({0})", spawnerName);
					GameObject spawnerObject = GameObject.Find(spawnerName);
					if (spawnerObject)
					{
						var spawner = spawnerObject.GetComponent<ObjectSpawner>();
						spawner.spawnedObjects.Add(torsoObject);
					}
				}
			}
		}

		if (torsoComponent)
		{
			AttachmentSlot[] limbSlots = {
				AttachmentSlot.LeftShoulder,
				AttachmentSlot.RightShoulder,
				AttachmentSlot.LeftHip,
				AttachmentSlot.RightHip,
			};

			foreach (var slot in limbSlots)
			{
				if (data[slot.ToString()] != null)
				{
					GameObject prefab = Resources.Load(data[slot.ToString()]["prefab"]) as GameObject;
					if (prefab != null)
					{
						var slotObject = GameObject.Instantiate(prefab, 
								transform.position, Quaternion.identity) as GameObject;
						slotObject.name = data[slot.ToString()]["prefab"];
						var slotComponent = slotObject.GetComponent<RobotComponent>();

						torsoComponent.Attach(torsoComponent.GetJointForSlot(slot),
								slotComponent.rootJoint);

						string spawnerName = data[slot.ToString()]["spawner"];
						if (spawnerName != null)
						{
							slotObject.name += string.Format("({0})", spawnerName);
							GameObject spawnerObject = GameObject.Find(spawnerName);
							if (spawnerObject)
							{
								var spawner = spawnerObject.GetComponent<ObjectSpawner>();
								spawner.spawnedObjects.Add(slotObject);
							}
						}
					}
				}
			}
		}
	}

	[InputSocket]
	public void TakeDamage(int damage)
	{
		// TODO maybe track health or blow off a random limb or something
		Die();
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

	public void OnLimbAdded(RobotComponent limb, AttachmentSlot slot, AttachmentType type)
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

		switch (slot)
		{
			case AttachmentSlot.LeftHip:
				anim.SetBool("hasLeftLeg", true);
				break;
			case AttachmentSlot.RightHip:
				anim.SetBool("hasRightLeg", true);
				break;
			case AttachmentSlot.Spine:
				if (limb is TorsoComponent)
				{
					anim.SetBool("hasTorso", true);
				}
				else
				{
					anim.SetBool("hasLimbAsTorso", true);

				}
				break;
		}
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentSlot slot, AttachmentType type)
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

		switch (slot)
		{
			case AttachmentSlot.LeftHip:
				anim.SetBool("hasLeftLeg", false);
				break;
			case AttachmentSlot.RightHip:
				anim.SetBool("hasRightLeg", false);
				break;
			case AttachmentSlot.Spine:
				if (limb is TorsoComponent)
				{
					anim.SetBool("hasTorso", false);
				}
				else
				{
					anim.SetBool("hasLimbAsTorso", false);

				}
				break;
		}
	}

	void Update ()
	{
	}

	void FixedUpdate () {

		// Check if in water...
		if (rigidbody2D)
		{
			if (allComponents.Any(c => c.InWater))
			{
				rigidbody2D.drag = 5;
			}
			else
			{
				rigidbody2D.drag = 0;
			}
		}

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
			if (activeArm)
			{
				activeArm.isActive = false;
			}
			activeArm = currentArms[activeIndex];
			activeArm.isActive = true;
			if (anim) 
			{
				anim.SetBool(activeArm.parentAttachmentPoint.animatorAimFlag, true);
			}
		}
		else
		{
			if (activeArm)
			{
				activeArm.isActive = false;
			}
			activeArm = null;
		}
	}

	public void NextLegAbility()
	{
		List<RobotComponent> legAbilities = currentLegs.Where(l => l is LimbComponent).ToList();

		int activeIndex = 0;
		if (activeLeg != null)
		{
			activeIndex = legAbilities.FindIndex(leg => leg == activeLeg);
			activeIndex += 1;
			if (legAbilities.Count > 0)
			{
				activeIndex %= legAbilities.Count;
			}
		}

		if (legAbilities.Count > 0)
		{
			if (activeLeg)
			{
				activeLeg.isActive = false;
			}
			activeLeg = legAbilities[activeIndex];
			activeLeg.isActive = true;
		}
		else
		{
			if (activeLeg)
			{
				activeLeg.isActive = false;
			}
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
