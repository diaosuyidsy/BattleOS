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
	public float walkingSpeed = 1f;
	public GameObject SpriteAndAnimation;
	public GameObject HitEffect;
	public int Coins = 1;
	public GameObject PopupCoinprefab;
	public GameObject HealthBar;
	public int EnemyLevel = 1;
	public GameObject TargetedImage;

	private float AttackCD;
	public float Health;
	private float AttackPower;
	private float Armor;
	private Color thisColor;
	private bool walking = true;
	private Animator EnemyAnimator;
	private bool beingTargeted = false;

	void Start ()
	{
		thisColor = SpriteAndAnimation.GetComponent<SpriteRenderer> ().color;

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
	}

	void setParam ()
	{
		switch (EnemyLevel) {
		case 1:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];
			break;
		case 2:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [0];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
			break;
		case 3:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
			break;
		case 4:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.EnemySprite [1];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
			break;
		}
		int baseIndex = 12;
		baseIndex += (EnemyLevel - 1);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		float.TryParse (Params [1], out maxAttackPower);
		float.TryParse (Params [2], out maxArmor);
		float.TryParse (Params [3], out maxAttackCD);
		float.TryParse (Params [4], out walkingSpeed);
		int.TryParse (Params [Params.Length - 1], out Coins);
	}


	void Update ()
	{
		if (walking)
			transform.Translate (Vector3.right * Time.deltaTime * walkingSpeed);
		Hit ();
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
					hit.collider.gameObject.SendMessage ("TakeDamage", AttackPower);
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
