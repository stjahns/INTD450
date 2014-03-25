using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class PulsingSprite : MonoBehaviour
{
	public float pulseFrequency = 1f;
	public float minAlpha = 0f; 

	private SpriteRenderer spriteRenderer;
	private Color fullColor;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		fullColor = spriteRenderer.color;
	}

	void Update()
	{
		float interpParam = (Mathf.Cos(Time.time * pulseFrequency) + 1) / 2;
		spriteRenderer.color = new Color(fullColor.r, fullColor.g, fullColor.b,
				Mathf.Lerp(minAlpha, fullColor.a, interpParam));
	}
}
