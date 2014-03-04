using UnityEngine;
using System.Collections;

public class ChainRenderer : MonoBehaviour
{
	public float chainWidth;
	public Material chainMaterial;

	public Transform start;
	public Transform end;

	public string sortingLayer;
	public int sortingOrder;

	public float pixelsPerUnit;

	private LineRenderer chainLine;

	private Material material;

	private bool severed = false;

	public void Start()
	{
		chainLine = gameObject.AddComponent<LineRenderer>();

		material = new Material(chainMaterial);
		chainLine.material = material;

		UpdateChain();
	}

	public void Update()
	{
		if (!severed)
		{
			UpdateChain();
		}
	}

	void UpdateChain()
	{
		float length = Vector2.Distance(start.position, end.position);
		material.mainTextureScale = new Vector2(length / (material.mainTexture.width / pixelsPerUnit), 1);

		chainLine.SetWidth(chainWidth, chainWidth);
		chainLine.SetPosition(0, start.position);
		chainLine.SetPosition(1, end.position);

		chainLine.sortingLayerName = sortingLayer;
		chainLine.sortingOrder = sortingOrder;
	}

	public void Sever()
	{
		severed = true;
	}
}
