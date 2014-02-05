using UnityEngine;
using System.Collections;

public class CannonComponent : LimbComponent 
{

	public Animator animator;

	public GameObject cannonballPrefab;
	public float shotVelocity = 10.0f;

	public Transform shotOrigin;

	public AudioClip fireClip;

	override public void FireAbility()
	{
		animator.SetTrigger("Fire");
		SFXSource.PlayOneShot(fireClip);

		// fire cannonball
		Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 direction = (target - shotOrigin.position).XY();
		direction.Normalize();

		GameObject cannonBall = Instantiate(cannonballPrefab, shotOrigin.position, Quaternion.identity) as GameObject;

		cannonBall.rigidbody2D.velocity = direction * shotVelocity;
		
	}
}
