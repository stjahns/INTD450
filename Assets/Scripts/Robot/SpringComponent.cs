using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpringComponent : LimbComponent
{

	public Animator animator;
	public Transform springRange;
	public float springForce;
	public float pushForce;

	public List<string> layers;

	public AudioClip fireClip;

	public bool alwaysAimPush = false;

	override public void FireAbility()
	{
		// BOING
		animator.SetTrigger("Fire");

		Vector2 forceDirection = Vector2.up;

		if (IsArm || alwaysAimPush)
		{
			// Use direction of arm
			forceDirection = transform.position - springRange.position;
			forceDirection.Normalize();
		}

		// Do a linetrace, if ground within range, push!
		int layerMask = 0;
		layers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));

		if (Physics2D.Linecast(transform.position, springRange.position, layerMask))
		{
			getRootComponent().rigidbody2D.AddForce(forceDirection * springForce);
		}

		// Push level objects...
		RaycastHit2D hit = Physics2D.Linecast(transform.position, springRange.position, layerMask);
		if (hit && hit.rigidbody)
		{
			hit.rigidbody.AddForce(-forceDirection * pushForce);
		}

		SFXSource.PlayOneShot(fireClip);
	}
}
