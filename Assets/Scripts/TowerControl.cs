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

	public float maxAttackCD {
		get {
			return ACD / AttackCDBuffer;
		}
		set {
			ACD = value;
		}
	}

	public float maxdodgeRate = 0f;

	public float Range_Range = 2f;
	public ParticleSystem WeaponTrail;
	public Transform hitPos;
	public GameObject TowerSpriteAndAnimation;
	public GameObject BaseSprite;
	public bool Engaged = false;
	public GameObject HealthBar;
	public GameObject TowerDestrucitonEffect;
	public GameObject RangeBulletPrefab;
	public GameObject RangeCircleImage;
	public GameObject HealingEffect;
	public Text LevelText;
	public SpriteRenderer[] EnterRepoSprites;
	public Material HealMaterial;
	public float Health;

	private float ACDB = 1;
	private float ACD;
	private float AP;
	private float AM;

	private float AttackCDBuffer {
		get {
			return ACDB;
		}
		set {
			ACDB = value;
			if (TowerAnimator != null) {
				TowerAnimator.SetFloat ("AttackSpeed", 1f / maxAttackCD);
			}
		}
	}

	float AttackPowerBuffer = 1f;
	float ArmorBuffer = 1f;

	private float AttackCD;

	public float AttackPower {
		get {
			return AP * AttackPowerBuffer;
		}
		set {
			AP = value;
		}
	}

	public float Armor {
		get {
			float result = AM * ArmorBuffer;
			return Mathf.Min (1f, result);

		}
		set {
			AM = value;
		}
	}

	private Animator TowerAnimator;
	private Color thisColor;
	private GameObject RangeTarget;
	private GameObject HealTarget;
	private bool stopFunctioning = false;
	string AbilityIntroStr;
	//Level 5 Abilities
	bool thorn = false;
	float leech = 0f;
	bool bleed = false;
	bool coldBullet = false;
	bool chainHeal = false;
	bool HealBuffArmor = false;

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
				TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.MeleeTowerSprites [Mathf.Min (TowerLevel - 2, 2)];
			}
			break;
		case TowerType.Range:
			if (TowerLevel != 1) {
				TowerSpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.RangeTowerSprites [Mathf.Min (TowerLevel - 2, 2)];
				BaseSprite.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.RangeTowerBaseSprites [Mathf.Min (TowerLevel - 2, 2)];
			}
			baseIndex = 8;
			break;
		case TowerType.Heal:
			if (TowerLevel > 1) {
				for (int i = 0; i < Mathf.Min (TowerLevel, 4) - 1; i++) {
					EnterRepoSprites [i].gameObject.SetActive (true);
				}
			}
			baseIndex = 16;
			break;
		case TowerType.Missile:
			baseIndex = 24;
			break;
		}
		baseIndex += Mathf.Min ((TowerLevel - 1), 7);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		float.TryParse (Params [1], out maxAttackPower);
		float.TryParse (Params [2], out maxArmor);
		float mCD = 0f;
		float.TryParse (Params [3], out mCD);
		maxAttackCD = mCD;
		float.TryParse (Params [4], out Range_Range);
		if (TT != TowerType.Missile)
			RangeCircleImage.transform.localScale = new Vector3 (Range_Range, Range_Range, 1f);
		for (int i = 8; i < TowerLevel; i++) {
			maxHealth *= 2f;
			maxAttackPower *= 2f;
			maxArmor *= 1.01f;
			maxAttackCD /= 1.01f;
		}
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
		case TowerType.Missile:
			MissileAttack ();
			break;
		}
	}

	void Heal ()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll (transform.position, Range_Range);
		bool hasInjury = false;
		float minHealth = 30000f;
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
			if (HealTarget != null) {
				HealTarget.GetComponent<TowerControl> ().TakeDamage (-1f * AttackPower);
				Instantiate (HealingEffect, new Vector3 (HealTarget.transform.position.x, HealTarget.transform.position.y, -6f), Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
				DrawLine (transform.position, HealTarget.transform.position, new Color (175f / 255f, 249f / 255f, 161f / 255f, 1f));
				//Level 5 Abilities
				if (chainHeal)
					ChainHeal (HealTarget, 2, AttackPower);
				if (HealBuffArmor) {
					StopCoroutine ("HealbuffArmor");
					StartCoroutine ("HealbuffArmor");
				}
				TowerAnimator.SetBool ("StartHealing", true);
			}
			AttackCD = maxAttackCD;
		}
	}

	IEnumerator HealbuffArmor ()
	{
		ArmorBuffer = Mathf.Max (1.2f, ArmorBuffer);
		yield return new WaitForSeconds (5f);
		ArmorBuffer = 1f;
	}

	void ChainHeal (GameObject from, int jumpTime, float healAmount)
	{
		if (jumpTime == 0)
			return;
		Collider2D[] hits = Physics2D.OverlapCircleAll (from.transform.position, 1.5f);
		float minHealth = 30000f;
		GameObject minHealee = null;
		foreach (Collider2D hit in hits) {
			if (hit != null && hit.gameObject.tag == "Tower" && hit.gameObject != from && hit.gameObject.GetComponent<TowerControl> ().Injured () && hit.gameObject.GetComponent<TowerControl> ().isFunctioning ()) {
				float curHealth = hit.gameObject.GetComponent<TowerControl> ().getHealth ();
				if (curHealth < minHealth) {
					minHealth = curHealth;
					minHealee = hit.gameObject;
				}
			}
		}
		if (minHealee != null) {
			minHealee.GetComponent<TowerControl> ().TakeDamage (-1f * healAmount);
			Instantiate (HealingEffect, new Vector3 (minHealee.transform.position.x, minHealee.transform.position.y, -6f), Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			DrawLine (from.transform.position, minHealee.transform.position, new Color (175f / 255f, 249f / 255f, 161f / 255f, 1f));
			ChainHeal (minHealee, jumpTime - 1, 0.5f * healAmount);
		}

	}

	void DrawLine (Vector3 start, Vector3 end, Color color, float duration = 0.3f)
	{
		start = new Vector3 (start.x, start.y, -3f);
		end = new Vector3 (end.x, end.y, -3f);
		GameObject myLine = new GameObject ();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer> ();
		LineRenderer lr = myLine.GetComponent<LineRenderer> ();
		lr.material = HealMaterial;
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.06f;
		lr.endWidth = 0.06f;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		GameObject.Destroy (myLine, duration);
	}

	void MissileAttack ()
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, Vector2.up, Range_Range);
		bool hasEnemy = false;
		foreach (RaycastHit2D hit in hits) {
			if (hit.collider != null && hit.collider.gameObject.tag == "Enemy") {
				hasEnemy = true;
			}
		}
		if (hasEnemy) {
			if (Mathf.Abs (AttackCD - 0.117f * maxAttackCD) <= 0.01f) {
				TowerAnimator.SetBool ("StartAttack", true);
			}
			AttackCD -= Time.deltaTime;
		} else {
			TowerAnimator.SetBool ("StartAttack", false);
			AttackCD = maxAttackCD;
		}
		if (AttackCD <= 0f) {
			GameObject bullet = (GameObject)Instantiate (RangeBulletPrefab, hitPos.position, Quaternion.identity);
			bullet.GetComponent<RangeBulletControl> ().SetTarget (null, AttackPower, true);
			AttackCD = maxAttackCD;
		}
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
			bullet.GetComponent<RangeBulletControl> ().SetTarget (RangeTarget, AttackPower, false, bleed, coldBullet);
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
					// Level 5 Leech Ability
					TakeDamage (-leech * AttackPower);
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
		if (TI.Equals (oTI) && GameManager.GM.hasEnoughCoin (TowerInfoToMergeCoin (TI)) && TowerLevel < 4) {
			// Consume Coin to merge
			GameManager.GM.AddCoin (-TowerInfoToMergeCoin (TI));
			// Instantiate Level up effect
			Instantiate (GameManager.GM.LevelUpEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			// Level up and gain numeric powerup
			LevelUp (oTI.currentHealth);
			return true;
		} else if (TowerLevel == 4 && GameManager.GM.hasEnoughCoin (TowerInfoToMergeCoin (TI)) && TI.CanMixMergeWith (oTI)) {
			// Consume Coin to merge
			GameManager.GM.AddCoin (-TowerInfoToMergeCoin (TI));
			// Instantiate Level up effect
			Instantiate (GameManager.GM.LevelUpEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
			// Level up and gain numeric powerup
			LevelUp (oTI.currentHealth, oTI);
			return true;
		} else {
			return false;
		}
	}

	void LevelUp (float otherTHealth)
	{
		float percentHealth = Mathf.Max (otherTHealth, Health) / maxHealth;
		TowerLevel++;
		setParam ();
		Health = maxHealth * percentHealth;
		TakeDamage (0f);
		TI.level = TowerLevel;
	}

	void LevelUp (float otherTHealth, TowerInfo oTI)
	{
		LevelUp (otherTHealth);
		TI.setSubTowerType (oTI.thisTowerType);
		ActivateNewAbility ();
	}

	void ActivateNewAbility ()
	{
		if (TT == TowerType.Tank && TI.subTowerType == TowerType.Tank) {
			maxdodgeRate = 0.25f;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [0];
		}
		if (TT == TowerType.Tank && TI.subTowerType == TowerType.Range) {
			thorn = true;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [1];
		}
		if (TT == TowerType.Tank && TI.subTowerType == TowerType.Heal) {
			leech = 0.5f;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [2];
		}
		if (TT == TowerType.Range && TI.subTowerType == TowerType.Range) {
			bleed = true;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [3];
		}
		if (TT == TowerType.Range && TI.subTowerType == TowerType.Tank) {
			coldBullet = true;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [4];
		}
		if (TT == TowerType.Range && TI.subTowerType == TowerType.Heal) {
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [5];
		}
		if (TT == TowerType.Heal && TI.subTowerType == TowerType.Heal) {
			chainHeal = true;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [6];
		}
		if (TT == TowerType.Heal && TI.subTowerType == TowerType.Tank) {
			HealBuffArmor = true;
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [7];
		}
		if (TT == TowerType.Heal && TI.subTowerType == TowerType.Range) {
			AbilityIntroStr = GameManager.GM.TowerAbilityIntros.text.Split ("\n" [0]) [8];
		}
		if (TT == TowerType.Missile && TI.subTowerType == TowerType.Missile) {

		}
	}

	public void TempBuff (float APBuff, float ArmorBuff, float AttackCDBuff, float duration)
	{
		StartCoroutine (tempBuff (AttackPowerBuffer, ArmorBuffer, AttackCDBuffer, duration));
		AttackPowerBuffer = Mathf.Max (AttackPowerBuffer, APBuff);
		ArmorBuffer = Mathf.Max (ArmorBuff, ArmorBuffer);
		AttackCDBuffer = Mathf.Max (AttackCDBuff, AttackCDBuffer);
	}

	IEnumerator tempBuff (float OriginalAPBuffer, float OriginalArmorBuffer, float OriginalAttackCDBuffer, float duration)
	{
		GameObject buffEffect = Instantiate (GameManager.GM.BuffEffect, new Vector3 (transform.position.x, transform.position.y, -8f), Quaternion.Euler (new Vector3 (-90f, 0f)), transform);
		yield return new WaitForSeconds (duration);
		AttackPowerBuffer = 1f;
		ArmorBuffer = 1f;
		AttackCDBuffer = 1f;
		Destroy (buffEffect);
	}

	public void TakeDamage (float dmg, GameObject caller = null)
	{
		// if dmg < 0, then it's healing, else it's damage
		if (dmg <= 0f) {
			Health -= dmg;
			if (Health > maxHealth)
				Health = maxHealth;
			StartCoroutine (flashYellow ());
		} else {
			//Level 5 Dodge Spell
			if (dodgedAttack ())
				return;
			dmg *= (1f - Armor);
			//Level 5 thorn spell
			if (thorn && caller != null)
				Thorn (dmg, caller);
			HealthBar.GetComponent<SpriteRenderer> ().color = Color.red;
			Health -= dmg;
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

	void Thorn (float dmg, GameObject enforcer)
	{
		enforcer.SendMessage ("TakeDamage", 0.2f * dmg);
	}

	bool dodgedAttack ()
	{
		float rand = Random.Range (0f, 1f);
		if (rand < maxdodgeRate) {
			//Dodge
			GameObject popupCoin = (GameObject)Instantiate (GameManager.GM.PopTextPrefab, Camera.main.WorldToScreenPoint (transform.position), Quaternion.identity, GameObject.Find ("MainCanvas").transform);
			popupCoin.GetComponent<PopupCoin> ().setText ("Miss");
			return true;
		} else
			return false;
	}

	public string[] getTowerInfos ()
	{
		string name = TT.ToString () + " " + convertToRoman (TowerLevel);
		string health = Health.ToString (".0#");
		string Armor = (maxArmor * 100f).ToString (".0#");
		string AP = maxAttackPower.ToString (".0#");
		return new string[] { name, health, Armor, AP };
	}

	public string[] getAbilityInfos ()
	{
		string[] infos = AbilityIntroStr.Split (';');
		return infos;
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
			if (TT == TowerType.Range)
				GetComponentInChildren <Animator> ().SetBool ("StayDull", true);
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
