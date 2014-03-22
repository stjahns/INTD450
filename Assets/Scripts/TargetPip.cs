using UnityEngine;
using System.Collections;
using System;

public class TargetPip : MonoBehaviour
{
	public float accelerationGain = 1;
	public float speedModifier = 1;
	public float maxSpeed = 1;
	public float maxForce = 1;

	public float pipAlpha = 0.5f;
	public float fadeTime = 0.5f;

	public event Action PipRemoved;

	public Transform target;

	private Renderer _renderer;

	IEnumerator Start()
	{
		_renderer = GetComponent<Renderer>();

		// fade in pip;
		float fadeTimer = 0;
		Color color = _renderer.material.GetColor("_TintColor");
		color.a = 0;
		_renderer.material.SetColor("_TintColor", color);

		while (fadeTimer < fadeTime)
		{
			color.a = Mathf.Lerp(0, pipAlpha, fadeTimer / fadeTime);
			_renderer.material.SetColor("_TintColor", color);

			fadeTimer += Time.deltaTime;
			yield return 0;
		}
	}

	void FixedUpdate()
	{
		// aim pip follows player target
		Vector3 pipToTarget = target.position - transform.position;

		// Calculate target velocity, proportional to distance, but capped at max speed
		Vector2 targetVelocity = Vector2.ClampMagnitude(speedModifier * pipToTarget, maxSpeed);
		Vector2 velocityError = targetVelocity - rigidbody2D.velocity;

		// Force proportional to gain and velocity error, capped at max force
		Vector2 force = Vector2.ClampMagnitude(accelerationGain * velocityError, maxForce);

		// Apply the force
		rigidbody2D.AddForce(force);
	}

	public void RemovePip()
	{
		StartCoroutine(RemovePipRoutine());
	}

	private IEnumerator RemovePipRoutine()
	{
		// fade out pip;
		float fadeTimer = 0;
		Color color = _renderer.material.GetColor("_TintColor");
		color.a = 0;
		_renderer.material.SetColor("_TintColor", color);

		while (fadeTimer < fadeTime)
		{
			color.a = Mathf.Lerp(pipAlpha, 0, fadeTimer / fadeTime);
			_renderer.material.SetColor("_TintColor", color);

			fadeTimer += Time.deltaTime;
			yield return 0;
		}

		if (PipRemoved != null)
		{
			PipRemoved();
		}

		// Destroy itself
		Destroy(gameObject);
	}

}
