using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemyControl : MonoBehaviour
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
	private Animator EnemyAnimator;
	private bool beingTargeted = false;
	private bool hasTarget = false;
	public GameObject EnemyTarget;

	void Start ()
	{
		setParam ();
		AttackCD = maxAttackCD;
		Health = maxHealth;
		Armor = maxArmor;
		AttackPower = maxAttackPower;
		thisColor = SpriteAndAnimation.GetComponent<SpriteRenderer> ().color;
		EnemyAnimator = SpriteAndAnimation.GetComponent<Animator> ();
		EnemyAnimator.SetFloat ("WalkingSpeed", walkingSpeed);
	}

	void setParam ()
	{
		switch (EnemyLevel) {
		case 1:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.SmallEnemySprite [0];
			break;
		case 2:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.SmallEnemySprite [0];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
			break;
		case 3:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.SmallEnemySprite [2];
			break;
		case 4:
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().sprite = GameManager.GM.SmallEnemySprite [2];
			SpriteAndAnimation.GetComponent<SpriteRenderer> ().color = new Color (253f / 255f, 132f / 255f, 132f / 255f);
			break;
		}
		int baseIndex = 9;
		baseIndex += (EnemyLevel - 1);
		string[] Params = GameManager.GM.TowerAndEnemyNum.text.Split ("\n" [0]) [baseIndex].Split (' ');
		float.TryParse (Params [0], out maxHealth);
		float.TryParse (Params [1], out maxAttackPower);
		float.TryParse (Params [2], out maxArmor);
		float.TryParse (Params [3], out maxAttackCD);
		float.TryParse (Params [4], out walkingSpeed);
	}

	void Update ()
	{
		if (hasTarget)
			Hit ();
		else {
			SearchForTarget ();
		}
			
	}

	public void AddWalkingSpeed (float addedspeed)
	{
		walkingSpeed += addedspeed;
		EnemyAnimator.SetFloat ("WalkingSpeed", walkingSpeed);
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
				if (hit != null && hit.gameObject.tag == "Tower" && hit.gameObject.GetComponent<TowerControl> ().TT != TowerType.Defense && hit.transform.parent.tag != "ProductionSlot") {
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
			EnemyTarget.gameObject.SendMessage ("TakeDamage", AttackPower);
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
