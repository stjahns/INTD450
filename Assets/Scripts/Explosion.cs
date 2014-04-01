using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

	public AudioClip explosionClip;

	public float time = 0.1f;
	public bool hurtPlayer = false;
	
	public float explosionRadius = 1.0f;
	public float explosionForce = 1.0f;
	public int explosionDamage = 5;

	void Start ()
	{
		if (explosionClip)
		{
			AudioSource.PlayClipAtPoint(explosionClip, transform.position);
		}

		// get everything in explosion radius and apply force
		Collider2D[] inExplosion = Physics2D.OverlapCircleAll(
				transform.position,
				explosionRadius);

		foreach (Collider2D collider in inExplosion)
		{

			float distance = Vector2.Distance(collider.transform.position,
					transform.position);

			if (collider.rigidbody2D)
			{
				collider.rigidbody2D.AddForce(
						(collider.transform.position - transform.position).normalized
						* explosionForce
						* (1.0f - distance / explosionRadius));
			}
		}

		StartCoroutine(ApplyDamage());
	}

	IEnumerator ApplyDamage()
	{
		yield return new WaitForSeconds(time);

		// get everything in explosion radius and apply damage
		Collider2D[] inExplosion = Physics2D.OverlapCircleAll(
				transform.position,
				explosionRadius);

		foreach (Collider2D collider in inExplosion)
		{
			RobotComponent component = collider.GetComponent<RobotComponent>();
			if (component && component.attachedToPlayer() && !hurtPlayer)
			{
				continue;
			}

			GameObject obj = collider.gameObject;
			obj.SendMessage("TakeDamage",
					explosionDamage,
					SendMessageOptions.DontRequireReceiver);
		}

		Destroy(gameObject);
	}
}

