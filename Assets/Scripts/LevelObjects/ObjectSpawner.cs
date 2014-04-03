using UnityEngine;
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

	public bool playSpawnClip = false;
	[Range(0,1)]
	public float spawnVolume = 1;
	public AudioClip spawnClip;

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
			if (destroyExistingOnRespawn && spawnedObjects.Count == spawnLimit)
			{
				// Destroy oldest object
				var destructable = spawnedObjects[0].GetComponent<DestructableBehaviour>();
				if (destructable)
				{
					spawnedObjects.RemoveAt(0);
					destructable.Destroy();
				}
				else
				{
					spawnedObjects.RemoveAt(0);
					Destroy(spawnedObjects[0]);
				}
			}

			if (spawnedObjects.Count < spawnLimit || spawnLimit == 0)
			{
				if (playSpawnClip)
				{
					AudioSource3D.PlayClipAtPoint(spawnClip, transform.position, spawnVolume);
				}

				var obj = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity)
					as GameObject;
				spawnedObjects.Add(obj);

				// If it has a destructable behavior, listen for Destroyed event
				DestructableBehaviour destructable = obj.GetComponent<DestructableBehaviour>();
				if (destructable)
				{
					destructable.Destroyed += ObjectDestroyed;
				}

                obj.gameObject.name = obj.gameObject.name.Split('(')[0]+"("+gameObject.name+")";


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

	void ObjectDestroyed(GameObject destroyedObject)
	{
		spawnedObjects.Remove(destroyedObject);
	}
}
