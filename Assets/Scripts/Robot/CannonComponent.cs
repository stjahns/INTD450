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
		GameObject cannonBall = Instantiate(cannonballPrefab, shotOrigin.position, Quaternion.identity) as GameObject;
		cannonBall.rigidbody2D.velocity = transform.rotation * Vector3.down * shotVelocity;
		
	}
}
