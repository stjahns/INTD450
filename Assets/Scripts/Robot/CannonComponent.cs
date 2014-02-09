﻿using UnityEngine;
using System.Collections;

public class CannonComponent : LimbComponent 
{

	public Animator animator;

	public GameObject cannonballPrefab;
	public float shotVelocity = 10.0f;

	public Transform shotOrigin;

	public AudioClip fireClip;

	private Transform forward;

	void Start ()
	{
		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = lowerLimb.transform;
		forwardObject.transform.localPosition = new Vector3(0, -1, 0);
		forward = forwardObject.transform;
	}

	override public void FireAbility()
	{
		animator.SetTrigger("Fire");
		SFXSource.PlayOneShot(fireClip);

		// fire cannonball
		Vector3 direction = forward.position - lowerLimb.transform.position;
		direction.Normalize();

		GameObject cannonBall = Instantiate(cannonballPrefab, shotOrigin.position, Quaternion.identity) as GameObject;

		cannonBall.rigidbody2D.velocity = direction * shotVelocity;
		
	}
}
