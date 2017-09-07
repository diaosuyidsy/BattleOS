using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelControl : MonoBehaviour
{
	public static LevelControl LC;
	public GameObject[] EnemySpawns;
	public GameObject[] SmallEnemySpawns;
	public GameObject EnemyPrefab;
	public GameObject SmallEnemyPrefab;
	public GameObject FlyEnemyPrefab;
	public GameObject BackgroundImage;
	public Color[] BackgroundColorsPool;
	public float spawnIntervals = 5f;

	int BackgroundcolorPointer = 0;
	public int epoch = 0;
	float ThreshHold1 = 100f;
	float ThreshHold2 = 100f;
	int lowerPartLevel = 1;
	int middlePartLevel = 2;
	int higherPartLevel = 3;
	float mdr = 0.8f;
	int perMultiSpawn = 4;
	float waitTime = 130f;
	float mi = 2f;

	float minInterval {
		get {
			return mi;
		}
		set {
			mi = Mathf.Max (1.4f, value);
		}
	}

	float minDecadeRate {
		get {
			return mdr;
		}
		set {
			mdr = Mathf.Max (0.4f, value);
		}
	}

	void Awake ()
	{
		LC = this;
	}

	void Start ()
	{
		StartCoroutine (StartSpawn ());
		StartCoroutine (constantlyAddCoins ());
	}

	IEnumerator StartSpawn ()
	{
		StartCoroutine (singleSpawn (1f));
		yield return new WaitForSeconds (12f);
		Debug.Log ("Phase 2");
		multipleSpawns (3, 6f, 0f, 6f);
		yield return new WaitForSeconds (30f);
		Debug.Log ("Phase 3");
		multipleSpawns (5, 5f, 0f, 5f);
		yield return new WaitForSeconds (35f);
		Debug.Log ("Phase 4");
		multipleSpawns (20, 5f, 0.3f, 2f);
		yield return new WaitForSeconds (80f);
		Debug.Log ("Phase 5");
		StartCoroutine (groupSpawns (10, 2, 2f, 3f));
		yield return new WaitForSeconds (80f);
		Debug.Log ("Phase 6");
		StartCoroutine (groupSpawns (10, 4, 2f, 2f));
		yield return new WaitForSeconds (130f);
		Debug.Log ("Phase 7");
		while (true) {
			StartCoroutine (groupSpawns (10, perMultiSpawn, minInterval, 2f));
			yield return new WaitForSeconds (waitTime);
		}
	}

	//called every epoch
	int epochToEnemyLevelNew ()
	{
		float randomNum = Random.Range (0f, 100f);
		if (epoch > 20) {
			ThreshHold1 -= minDecadeRate;
		}
		if (epoch > 90) {
			ThreshHold2 -= minDecadeRate;
		}
		if (ThreshHold1 <= 0f) {
			ThreshHold1 = 100f;
			lowerPartLevel++;
			middlePartLevel++;
			higherPartLevel++;
			changeBackgroundColor ();
			changeFortifySpellCoin ();
			minDecadeRate = 0.8f;
			perMultiSpawn = 4;
			waitTime = 130f;
			minInterval = 2f;
		}
		if (ThreshHold2 <= 0f) {
			ThreshHold2 = 100f;
			lowerPartLevel++;
			middlePartLevel++;
			higherPartLevel++;
			changeBackgroundColor ();
			changeFortifySpellCoin ();
			minDecadeRate = 0.8f;
			perMultiSpawn = 4;
			waitTime = 130f;
			minInterval = 2f;
		}
		if (higherPartLevel == 5) {
			minDecadeRate = 0.8f;
		}
		float smallerHold = Mathf.Min (ThreshHold1, ThreshHold2);
		float biggerHold = Mathf.Max (ThreshHold1, ThreshHold2);
		if (smallerHold <= 15f) {
			perMultiSpawn = 5;
			waitTime = 140f;
			minInterval -= 0.04f;
			minDecadeRate -= 0.2f;
		} else if (smallerHold <= 7f) {
			perMultiSpawn = 6;
			waitTime = 145f;
		}
		ConsoleProDebug.Watch ("Lower Thresh Hold", smallerHold.ToString ());
		ConsoleProDebug.Watch ("Higher Thresh Hold", biggerHold.ToString ());
		ConsoleProDebug.Watch ("Min Decade Rate", minDecadeRate.ToString ());

		if (randomNum <= smallerHold) {
			return lowerPartLevel;
		} else if (randomNum <= biggerHold) {
			return middlePartLevel;
		} else {
			return higherPartLevel;
		}
	}

	IEnumerator singleSpawn (float time)
	{
		yield return new WaitForSeconds (time);
		epoch++;
		if (epoch == 4)
			LevelControl.LC.EnemySpawns [0].SetActive (true);
		if (epoch == 20)
			LevelControl.LC.EnemySpawns [4].SetActive (true);
		ConsoleProDebug.Watch ("Epoch#", epoch.ToString ());
		int level = epochToEnemyLevelNew ();
		int randomIndex = Random.Range (0, EnemySpawns.Length);
		while (!EnemySpawns [randomIndex].activeSelf) {
			randomIndex = Random.Range (0, EnemySpawns.Length);
		}
		GameObject newEnemy = Instantiate (EnemyPrefab, EnemySpawns [randomIndex].transform.position, Quaternion.Euler (new Vector3 (0f, 0f, -90f)));
		newEnemy.GetComponent<EnemyControl> ().setLevel (level);
	}

	void multipleSpawns (int num, float interval, float decrease, float limit)
	{
		float lastInterval = 0;
		for (int i = 0; i < num; i++) {
			StartCoroutine (singleSpawn (lastInterval));
			if (interval > limit)
				interval -= decrease;
			lastInterval += interval;
		}
	}

	// how many groups, enmey per group, interval per enemy per group
	IEnumerator groupSpawns (int groups, int epergroup, float minterval, float groupinterval)
	{
		for (int i = 0; i < groups; i++) {
			multipleSpawns (epergroup, minterval, 0f, minterval);
			yield return new WaitForSeconds (epergroup * minterval + groupinterval);
		}
	}

	IEnumerator constantlyAddCoins ()
	{
		GameManager.GM.AddCoin (1);
		yield return new WaitForSeconds (minDecadeRate * 5f);
		StartCoroutine (constantlyAddCoins ());
	}

	public int getMiddleLevel ()
	{
		return middlePartLevel;
	}

	void changeBackgroundColor ()
	{
		BackgroundcolorPointer++;
		BackgroundcolorPointer %= BackgroundColorsPool.Length;
		BackgroundImage.GetComponent<Image> ().color = BackgroundColorsPool [BackgroundcolorPointer];
	}

	void changeFortifySpellCoin ()
	{
		GameManager.GM.FortifySpell.GetComponent<FortifyControl> ().refresh ();
	}

}
