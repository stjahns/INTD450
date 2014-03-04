using UnityEngine;
using System.Collections;

public class ChainRenderer : MonoBehaviour
{
	public float chainWidth;
	public Material chainMaterial;

	public Transform start;
	public Transform end;

	private LineRenderer chainLine;

	public void Start()
	{
		chainLine = gameObject.AddComponent<LineRenderer>();
		chainLine.material = chainMaterial;
		chainLine.SetWidth(chainWidth, chainWidth);
		chainLine.SetPosition(0, start.position);
		chainLine.SetPosition(1, end.position);
	}

	public void Update()
	{
		chainLine.SetWidth(chainWidth, chainWidth);
		chainLine.material = chainMaterial;
		chainLine.SetPosition(0, start.position);
		chainLine.SetPosition(1, end.position);
	}
}
