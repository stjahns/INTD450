using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject objectPrefab;
	public Transform spawnPoint;

	public List<GameObject> spawnedObjects = new List<GameObject>();

	public int spawnLimit = 1;

	public float cooloffTime = 1.0f;

	private float timer = 0.0f;

	//
	// Draw a blue sphere at the spawn position
	//
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0, 0, 155);
		Gizmos.DrawSphere(spawnPoint.position, 0.25f);
	}

	//
	// 
	//
	void Update()
	{
		if (timer > 0.0f)
		{
			timer -= Time.deltaTime;
		}
	}

	//
	// Instantiates the prefab at the spawnPoint
	//
	[InputSocket]
	public void Spawn()
	{
		if (timer <= 0.0f && spawnedObjects.Count < spawnLimit)
		{
			var obj = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity)
				as GameObject;
			spawnedObjects.Add(obj);

			timer = cooloffTime;

			// TODO -- would be nice to have some kind of event to listen for when the 
			// object is destroyed by something
		}
	}
}
