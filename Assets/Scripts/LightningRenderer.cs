using UnityEngine;
using System.Collections;

public class LightningRenderer : MonoBehaviour
{
	public Transform start;
	public Transform end;

	public int segmentCount;
	public float spread;

	public Color color
	{
		set
		{
			lineRenderer.SetColors(value, value);
		}
	}

	public string sortingLayerName
	{
		set
		{
			if (lineRenderer)
			{
				lineRenderer.sortingLayerName = value;
			}
		}
	}

	public int sortingOrder
	{
		set
		{
			if (lineRenderer)
			{
				lineRenderer.sortingOrder = value;
			}
		}
	}

	private LineRenderer lineRenderer;

	public bool EffectEnabled
	{
		set 
		{ 
			this.enabled = value;

			if (lineRenderer)
			{
				lineRenderer.enabled = value; 
			}
		}
		get 
		{
			return this.enabled;
		}
	}

	void Awake ()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}
	
	void Update ()
	{
		if (start == null || end == null)
		{
			return;
		}

		lineRenderer.SetVertexCount(segmentCount);

		lineRenderer.SetPosition(0, start.position);

		for (int i = 1; i < segmentCount - 1; ++i)
		{
			Vector3 position = Vector3.Lerp(start.position, end.position, (float)i / (float)segmentCount);
			position.x += Random.Range(-spread, spread);
			position.y += Random.Range(-spread, spread);
			lineRenderer.SetPosition(i, position);
		}

		lineRenderer.SetPosition(segmentCount - 1, end.position);
	}
}
