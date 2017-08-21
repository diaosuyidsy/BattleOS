using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
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

	private float ACDB = 1f;
	private float ACD;
	private float AP;
	private float AM;
	private float WS;
	private float WSB = 1f;

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
	private bool walking = true;
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
		if (EnemyLevel % 4 == 1) {
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];

		} else if (EnemyLevel % 4 == 2) {
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
		} else if (EnemyLevel % 4 == 3) {
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
		} else {
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
		}
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
			maxHealth *= 2f;
			maxAttackPower *= 2f;
			maxArmor *= 1.01f;
			maxAttackCD /= 1.01f;
			Coins *= 2;
		}
	}


	void Update ()
	{
		if (walking)
			transform.Translate (Vector3.right * Time.deltaTime * walkingSpeed);
		Hit ();
		if (bleedTime > 0f)
			bleeding ();
	}

	public void startBleed (float bleedt, float bleedD)
	{
		bleedTime = bleedt;
		bleedDmgPerTime = Mathf.Max (bleedD / (bleedTime / 2f), bleedDmgPerTime);
	}

	void bleeding ()
	{
		bleedTime -= Time.deltaTime;
		bleedTimePool += Time.deltaTime;
		if (bleedTimePool >= 2f) {
			bleedTimePool = 0f;
			TakeDamage (bleedDmgPerTime);
		}
	}

	public void startCold (float duration, float rate = 0.8f)
	{
		StopCoroutine ("cold");
		StartCoroutine ("cold", rate);
	}

	IEnumerator cold (float rate = 0.8f)
	{
		AttackCDBuffer = rate;
		yield return new WaitForSeconds (3f);
		Debug.Log ("Back");
		AttackCDBuffer = 1f;
	}

	public void AddWalkingSpeed (float addedspeed)
	{
		walkingSpeed += addedspeed;
		EnemyAnimator.SetFloat ("WalkingSpeed", walkingSpeed);
	}

	void Hit ()
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll (transform.position, Vector2.down, 0.4f);
		bool hasBlockAhead = false;
		foreach (RaycastHit2D hit in hits) {
			if (hit.collider != null && hit.collider.gameObject.tag == "Tower" && hit.transform.parent.tag != "ProductionSlot") {
				hasBlockAhead = true;
			}
		}
		walking = !hasBlockAhead;
		if (hasBlockAhead) {
			AttackCD -= Time.deltaTime;
			if (Mathf.Abs (AttackCD - 0.5f * maxAttackCD) <= 0.01f)
				EnemyAnimator.SetBool ("StartAttacking", true);
		} else {
			EnemyAnimator.SetBool ("StartAttacking", false);
		}
		if (AttackCD <= 0f) {
			foreach (RaycastHit2D hit in hits) {
				if (hit.collider != null && hit.collider.gameObject.tag == "Tower") {
					hit.collider.gameObject.GetComponent<TowerControl> ().TakeDamage (AttackPower, gameObject);
				}
			}
			AttackCD = maxAttackCD;
		}
	}

	public void TakeDamage (float dmg)
	{
		Health -= (dmg * (1f - Armor));
		HealthBarControl (Health);
		StopCoroutine ("flashRed");
		StartCoroutine ("flashRed");
//		Instantiate (HitEffect, transform.position, Quaternion.Euler (new Vector3 (-90f, 0f, 0f)));
		if (Health <= 0f) {
			GameObject popupCoin = (GameObject)Instantiate (PopupCoinprefab, Camera.main.WorldToScreenPoint (transform.position), Quaternion.identity, GameObject.Find ("MainCanvas").transform);
			popupCoin.GetComponent<PopupCoin> ().setText (Coins);
			GameManager.GM.AddCoin (Coins);
			GameManager.GM.onScore (EnemyLevel);
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
