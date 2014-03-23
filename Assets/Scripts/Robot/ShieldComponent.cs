using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShieldComponent : LimbComponent 
{
	public AudioClip enableClip;
	public AudioClip disableClip;

	public float floatForce;

	public GameObject shieldEffectPrefab;
	public Transform shieldEffectOrigin;

	private GameObject shieldEffect;
	private bool shieldActive;
	private bool inWater;

	override public void Start ()
	{
		base.Start();
		shieldActive = false;
		inWater = false;
	}

	override public void FireAbility()
	{
		if (shieldActive)
		{
			shieldActive = false;
			Destroy(shieldEffect);
			shieldEffect = null;
			SFXSource.PlayOneShot(disableClip);
		}
		else
		{
			shieldActive = true;
			shieldEffect = Instantiate(shieldEffectPrefab, shieldEffectOrigin.position, transform.rotation)
				as GameObject;
			shieldEffect.transform.parent = transform;
			SFXSource.PlayOneShot(enableClip);
		}
	}

	public override void  Update()
	{
		if (shieldEffect)
		{
			shieldEffect.transform.position = shieldEffectOrigin.position;
			shieldEffect.transform.rotation = transform.rotation;
		}
	}

	void FixedUpdate()
	{
		if (shieldActive && inWater)
		{
			// apply upward force
			PlayerBehavior.Player.rigidbody2D.AddForce(Vector3.up * floatForce);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			inWater = true;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			inWater = false;
		}
	}

	override public void OnRemove()
	{
		if (shieldActive)
		{
			shieldActive = false;
			Destroy(shieldEffect);
			shieldEffect = null;
			SFXSource.PlayOneShot(disableClip);
		}
	}
}
