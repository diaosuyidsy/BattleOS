using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeBulletControl : MonoBehaviour
{
	public float MoveSpeed = 4f;

	private bool targetSet;
	private GameObject target;
	private float attackdmg;
	bool straght = false;
	
	// Update is called once per frame
	void Update ()
	{
		if (targetSet && !straght) {
			if (target == null) {
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
		if (targetSet && straght)
			transform.position += transform.up * MoveSpeed * Time.deltaTime;
	}

	public void SetTarget (GameObject go, float dmg, bool straight)
	{
		target = go;
		attackdmg = dmg;
		straght = straight;
		targetSet = true;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.tag == "Enemy") {
			other.gameObject.SendMessage ("TakeDamage", attackdmg);
			Destroy (gameObject);
		}

	}
}
