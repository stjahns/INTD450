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

	private Renderer renderer;

	override public void Start ()
	{
		base.Start();
		shieldActive = false;
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
			renderer = shieldEffect.GetComponent<Renderer>();
		}
	}

	public override void  Update()
	{
		if (shieldEffect)
		{
			shieldEffect.transform.position = shieldEffectOrigin.position;
			shieldEffect.transform.rotation = transform.rotation;
			renderer.sortingLayerID = sprites[0].sortingLayerID;
		}
	}

	void FixedUpdate()
	{
		int layerMask = 1 << LayerMask.NameToLayer("Water");
		bool inWater = Physics2D.Linecast(transform.position, groundCheck.position, layerMask);

		if (shieldActive && inWater)
		{
			// apply upward force
			PlayerBehavior.Player.rigidbody2D.AddForce(Vector3.up * floatForce);
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
