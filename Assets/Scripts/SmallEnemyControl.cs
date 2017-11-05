using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallEnemyControl : MonoBehaviour
{
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

	public float walkingSpeed {
		get {
			return WS * WalkingSpBuffer;
		}
		set {
			WS = value;
		}
	}

	public GameObject SpriteAndAnimation;
	public GameObject HitEffect;
	public int Coins = 1;
	public GameObject PopupCoinprefab;
	public GameObject HealthBar;
	public int EnemyLevel = 1;
	public GameObject TargetedImage;
	public Text EnemyLevelText;
	public GameObject EnemyTarget;

	private float ACDB = 1f;
	private float ACD;
	private float AP;
	private float AM;
	private float WS;
	private float WSB = 1f;
	private bool hasTarget;

	private float WalkingSpBuffer {
		get {
			return WSB;
		}
		set {
			WSB = value;
			if (EnemyAnimator != null) {
				EnemyAnimator.SetFloat ("WalkingSpeed", walkingSpeed);
			}
		}
	}

	private float AttackCDBuffer {
		get {
			return ACDB;
		}
		set {
			ACDB = value;
			if (EnemyAnimator != null) {
				EnemyAnimator.SetFloat ("AttackSpeed", 1f / maxAttackCD);
			}
		}
	}

	float AttackPowerBuffer = 1f;
	float ArmorBuffer = 1f;
	private float AttackCD;
	public float Health;

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

	private Color thisColor;
	private Animator EnemyAnimator;
	private bool beingTargeted = false;

	//Level 5 Abilities
	float bleedTime;
	float bleedDmgPerTime;
	//not important
	float bleedTimePool;
	float freezeDuration;
	float freezeRate;

	void Start ()
	{
		thisColor = SpriteAndAnimation.GetComponent<SpriteRenderer> ().color;
		EnemyAnimator = SpriteAndAnimation.GetComponent<Animator> ();
	}

	public void setLevel (int level)
	{
		EnemyLevel = level;
		setParam ();
		AttackCD = maxAttackCD;
		Health = maxHealth;
		Armor = maxArmor;
		AttackPower = maxAttackPower;
		EnemyAnimator = SpriteAndAnimation.GetComponent<Animator> ();
		EnemyAnimator.SetFloat ("WalkingSpeed", walkingSpeed);
		EnemyAnimator.SetFloat ("AttackSpeed", 1f / maxAttackCD);
	}

	void setParam ()
	{
		EnemyLevelText.text = EnemyLevel.ToString ();
//		if (EnemyLevel % 4 == 1) {
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];
//
//		} else if (EnemyLevel % 4 == 2) {
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
//		} else if (EnemyLevel % 4 == 3) {
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
//		} else {
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
//			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
//		}
		for (int i = 0; i < EnemyLevel / 4; i++) {
			SpriteAndAnimation.transform.localScale += new Vector3 (0.5f, 0.5f, 0f);
		}
		int baseIndex = 33;
		baseIndex += Mathf.Min ((EnemyLevel - 1), 7);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		float.TryParse (Params [1], out maxAttackPower);
		float.TryParse (Params [2], out maxArmor);
		float mCD = 0f;
		float.TryParse (Params [3], out mCD);
		maxAttackCD = mCD;
		float ws = 0f;
		float.TryParse (Params [4], out ws);
		walkingSpeed = ws;
		int.TryParse (Params [Params.Length - 1], out Coins);
		for (int i = 8; i < EnemyLevel; i++) {
			maxHealth *= 2.11f;
			maxAttackPower *= 2f;
			maxArmor *= 1.01f;
			maxAttackCD /= 1.01f;
			Coins *= (i + 1);
		}
	}


	void Update ()
	{
		if (hasTarget)
			Hit ();
		else {
			SearchForTarget ();
		}
			
	}

	void SearchForTarget ()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll (transform.position, 0.5f);
		GameObject minEnemy = null;

		float minRange = 100f;
		if (EnemyTarget == null) {
			transform.Translate (Vector3.down * Time.deltaTime * walkingSpeed, Space.World);
			transform.rotation = Quaternion.identity;
			foreach (Collider2D hit in hits) {
				if (hit != null && hit.gameObject.tag == "Tower" && hit.gameObject.GetComponent<TowerControl> ().TT != TowerType.Tank && hit.transform.parent.tag != "ProductionSlot") {
					float dist = Vector3.Distance (transform.position, hit.transform.position);
					if (dist < minRange) {
						minRange = dist;
						minEnemy = hit.gameObject;
					}
				}
			}
			EnemyTarget = minEnemy;
		} else {
			transform.Translate (Vector3.down * Time.deltaTime * walkingSpeed);
			Vector3 playerPos = EnemyTarget.transform.position;
			Vector3 diff = playerPos - transform.position;
			diff.Normalize ();

			float rot_z = Mathf.Atan2 (diff.y, diff.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler (0f, 0f, rot_z + 90);
			if (Vector2.Distance (transform.position, EnemyTarget.transform.position) <= 0.5f) {
				hasTarget = true;
			}
		}

	}

	void Hit ()
	{
		if (EnemyTarget == null) {
			EnemyAnimator.SetBool ("StartAttacking", false);
			hasTarget = false;
			return;
		}
		AttackCD -= Time.deltaTime;
		if (Mathf.Abs (AttackCD - 0.5f * maxAttackCD) <= 0.01f)
			EnemyAnimator.SetBool ("StartAttacking", true);
		if (AttackCD <= 0f) {
			EnemyTarget.gameObject.GetComponent<TowerControl> ().TakeDamage (AttackPower, gameObject);
			AttackCD = maxAttackCD;
		}
	}

	public void TakeDamage (float dmg)
	{
		Health -= (dmg * (1f - Armor));
		HealthBarControl (Health);
		StopCoroutine ("flashRed");
		StartCoroutine ("flashRed");
		Instantiate (HitEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
		if (Health <= 0f) {
			GameObject popupCoin = (GameObject)Instantiate (PopupCoinprefab, Camera.main.WorldToScreenPoint (transform.position), Quaternion.identity, GameObject.Find ("MainCanvas").transform);
			popupCoin.GetComponent<PopupCoin> ().setText (Coins);
			GameManager.GM.AddCoin (Coins);
			Destroy (gameObject);
		}
	}

	void HealthBarControl (float health)
	{
		float percentHealth = health / maxHealth;
		HealthBar.transform.localScale = new Vector3 (percentHealth, HealthBar.transform.localScale.y, HealthBar.transform.localScale.z);
		HealthBar.transform.localPosition = new Vector3 (4.5f * percentHealth - 4.5f, HealthBar.transform.localPosition.y, HealthBar.transform.localPosition.z);
	}

	IEnumerator flashRed ()
	{
		SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = Color.red;
		HealthBar.GetComponent<SpriteRenderer> ().color = Color.red;

		yield return new WaitForSeconds (0.3f);
		SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = thisColor;
		HealthBar.GetComponent<SpriteRenderer> ().color = Color.white;

	}

	void OnMouseUp ()
	{
		targeted ();
		GameManager.GM.ForceTargetEnemy (gameObject);
	}

	public void targeted ()
	{
		beingTargeted = !beingTargeted;
		if (!beingTargeted) {
			TargetedImage.SetActive (false);
		} else
			TargetedImage.SetActive (true);
	}
}
