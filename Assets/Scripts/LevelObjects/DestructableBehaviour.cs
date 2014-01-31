using UnityEngine;
using System.Collections;

public class DestructableBehaviour : MonoBehaviour
{
	public GameObject explosionPrefab;
	public float explosionTime = 0.1f;

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
}
