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
	public Sprite[] RangeTowerSprites;
	public Sprite[] RangeTowerBaseSprites;
	public Sprite[] MeleeTowerSprites;
	public GameObject TargetedEnemy;
	public GameObject Level1TankTower;
	public GameObject Level1HealTower;
	public Text PlayerHealthText;
	public GameObject GameOverHolder;
	public Text[] TowerInfoPanelTexts;
	public GAui TowerInfoPanel;
	public Text ScoreText;
	private int score;

	private int PlayerHealth = 2;
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
		PlayerHealthText.text = PlayerHealth.ToString ();
	}

	public bool hasEnoughCoin (int tryToSpend)
	{
		if (tryToSpend <= Coins)
			return true;
		StartCoroutine (stopShaking ());
		return false;
	}

	public bool hasEnoughCoin_Plain (int tryToSpend)
	{
		return tryToSpend <= Coins;
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
				if (hasEnoughCoin_Plain (Slots [i].GetComponent<ReproduceControl> ().productionCoin)) {
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

	public void onScore (int scorenum)
	{
		score += scorenum;
		ScoreText.text = score.ToString ();
	}

	public void UnlockSlot ()
	{
		if (!hasEnoughCoin (UnlockNextSlotCoin [slotNum]) || slotNum > 5)
			return;
		if (slotNum == 0) {
			int slotIndex = 28;
			while (Slots [slotIndex].transform.childCount != 0) {
				slotIndex--;
			}
			Instantiate (Level1TankTower, Slots [slotIndex].transform.position, Quaternion.identity, Slots [slotIndex].transform);
		}
		if (slotNum == 1) {
			int slotIndex = 18;
			while (Slots [slotIndex].transform.childCount != 0) {
				slotIndex--;
			}
			Instantiate (Level1HealTower, Slots [slotIndex].transform.position, Quaternion.identity, Slots [slotIndex].transform);
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
		PlayerHealth++;
		PlayerHealthText.text = PlayerHealth.ToString ();
	}

	public void onSelectedTower ()
	{
		if (selectedTower == null) {
			TowerInfoPanel.MoveOut (GUIAnimSystem.eGUIMove.SelfAndChildren);
			return;
		}
		string[] allInfo = selectedTower.GetComponent<TowerControl> ().getTowerInfos ();
		for (int i = 0; i < TowerInfoPanelTexts.Length; i++) {
			TowerInfoPanelTexts [i].text = allInfo [i];
		}
		TowerInfoPanel.MoveIn (GUIAnimSystem.eGUIMove.SelfAndChildren);
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

	public void HarmPlayer ()
	{
		PlayerHealth--;
		PlayerHealthText.text = PlayerHealth.ToString ();
		if (PlayerHealth <= 0)
			GameOver ();
	}

	void GameOver ()
	{
		GameOverHolder.SetActive (true);
		Time.timeScale = 0f;
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
	Tank,
	Range,
	Heal,
	Missile,
};

public class TowerInfo
{
	public TowerType thisTowerType;
	public int level;
	public float currentHealth;

	public TowerInfo ()
	{
		thisTowerType = TowerType.Tank;
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
