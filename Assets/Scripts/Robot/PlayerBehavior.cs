using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerBehavior : MonoBehaviour {

	public float moveForce = 1.0f;
	public float jumpForce = 1.0f;

	public bool onGround = false;

	private Animator anim = null;

	public HeadComponent head;

	public RobotComponent activeArm = null;
	public RobotComponent activeLeg = null;

	public List<RobotComponent> currentArms;
	public List<RobotComponent> currentLegs;

	public List<RobotComponent> allComponents = new List<RobotComponent>();

	public List<string> jumpableLayers = new List<string>();

	public AudioSource soundSource;
	public AudioClip deathSound;
	public AudioClip jumpSound;
	public AudioClip limbAttachSound;
	public AudioClip limbRemoveSound;

	private bool dying = false;

	public float hopForce = 10;

	[HideInInspector]
	public static PlayerBehavior Player;

	// True if player has any limbs attatched
	public bool HasLimbs { get { return allComponents.Count > 0; } }

	public delegate void OnDestroyHandler(PlayerBehavior behaviour);
	public event OnDestroyHandler OnDestroy;

	// Use this for initialization
	void Start () {

		// Set static reference (kinda hacky :/)
		PlayerBehavior.Player = this;

		anim = GetComponentInChildren<Animator>();
		currentArms = new List<RobotComponent>();
		currentLegs = new List<RobotComponent>();
		head.LimbAdded += OnLimbAdded;
		head.LimbRemoved += OnLimbRemoved;

		// If we are just a head, roll around
		rigidbody2D.fixedAngle = allComponents.Count != 0;

		head.PhysicsReset += c => {
			// If physics is reset, head should roll if there's no attatched components
			if (c == head && allComponents.Count == 0)
			{
				rigidbody2D.fixedAngle = false;
			}
			else
			{
				rigidbody2D.fixedAngle = true;
			}
		};

		head.OnDestroy += h => {
			Debug.Log("ON DESTROY");
			if (OnDestroy != null)
			{
				OnDestroy(this);
			}
		};
	}

	void Update () {

		int layerMask = 0;
		foreach (string layer in jumpableLayers)
		{
			layerMask |= 1 << LayerMask.NameToLayer(layer);
		}
		onGround = head.checkOnGround(layerMask);
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
		if (soundSource && deathSound)
		{
			soundSource.PlayOneShot(deathSound);
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
				nextArmAbility();
			}
		}

		if (type == AttachmentType.Leg)
		{
			currentLegs.Add(limb);
			if (activeLeg == null)
			{
				nextLegAbility();
			}
		}

		allComponents.Add(limb);

		transform.eulerAngles = Vector3.zero;

		soundSource.PlayOneShot(limbRemoveSound);
	}

	public void OnLimbRemoved(RobotComponent limb, AttachmentType type)
	{
		if (type == AttachmentType.Arm)
		{
			currentArms.Remove(limb);
			if (limb == activeArm)
			{
				activeArm = null;
				nextArmAbility();
			}
		}

		if (type == AttachmentType.Leg)
		{
			currentLegs.Remove(limb);
			if (limb == activeLeg)
			{
				activeLeg = null;
				nextLegAbility();
			}
		}

		allComponents.Remove(limb);
		if (allComponents.Count == 0)
		{
			rigidbody2D.fixedAngle = false;
		}

		soundSource.PlayOneShot(limbAttachSound);
	}

	// Update is called once per frame
	void FixedUpdate () {

		if (Input.GetKey(KeyCode.A)) {
			// move left
			rigidbody2D.AddForce(Vector2.right * -moveForce);
		}

		if (Input.GetKey(KeyCode.D)) {
			// move right
			rigidbody2D.AddForce(Vector2.right * moveForce);
		}

		if (Input.GetKeyDown(KeyCode.Q)) {
			nextArmAbility();
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			nextLegAbility();
		}

		if (onGround && Input.GetKeyDown(KeyCode.Space)) {
			// jump
			soundSource.PlayOneShot(jumpSound);
			rigidbody2D.AddForce(Vector2.up * jumpForce);
		}

		if (activeArm && Input.GetMouseButtonDown(0))
		{
			activeArm.FireAbility();
		}

		if (activeLeg && Input.GetMouseButtonDown(1))
		{
			activeLeg.FireAbility();
		}
		

		anim = GetComponentInChildren<Animator>();
		if (anim) {

			if (rigidbody2D)
			{
				anim.SetFloat("lateralVelocity", Mathf.Abs(rigidbody2D.velocity.x));
			}


			if (activeArm && activeArm.shouldAim) {
				Vector2 jointOrigin = activeArm.parentAttachmentPoint.transform.position;
				Vector2 aimOrigin = Camera.main.WorldToScreenPoint(jointOrigin);
				Vector2 playerToPointer;

				playerToPointer.x = Input.mousePosition.x - aimOrigin.x;
		    	playerToPointer.y = Input.mousePosition.y - aimOrigin.y;
				playerToPointer.Normalize();

				string xVar = activeArm.parentAttachmentPoint.aimX;
				string yVar = activeArm.parentAttachmentPoint.aimY;

				anim.SetFloat(xVar, playerToPointer.x);
				anim.SetFloat(yVar, playerToPointer.y);
			}
		}

	}

	void nextArmAbility() {
		anim = GetComponentInChildren<Animator>();
		int activeIndex = 0;
		if (activeArm != null)
		{
			if (anim && activeArm.shouldAim == true) {
				anim.SetLayerWeight(activeArm.parentAttachmentPoint.animatorAimLayer, 0);
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
			if (anim) {
				anim.SetLayerWeight(activeArm.parentAttachmentPoint.animatorAimLayer, 1);
			}
		}
		else
		{
			activeArm = null;
		}
	}

	void nextLegAbility() {
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
}
