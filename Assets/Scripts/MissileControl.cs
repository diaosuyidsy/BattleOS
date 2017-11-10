using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileControl : MonoBehaviour
{

	public float MoveSpeed = 4f;
	public GameObject explosionEffectPrefab;

	private bool targetSet;
	private GameObject target;

	// Update is called once per frame
	void Update ()
	{
		if (targetSet) {
			if (!target.activeSelf) {
				Destroy (gameObject);
				return;
			}
			Vector3 playerPos = target.transform.position;
			Vector3 diff = playerPos - transform.position;
			diff.Normalize ();

			float rot_z = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler (0f, 0f, rot_z - 90);

			if (Vector3.Distance (transform.position, playerPos) >= 0f) {
				transform.position += transform.up * MoveSpeed * Time.deltaTime;
			}
		}
		if (targetSet)
			transform.position += transform.up * MoveSpeed * Time.deltaTime;
		if (targetSet) {
			// Explode when near target
			if (Vector2.Distance (gameObject.transform.position, target.transform.position) <= 0.3f) {
				onExplode ();
			}
		}
	}

	public void SetTarget (GameObject go, float dmg)
	{
		target = go;
		targetSet = true;
	}

	void onExplode ()
	{
		//Instantiate Explosion Effect
		Instantiate (explosionEffectPrefab, transform.position, Quaternion.identity);

		if (target.transform.childCount > 0)
			Destroy (target.transform.GetChild (0).gameObject);
		// Also check if any enemy nearby
		Collider2D[] colliders = Physics2D.OverlapCircleAll (target.transform.position, 0.8f);
		foreach (Collider2D co in colliders) {
			if (co.gameObject != null && co.tag == "Enemy")
				Destroy (co.gameObject);
		}
		LevelControl.LC.startDisable (target, 1.5f);
		LevelControl.LC.startDebris (target.transform, 1.5f);
		Destroy (gameObject);
	}
}
