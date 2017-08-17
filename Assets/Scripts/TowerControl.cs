using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public GameObject RangeBulletPrefab;
	public GameObject RangeCircleImage;
	public GameObject HealingEffect;
	public Text LevelText;
	public SpriteRenderer[] EnterRepoSprites;

	private float AttackCD;
	public float Health;
	private float AttackPower;
	private float Armor;
	private Animator TowerAnimator;
	private Color thisColor;
	private GameObject RangeTarget;
	private GameObject HealTarget;
	private bool stopFunctioning = false;


	void Start ()
	{
		TowerAnimator = TowerSpriteAndAnimation.GetComponent<Animator> ();

		setParam ();
		init ();
	}

	void init ()
	{
		thisColor = HealthBar.GetComponent<SpriteRenderer> ().color;
		AttackCD = maxAttackCD;
		Health = maxHealth;
		TI = new TowerInfo (TT, TowerLevel, Health);

	}

	void setParam ()
	{
		LevelText.text = TowerLevel.ToString ();
		int baseIndex = 0;
		switch (TT) {
		case TowerType.Tank:
			if (TowerLevel != 1) {
				TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.MeleeTowerSprites [TowerLevel - 2];
			}
			break;
		case TowerType.Range:
			if (TowerLevel != 1) {
				TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.RangeTowerSprites [TowerLevel - 2];
			}
			baseIndex = 4;
			break;
		case TowerType.Heal:
			if (TowerLevel != 1) {
				for (int i = 0; i < TowerLevel - 1; i++) {
					EnterRepoSprites [i].gameObject.SetActive (true);
				}
			}
			foreach (SpriteRenderer sr in EnterRepoSprites) {
				if (sr.gameObject.activeSelf) {
					Color c = sr.color;
					c.a = 1f;
					sr.color = c;
				}
			}
			baseIndex = 8;
			break;
		}
		baseIndex += (TowerLevel - 1);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		float.TryParse (Params [1], out maxAttackPower);
		float.TryParse (Params [2], out maxArmor);
		float.TryParse (Params [3], out maxAttackCD);
		float.TryParse (Params [4], out Range_Range);
		RangeCircleImage.transform.localScale = new Vector3 (Range_Range, Range_Range, 1f);
		Armor = maxArmor;
		AttackPower = maxAttackPower;
		TowerAnimator.SetFloat ("AttackSpeed", 1f / maxAttackCD);
	}

	void Update ()
	{
		if (stopFunctioning)
			return;
		switch (TT) {
		case TowerType.Tank:
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
		float minHealth = 1000f;
		GameObject minHealee = null;
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.gameObject.tag == "Tower" && hit.gameObject.GetComponent<TowerControl> ().Injured () && hit.gameObject.GetComponent<TowerControl> ().isFunctioning ()) {
				float curHealth = hit.gameObject.GetComponent<TowerControl> ().getHealth ();
				if (curHealth < minHealth) {
					minHealth = curHealth;
					minHealee = hit.gameObject;
				}
				hasInjury = true;
			}
		}
		if (hasInjury) {
			HealTarget = minHealee;
			AttackCD -= Time.deltaTime;
		} else {
			AttackCD = maxAttackCD;
			TowerAnimator.SetBool ("StartHealing", false);
		}
		if (AttackCD <= 0f) {
			HealTarget.SendMessage ("TakeDamage", -1f * AttackPower);
			Instantiate (HealingEffect, new Vector3 (HealTarget.transform.position.x, HealTarget.transform.position.y, -6f), Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			DrawLine (transform.position, HealTarget.transform.position, new Color (175f / 255f, 249f / 255f, 161f / 255f, 1f));
			TowerAnimator.SetBool ("StartHealing", true);
			AttackCD = maxAttackCD;
		}
	}

	void DrawLine (Vector3 start, Vector3 end, Color color, float duration = 0.2f)
	{
		GameObject myLine = new GameObject ();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer> ();
		LineRenderer lr = myLine.GetComponent<LineRenderer> ();
		lr.material = new Material (Shader.Find ("Particles/Alpha Blended Premultiply"));
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.06f;
		lr.endWidth = 0.06f;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		GameObject.Destroy (myLine, duration);
	}

	void RangeAttacking ()
	{
		bool hasEnemy = false;
		GameObject minEnemy = null;

		if (GameManager.GM.TargetedEnemy != null && Vector2.Distance (transform.position, GameManager.GM.TargetedEnemy.transform.position) <= Range_Range) {
			RangeTarget = GameManager.GM.TargetedEnemy;
			hasEnemy = true;
		} else {
			Collider2D[] hits = Physics2D.OverlapCircleAll (transform.position, Range_Range);
			float minRange = 100f;
			foreach (Collider2D hit in hits) {
				if (hit != null && hit.gameObject.tag == "Enemy") {
					float dist = Vector2.Distance (transform.position, hit.transform.position);
					if (dist < minRange) {
						minRange = dist;
						minEnemy = hit.gameObject;
					}
					hasEnemy = true;
				}
			}
		}

		if (hasEnemy) {
			Engaged = true;
			TowerAnimator.SetBool ("StayDull", true);
			// Do not switch target if current target is alive
			if (RangeTarget == null || (RangeTarget != null && Vector2.Distance (transform.position, RangeTarget.transform.position) > Range_Range))
				RangeTarget = minEnemy;
			Vector3 playerPos = RangeTarget.transform.position;
			Vector3 diff = playerPos - transform.position;
			diff.Normalize ();

			float rot_z = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			TowerSpriteAndAnimation.transform.parent.transform.rotation = Quaternion.Euler (0f, 0f, rot_z - 90);

			AttackCD -= Time.deltaTime;
		} else {
			TowerSpriteAndAnimation.transform.parent.transform.rotation = Quaternion.identity;
			TowerAnimator.SetBool ("StayDull", false);

			TowerAnimator.SetBool ("StartAttack", false);
			Engaged = false;
			AttackCD = maxAttackCD;
		}
		GetComponent<PlayerControl> ().Engaged = Engaged;
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
			if (Mathf.Abs (AttackCD - 0.4f * maxAttackCD) <= 0.01f) {
				TowerAnimator.SetBool ("StartAttack", true);
			}
		} else {
			Engaged = false;
			TowerAnimator.SetBool ("StartAttack", false);
			AttackCD = maxAttackCD;
			WeaponTrail.Stop ();
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
		return maxHealth - Health >= 0.1f;
	}

	public bool AbsorbOtherTower (TowerInfo oTI)
	{
		if (TI.Equals (oTI) && GameManager.GM.hasEnoughCoin (TowerInfoToMergeCoin (TI)) && TowerLevel <= 4) {
			// Consume Coin to merge
			GameManager.GM.AddCoin (-TowerInfoToMergeCoin (TI));
			// Instantiate Level up effect
			Instantiate (GameManager.GM.LevelUpEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			// Level up and gain numeric powerup
			LevelUp (oTI.currentHealth);
			return true;
		} else {
			return false;
		}
	}

	void LevelUp (float otherTHealth)
	{
		if (otherTHealth > Health)
			TakeDamage (-1f * (otherTHealth - Health));
		TowerLevel++;
		setParam ();
		TI.level = TowerLevel;
	}

	public void TakeDamage (float dmg)
	{
		// if dmg < 0, then it's healing, else it's damage
		if (dmg < 0f) {
			Health -= dmg;
			if (Health > maxHealth)
				Health = maxHealth;
			StartCoroutine (flashYellow ());
		} else {
			HealthBar.GetComponent<SpriteRenderer> ().color = Color.red;
			Health -= (dmg * (1f - Armor));
		}
			
		StartCoroutine ("flashRed");
		// Decrease HealthBar
		HealthBarControl (Health);
		TI.setHealth (Health);
		if (Health <= 0f) {
			Instantiate (TowerDestrucitonEffect, transform.position, Quaternion.identity);
			Destroy (gameObject);
		}
	}

	public string[] getTowerInfos ()
	{
		string name = TT.ToString () + " " + convertToRoman (TowerLevel);
		string health = Health.ToString (".0#");
		string Armor = (maxArmor * 100f).ToString (".0#");
		string AP = maxAttackPower.ToString (".0#");
		return new string[] { name, health, Armor, AP };
	}

	string convertToRoman (int num)
	{
		//Roman numerals to have <= 3 consecutive characters, the distances between deciaml values conform to this
		int[] decimalValue = { 10, 9, 5, 4, 1 };
		string[] romanNumeral = { "X", "IX", "V", "IV", "I" };
		int num_cp = num; // copy the function parameter into num_cp
		string result = "";

		for (var i = 0; i < decimalValue.Length; i++) { //itarate through array of decimal values
			//iterate more to find values not explicitly provided in the decimalValue array
			while (decimalValue [i] <= num_cp) {
				result += romanNumeral [i];
				num_cp -= decimalValue [i];
			}
		}
		return result;
	}

	void HealthBarControl (float health)
	{
		float percentHealth = health / maxHealth;
		HealthBar.transform.localScale = new Vector3 (percentHealth, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
		HealthBar.transform.localPosition = new Vector3 (4.5f * percentHealth - 4.5f, HealthBar.transform.localPosition.y, HealthBar.transform.localPosition.z);
	}

	public static int TowerInfoToMergeCoin (TowerInfo ti)
	{
		int baseMergeCoin = 7;
		for (int i = ti.level + 1; i > 0; i--) {
			baseMergeCoin *= i;
		}
		return baseMergeCoin;
	}

	public static int TowerInfoToRepCoin (TowerInfo ti)
	{
		int baseRepCoin = 10;
		baseRepCoin *= ti.level;
		return baseRepCoin;
	}

	public int thisTowerToRepCoin ()
	{
		int baseRepCoin = 10;
		baseRepCoin *= TowerLevel;
		return baseRepCoin;
	}

	public float thisTowerToRepTime ()
	{
		float baseTime = 15f;
		switch (TT) {
		case TowerType.Tank:
		case TowerType.Heal:
		case TowerType.Range:
			break;
		}
		baseTime *= TowerLevel;
		return baseTime;
	}

	public static float TowerInfoToRepTime (TowerInfo ti)
	{
		float baseTime = 8f;
		switch (ti.thisTowerType) {
		case TowerType.Tank:
		case TowerType.Heal:
		case TowerType.Range:
			break;
		}
		baseTime *= ti.level;
		return baseTime;
	}

	IEnumerator flashYellow ()
	{
		GetComponent<TowerControl> ().TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (175f / 255f, 249f / 255f, 161f / 255f);
		yield return new WaitForSeconds (0.6f);
		GetComponent<TowerControl> ().TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.white;
	}

	IEnumerator flashRed ()
	{
		yield return new WaitForSeconds (0.3f);
		HealthBar.GetComponent<SpriteRenderer> ().color = thisColor;

	}

	public void setFunctioning (bool stop)
	{
		stopFunctioning = stop;
		if (stopFunctioning) {
			GetComponentInChildren <Animator> ().SetBool ("StayDull", true);
//			GetComponentInChildren<Animator> ().stop;
			TowerSpriteAndAnimation.transform.rotation = Quaternion.identity;
		}
	}

	public bool isFunctioning ()
	{
		return !stopFunctioning;
	}

	public float getHealth ()
	{
		return Health;
	}
}
