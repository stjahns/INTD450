﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject objectPrefab;
	public Transform spawnPoint;

	public float initialSpeed;
	public Transform spawnDirection;

	public List<GameObject> spawnedObjects = new List<GameObject>();

	public int spawnLimit = 1;

	public float cooloffTime = 1.0f;

	public bool destroyExistingOnRespawn = false;

	private float timer = 0.0f;
	

	//
	// lets us set an icon...
	//
	void OnDrawGizmos()
	{
	}

	//
	// Count down on cooloff time
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
		if (timer <= 0.0f)
		{
			if (destroyExistingOnRespawn)
			{
				foreach (GameObject spawnedObject in spawnedObjects)
				{
					// TODO more general..
					spawnedObject.SendMessage("DestroyRobotComponent", SendMessageOptions.DontRequireReceiver);
				}

				spawnedObjects.Clear();
			}

			if (spawnedObjects.Count < spawnLimit || spawnLimit == 0)
			{
				var obj = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity)
					as GameObject;
				spawnedObjects.Add(obj);

				Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
				if (body && spawnDirection)
				{
					Vector2 velocity = (spawnDirection.position - transform.position).normalized
						* initialSpeed;
					body.velocity = velocity;
				}

				timer = cooloffTime;
			}

			// TODO -- would be nice to have some kind of event to listen for when the 
			// object is destroyed by something
		}
	}
}
