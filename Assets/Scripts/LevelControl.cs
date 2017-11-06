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
	public GameObject LockDownPrefab;
	public GameObject missilePrefab;
	public Transform[] showerSpawns;
	public GameObject debrisPrefab;
	public GameObject aggrPrefab;
	public GameObject aggrMissilePrefab;

	int BackgroundcolorPointer = 0;
	public int epoch = 0;
	float ThreshHold1 = 100f;
	float ThreshHold2 = 100f;
	int lowerPartLevel = 1;
	int middlePartLevel = 2;
	int higherPartLevel = 3;
	float mdr = 0.8f;
	int perMultiSpawn = 4;
	float waitTime = 65f;
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

	public void StartGame ()
	{
		StartCoroutine (StartSpawn ());
	}

	IEnumerator singleAggregate ()
	{
		// Find the lower left corner of the aggregation
		// (1,2) - (4,5)
		int randPos = Random.Range (10, 30);
		while (randPos == 14 || randPos == 15 || randPos == 24 || randPos == 25) {
			randPos = Random.Range (10, 30);
		}
		GameObject aggr = (GameObject)Instantiate (aggrPrefab, GameManager.GM.Slots [randPos].transform.position, Quaternion.identity);
		yield return new WaitForSeconds (8f);
		//Generate Rocket
		int randSpawnPos = Random.Range (0, 5);
		GameObject AgreMissile = (GameObject)Instantiate (aggrMissilePrefab, showerSpawns [randSpawnPos].position, Quaternion.identity);
		AgreMissile.GetComponent<AggregateMissileControl> ().SetTarget (aggr.transform.GetChild (0).transform.position, 2f);
		yield return new WaitForSeconds (1f);
		Destroy (aggr);
	}

	IEnumerator singleShower ()
	{
		// First find a occupied spot
		int randPos = Random.Range (10, 35);
		while (!GameManager.GM.Slots [randPos].activeSelf) {
			randPos = Random.Range (10, 35);
		}
		// Create Lockdown
		GameObject lockDown = (GameObject)Instantiate (LockDownPrefab, GameManager.GM.Slots [randPos].transform.position, Quaternion.identity);
		yield return new WaitForSeconds (5f);
		//Generate Rocket
		int randSpawnPos = Random.Range (0, 5);
		GameObject missile = (GameObject)Instantiate (missilePrefab, showerSpawns [randSpawnPos].position, Quaternion.identity);
		missile.GetComponent<MissileControl> ().SetTarget (GameManager.GM.Slots [randPos], 100f);
		Destroy (lockDown, 0.5f);
	}

	IEnumerator multipleShower (int amount)
	{
		for (int i = 0; i < amount; i++) {
			StartCoroutine (singleShower ());
			yield return new WaitForSeconds (4f);
		}
	}

	IEnumerator startExtra ()
	{
		while (true) {
			int rand = Random.Range (0, 2);
//			int rand = 1;
			if (rand == 0) {
				StartCoroutine (multipleShower (10));
				yield return new WaitForSeconds (65f);
			} else {
				StartCoroutine (singleAggregate ());
				yield return new WaitForSeconds (20f);
			}
		}
	}

	IEnumerator StartSpawn ()
	{
		yield return new WaitForSeconds (3f);
		StartCoroutine (singleSpawn (1f));

		yield return new WaitForSeconds (10f);
//		Debug.Log ("Phase 2");
//		multipleSpawns (3, 6f, 0f, 6f);
//		yield return new WaitForSeconds (30f);
//		Debug.Log ("Phase 3");
		multipleSpawns (5, 5f, 0f, 5f);
		yield return new WaitForSeconds (15f);
		Debug.Log ("Phase 4");
		multipleSpawns (20, 3.5f, 0.3f, 2f);
		yield return new WaitForSeconds (40f);
		Debug.Log ("Phase 5");
		StartCoroutine (groupSpawns (10, 2, 2f, 3f));
		yield return new WaitForSeconds (40f);
		Debug.Log ("Phase 6");
		// Let the shower begin
		StartCoroutine (startExtra ());
		Debug.Log ("Shower Started");
		StartCoroutine (groupSpawns (10, 4, 2f, 2f));
		yield return new WaitForSeconds (65f);
		Debug.Log ("Phase 7");
		// Let the other things come in!

		while (true) {
			StartCoroutine (groupSpawns (10, perMultiSpawn, minInterval, 2f));
			yield return new WaitForSeconds (waitTime);
		}
	}

	//called every epoch
	int epochToEnemyLevelNew ()
	{
		float randomNum = Random.Range (0f, 100f);
//		if (epoch > 20) {
//			ThreshHold1 -= minDecadeRate;
//		}
//		if (epoch > 90) {
//			ThreshHold2 -= minDecadeRate;
//		}
		if (epoch > 10)
			ThreshHold1 -= minDecadeRate;
		if (epoch > 60) {
			ThreshHold2 -= minDecadeRate;
		}
		if (ThreshHold1 <= 0f) {
			ThreshHold1 = 100f;
			lowerPartLevel++;
			middlePartLevel++;
			higherPartLevel++;
			changeBackgroundColor ();
			minDecadeRate = 1f;
			perMultiSpawn = 4;
			waitTime = 90f;
			minInterval = 2f;
		}
		if (ThreshHold2 <= 0f) {
			ThreshHold2 = 100f;
			lowerPartLevel++;
			middlePartLevel++;
			higherPartLevel++;
			changeBackgroundColor ();
			minDecadeRate = 1f;
			perMultiSpawn = 4;
			waitTime = 90f;
			minInterval = 2f;
		}
		if (higherPartLevel == 5) {
			minDecadeRate = 1f;
		}
		float smallerHold = Mathf.Min (ThreshHold1, ThreshHold2);
		float biggerHold = Mathf.Max (ThreshHold1, ThreshHold2);
		if (smallerHold <= 15f) {
			perMultiSpawn = 5;
			waitTime = 90f;
//			minInterval -= 0.04f;
//			minDecadeRate -= 0.2f;
		} else if (smallerHold <= 7f) {
			perMultiSpawn = 6;
			waitTime = 90f;
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
		// If epoch bigger than 85, 10% chances strange things might come in
		Random.InitState (System.Environment.TickCount);
		float rand = Random.Range (0, 1);
		if (rand < 0.1f && epoch > 86) {
			// Then we spawn strange things
			int ran = Random.Range (0, 2);
			if (ran == 0) {
				// Spawn Small Zombie
				int randomIndex = Random.Range (0, SmallEnemySpawns.Length);
				Debug.Log ("Called Small Zombie");
				GameObject newSmallEnemy = Instantiate (SmallEnemyPrefab, SmallEnemySpawns [randomIndex].transform.position, Quaternion.Euler (new Vector3 (0f, 0f, -90f)));
				newSmallEnemy.GetComponent<SmallEnemyControl> ().setLevel (level);
			} else {
				// Spawn Plane
				int randomIndex = Random.Range (0, SmallEnemySpawns.Length);
				GameObject newFlyEnemy = Instantiate (FlyEnemyPrefab, SmallEnemySpawns [randomIndex].transform.position, Quaternion.Euler (new Vector3 (0f, 0f, -90f)));
				newFlyEnemy.GetComponent<FlyEnemyControl> ().setLevel (middlePartLevel);
			}
		} else {
			// Else we just spawn a normal zombie
			int randomIndex = Random.Range (0, EnemySpawns.Length);
			while (!EnemySpawns [randomIndex].activeSelf) {
				randomIndex = Random.Range (0, EnemySpawns.Length);
			}
			GameObject newEnemy = Instantiate (EnemyPrefab, EnemySpawns [randomIndex].transform.position, Quaternion.Euler (new Vector3 (0f, 0f, -90f)));
			newEnemy.GetComponent<EnemyControl> ().setLevel (level);
		}
			


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

	public int getLowerLevel ()
	{
		return lowerPartLevel;
	}

	void changeBackgroundColor ()
	{
		BackgroundcolorPointer++;
		BackgroundcolorPointer %= BackgroundColorsPool.Length;
		BackgroundImage.GetComponent<Image> ().color = BackgroundColorsPool [BackgroundcolorPointer];
	}

	//utility function
	public void startDebris (Transform pos, float time)
	{
		GameObject debris = (GameObject)Instantiate (debrisPrefab, pos.position, Quaternion.identity);
		StartCoroutine (end (debris, time));
	}

	IEnumerator end (GameObject go, float time)
	{
		yield return new WaitForSeconds (time);
		Destroy (go);
	}

	public void startDisable (GameObject go, float time)
	{
		go.SetActive (false);
		StartCoroutine (istartD (go, time));
	}

	IEnumerator istartD (GameObject go, float time)
	{
		yield return new WaitForSeconds (time);
		go.SetActive (true);
	}

	// Tutorial Design
	public void SpawnFirstEnemy ()
	{
		StartCoroutine (singleSpawn (0f));
	}

	public void SpawnMoreEnemies ()
	{
		multipleSpawns (3, 6f, 0, 4);
	}
}
