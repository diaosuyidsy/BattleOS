using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
	public float maxHealth;
	public float maxAttackPower;
	[Range (0f, 1f)]
	public float maxArmor;
	public float maxAttackCD = 0.5f;
	public GameObject SpriteAndAnimation;
	public GameObject HitEffect;

	private float AttackCD;
	public float Health;
	private float AttackPower;
	private float Armor;
	private Color thisColor;
	private bool walking = true;
	private Animator EnemyAnimator;

	void Start ()
	{
		AttackCD = maxAttackCD;
		Health = maxHealth;
		Armor = maxArmor;
		AttackPower = maxAttackPower;
		thisColor = SpriteAndAnimation.GetComponent<SpriteRenderer> ().color;
		EnemyAnimator = SpriteAndAnimation.GetComponent<Animator> ();
	}

	void Update ()
	{
		if (walking)
			transform.Translate (Vector3.right * Time.deltaTime);
		Hit ();
	}

	void Hit ()
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, Vector2.down, 0.4f);
		bool hasBlockAhead = false;
		foreach (RaycastHit2D hit in hits) {
			if (hit.collider != null && hit.collider.gameObject.tag == "Tower") {
				hasBlockAhead = true;
			}
		}
		walking = !hasBlockAhead;
		if (hasBlockAhead) {
			AttackCD -= Time.deltaTime;
			EnemyAnimator.SetBool ("StartAttacking", true);
		} else {
			EnemyAnimator.SetBool ("StartAttacking", false);
		}
		if (AttackCD <= 0f) {
			foreach (RaycastHit2D hit in hits) {
				if (hit.collider != null && hit.collider.gameObject.tag == "Tower") {
					hit.collider.gameObject.SendMessage ("TakeDamage", AttackPower);
				}
			}
			AttackCD = maxAttackCD;
		}
	}

	public void TakeDamage (float dmg)
	{
		Health -= (dmg * (1f - Armor));
		StopCoroutine ("flashRed");
		StartCoroutine ("flashRed");
//		Instantiate (HitEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
		if (Health <= 0f) {
			Destroy (gameObject);
		}
	}

	IEnumerator flashRed ()
	{
		SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.red;
		yield return new WaitForSeconds (0.3f);
		SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = thisColor;
	}
}
