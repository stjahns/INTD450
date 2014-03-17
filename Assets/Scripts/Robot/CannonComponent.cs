using UnityEngine;
using System.Collections;

public class CannonComponent : LimbComponent 
{

	public Animator animator;

	public GameObject cannonballPrefab;
	public float shotVelocity = 10.0f;

	public Transform shotOrigin;

	public int chargeCount = 2;
	public float chargeTime = 2.0f;

	public AudioClip fireClip;

	private Transform forward;

	private float chargeTimer;
	private int charges;

	override public void Start ()
	{
		base.Start();

		GameObject forwardObject = new GameObject("Forward");
		forwardObject.transform.parent = transform;
		forwardObject.transform.localPosition = new Vector3(0, -1, 0);
		forward = forwardObject.transform;

		chargeCount = charges;
	}

	override public void Update()
	{
		base.Update();

		if (charges < chargeCount)
		{
			chargeTimer += Time.deltaTime;
			if (chargeTimer > chargeTime)
			{
				++charges;
				chargeTimer = 0f;
			}
		}
	}

	override public void FireAbility()
	{
		if (charges > 0)
		{
			animator.SetTrigger("Fire");
			SFXSource.PlayOneShot(fireClip);

			// fire cannonball
			Vector3 direction = forward.position - transform.position;
			direction.Normalize();

			GameObject cannonBall = Instantiate(cannonballPrefab, shotOrigin.position, Quaternion.identity) as GameObject;

			cannonBall.rigidbody2D.velocity = direction * shotVelocity;

			charges -= 1;
			chargeTimer = 0f;
		}
	}
}
