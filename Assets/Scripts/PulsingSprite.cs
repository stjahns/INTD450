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
	private List<Color> fullColors;
	private bool enabled = false;

	void Start()
	{
		spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
		fullColors = new List<Color>();

		foreach (var renderer in spriteRenderers)
		{
			fullColors.Add(renderer.color);
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
				spriteRenderers[i].color = new Color(fullColors[i].r, fullColors[i].g, fullColors[i].b,
						Mathf.Lerp(minAlpha, fullColors[i].a, interpParam));
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
			spriteRenderers[i].color = fullColors[i];
		}
	}
}
