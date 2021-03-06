﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour {

	public AudioClip explosionClip;

	public List<AudioClip> secondarySounds;

	public float damageDelay = 0.1f;
	public bool hurtPlayer = false;
	
	public float explosionRadius = 1.0f;
	public float explosionForce = 1.0f;
	public int explosionDamage = 5;

	public List<string> excludeTags;

	void Start ()
	{
		if (explosionClip)
		{
			AudioSource3D.PlayClipAtPoint(explosionClip, transform.position);
		}

		// get everything in explosion radius and apply force
		Collider2D[] inExplosion = Physics2D.OverlapCircleAll(
				transform.position,
				explosionRadius);

		foreach (Collider2D collider in inExplosion)
		{
			if (collider.rigidbody2D &&
				   	excludeTags.Contains(collider.attachedRigidbody.tag))
			{
				continue;
			}

			float distance = Vector2.Distance(collider.transform.position,
					transform.position);

			if (collider.rigidbody2D)
			{
				collider.rigidbody2D.AddForce(
						(collider.transform.position - transform.position).normalized
						* explosionForce
						* (1.0f - distance / explosionRadius));
			}
		}

		// If this thing spawns little children, unparent them
		while (transform.GetChildCount() > 0)
		{
			transform.GetChild(0).parent = null;
		}

		StartCoroutine(ApplyDamage());
	}

	IEnumerator ApplyDamage()
	{
		yield return new WaitForSeconds(damageDelay);

		// get everything in explosion radius and apply damage
		Collider2D[] inExplosion = Physics2D.OverlapCircleAll(
				transform.position,
				explosionRadius);

		foreach (Collider2D collider in inExplosion)
		{
			if (collider.rigidbody2D &&
				   	excludeTags.Contains(collider.attachedRigidbody.tag))
			{
				continue;
			}

			RobotComponent component = collider.GetComponent<RobotComponent>();
			if (component && component.attachedToPlayer() && !hurtPlayer)
			{
				continue;
			}

			GameObject obj = collider.gameObject;
			obj.SendMessage("TakeDamage",
					explosionDamage,
					SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PlaySecondarySound(int index)
	{
		if (index < secondarySounds.Count)
		{
			AudioSource3D.PlayClipAtPoint(secondarySounds[index], transform.position);
		}
	}

	public void End()
	{
		Destroy(gameObject);
	}
}

