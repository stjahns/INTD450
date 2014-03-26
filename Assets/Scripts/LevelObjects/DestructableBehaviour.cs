using UnityEngine;
using System.Collections;

public class DestructableBehaviour : MonoBehaviour
{
	public GameObject explosionPrefab;

	public int health = 1;

	public bool randomHealth = false;
	public int minRandomHealth = 1;
	public int maxRandomHealth = 2;

	public event System.Action<GameObject> Destroyed;

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
				 Instantiate(explosionPrefab, 
						transform.position, 
						Quaternion.identity) ;
			}

			Destroy(gameObject);
		}
	}

	[InputSocket]
	public void Destroy()
	{
		Destroy(gameObject);
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

	void OnDestroy()
	{
		if (Destroyed != null)
		{
			Destroyed(gameObject);
		}
	}
}
