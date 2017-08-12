using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerControl : MonoBehaviour
{
	public TowerInfo TI;
	public TowerType TT;
	public int TowerLevel = 1;
	public float maxHealth;
	public float maxAttackPower;
	[Range (0f, 1f)]
	public float maxArmor;
	public float maxAttackCD = 1f;
	public float Range_Range = 2f;
	public ParticleSystem WeaponTrail;
	public Transform hitPos;
	public GameObject TowerSpriteAndAnimation;
	public bool Engaged = false;
	public GameObject HealthBar;
	public GameObject TowerDestrucitonEffect;
	public float repTime;
	public GameObject RangeBulletPrefab;
	public GameObject RangeCircleImage;
	public GameObject HealingEffect;

	private float AttackCD;
	private float Health;
	private float AttackPower;
	private float Armor;
	private Animator TowerAnimator;
	private Color thisColor;
	private GameObject RangeTarget;
	private GameObject HealTarget;

	void Start ()
	{
		init ();
	}

	void init ()
	{
		RangeCircleImage.transform.localScale = new Vector3 (Range_Range, Range_Range, 1f);
		thisColor = HealthBar.GetComponent<SpriteRenderer> ().color;
		AttackCD = maxAttackCD;
		Health = maxHealth;
		Armor = maxArmor;
		AttackPower = maxAttackPower;
		TowerAnimator = TowerSpriteAndAnimation.GetComponent<Animator> ();
		TI = new TowerInfo (TT, TowerLevel);
		repTime = TowerInfoToRepTime (TI);
	}

	void Update ()
	{
		switch (TT) {
		case TowerType.Defense:
			meleeAttacking ();
			break;
		case TowerType.Range:
			RangeAttacking ();
			break;
		case TowerType.Heal:
			Heal ();
			break;
		}

	}

	void Heal ()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll (transform.position, Range_Range);
		bool hasInjury = false;
		float minRange = 100f;
		GameObject minHealee = null;
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.gameObject.tag == "Tower" && hit.gameObject.GetComponent<TowerControl> ().Injured ()) {
				float dist = Vector3.Distance (transform.position, hit.transform.position);
				if (dist < minRange) {
					minRange = dist;
					minHealee = hit.gameObject;
				}
				hasInjury = true;
			}
		}
		if (hasInjury) {
			HealTarget = minHealee;
			Vector3 playerPos = HealTarget.transform.position;
			Vector3 diff = playerPos - transform.position;
			diff.Normalize ();

			float rot_z = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler (0f, 0f, rot_z - 90);
			AttackCD -= Time.deltaTime;
		} else {
			
		}
		if (AttackCD <= 0f) {
			HealTarget.SendMessage ("TakeDamage", -1f * AttackPower);
			Instantiate (HealingEffect, new Vector3 (HealTarget.transform.position.x, HealTarget.transform.position.y, -6f), Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			StartCoroutine (setHealing ());
			AttackCD = maxAttackCD;
		}
	}

	IEnumerator setHealing ()
	{
		TowerAnimator.SetBool ("StartHealing", true);
		yield return new WaitForSeconds (1f);
		TowerAnimator.SetBool ("StartHealing", false);
	}

	void RangeAttacking ()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll (transform.position, Range_Range);
		bool hasEnemy = false;
		float minRange = 100f;
		GameObject minEnemy = null;
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.gameObject.tag == "Enemy") {
				float dist = Vector3.Distance (transform.position, hit.transform.position);
				if (dist < minRange) {
					minRange = dist;
					minEnemy = hit.gameObject;
				}
				hasEnemy = true;
			}
		}
		if (hasEnemy) {
			Engaged = true;
			RangeTarget = minEnemy;
			Vector3 playerPos = RangeTarget.transform.position;
			Vector3 diff = playerPos - transform.position;
			diff.Normalize ();

			float rot_z = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler (0f, 0f, rot_z - 90);

			AttackCD -= Time.deltaTime;
		} else {
			TowerAnimator.SetBool ("StartAttack", false);
			Engaged = false;
		}
		if (AttackCD <= 0f) {
			TowerAnimator.SetBool ("StartAttack", true);
			GameObject bullet = (GameObject)Instantiate (RangeBulletPrefab, hitPos.position, Quaternion.identity);
			bullet.GetComponent<RangeBulletControl> ().SetTarget (RangeTarget, AttackPower);
			AttackCD = maxAttackCD;
		}
	}

	void meleeAttacking ()
	{
		Collider2D[] hits = Physics2D.OverlapCapsuleAll (hitPos.position, new Vector2 (0.7f, 0.7f), CapsuleDirection2D.Horizontal, 0f);
		bool hasEnemy = false;
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.gameObject.tag == "Enemy") {
				hasEnemy = true;
			}
		}
		if (hasEnemy) {
			Engaged = true;
			AttackCD -= Time.deltaTime;
			TowerAnimator.SetBool ("StartAttack", true);

		} else {
			Engaged = false;
			TowerAnimator.SetBool ("StartAttack", false);
		}

		GetComponent<PlayerControl> ().Engaged = Engaged;

		if (AttackCD <= 0f) {
			WeaponTrail.Play ();
			foreach (Collider2D hit in hits) {
				if (hit != null && hit.gameObject.tag == "Enemy") {
					hit.gameObject.SendMessage ("TakeDamage", AttackPower);
				}
			}
			AttackCD = maxAttackCD;
		}
	}

	public bool Injured ()
	{
		return !Mathf.Approximately (maxHealth, Health);
	}

	public bool AbsorbOtherTower (TowerInfo oTI)
	{
		if (TI.Equals (oTI)) {
			return true;
		} else {
			return false;
		}
	}

	public void TakeDamage (float dmg)
	{
		if (Health - dmg < 0f || Health - dmg > maxHealth) {
			dmg = dmg < 0f ? Health - maxHealth : Health;
		}
		float percentDmg = 0f;
		if (dmg < 0f) {
			HealthBar.GetComponent<SpriteRenderer> ().color = Color.green;
			percentDmg = dmg / maxHealth;
			Health -= dmg;
			StartCoroutine (flashYellow ());
		} else {
			percentDmg = dmg * (1f - Armor) / maxHealth;
			HealthBar.GetComponent<SpriteRenderer> ().color = Color.red;
			Health -= (dmg * (1f - Armor));
		}
			
		StartCoroutine ("flashRed");
		// Decrease HealthBar
		HealthBar.transform.localScale = new Vector3 (HealthBar.transform.localScale.x - percentDmg, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
		HealthBar.transform.localPosition = new Vector3 (HealthBar.transform.localPosition.x - 4.5f * percentDmg, HealthBar.transform.localPosition.y, HealthBar.transform.localPosition.z);
		if (Health <= 0f) {
			Instantiate (TowerDestrucitonEffect, transform.position, Quaternion.identity);
			Destroy (gameObject);
		}
	}

	public float TowerInfoToRepTime (TowerInfo ti)
	{
		float baseTime = 8f;
		switch (ti.thisTowerType) {
		case TowerType.Defense:
		case TowerType.Heal:
		case TowerType.Range:
			break;
		}
		baseTime *= ti.level;
		return baseTime;
	}

	IEnumerator flashYellow ()
	{
		GetComponent<TowerControl> ().TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.yellow;
		yield return new WaitForSeconds (0.6f);
		GetComponent<TowerControl> ().TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	IEnumerator flashRed ()
	{
		yield return new WaitForSeconds (0.3f);
		HealthBar.GetComponent<SpriteRenderer> ().color = thisColor;

	}

}
