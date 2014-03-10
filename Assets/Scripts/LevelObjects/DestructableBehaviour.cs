using UnityEngine;
using System.Collections;

public class DestructableBehaviour : MonoBehaviour
{
	public GameObject explosionPrefab;
	public float explosionTime = 0.1f;

	public int health = 1;

	public bool randomHealth = false;
	public int minRandomHealth = 1;
	public int maxRandomHealth = 2;

	public void Start()
	{
		if (randomHealth)
		{
			health = Random.Range(minRandomHealth, maxRandomHealth + 1);
		}
	}

	[InputSocket]
	public void Explode()
	{
		if (enabled)
		{
			if (explosionPrefab)
			{
				GameObject explosion = Instantiate(explosionPrefab, 
						transform.position, 
						Quaternion.identity) as GameObject;

				Destroy(explosion, explosionTime);
			}

			Destroy(gameObject);
		}
	}

	[InputSocket]
	public void TakeDamage(int damage)
	{
		health -= damage;

		if (health < 1)
		{
			Explode();
		}
	}
}
