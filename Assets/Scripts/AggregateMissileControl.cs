using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggregateMissileControl : MonoBehaviour
{

	public float MoveSpeed = 4f;
	public GameObject explosionEffectPrefab;

	private bool targetSet;
	private Vector3 targetPos;
	private float dmgP;

	// Update is called once per frame
	void Update ()
	{
		if (targetSet) {
			Vector3 playerPos = targetPos;
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
			if (Vector2.Distance (gameObject.transform.position, targetPos) <= 0.1f) {
				onExplode ();
			}
		}
	}

	public void SetTarget (Vector3 tp, float dmgPercent)
	{
		targetPos = tp;
		targetSet = true;
		dmgP = dmgPercent;
	}

	void onExplode ()
	{
		// First should instantiate explosion effect
		Instantiate (explosionEffectPrefab, transform.position, Quaternion.identity);
		// Check if there is any friendly towers nearby, if there is, split the dmg,
		// If not, dealt dmg to all towers
		Collider2D[] colliders = Physics2D.OverlapCircleAll (targetPos, 1f);
		List<GameObject> towers = new List<GameObject> ();
		foreach (Collider2D co in colliders) {
			if (co.tag == "Tower") {
				towers.Add (co.gameObject);
			}
		}
		// If TC == 0, deal dmg to all towers
		if (towers.Count == 0) {
			GameObject[] tws = GameObject.FindGameObjectsWithTag ("Tower");
			foreach (GameObject tw in tws) {
				tw.GetComponent<TowerControl> ().TakeDamage (tw.GetComponent<TowerControl> ().maxHealth);
			}
		}// If you have anything in the area, split the damage
		else {
			dmgP /= towers.Count;
			foreach (GameObject tw in towers) {
				tw.GetComponent<TowerControl> ().TakeDamage (tw.GetComponent<TowerControl> ().maxHealth * dmgP);
			}
		}
		Destroy (gameObject);
	}
}
