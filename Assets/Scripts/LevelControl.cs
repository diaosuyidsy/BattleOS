using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelControl : MonoBehaviour
{
	public static LevelControl LC;
	public GameObject[] EnemySpawns;
	public GameObject[] SmallEnemySpawns;
	public GameObject EnemyPrefab;
	public GameObject SmallEnemyPrefab;
	public GameObject FlyEnemyPrefab;
	public float spawnIntervals = 5f;

	public int epoch = 0;
	float Level2ThreshHold = 0f;
	float Level3ThreshHold = 0f;
	float Level4ThreshHold = 0f;
	bool level2Add = true;

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
		yield return new WaitForSeconds (120f);
		Debug.Log ("Phase 7");
		StartCoroutine (groupSpawns (15, 4, 2f, 2f));
	}

	int epochToEnemyLevel ()
	{
		float randomNum = Random.Range (0f, 100f);
		if (epoch == 20) {
			Level2ThreshHold = 5f;
			Level3ThreshHold = Level2ThreshHold;
			Level4ThreshHold = Level2ThreshHold;
		}
		if (epoch > 20) {
			if (level2Add) {
				if (Level2ThreshHold <= 75f) {
					Level2ThreshHold++;
					Level3ThreshHold++;
					Level4ThreshHold++;
				}
			} else {
				Level2ThreshHold -= 0.8f;
			}
		}
		if (Level2ThreshHold >= 75f) {
			level2Add = false;
		}
		if (epoch > 80) {
			if (Level3ThreshHold <= 85f) {
				Level3ThreshHold++;
				Level4ThreshHold++;
			}
		}

		if (epoch > 140) {
			if (Level4ThreshHold <= 92f)
				Level4ThreshHold += 0.2f;
		}
		ConsoleProDebug.Watch ("Level 2 Threshold: ", Level2ThreshHold.ToString ());
		ConsoleProDebug.Watch ("Level 3 Threshold: ", Level3ThreshHold.ToString ());
		ConsoleProDebug.Watch ("Level 4 Threshold: ", Level4ThreshHold.ToString ());
		if (randomNum <= Level2ThreshHold) {
			return 2;
		} else if (randomNum <= Level3ThreshHold) {
			return 3;
		} else if (randomNum <= Level4ThreshHold) {
			return 4;
		} else {
			return 1;
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
		ConsoleProDebug.Watch ("Epoch#: ", epoch.ToString ());
		int level = epochToEnemyLevel ();
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
		yield return new WaitForSeconds (5f);
		StartCoroutine (constantlyAddCoins ());
	}

		
}
