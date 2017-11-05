using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyEnemyControl : MonoBehaviour
{
	public float maxHealth;
	[Range (0f, 1f)]
	public float maxArmor;

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

	float ArmorBuffer = 1f;
	private float AttackCD;
	public float Health;

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
	}

	public void setLevel (int level)
	{
		EnemyLevel = level;
		setParam ();
		Health = maxHealth;
		Armor = maxArmor;

	}

	void setParam ()
	{
		int baseIndex = 33;
		baseIndex += Mathf.Min ((EnemyLevel - 1), 7);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		//Plane Enemy should be 1.5 more health
		maxHealth *= 1.5f;
		float.TryParse (Params [2], out maxArmor);
		float mCD = 0f;
		float.TryParse (Params [3], out mCD);
		float ws = 0f;
		float.TryParse (Params [4], out ws);
		walkingSpeed = ws;
		int.TryParse (Params [Params.Length - 1], out Coins);
		for (int i = 8; i < EnemyLevel; i++) {
			maxHealth *= 2.11f;
			maxArmor *= 1.01f;
			Coins *= (i + 1);
		}
	}

	void Update ()
	{
		transform.Translate (Vector3.right * Time.deltaTime * walkingSpeed);
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
