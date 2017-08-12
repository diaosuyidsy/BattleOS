using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager GM;
	public GameObject[] Slots;
	public GameObject selectedTower;

	void Awake ()
	{
		GM = this;
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

	public TowerInfo ()
	{
		thisTowerType = TowerType.Defense;
		level = 1;
	}

	public TowerInfo (TowerType tt)
	{
		thisTowerType = tt;
		level = 1;
	}

	public TowerInfo (TowerType tt, int l)
	{
		thisTowerType = tt;
		level = l;
	}

	public bool Equals (TowerInfo other)
	{
		return  thisTowerType == other.thisTowerType && level == other.level;
	}
}
