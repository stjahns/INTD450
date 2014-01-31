using UnityEngine;
using System.Collections;
using System.Linq;

public class SpringComponent : LimbComponent
{

	public Animator animator;
	public Transform springRange;
	public float springForce;

	public AudioClip fireClip;

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

			Collider2D collider = GetComponents<Collider2D>()
				.First(c => c.attachedRigidbody != null);

			collider.attachedRigidbody.AddForce(force * springForce);
		}

		SFXSource.PlayOneShot(fireClip);
	}
}
