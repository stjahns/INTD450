using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCannon : CannonComponent 
{
	public LineRenderer aimLine;

	public TargetPip pip
	{
		get { return _pip; }
		set
		{

			if (value != null)
			{
				// Enable lasery liney thingy
				aimLine.enabled = true;
				aimLine.SetPosition(0, shotOrigin.position);
				aimLine.SetPosition(1, value.transform.position);

				value.PipRemoved += () => {
					// Disable lasery liney thingy
					aimLine.enabled = false;
					pip = null;
				};
			}
			else
			{
				aimLine.enabled = false;
			}

			_pip = value;
		}
	}

	private TargetPip _pip;

	override public void Start()
	{
		base.Start();
		aimLine.enabled = false;
	}

	override public void Update()
	{
		base.Update();

		if (pip != null)
		{
			aimLine.SetPosition(0, shotOrigin.position);
			aimLine.SetPosition(1, pip.transform.position);
		}
	}

	override public void FireAbility()
	{
		// fire beamy explosiony thing
		animator.SetTrigger("Fire");
		SFXSource.PlayOneShot(fireClip);

		beamRenderer.enabled = true;
		beamRenderer.SetPosition(0, shotOrigin.position);
		beamRenderer.SetPosition(1, pip.transform.position);
		beamTimer = beamTime;

		// spawn hit effect
		Instantiate(hitEffectPrefab, pip.transform.position, Quaternion.identity);

		charges -= 1;
		chargeTimer = 0f;
	}
}
