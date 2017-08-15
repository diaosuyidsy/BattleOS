using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
	public static GameManager GM;
	public GameObject[] Slots;
	public GameObject[] ProductionLocks;
	public GameObject[] ProductionStarters;
	public GameObject selectedTower;
	public int Coins = 0;
	public Text CoinText;
	public int[] UnlockNextSlotCoin;
	public int nextSlotMultiplier;
	public GameObject ConsumeCoinHolder;
	public GameObject LevelUpEffect;
	public Animator CoinAnimator;
	public TextAsset TowerAndEnemyNum;
	public string[] PlayerDesignNumberStrs;
	public Sprite[] EnemySprite;
	public Sprite[] SmallEnemySprite;
	public GameObject TargetedEnemy;

	private int slotNum = 0;

	void Awake ()
	{
		GM = this;
	}

	void Start ()
	{
		slotNum = 0;
		foreach (GameObject pl in ProductionLocks) {
			pl.GetComponentInChildren<Text> ().text = UnlockNextSlotCoin [slotNum].ToString ();
		}
		int startCoin = Coins;
		Coins = 0;
		AddCoin (startCoin);
		PlayerDesignNumberStrs = TowerAndEnemyNum.text.Split ("\n" [0]);

	}

	public bool hasEnoughCoin (int tryToSpend)
	{
		if (tryToSpend <= Coins)
			return true;
		StartCoroutine (stopShaking ());
		return false;
	}

	IEnumerator stopShaking ()
	{
		CoinAnimator.SetBool ("StartShaking", true);
		yield return new WaitForSeconds (0.2f);
		CoinAnimator.SetBool ("StartShaking", false);
	}

	public void AddCoin (int amount)
	{
		Coins += amount;
		CoinText.text = Coins.ToString ();
		// Each time coin change, Check stuff
		for (int i = 0; i < ProductionStarters.Length; i++) {
			if (ProductionStarters [i].activeSelf) {
				if (hasEnoughCoin (Slots [i].GetComponent<ReproduceControl> ().productionCoin)) {
					ProductionStarters [i].transform.GetChild (1).GetComponent<Text> ().color = Color.white;
				} else {
					ProductionStarters [i].transform.GetChild (1).GetComponent<Text> ().color = Color.red;
				}
			}
		}
		if (Coins >= UnlockNextSlotCoin [slotNum]) {
			foreach (GameObject pl in ProductionLocks) {
				pl.GetComponentInChildren<Text> ().color = Color.white;
			}
		} else {
			foreach (GameObject pl in ProductionLocks) {
				pl.GetComponentInChildren<Text> ().color = Color.red;
			}
		}
	}

	public void UnlockSlot ()
	{
		if (!hasEnoughCoin (UnlockNextSlotCoin [slotNum]) || slotNum > 5)
			return;
		if (slotNum == 0) {
			
		}
		AddCoin (-UnlockNextSlotCoin [slotNum]);
		slotNum++;
		int targetIndex = 0;
		targetIndex = EventSystem.current.currentSelectedGameObject.transform.parent.GetSiblingIndex ();
		Slots [targetIndex].GetComponent<ReproduceControl> ().Locked = false;
		ProductionLocks [targetIndex].SetActive (false);
		if (slotNum > 5)
			return;
		for (int i = 0; i < ProductionLocks.Length; i++) {
			ProductionLocks [i].GetComponentInChildren<Text> ().text = UnlockNextSlotCoin [slotNum].ToString ();
		}


	}

	public void ForceTargetEnemy (GameObject newTarget)
	{
		if (TargetedEnemy == null)
			TargetedEnemy = newTarget;
		else {
			if (newTarget != TargetedEnemy) {
				TargetedEnemy.SendMessage ("targeted");
				TargetedEnemy = newTarget;
			} else
				TargetedEnemy = null;
		}
	}

	public void Restart ()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void PauseGame ()
	{
		Time.timeScale = 0f;
	}
}

public enum TowerType
{
	Defense,
	Range,
	Heal,
};

public class TowerInfo
{
	public TowerType thisTowerType;
	public int level;
	public float currentHealth;

	public TowerInfo ()
	{
		thisTowerType = TowerType.Defense;
		level = 1;
		currentHealth = 0f;
	}

	public TowerInfo (TowerType tt)
	{
		thisTowerType = tt;
		level = 1;
		currentHealth = 0f;
	}

	public TowerInfo (TowerType tt, int l)
	{
		thisTowerType = tt;
		level = l;
		currentHealth = 0f;
	}

	public TowerInfo (TowerType tt, int l, float health)
	{
		thisTowerType = tt;
		level = l;
		currentHealth = health;
	}

	public void setHealth (float health)
	{
		currentHealth = health;
	}

	public bool Equals (TowerInfo other)
	{
		return  thisTowerType == other.thisTowerType && level == other.level;
	}
}
