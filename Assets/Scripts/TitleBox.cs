using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TitleBox : MonoBehaviour
{
	virtual public void Start()
	{
		foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
		{
			renderer.sortingLayerName = "UI";
			renderer.sortingOrder = 1;
		}
	}
}

