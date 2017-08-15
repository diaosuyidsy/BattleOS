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

	void Awake ()
	{
		LC = this;
	}

	void Start ()
	{
		StartSpawn ();
	}

	public void StartSpawn ()
	{
		StartCoroutine (spawn (spawnIntervals));
	}

	IEnumerator spawn (float time)
	{
		yield return new WaitForSeconds (time);
		int randomIndex = Random.Range (0, EnemySpawns.Length);
		if (spawnIntervals >= 2.5f)
			spawnIntervals -= 0.05f;
		Instantiate (EnemyPrefab, EnemySpawns [randomIndex].transform.position, Quaternion.Euler (new Vector3 (0f, 0f, -90f)));
		StartCoroutine (spawn (spawnIntervals));
	}
		
}
