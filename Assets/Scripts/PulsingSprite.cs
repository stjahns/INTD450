using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class PulsingSprite : MonoBehaviour
{
	public float pulseFrequency = 1f;
	public float minAlpha = 0f; 
	public bool startEnabled = false;

	private List<SpriteRenderer> spriteRenderers;
	private List<float> fullAlphas;
	private bool enabled = false;

	void Start()
	{
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
		fullAlphas = new List<float>();

		foreach (var renderer in spriteRenderers)
		{
			fullAlphas.Add(renderer.color.a);
		}

		if (startEnabled)
		{
			Enable();
		}
	}

	void Update()
	{
		if (enabled)
		{
			float interpParam = (Mathf.Cos(Time.time * pulseFrequency) + 1) / 2;

			for (int i = 0; i < spriteRenderers.Count; ++i)
			{
				Color color = spriteRenderers[i].color;
				color.a = Mathf.Lerp(minAlpha, fullAlphas[i], interpParam);
				spriteRenderers[i].color = color;
			}
		}
	}

	public void Enable()
	{
		enabled = true;
	}

	public void Disable()
	{
		enabled = false;
		for (int i = 0; i < spriteRenderers.Count; ++i)
		{
			Color color = spriteRenderers[i].color;
			color.a = fullAlphas[i];
			spriteRenderers[i].color = color;
		}
	}
}
