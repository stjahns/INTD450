using UnityEngine;
using System.Collections;

public class SpringComponent : LimbComponent
{

	public Animator animator;
	public Transform springRange;
	public float springForce;

	override public void FireAbility()
	{
		// BOING
		animator.SetTrigger("Fire");

		// Do a linetrace, if ground within range, push!
		int groundOnly = 1 << LayerMask.NameToLayer("Ground");
		if (Physics2D.Linecast(transform.position, springRange.position, groundOnly))
		{
			Vector2 force = transform.position - springRange.position;
			force.Normalize();
			collider2D.attachedRigidbody.AddForce(force * springForce);
		}
	}
}
