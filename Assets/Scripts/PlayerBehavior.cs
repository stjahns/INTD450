﻿using UnityEngine;
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

	public List<string> jumpableLayers = new List<string>();

	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator>();
		currentArms = new List<RobotComponent>();
		currentLegs = new List<RobotComponent>();
		head.LimbAdded += OnLimbAdded;
		head.LimbRemoved += OnLimbRemoved;
	}

	void Update () {

		int layerMask = 0;
		foreach (string layer in jumpableLayers)
		{
			layerMask |= 1 << LayerMask.NameToLayer(layer);
		}
		onGround = head.checkOnGround(layerMask);
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
			anim.SetFloat("lateralVelocity", Mathf.Abs(rigidbody2D.velocity.x));


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
