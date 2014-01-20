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

	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator>();
	}

	void Update () {
		onGround = head.checkOnGround();
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
			nextAbility();
		}

		if (onGround && Input.GetKeyDown(KeyCode.Space)) {
			// jump
			rigidbody2D.AddForce(Vector2.up * jumpForce);
		}

		if (activeArm && Input.GetMouseButtonDown(0))
		{
			activeArm.FireAbility();
		}

		anim = GetComponentInChildren<Animator>();
		if (anim) {
			anim.SetFloat("lateralVelocity", Mathf.Abs(rigidbody2D.velocity.x));


			if (activeArm && activeArm.shouldAim) {
				Vector2 aimOrigin = Camera.main.WorldToScreenPoint(activeArm.parentAttachmentPoint.transform.position);
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

	void nextAbility() {
		List<RobotComponent> armAbilities = head.getArmLimbs();

		anim = GetComponentInChildren<Animator>();
		int activeIndex = 0;
		if (activeArm != null)
		{
			if (anim && activeArm.shouldAim == true) {
				anim.SetLayerWeight(activeArm.parentAttachmentPoint.animatorAimLayer, 0);
			}
			activeIndex = armAbilities.FindIndex(arm => arm == activeArm);
			activeIndex += 1;
			activeIndex %= armAbilities.Count;
		}

		if (armAbilities.Count > 0)
		{
			activeArm = armAbilities[activeIndex];
			if (anim) {
				anim.SetLayerWeight(activeArm.parentAttachmentPoint.animatorAimLayer, 1);
			}
		}
		else
		{
			activeArm = null;
		}
	}
}
