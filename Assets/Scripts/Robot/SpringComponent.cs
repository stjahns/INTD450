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

	override public void FireAbility()
	{
		// BOING
		animator.SetTrigger("Fire");

		// Do a linetrace, if ground within range, push!
		int layerMask = 0;
		layers.ForEach(l => layerMask |= 1 << LayerMask.NameToLayer(l));

		if (Physics2D.Linecast(transform.position, springRange.position, layerMask))
		{
			Vector2 force = transform.position - springRange.position;
			force.Normalize();

			PlayerBehavior.Player.rigidbody2D.AddForce(force * springForce);
		}

		// Push level objects...
		RaycastHit2D hit = Physics2D.Linecast(transform.position, springRange.position, layerMask);
		if (hit && hit.rigidbody)
		{
			Vector2 force = springRange.position - transform.position;
			force.Normalize();
			hit.rigidbody.AddForce(force * pushForce);
		}

		SFXSource.PlayOneShot(fireClip);
	}
}
